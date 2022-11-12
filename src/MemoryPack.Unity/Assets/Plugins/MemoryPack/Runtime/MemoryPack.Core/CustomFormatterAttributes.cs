using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

#nullable enable
using MemoryPack.Formatters;

namespace MemoryPack {

#if !UNITY_2021_2_OR_NEWER

public sealed class Utf8StringFormatterAttribute : MemoryPackCustomFormatterAttribute<string>
{
    public override IMemoryPackFormatter<string> GetFormatter()
    {
        return Utf8StringFormatter.Default;
    }
}

public sealed class Utf16StringFormatterAttribute : MemoryPackCustomFormatterAttribute<string>
{
    public override IMemoryPackFormatter<string> GetFormatter()
    {
        return Utf16StringFormatter.Default;
    }
}

public sealed class OrdinalIgnoreCaseStringDictionaryFormatter<TValue> : MemoryPackCustomFormatterAttribute<Dictionary<string, TValue?>>
{
    static readonly DictionaryFormatter<string, TValue?> formatter = new DictionaryFormatter<string, TValue?>(StringComparer.OrdinalIgnoreCase);

    public override IMemoryPackFormatter<Dictionary<string, TValue?>> GetFormatter()
    {
        return formatter;
    }
}

#endif

}