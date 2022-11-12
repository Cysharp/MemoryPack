using MemoryPack.Internal;

namespace MemoryPack.Formatters;

[Preserve]
public sealed class StringFormatter : MemoryPackFormatter<string>
{
    public static readonly StringFormatter Default = new StringFormatter();

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

[Preserve]
public sealed class Utf8StringFormatter : MemoryPackFormatter<string>
{
    public static readonly Utf8StringFormatter Default = new Utf8StringFormatter();

    [Preserve]
    public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref string? value)
    {
        writer.WriteUtf8(value);
    }

    [Preserve]
    public override void Deserialize(ref MemoryPackReader reader, scoped ref string? value)
    {
        value = reader.ReadString();
    }
}

[Preserve]
public sealed class Utf16StringFormatter : MemoryPackFormatter<string>
{
    public static readonly Utf16StringFormatter Default = new Utf16StringFormatter();

    [Preserve]
    public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref string? value)
    {
        writer.WriteUtf16(value);
    }

    [Preserve]
    public override void Deserialize(ref MemoryPackReader reader, scoped ref string? value)
    {
        value = reader.ReadString();
    }
}
