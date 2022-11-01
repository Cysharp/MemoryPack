using MemoryPack.Internal;
using System.Buffers;

namespace MemoryPack.Formatters;

[Preserve]
public sealed class TimeZoneInfoFormatter : MemoryPackFormatter<TimeZoneInfo>
{
    [Preserve]
    public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref TimeZoneInfo? value)
    {
        writer.WriteString(value?.ToSerializedString());
    }

    [Preserve]
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
}
