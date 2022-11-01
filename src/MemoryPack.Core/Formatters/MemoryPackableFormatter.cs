using MemoryPack.Internal;

namespace MemoryPack.Formatters;

#if NET7_0_OR_GREATER

[Preserve]
public sealed class MemoryPackableFormatter<T> : MemoryPackFormatter<T>
    where T : IMemoryPackable<T>
{
    [Preserve]
    public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref T? value)
    {
        writer.WritePackable(value);
    }

    [Preserve]
    public override void Deserialize(ref MemoryPackReader reader, scoped ref T? value)
    {
        reader.ReadPackable(ref value);
    }
}

#endif
