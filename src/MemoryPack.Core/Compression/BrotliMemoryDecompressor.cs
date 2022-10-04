using MemoryPack.Internal;
using System.Buffers;
using System.IO.Compression;

namespace MemoryPack.Compression;

public struct BrotliMemoryDecompressor : IDisposable
{
    byte[]? buffer;
    BrotliSequenceDecompressor sequenceDecompressor;

    public BrotliMemoryDecompressor()
    {
        this.buffer = null;
        this.sequenceDecompressor = default;
    }

    public ReadOnlyMemory<byte> Decompress(ReadOnlyMemory<byte> compressedBuffer)
    {
        if (buffer != null)
        {
            throw new Exception(); // TODO:...
        }

        buffer = ArrayPool<byte>.Shared.Rent(GetDoubleCapacity(compressedBuffer.Length));

        // Fast path
        if (BrotliDecoder.TryDecompress(compressedBuffer.Span, buffer, out var written))
        {
            return buffer.AsMemory(0, written);
        }

        // Slow path
        ArrayPool<byte>.Shared.Return(buffer);

        sequenceDecompressor = new BrotliSequenceDecompressor(new ReadOnlySequence<byte>(compressedBuffer));

        var decompressedSeqeunce = sequenceDecompressor.Decompress();

        if (decompressedSeqeunce.IsSingleSegment)
        {
            return decompressedSeqeunce.First;
        }

        var destLength = (int)decompressedSeqeunce.Length;
        buffer = ArrayPool<byte>.Shared.Rent(destLength);
        decompressedSeqeunce.CopyTo(buffer);

        return buffer.AsMemory(0, destLength);

    }

    public void Dispose()
    {
        if (buffer != null)
        {
            ArrayPool<byte>.Shared.Return(buffer);
            buffer = null;
        }

        sequenceDecompressor.Dispose();
    }

    int GetDoubleCapacity(int length)
    {
        var newCapacity = unchecked(length * 2);
        if ((uint)newCapacity > int.MaxValue) newCapacity = int.MaxValue;
        // TODO: return Math.Max(newCapacity, 4096);
        return newCapacity;
    }
}
