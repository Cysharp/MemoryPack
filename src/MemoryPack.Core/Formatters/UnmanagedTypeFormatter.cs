using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MemoryPack.Formatters;

// for unamanged types( https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/unmanaged-types )
// * sbyte, byte, short, ushort, int, uint, long, ulong, char, float, double, decimal, or bool
// * Any enum type
// * Any pointer type
// * Any user-defined struct type that contains fields of unmanaged types only
public sealed class UnmanagedTypeFormatter<T> : MemoryPackFormatter<T>
    where T : unmanaged
{
    static readonly int size = Unsafe.SizeOf<T>(); // TODO:which faster? load from field or Unsafe.SizeOf<T> directly

    public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref T value)
    {
        Unsafe.WriteUnaligned(ref writer.GetSpanReference(size), value);
        writer.Advance(size);
    }

    public override void Deserialize(ref MemoryPackReader reader, scoped ref T value)
    {
        value = Unsafe.ReadUnaligned<T>(ref reader.GetSpanReference(size));
        reader.Advance(size);
    }
}

// TODO:not yet.
public sealed class NullableUnmanagedTypeFormatter<T> : MemoryPackFormatter<T?>
    where T : unmanaged
{
    public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref T? value)
    {


        throw new NotImplementedException();
    }

    public override void Deserialize(ref MemoryPackReader reader, scoped ref T? value)
    {
        throw new NotImplementedException();
    }
}
