using MemoryPack.Internal;
using System.Buffers;
using System.Collections;

namespace MemoryPack.Formatters;

[Preserve]
public sealed class BitArrayFormatter : MemoryPackFormatter<BitArray>
{
    // serialize [length(bit count), int[]]

    // Private fields of BitArray depend on the runtime(.NET 9 or lower is (int[] m_array, int m_length), .NET 10 or later is (byte[] _array, int _bitLength), Mono has different field offsets),
    // so only use the public API.

    [Preserve]
    public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref BitArray? value)
    {
        if (value == null)
        {
            writer.WriteNullObjectHeader();
            return;
        }

        var length = value.Length;
        var arrayLength = GetInt32ArrayLength(length);

        var rented = ArrayPool<int>.Shared.Rent(arrayLength);
        try
        {
            value.CopyTo(rented, 0);
            var span = rented.AsSpan(0, arrayLength);

            // unused bits of the last element are not cleared in some runtimes, clear to write the same binary.
            var extraBits = length & 31;
            if (extraBits != 0)
            {
                span[span.Length - 1] &= (1 << extraBits) - 1;
            }

            writer.WriteUnmanagedWithObjectHeader(2, length);
            writer.WriteUnmanagedSpan(span);
        }
        finally
        {
            ArrayPool<int>.Shared.Return(rented);
        }
    }

    [Preserve]
    public override void Deserialize(ref MemoryPackReader reader, scoped ref BitArray? value)
    {
        if (!reader.TryReadObjectHeader(out var count))
        {
            value = null;
            return;
        }

        if (count != 2) MemoryPackSerializationException.ThrowInvalidPropertyCount(2, count);

        reader.ReadUnmanaged(out int length);
        var array = reader.ReadUnmanagedArray<int>() ?? Array.Empty<int>();

        // the array can be longer than required(BitArray does not always shrink the inner array) but must not be shorter.
        if (length < 0 || array.Length < GetInt32ArrayLength(length))
        {
            MemoryPackSerializationException.ThrowMessage($"BitArray length {length} is not matched to array length {array.Length}.");
        }

        var bitArray = new BitArray(array);
        bitArray.Length = length;
        value = bitArray;
    }

    static int GetInt32ArrayLength(int bitLength)
    {
        return bitLength > 0 ? ((bitLength - 1) >> 5) + 1 : 0;
    }
}
