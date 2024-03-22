namespace MemoryPack.Tests.Models;

[MemoryPackable]
partial class DefaultValuePlaceholder
{
    public int X { get; set; }
}

[MemoryPackable(GenerateType.VersionTolerant, SerializeLayout.Sequential)]
partial class HasDefaultValueWithVersionTolerant
{
    public int X;

    public int Y = 12345;
    public float Z { get; set; } = 678.9f;

    [SuppressDefaultInitialization]
    public int Y2 = 12345;

    [SuppressDefaultInitialization]
    public float Z2 { get; set; } = 678.9f;
}

[MemoryPackable]
partial class HasDefaultValue
{
    public int X;

    public int Y = 12345;
    public float Z { get; set; } = 678.9f;

    [SuppressDefaultInitialization]
    public int Y2 = 12345;

    [SuppressDefaultInitialization]
    public float Z2 { get; set; } = 678.9f;
}
