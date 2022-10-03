namespace MemoryPack;

public record MemoryPackSerializeOptions
{
    // Default is Utf8
    public static readonly MemoryPackSerializeOptions Default = new MemoryPackSerializeOptions { StringEncoding = StringEncoding.Utf8 };

    public static readonly MemoryPackSerializeOptions Utf8 = Default with { StringEncoding = StringEncoding.Utf8 };
    public static readonly MemoryPackSerializeOptions Utf16 = Default with { StringEncoding = StringEncoding.Utf16 };

    public StringEncoding StringEncoding { get; init; }
}

public enum StringEncoding : byte
{
    Utf16,
    Utf8,
}
