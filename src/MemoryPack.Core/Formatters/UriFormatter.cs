using System.Buffers;

namespace MemoryPack.Formatters;

public class UriFormatter : IMemoryPackFormatter<Uri>
{
    // treat as a string(OriginalString).

    public void Serialize<TBufferWriter>(ref SerializationContext<TBufferWriter> context, ref Uri? value)
        where TBufferWriter : IBufferWriter<byte>
    {
        context.WriteString(value?.OriginalString);
    }

    public void Deserialize(ref DeserializationContext context, ref Uri? value)
    {
        var str = context.ReadString();
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
