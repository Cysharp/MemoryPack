using System.Buffers;
using System.Runtime.CompilerServices;

namespace MemoryPack.Formatters;

public sealed class UriFormatter : IMemoryPackFormatter<Uri>
{
    [ModuleInitializer]
    internal static void RegisterFormatter() => MemoryPackFormatterProvider.Register(new UriFormatter());

    // treat as a string(OriginalString).

    public void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> context, scoped ref Uri? value)
        where TBufferWriter : IBufferWriter<byte>
    {
        context.WriteString(value?.OriginalString);
    }

    public void Deserialize(ref MemoryPackReader context, scoped ref Uri? value)
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
