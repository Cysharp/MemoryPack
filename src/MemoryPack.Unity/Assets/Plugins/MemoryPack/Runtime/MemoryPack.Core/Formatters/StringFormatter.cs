using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

#nullable enable
using MemoryPack.Internal;

namespace MemoryPack.Formatters {

[Preserve]
public sealed class StringFormatter : MemoryPackFormatter<string>
{
    public static readonly StringFormatter Default = new StringFormatter();

    [Preserve]
    public override void Serialize(ref MemoryPackWriter writer, ref string? value)
    {
        writer.WriteString(value);
    }

    [Preserve]
    public override void Deserialize(ref MemoryPackReader reader, ref string? value)
    {
        value = reader.ReadString();
    }
}

[Preserve]
public sealed class Utf8StringFormatter : MemoryPackFormatter<string>
{
    public static readonly Utf8StringFormatter Default = new Utf8StringFormatter();

    [Preserve]
    public override void Serialize(ref MemoryPackWriter writer, ref string? value)
    {
        writer.WriteUtf8(value);
    }

    [Preserve]
    public override void Deserialize(ref MemoryPackReader reader, ref string? value)
    {
        value = reader.ReadString();
    }
}

[Preserve]
public sealed class Utf16StringFormatter : MemoryPackFormatter<string>
{
    public static readonly Utf16StringFormatter Default = new Utf16StringFormatter();

    [Preserve]
    public override void Serialize(ref MemoryPackWriter writer, ref string? value)
    {
        writer.WriteUtf16(value);
    }

    [Preserve]
    public override void Deserialize(ref MemoryPackReader reader, ref string? value)
    {
        value = reader.ReadString();
    }
}

}