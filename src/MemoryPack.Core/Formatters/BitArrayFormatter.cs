using MemoryPack.Internal;
using System.Collections;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace MemoryPack.Formatters;

[Preserve]
public sealed class BitArrayFormatter : MemoryPackFormatter<BitArray>
{
    // serialize [m_length, m_array]

    // BitArrayView depends on private field layout of BitArray(int[] m_array, int m_length).
    // .NET 10 changed it to (byte[] _array, int _bitLength), reinterpreting it reads/writes out of the array.
    // When layout is not matched, use public API instead of BitArrayView(binary format is same).
    static readonly bool canUseBitArrayView = CanUseBitArrayView();

    static bool CanUseBitArrayView()
    {
        try
        {
            const BindingFlags Flags = BindingFlags.Instance | BindingFlags.NonPublic;
            if (typeof(BitArray).GetField("m_array", Flags)?.FieldType != typeof(int[])
             || typeof(BitArray).GetField("m_length", Flags)?.FieldType != typeof(int))
            {
                return false;
            }

            // fields exist but the offsets can be different from BitArrayView(depends on the runtime, for example it has other fields), check it by actual value.
            var bitArray = new BitArray(37);
            bitArray[36] = true;
            ref var view = ref Unsafe.As<BitArray, BitArrayView>(ref bitArray);
            object? array = view.m_array;
            return array != null
                && array.GetType() == typeof(int[])
                && view.m_array.Length == 2
                && view.m_array[0] == 0
                && view.m_array[1] == 16
                && view.m_length == 37;
        }
        catch
        {
            return false;
        }
    }

    [Preserve]
    public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref BitArray? value)
    {
        if (value == null)
        {
            writer.WriteNullObjectHeader();
            return;
        }

        if (!canUseBitArrayView)
        {
            var length = value.Length;
            var array = new int[length > 0 ? ((length - 1) >> 5) + 1 : 0]; // same as m_array of BitArray(length)
            value.CopyTo(array, 0);

            writer.WriteUnmanagedWithObjectHeader(2, length);
            writer.WriteUnmanagedArray(array);
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

        if (!canUseBitArrayView)
        {
            var array = reader.ReadUnmanagedArray<int>();
            value = new BitArray(array ?? Array.Empty<int>());
            value.Length = length;
            return;
        }

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
