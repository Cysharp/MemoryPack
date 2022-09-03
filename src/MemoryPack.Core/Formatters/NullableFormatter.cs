using System.Buffers;
using System.Runtime.CompilerServices;

namespace MemoryPack.Formatters;

public sealed class NullableFormatter<T> : IMemoryPackFormatter<T?>
    where T : struct
{
    // Nullable<T> is sometimes serialized on UnmanagedTypeFormatter.
    // to keep same result, check if type is unamanged.

    public void Serialize<TBufferWriter>(ref SerializationContext<TBufferWriter> context, ref T? value)
        where TBufferWriter : IBufferWriter<byte>
    {
        if (!RuntimeHelpers.IsReferenceOrContainsReferences<T>())
        {
            Unsafe.WriteUnaligned(ref context.GetSpanReference(Unsafe.SizeOf<T>()), value);
            context.Advance(Unsafe.SizeOf<T>());
            return;
        }

        if (!value.HasValue)
        {
            context.WriteNullObjectHeader();
            return;
        }

        context.WriteObjectHeader();

        var v = value.Value;
        context.GetRequiredFormatter<T>().Serialize(ref context, ref v);
    }

    public void Deserialize(ref DeserializationContext context, ref T? value)
    {
        if (!RuntimeHelpers.IsReferenceOrContainsReferences<T>())
        {
            value = Unsafe.ReadUnaligned<T?>(ref context.GetSpanReference(Unsafe.SizeOf<T>()));
            context.Advance(Unsafe.SizeOf<T>());
            return;
        }

        if (context.ReadIsNull())
        {
            value = null;
            return;
        }

        T v = default;
        context.GetRequiredFormatter<T>().Deserialize(ref context, ref v);
        value = v;
    }
}
