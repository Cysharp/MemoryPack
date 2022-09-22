namespace MemoryPack.Formatters;

public sealed class MemoryPackableFormatter<T> : MemoryPackFormatter<T>
    where T : IMemoryPackable<T>
{
    public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref T? value)
    {
        writer.WritePackable(value);
    }

    public override void Deserialize(ref MemoryPackReader reader, scoped ref T? value)
    {
        reader.ReadPackable(ref value);
    }
}
