using MemoryPack.Internal;

namespace MemoryPack.Formatters;

[Preserve]
public sealed class StringFormatter : MemoryPackFormatter<string>
{
    [Preserve]
    public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref string? value)
    {
        writer.WriteString(value);
    }

    [Preserve]
    public override void Deserialize(ref MemoryPackReader reader, scoped ref string? value)
    {
        value = reader.ReadString();
    }
}
