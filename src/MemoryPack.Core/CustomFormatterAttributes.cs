using MemoryPack.Compression;
using MemoryPack.Formatters;

namespace MemoryPack;

#if !UNITY_2021_2_OR_NEWER

public sealed class Utf8StringFormatterAttribute : MemoryPackCustomFormatterAttribute<Utf8StringFormatter, string>
{
    public override Utf8StringFormatter GetFormatter()
    {
        return Utf8StringFormatter.Default;
    }
}

public sealed class Utf16StringFormatterAttribute : MemoryPackCustomFormatterAttribute<Utf16StringFormatter, string>
{
    public override Utf16StringFormatter GetFormatter()
    {
        return Utf16StringFormatter.Default;
    }
}

public sealed class OrdinalIgnoreCaseStringDictionaryFormatter<TValue> : MemoryPackCustomFormatterAttribute<DictionaryFormatter<string, TValue?>, Dictionary<string, TValue?>>
{
    static readonly DictionaryFormatter<string, TValue?> formatter = new DictionaryFormatter<string, TValue?>(StringComparer.OrdinalIgnoreCase);

    public override DictionaryFormatter<string, TValue?> GetFormatter()
    {
        return formatter;
    }
}

public sealed class InternStringFormatterAttribute : MemoryPackCustomFormatterAttribute<InternStringFormatter, string>
{
    public override InternStringFormatter GetFormatter()
    {
        return InternStringFormatter.Default;
    }
}

public sealed class BitPackFormatterAttribute : MemoryPackCustomFormatterAttribute<BitPackFormatter, bool[]>
{
    public override BitPackFormatter GetFormatter()
    {
        return BitPackFormatter.Default;
    }
}

#endif
