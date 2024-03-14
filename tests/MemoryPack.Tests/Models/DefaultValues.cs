namespace MemoryPack.Tests.Models;

[MemoryPackable]
partial class DefaultValuePlaceholder
{
    public int X { get; set; }
}

[MemoryPackable]
partial class FieldDefaultValue
{
    public int X;
    public int Y = 12345;
    public float Z = 678.9f;
    public string S = "aaaaaaaaa";
    public bool B = true;
}

[MemoryPackable]
partial class PropertyDefaultValue
{
    public int X { get; set; }
    public int Y { get; set; } = 12345;
    public float Z { get; set; } = 678.9f;
    public string S { get; set; } = "aaaaaaaaa";
    public bool B { get; set; } = true;
}

[MemoryPackable]
partial class CtorParamDefaultValue
{
    public int X;
    public int Y;
    public float Z;
    public string S;
    public bool B;

    [MemoryPackConstructor]
    public CtorParamDefaultValue(int x, int y = 12345, float z = 678.9f, string s = "aaaaaa", bool b = true)
    {
        X = x;
        Y = y;
        Z = z;
        S = s;
        B = b;
    }
}
