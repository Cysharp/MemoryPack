using MemoryPack.Internal;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.IO.Compression;
using System.Text;

namespace MemoryPack.Compression;
















public struct BrotliBufferWriter : IBufferWriter<byte>, IDisposable
{
    ReusableLinkedArrayBufferWriter? bufferWriter;
    readonly int quality;
    readonly int window;

    public BrotliBufferWriter()
        : this(CompressionLevel.Optimal)
    {

    }

    public BrotliBufferWriter(CompressionLevel compressionLevel)
        : this(BrotliUtils.GetQualityFromCompressionLevel(compressionLevel), BrotliUtils.WindowBits_Default)
    {

    }

    public BrotliBufferWriter(int quality = 4, int window = 22)
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
            var destination = finalBuffer.AsMemory(0, maxLength);
            foreach (var source in bufferWriter)
            {
                var status = encoder.Compress(source.Span, destination.Span, out var bytesConsumed, out var bytesWritten, isFinalBlock: false);
                if (status != OperationStatus.Done)
                {
                    // TODO: throw
                }
                // TODO: check bytesConsumed

                if (bytesWritten > 0)
                {
                    destination = destination.Slice(bytesWritten); // TODO: bytesWritten???
                    writtenCount += bytesWritten;
                }
            }

            // call BrotliEncoderOperation.Finish
            var finalStatus = encoder.Compress(ReadOnlySpan<byte>.Empty, destination.Span, out var consumed, out var written, isFinalBlock: true);
            writtenCount += written;

            return finalBuffer.AsSpan(0, writtenCount).ToArray();
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(finalBuffer);
            encoder.Dispose();
        }
    }

    // Flush
    public void Dispose()
    {
        if (bufferWriter == null) return;

        bufferWriter.Reset();
        bufferWriter = null!;
        ReusableLinkedArrayBufferWriterPool.Return(bufferWriter);
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
