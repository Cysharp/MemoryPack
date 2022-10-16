using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

#nullable enable
namespace MemoryPack.Formatters {

public sealed class StringFormatter : MemoryPackFormatter<string>
{
    public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref string? value)
    {
        writer.WriteString(value);
    }

    public override void Deserialize(ref MemoryPackReader reader, ref string? value)
    {
        value = reader.ReadString();
    }
}

}