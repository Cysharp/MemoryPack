using MemoryPack.Internal;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.IO.Compression;

namespace MemoryPack.Compression;

public struct BrotliCompressor : IBufferWriter<byte>, IDisposable
{
    ReusableLinkedArrayBufferWriter? bufferWriter;
    readonly int quality;
    readonly int window;

    public BrotliCompressor()
        : this(CompressionLevel.Fastest)
    {

    }

    public BrotliCompressor(CompressionLevel compressionLevel)
        : this(BrotliUtils.GetQualityFromCompressionLevel(compressionLevel), BrotliUtils.WindowBits_Default)
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

    public byte[] ToArray()
    {
        ThrowIfDisposed();

        using var encoder = new BrotliEncoder(quality, window);

        var maxLength = BrotliEncoder.GetMaxCompressedLength(bufferWriter.TotalWritten);
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

    public void CopyTo<TBufferWriter>(TBufferWriter destBufferWriter)
        where TBufferWriter : IBufferWriter<byte>
    {
        ThrowIfDisposed();

        var encoder = new BrotliEncoder(quality, window);
        try
        {
            var writtenNotAdvanced = 0;
            foreach (var item in bufferWriter)
            {
                writtenNotAdvanced = CompressCore(ref encoder, item.Span, destBufferWriter, initialLength: null, isFinalBlock: false);
            }

            // call BrotliEncoderOperation.Finish
            var finalBlockLength = (writtenNotAdvanced == 0) ? null : (int?)(writtenNotAdvanced + 10);
            CompressCore(ref encoder, ReadOnlySpan<byte>.Empty, destBufferWriter, initialLength: finalBlockLength, isFinalBlock: true);
        }
        finally
        {
            encoder.Dispose();
        }
    }

    static int CompressCore<TBufferWriter>(ref BrotliEncoder encoder, ReadOnlySpan<byte> source, TBufferWriter destBufferWriter, int? initialLength, bool isFinalBlock)
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

    public void Dispose()
    {
        if (bufferWriter == null) return;

        bufferWriter.Reset();
        ReusableLinkedArrayBufferWriterPool.Return(bufferWriter);
        bufferWriter = null!;
    }

    [MemberNotNull(nameof(bufferWriter))]
    void ThrowIfDisposed()
    {
        if (bufferWriter == null)
        {
            throw new ObjectDisposedException(null);
        }
    }
}

file static partial class BrotliUtils
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
            CompressionLevel.SmallestSize => Quality_Max,
            _ => throw new ArgumentException()
        };
}
