namespace MemoryPack.Formatters;

public sealed class StringFormatter : MemoryPackFormatter<string>
{
    public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref string? value)
    {
        writer.WriteString(value);
    }

    public override void Serialize(ref DoNothingMemoryPackWriter writer, scoped ref string? value)
    {
        writer.WriteString(value);
    }

    public override void Deserialize(ref MemoryPackReader reader, scoped ref string? value)
    {
        value = reader.ReadString();
    }
}
