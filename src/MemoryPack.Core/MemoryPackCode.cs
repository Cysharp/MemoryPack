namespace MemoryPack;

public static class MemoryPackCode
{
    // Collection Header
    // 0~* is length, -1 is null
    public const int NullCollection = -1;

    // Object/Union Header
    // 0~249 is member count or tag, 250~254 is unused, 255 is null
    public const byte Reserved1 = 250;
    public const byte Reserved2 = 251;
    public const byte Reserved3 = 252;
    public const byte Reserved4 = 253;
    public const byte Reserved5 = 254;
    public const byte NullObject = 255;

    // predefined, C# compiler optimize byte[] as ReadOnlySpan<byte> property
    internal static ReadOnlySpan<byte> NullCollectionData => new byte[] { 255, 255, 255, 255 }; // -1
    internal static ReadOnlySpan<byte> ZeroCollectionData => new byte[] { 0, 0, 0, 0 }; // 0
}
