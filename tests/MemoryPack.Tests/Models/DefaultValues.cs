namespace MemoryPack.Tests.Models;

[MemoryPackable]
partial class DefaultValuePlaceholder
{
    public int X { get; set; }
}

static class StaticMethod
{
    public static int GetNumberCalls;

    public static int GetNumber()
    {
        GetNumberCalls++;
        return 100;
    }
}

[MemoryPackable]
partial class FieldDefaultValue
{
    public int X;
    public int Y = 12345;

    [SuppressDefaultInitialization]
    public float Z = 678.9f;

    [SuppressDefaultInitialization]
    public int FromMethod = StaticMethod.GetNumber();
}

[MemoryPackable]
partial class PropertyDefaultValue
{
    public int X { get; init; }
    public int Y { get; set; } = 12345;

    [SuppressDefaultInitialization]
    public float Z { get; set; } = 678.9f;

    [SuppressDefaultInitialization]
    public float W { get; init; } = 678.9f;

    [SuppressDefaultInitialization]
    public int FromMethod { get; set; } = StaticMethod.GetNumber();
}
