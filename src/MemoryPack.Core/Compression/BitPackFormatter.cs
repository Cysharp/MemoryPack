using MemoryPack.Internal;
using System.Runtime.CompilerServices;

namespace MemoryPack.Compression;

[Preserve]
public sealed class BitPackFormatter : MemoryPackFormatter<bool[]>
{
    public static readonly BitPackFormatter Default = new BitPackFormatter();

    [Preserve]
    public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref bool[]? value)
    {
        if (value == null)
        {
            writer.WriteNullCollectionHeader();
            return;
        }
        writer.WriteCollectionHeader(value.Length);

        var data = 0;
        var bit = 0;
        foreach (var item in value)
        {
            Set(ref data, bit, item);

            bit += 1;

            if (bit == 32)
            {
                writer.WriteUnmanaged(data);
                data = 0;
                bit = 0;
            }
        }

        if (bit != 0)
        {
            writer.WriteUnmanaged(data);
        }
    }

    [Preserve]
    public override void Deserialize(ref MemoryPackReader reader, scoped ref bool[]? value)
    {
        if (!reader.DangerousTryReadCollectionHeader(out var length))
        {
            value = null;
            return;
        }

        if (length == 0)
        {
            value = Array.Empty<bool>();
            return;
        }

        var readCount = ((length - 1) / 32) + 1;
        var requireSize = readCount * 4;
        if (reader.Remaining < requireSize)
        {
            MemoryPackSerializationException.ThrowInsufficientBufferUnless(length);
        }

        if (value == null || value.Length != length)
        {
            value = new bool[length];
        }

        var bit = 0;
        var data = 0;
        for (int i = 0; i < value.Length; i++)
        {
            if (bit == 0)
            {
                reader.ReadUnmanaged(out data);
            }

            value[i] = Get(data, bit);

            bit += 1;

            if (bit == 32)
            {
                data = 0;
                bit = 0;
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Get(int data, int index)
    {
        return (data & (1 << index)) != 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Set(ref int data, int index, bool value)
    {
        int bitMask = 1 << index;
        if (value)
        {
            data |= bitMask;
        }
        else
        {
            data &= ~bitMask;
        }
    }
}
