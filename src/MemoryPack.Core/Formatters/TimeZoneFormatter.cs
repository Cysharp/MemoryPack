using System.Buffers;

namespace MemoryPack.Formatters;

public sealed class TimeZoneFormatter : IMemoryPackFormatter<TimeZoneInfo>
{
    public void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref TimeZoneInfo? value)
        where TBufferWriter : IBufferWriter<byte>
    {
        writer.WriteString(value?.ToSerializedString());
    }

    public void Deserialize(ref MemoryPackReader reader, scoped ref TimeZoneInfo? value)
    {
        var source = reader.ReadString();
        if (source == null)
        {
            value = null;
            return;
        }

        value = TimeZoneInfo.FromSerializedString(source);
    }
}
