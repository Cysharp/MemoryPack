using MemoryPack.Internal;
using System.Runtime.CompilerServices;

namespace MemoryPack.Formatters;

#if NET7_0_OR_GREATER

[Preserve]
public sealed class MemoryPackableFormatter<T> : MemoryPackFormatter<T>
    where T : IMemoryPackable<T>
{
    [Preserve]
    public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref T? value)
    {
        T.Serialize(ref writer, ref Unsafe.AsRef(in value));
    }

    [Preserve]
    public override void Deserialize(ref MemoryPackReader reader, scoped ref T? value)
    {
        T.Deserialize(ref reader, ref value);
    }
}

#endif
