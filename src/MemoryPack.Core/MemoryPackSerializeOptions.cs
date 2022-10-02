namespace MemoryPack;

public record MemoryPackSerializeOptions
{
    public static MemoryPackSerializeOptions Default = new MemoryPackSerializeOptions { StringEncoding = StringEncoding.Utf16 };
    public static MemoryPackSerializeOptions Utf8 = Default with { StringEncoding = StringEncoding.Utf8 };

    public StringEncoding StringEncoding { get; init; }
}

public enum StringEncoding : byte
{
    Utf16,
    Utf8,
}
