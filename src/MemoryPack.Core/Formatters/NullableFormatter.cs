using System.Buffers;
using System.Runtime.CompilerServices;

namespace MemoryPack.Formatters;

public sealed class NullableFormatter<T> : IMemoryPackFormatter<T?>
    where T : struct
{
    // Nullable<T> is sometimes serialized on UnmanagedTypeFormatter.
    // to keep same result, check if type is unmanaged.

    public void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> context, scoped ref T? value)
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
        else
        {
            context.WriteObjectHeader(1);
        }

        var v = value.Value;
        context.WriteObject(ref v);
    }

    public void Deserialize(ref MemoryPackReader context, scoped ref T? value)
    {
        if (!RuntimeHelpers.IsReferenceOrContainsReferences<T>())
        {
            value = Unsafe.ReadUnaligned<T?>(ref context.GetSpanReference(Unsafe.SizeOf<T>()));
            context.Advance(Unsafe.SizeOf<T>());
            return;
        }

        if (!context.TryReadPropertyCount(out var count))
        {
            value = null;
            return;
        }

        if (count != 1) ThrowHelper.ThrowInvalidPropertyCount(1, count);

        T v = default;
        MemoryPackFormatterProvider.GetRequiredFormatter<T>().Deserialize(ref context, ref v);
        value = v;
    }
}
