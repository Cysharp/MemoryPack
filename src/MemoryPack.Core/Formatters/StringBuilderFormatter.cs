using MemoryPack.Internal;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace MemoryPack.Formatters;

[Preserve]
public sealed class StringBuilderFormatter : MemoryPackFormatter<StringBuilder>
{
    [Preserve]
    public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref StringBuilder? value)
    {
        if (value == null)
        {
            writer.WriteNullCollectionHeader();
            return;
        }

#if NET7_0_OR_GREATER

        // for performance reason, currently StringBuilder encode as Utf16, however try to write Utf8?
        // if (writer.Options.StringEncoding == StringEncoding.Utf16)
        {
            writer.WriteCollectionHeader(value.Length);

            foreach (var chunk in value.GetChunks())
            {
                ref var p = ref writer.GetSpanReference(checked(chunk.Length * 2));
                ref var src = ref Unsafe.As<char, byte>(ref MemoryMarshal.GetReference(chunk.Span));
                Unsafe.CopyBlockUnaligned(ref p, ref src, (uint)chunk.Length * 2);

                writer.AdvanceUnsafe(chunk.Length * 2);
            }
            return;
        }

#else
        // write as utf16
        writer.WriteUtf16(value.ToString());
#endif
    }

    [Preserve]
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

        // note: require to check is Utf8
        // note: to improvement append as chunk(per 64K?)
        var size = checked(length * 2);
        ref var p = ref reader.GetSpanReference(size);
        var src = MemoryMarshal.CreateSpan(ref Unsafe.As<byte, char>(ref p), length);
        value.Append(src);

        reader.AdvanceUnsafe(size);
    }
}
