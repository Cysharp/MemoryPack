using System;
using System.Collections.Generic;

namespace MemoryPack.Tests.Models;

enum TestEnum
{
    A, B, C
}

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
    internal enum NestedEnum
    {
        A, B
    }

    public int X { get; set; }
    public int Y { get; set; } = 12345;
    public float Z { get; set; } = 678.9f;
    public string S { get; set; } = "aaaaaaaaa";
    public bool B { get; set; } = true;
    public List<string> Alpha { get; set; } = new List<string>(new HashSet<string>());
    public TestEnum E { get; set; } = TestEnum.A;
    public NestedEnum E2 { get; set; } = NestedEnum.A;
    public (TestEnum, List<string>) Tuple { get; set; } = (TestEnum.A, new List<string>(new HashSet<string>()));
    public DateTime Struct { get; set; } = default!;
}

[MemoryPackable]
partial class CtorParamDefaultValue
{
    public int X;
    public int Y;
    public float Z;
    public string S;
    public bool B;
    public decimal D;
    public DateTime StructValue;

    [MemoryPackConstructor]
    public CtorParamDefaultValue(int x, int y = 12345, float z = 678.9f, string s = "aaaaaa", bool b = true, decimal d = 99M, DateTime structValue = default)
    {
        X = x;
        Y = y;
        Z = z;
        S = s;
        B = b;
        D = d;
        StructValue = structValue;
    }
}
