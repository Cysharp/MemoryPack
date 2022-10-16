using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

#nullable enable
using System.Buffers;

namespace MemoryPack.Formatters {

public sealed class UriFormatter : MemoryPackFormatter<Uri>
{
    // treat as a string(OriginalString).

    public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref Uri? value)
    {
        writer.WriteString(value?.OriginalString);
    }

    public override void Deserialize(ref MemoryPackReader reader, ref Uri? value)
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

}