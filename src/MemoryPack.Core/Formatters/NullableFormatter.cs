using System.Buffers;
using System.Runtime.CompilerServices;

namespace MemoryPack.Formatters;

public sealed class NullableFormatter<T> : MemoryPackFormatter<T?>
    where T : struct
{
    // Nullable<T> is sometimes serialized on UnmanagedTypeFormatter.
    // to keep same result, check if type is unmanaged.

    public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref T? value)
    {
        if (!RuntimeHelpers.IsReferenceOrContainsReferences<T>())
        {
            Unsafe.WriteUnaligned(ref writer.GetSpanReference(Unsafe.SizeOf<T>()), value);
            writer.Advance(Unsafe.SizeOf<T>());
            return;
        }

        if (!value.HasValue)
        {
            writer.WriteNullObjectHeader();
            return;
        }
        else
        {
            writer.WriteObjectHeader(1);
        }

        writer.WriteObject(value.Value);
    }

    public override void Deserialize(ref MemoryPackReader reader, scoped ref T? value)
    {
        if (!RuntimeHelpers.IsReferenceOrContainsReferences<T>())
        {
            value = Unsafe.ReadUnaligned<T?>(ref reader.GetSpanReference(Unsafe.SizeOf<T>()));
            reader.Advance(Unsafe.SizeOf<T>());
            return;
        }

        if (!reader.TryReadObjectHeader(out var count))
        {
            value = null;
            return;
        }

        if (count != 1) ThrowHelper.ThrowInvalidPropertyCount(1, count);

        T v = default;
        MemoryPackFormatterProvider.GetFormatter<T>().Deserialize(ref reader, ref v);
        value = v;
    }
}
