using MemoryPack.Internal;
using System.Collections;
using System.Runtime.CompilerServices;

namespace MemoryPack.Formatters;

[Preserve]
public sealed class BitArrayFormatter : MemoryPackFormatter<BitArray>
{
    public static readonly BitArrayFormatter Default = new BitArrayFormatter();

    // serialize [m_length, m_array]

    [Preserve]
    public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref BitArray? value)
    {
        if (value == null)
        {
            writer.WriteNullObjectHeader();
            return;
        }

        ref var view = ref Unsafe.As<BitArray, BitArrayView>(ref value);

        writer.WriteUnmanagedWithObjectHeader(2, view.m_length);
        writer.WriteUnmanagedArray(view.m_array);
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

        var bitArray = new BitArray(length, false); // create internal int[] and set m_length to length

        ref var view = ref Unsafe.As<BitArray, BitArrayView>(ref bitArray);
        reader.ReadUnmanagedArray(ref view.m_array!);

        value = bitArray;
    }
}

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
[Preserve]
internal class BitArrayView
{
    public int[] m_array;
    public int m_length;
    public int _version;
}
