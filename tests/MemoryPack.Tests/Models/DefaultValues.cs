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
[method: MemoryPackConstructor]
partial class CtorParamDefaultValue(int x, int y = 12345, float z = 678.9f, string s = "aaaaaa", bool b = true)
{
    public int X = x;
    public int Y = y;
    public float Z = z;
    public string S = s;
    public bool B = b;
}
