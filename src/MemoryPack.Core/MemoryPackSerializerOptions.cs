namespace MemoryPack;

public record MemoryPackSerializerOptions
{
    // Default is Utf8
    public static readonly MemoryPackSerializerOptions Default = new MemoryPackSerializerOptions { StringEncoding = StringEncoding.Utf8 };

    public static readonly MemoryPackSerializerOptions Utf8 = Default with { StringEncoding = StringEncoding.Utf8 };
    public static readonly MemoryPackSerializerOptions Utf16 = Default with { StringEncoding = StringEncoding.Utf16 };

    public StringEncoding StringEncoding { get; private set; }

    MemoryPackSerializerOptions()
    {
    }
}

public enum StringEncoding : byte
{
    Utf16,
    Utf8,
}
