using MemoryPack.Internal;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.IO.Compression;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MemoryPack.Compression;

#if !NET7_0_OR_GREATER
#pragma warning disable CS8602
#endif

public
#if NET7_0_OR_GREATER
    struct
#else
    class
#endif
    BrotliCompressor : IBufferWriter<byte>, IDisposable
{
    ReusableLinkedArrayBufferWriter? bufferWriter;
    readonly int quality;
    readonly int window;

#if NET7_0_OR_GREATER

    public BrotliCompressor()
        : this(CompressionLevel.Fastest)
    {

    }

#endif

    public BrotliCompressor(CompressionLevel compressionLevel)
        : this(BrotliUtils.GetQualityFromCompressionLevel(compressionLevel), BrotliUtils.WindowBits_Default)
    {

    }

    public BrotliCompressor(CompressionLevel compressionLevel, int window)
        : this(BrotliUtils.GetQualityFromCompressionLevel(compressionLevel), window)
    {

    }

    public BrotliCompressor(int quality = 1, int window = 22)
    {
        this.bufferWriter = ReusableLinkedArrayBufferWriterPool.Rent();
        this.quality = quality;
        this.window = window;
    }

    void IBufferWriter<byte>.Advance(int count)
    {
        ThrowIfDisposed();
        bufferWriter.Advance(count);
    }

    Memory<byte> IBufferWriter<byte>.GetMemory(int sizeHint)
    {
        ThrowIfDisposed();
        return bufferWriter.GetMemory(sizeHint);
    }

    Span<byte> IBufferWriter<byte>.GetSpan(int sizeHint)
    {
        ThrowIfDisposed();
        return bufferWriter.GetSpan(sizeHint);
    }

    public int GetMaxCompressedLength()
    {
        ThrowIfDisposed();
        return BrotliUtils.BrotliEncoderMaxCompressedSize(bufferWriter.TotalWritten);
    }

    public byte[] ToArray()
    {
        ThrowIfDisposed();

        using var encoder = new BrotliEncoder(quality, window);

        var maxLength = BrotliUtils.BrotliEncoderMaxCompressedSize(bufferWriter.TotalWritten);

        var finalBuffer = ArrayPool<byte>.Shared.Rent(maxLength);
        try
        {
            var writtenCount = 0;
            var destination = finalBuffer.AsSpan(0, maxLength);
            foreach (var source in bufferWriter)
            {
                var status = encoder.Compress(source.Span, destination, out var bytesConsumed, out var bytesWritten, isFinalBlock: false);
                if (status != OperationStatus.Done)
                {
                    MemoryPackSerializationException.ThrowCompressionFailed(status);
                }

                if (bytesConsumed != source.Span.Length)
                {
                    MemoryPackSerializationException.ThrowCompressionFailed();
                }

                if (bytesWritten > 0)
                {
                    destination = destination.Slice(bytesWritten);
                    writtenCount += bytesWritten;
                }
            }

            // call BrotliEncoderOperation.Finish
            var finalStatus = encoder.Compress(ReadOnlySpan<byte>.Empty, destination, out var consumed, out var written, isFinalBlock: true);
            writtenCount += written;

            return finalBuffer.AsSpan(0, writtenCount).ToArray();
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(finalBuffer);
        }
    }

    public void CopyTo<TBufferWriter>(in TBufferWriter destBufferWriter)
        where TBufferWriter : IBufferWriter<byte>
    {
        ThrowIfDisposed();

        var encoder = new BrotliEncoder(quality, window);
        try
        {
            var writtenNotAdvanced = 0;
            foreach (var item in bufferWriter)
            {
                writtenNotAdvanced = CompressCore(ref encoder, item.Span, ref Unsafe.AsRef(in destBufferWriter), initialLength: null, isFinalBlock: false);
            }

            // call BrotliEncoderOperation.Finish
            var finalBlockLength = (writtenNotAdvanced == 0) ? null : (int?)(writtenNotAdvanced + 10);
            CompressCore(ref encoder, ReadOnlySpan<byte>.Empty, ref Unsafe.AsRef(in destBufferWriter), initialLength: finalBlockLength, isFinalBlock: true);
        }
        finally
        {
            encoder.Dispose();
        }
    }

    public async ValueTask CopyToAsync(Stream stream, int bufferSize = 65535, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        using var encoder = new BrotliEncoder(quality, window);

        var buffer = ArrayPool<byte>.Shared.Rent(bufferSize);
        try
        {
            foreach (var item in bufferWriter)
            {
                var source = item;
                var lastResult = OperationStatus.DestinationTooSmall;
                while (lastResult == OperationStatus.DestinationTooSmall)
                {
                    lastResult = encoder.Compress(source.Span, buffer, out int bytesConsumed, out int bytesWritten, isFinalBlock: false);
                    if (lastResult == OperationStatus.InvalidData) MemoryPackSerializationException.ThrowCompressionFailed();
                    if (bytesWritten > 0)
                    {
                        await stream.WriteAsync(buffer.AsMemory(0, bytesWritten), cancellationToken).ConfigureAwait(false);
                    }
                    if (bytesConsumed > 0)
                    {
                        source = source.Slice(bytesConsumed);
                    }
                }
            }

            // call BrotliEncoderOperation.Finish
            var finalStatus = OperationStatus.DestinationTooSmall;
            while (finalStatus == OperationStatus.DestinationTooSmall)
            {
                finalStatus = encoder.Compress(ReadOnlySpan<byte>.Empty, buffer, out var consumed, out var written, isFinalBlock: true);
                if (written > 0)
                {
                    await stream.WriteAsync(buffer.AsMemory(0, written), cancellationToken).ConfigureAwait(false);
                }
            }
            if (finalStatus != OperationStatus.Done)
            {
                MemoryPackSerializationException.ThrowCompressionFailed(finalStatus);
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    public void CopyTo<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> memoryPackWriter)
#if NET7_0_OR_GREATER
        where TBufferWriter : IBufferWriter<byte>
#else
        where TBufferWriter : class, IBufferWriter<byte>
#endif
    {
        ThrowIfDisposed();

        var encoder = new BrotliEncoder(quality, window);
        try
        {
            var bytesWritten = 0;
            foreach (var item in bufferWriter)
            {
                var span = item.Span;
                if (span.Length <= 0) continue;
                bytesWritten += CompressCore(ref encoder, item.Span, ref memoryPackWriter, initialLength: null, isFinalBlock: false);
            }

            // call BrotliEncoderOperation.Finish
            var finalBlockMaxLength = BrotliUtils.BrotliEncoderMaxCompressedSize(bytesWritten) - bytesWritten;
            CompressCore(ref encoder, ReadOnlySpan<byte>.Empty, ref memoryPackWriter, initialLength: finalBlockMaxLength, isFinalBlock: true);
        }
        finally
        {
            encoder.Dispose();
        }
    }

    static int CompressCore<TBufferWriter>(ref BrotliEncoder encoder, ReadOnlySpan<byte> source, ref TBufferWriter destBufferWriter, int? initialLength, bool isFinalBlock)
        where TBufferWriter : IBufferWriter<byte>
    {
        var writtenNotAdvanced = 0;

        var lastResult = OperationStatus.DestinationTooSmall;
        while (lastResult == OperationStatus.DestinationTooSmall)
        {
            var dest = destBufferWriter.GetSpan(initialLength ?? source.Length);

            lastResult = encoder.Compress(source, dest, out int bytesConsumed, out int bytesWritten, isFinalBlock: isFinalBlock);
            writtenNotAdvanced += bytesConsumed;

            if (lastResult == OperationStatus.InvalidData) MemoryPackSerializationException.ThrowCompressionFailed();
            if (bytesWritten > 0)
            {
                destBufferWriter.Advance(bytesWritten);
                writtenNotAdvanced = 0;
            }
            if (bytesConsumed > 0)
            {
                source = source.Slice(bytesConsumed);
            }
        }

        return writtenNotAdvanced;
    }

    static int CompressCore<TBufferWriter>(ref BrotliEncoder encoder, ReadOnlySpan<byte> source, ref MemoryPackWriter<TBufferWriter> destBufferWriter, int? initialLength, bool isFinalBlock)
#if NET7_0_OR_GREATER
        where TBufferWriter : IBufferWriter<byte>
#else
        where TBufferWriter : class, IBufferWriter<byte>
#endif
    {
        var totalWritten = 0;

        var lastResult = OperationStatus.DestinationTooSmall;
        while (lastResult == OperationStatus.DestinationTooSmall)
        {
            ref var spanRef = ref destBufferWriter.GetSpanReference(initialLength ?? source.Length);
            var dest = MemoryMarshal.CreateSpan(ref spanRef, destBufferWriter.BufferLength);

            lastResult = encoder.Compress(source, dest, out int bytesConsumed, out int bytesWritten, isFinalBlock: isFinalBlock);
            totalWritten += bytesWritten;

            if (lastResult == OperationStatus.InvalidData) MemoryPackSerializationException.ThrowCompressionFailed();
            if (bytesWritten > 0)
            {
                destBufferWriter.Advance(bytesWritten);
            }
            if (bytesConsumed > 0)
            {
                source = source.Slice(bytesConsumed);
            }
        }

        return totalWritten;
    }

    public void Dispose()
    {
        if (bufferWriter == null) return;

        bufferWriter.Reset();
        ReusableLinkedArrayBufferWriterPool.Return(bufferWriter);
        bufferWriter = null!;
    }

#if NET7_0_OR_GREATER
    [MemberNotNull(nameof(bufferWriter))]
#endif
    void ThrowIfDisposed()
    {
        if (bufferWriter == null)
        {
            throw new ObjectDisposedException(null);
        }
    }
}

internal static partial class BrotliUtils
{
    public const int WindowBits_Min = 10;
    public const int WindowBits_Default = 22;
    public const int WindowBits_Max = 24;
    public const int Quality_Min = 0;
    public const int Quality_Default = 4;
    public const int Quality_Max = 11;
    public const int MaxInputSize = int.MaxValue - 515; // 515 is the max compressed extra bytes

    internal static int GetQualityFromCompressionLevel(CompressionLevel compressionLevel) =>
        compressionLevel switch
        {
            CompressionLevel.NoCompression => Quality_Min,
            CompressionLevel.Fastest => 1,
            CompressionLevel.Optimal => Quality_Default,
#if NET7_0_OR_GREATER
            CompressionLevel.SmallestSize => Quality_Max,
#endif
            _ => throw new ArgumentException()
        };


    // https://github.com/dotnet/runtime/issues/35142
    // BrotliEncoder.GetMaxCompressedLength is broken in .NET 7
    // port from encode.c https://github.com/google/brotli/blob/3914999fcc1fda92e750ef9190aa6db9bf7bdb07/c/enc/encode.c#L1200
    internal static int BrotliEncoderMaxCompressedSize(int input_size)
    {
        var num_large_blocks = input_size >> 14;
        var overhead = 2 + (4 * num_large_blocks) + 3 + 1;
        var result = input_size + overhead;
        if (input_size == 0) return 2;
        return (result < input_size) ? 0 : result;
    }
}
