using System.Buffers;

namespace MemoryPack.Formatters;

public sealed class UriFormatter : IMemoryPackFormatter<Uri>
{
    // treat as a string(OriginalString).

    public void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref Uri? value)
        where TBufferWriter : IBufferWriter<byte>
    {
        writer.WriteString(value?.OriginalString);
    }

    public void Deserialize(ref MemoryPackReader reader, scoped ref Uri? value)
    {
        var str = reader.ReadString();
        if (str == null)
        {
            value = null;
        }
        else
        {
            value = new Uri(str, UriKind.RelativeOrAbsolute);
        }
    }
}
