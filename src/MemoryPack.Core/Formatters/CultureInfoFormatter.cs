using System.Globalization;
using MemoryPack.Internal;

namespace MemoryPack.Formatters;

[Preserve]
public sealed class CultureInfoFormatter : MemoryPackFormatter<CultureInfo>
{
    // treat as a string(Name).

    [Preserve]
    public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref CultureInfo? value)
    {
        writer.WriteString(value?.Name);
    }

    [Preserve]
    public override void Deserialize(ref MemoryPackReader reader, scoped ref CultureInfo? value)
    {
        var str = reader.ReadString();
        if (str == null)
        {
            value = null;
        }
        else
        {
            value = CultureInfo.GetCultureInfo(str);
        }
    }
}