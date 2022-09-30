using System.Buffers;

namespace MemoryPack.Formatters;

public sealed class TimeZoneInfoFormatter : MemoryPackFormatter<TimeZoneInfo>
{
    public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref TimeZoneInfo? value)
    {
        writer.WriteString(value?.ToSerializedString());
    }

    public override void Deserialize(ref MemoryPackReader reader, scoped ref TimeZoneInfo? value)
    {
        var source = reader.ReadString();
        if (source == null)
        {
            value = null;
            return;
        }

        value = TimeZoneInfo.FromSerializedString(source);
    }

    public override void Serialize(ref DoNothingMemoryPackWriter writer, scoped ref TimeZoneInfo? value)
    {
        throw new NotImplementedException();
    }
}
