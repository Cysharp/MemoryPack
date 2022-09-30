using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace MemoryPack.Formatters;

public sealed class StringBuilderFormatter : MemoryPackFormatter<StringBuilder>
{
    public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref StringBuilder? value)
    {
        if (value == null)
        {
            writer.WriteNullObjectHeader();
            return;
        }

        // strign is same as collection, [length, char[]]
        writer.WriteCollectionHeader(value.Length);
        foreach (var chunk in value.GetChunks())
        {
            ref var p = ref writer.GetSpanReference(chunk.Length * 2);
            ref var src = ref Unsafe.As<char, byte>(ref MemoryMarshal.GetReference(chunk.Span));
            Unsafe.CopyBlockUnaligned(ref p, ref src, (uint)chunk.Length * 2);

            writer.Advance(chunk.Length * 2);
        }
    }

    public override void Deserialize(ref MemoryPackReader reader, scoped ref StringBuilder? value)
    {
        if (!reader.TryReadCollectionHeader(out var length))
        {
            value = null;
            return;
        }

        if (value == null)
        {
            value = new StringBuilder(length);
        }
        else
        {
            value.Clear();
            value.EnsureCapacity(length);
        }

        // note: if GetSpanRefernce supports sizeHint = 0, more performant(avoid copy when string is large).
        var size = length * 2;
        ref var p = ref reader.GetSpanReference(size);
        var src = MemoryMarshal.CreateSpan(ref Unsafe.As<byte, char>(ref p), length);
        value.Append(src);

        reader.Advance(size);
    }

    public override void Serialize(ref DoNothingMemoryPackWriter writer, scoped ref StringBuilder? value)
    {
        throw new NotImplementedException();
    }
}
