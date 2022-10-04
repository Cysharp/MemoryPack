using MemoryPack.Internal;
using System.Buffers;
using System.IO.Compression;

namespace MemoryPack.Compression;

public struct BrotliSequenceDecompressor : IDisposable
{
    ReadOnlySequence<byte> compressedSequence;
    ReusableReadOnlySequenceBuilder? sequenceBuilder;

    public BrotliSequenceDecompressor(ReadOnlySequence<byte> compressedSequence)
    {
        this.compressedSequence = compressedSequence;
        this.sequenceBuilder = null;
    }

    public ReadOnlySequence<byte> Decompress()
    {
        if (sequenceBuilder != null)
        {
            throw new Exception(); // TODO:exeception
        }

        sequenceBuilder = ReusableReadOnlySequenceBuilderPool.Rent();

        using var decoder = new BrotliDecoder();

        var status = OperationStatus.DestinationTooSmall;
        byte[]? buffer = null;
        foreach (var item in compressedSequence)
        {
            var source = item.Span;

            status = OperationStatus.DestinationTooSmall;
            var nextCapacity = source.Length;
            while (status == OperationStatus.DestinationTooSmall)
            {
                if (buffer == null)
                {
                    nextCapacity = GetDoubleCapacity(nextCapacity);
                    buffer = ArrayPool<byte>.Shared.Rent(nextCapacity);
                }

                status = decoder.Decompress(source, buffer, out var bytesConsumed, out var bytesWritten);

                if (status == OperationStatus.InvalidData)
                {
                    // TODO: error
                }

                if (status == OperationStatus.NeedMoreData)
                {
                    if (bytesWritten > 0)
                    {
                        sequenceBuilder.Add(buffer.AsMemory(0, bytesWritten), true);
                        buffer = null;
                    }
                    if (bytesConsumed > 0)
                    {
                        source = source.Slice(bytesConsumed);
                    }
                    if (source.Length != 0)
                    {
                        // TODO: error
                    }
                    goto NEXT_ITEM;
                }

                if (bytesConsumed > 0)
                {
                    source = source.Slice(bytesConsumed);
                }
                if (bytesWritten > 0)
                {
                    sequenceBuilder.Add(buffer.AsMemory(0, bytesWritten), true);
                    buffer = null;
                }
            }

        NEXT_ITEM:
            continue;
        }

        if (buffer != null)
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }

        if (status == OperationStatus.NeedMoreData)
        {
            throw new Exception("NEED MORE DATA"); // TODO:exception
        }

        return sequenceBuilder.Build();
    }

    public void Dispose()
    {
        if (sequenceBuilder != null)
        {
            ReusableReadOnlySequenceBuilderPool.Return(sequenceBuilder);
            sequenceBuilder = null;
        }
    }

    int GetDoubleCapacity(int length)
    {
        var newCapacity = unchecked(length * 2);
        if ((uint)newCapacity > int.MaxValue) newCapacity = int.MaxValue;
        // TODO: return Math.Max(newCapacity, 4096);
        return newCapacity;
    }
}
