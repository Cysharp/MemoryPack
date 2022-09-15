using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MemoryPack.Formatters;

// for unamanged types( https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/unmanaged-types )
// * sbyte, byte, short, ushort, int, uint, long, ulong, char, float, double, decimal, or bool
// * Any enum type
// * Any pointer type
// * Any user-defined struct type that contains fields of unmanaged types only
public sealed class UnmanagedTypeFormatter<T> : IMemoryPackFormatter<T>
    where T : unmanaged
{
    static readonly int size = Unsafe.SizeOf<T>(); // TODO:which faster? load from field or Unsafe.SizeOf<T> directly

    public void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> context, scoped ref T value)
        where TBufferWriter : IBufferWriter<byte>
    {
        Unsafe.WriteUnaligned(ref context.GetSpanReference(size), value);
        context.Advance(size);
    }

    public void Deserialize(ref MemoryPackReader context, scoped ref T value)
    {
        value = Unsafe.ReadUnaligned<T>(ref context.GetSpanReference(size));
        context.Advance(size);
    }
}

// TODO:not yet.
public sealed class NullableUnmanagedTypeFormatter<T> : IMemoryPackFormatter<T?>
    where T : unmanaged
{
    public void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> context, scoped ref T? value)
        where TBufferWriter : IBufferWriter<byte>
    {


        throw new NotImplementedException();
    }

    public void Deserialize(ref MemoryPackReader context, scoped ref T? value)
    {
        throw new NotImplementedException();
    }
}
