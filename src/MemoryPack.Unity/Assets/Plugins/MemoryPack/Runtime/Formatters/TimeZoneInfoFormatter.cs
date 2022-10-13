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

public sealed class TimeZoneInfoFormatter : MemoryPackFormatter<TimeZoneInfo>
{
    public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref TimeZoneInfo? value)
    {
        writer.WriteString(value?.ToSerializedString());
    }

    public override void Deserialize(ref MemoryPackReader reader, ref TimeZoneInfo? value)
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

}