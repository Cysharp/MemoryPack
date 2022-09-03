using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MemoryPack.Formatters;

// for unamanged types( https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/unmanaged-types )
// * sbyte, byte, short, ushort, int, uint, long, ulong, char, float, double, decimal, or bool
// * Any enum type
// * Any pointer type
// * Any user-defined struct type that contains fields of unmanaged types only
public class UnmanagedTypeFormatter<T> : IMemoryPackFormatter<T>
    where T : unmanaged
{
    static readonly int size = Unsafe.SizeOf<T>();

    public void Serialize<TBufferWriter>(ref SerializationContext<TBufferWriter> context, ref T value)
        where TBufferWriter : IBufferWriter<byte>
    {
        var span = context.GetSpan(size);
        Unsafe.WriteUnaligned(ref MemoryMarshal.GetReference(span), value);

        context.Advance(size);
    }

    public void Deserialize(ref DeserializationContext context, ref T value)
    {
        var span = context.GetSpan(size);
        value = Unsafe.ReadUnaligned<T>(ref MemoryMarshal.GetReference(span));
    }
}

// TODO:not yet.
public class NullableUnmanagedTypeFormatter<T> : IMemoryPackFormatter<T?>
    where T : unmanaged
{
    public void Serialize<TBufferWriter>(ref SerializationContext<TBufferWriter> context, ref T? value)
        where TBufferWriter : IBufferWriter<byte>
    {


        throw new NotImplementedException();
    }

    public void Deserialize(ref DeserializationContext context, ref T? value)
    {
        throw new NotImplementedException();
    }
}