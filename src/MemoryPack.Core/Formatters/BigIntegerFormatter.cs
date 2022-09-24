using System.Numerics;
using System.Text;

namespace MemoryPack.Formatters;

public sealed class BigIntegerFormatter : MemoryPackFormatter<BigInteger>
{
    public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref BigInteger value)
    {
        Span<byte> temp = stackalloc byte[255];
        if (value.TryWriteBytes(temp, out var written))
        {
            writer.WriteUnmanagedSpan(temp.Slice(written));
            return;
        }
        else
        {
            var byteArray = value.ToByteArray();
            writer.WriteUnmanagedArray(byteArray);
        }
    }

    public override void Deserialize(ref MemoryPackReader reader, scoped ref BigInteger value)
    {
        if (reader.TryReadUnmanagedSpan<byte>(out var view, out var length))
        {
            value = new BigInteger(view);
            reader.Advance(length);
        }

        ThrowHelper.ThrowInvalidCollection();
    }
}
