﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemoryPack.Tests.Models;

[MemoryPackable]
public partial struct UnmanagedStruct
{
    public int X;
    public int Y;
    public int Z;
}

[MemoryPackable]
public partial struct IncludesReferenceStruct
{
    public int X;
    public string? Y;
}

#if NET7_0_OR_GREATER

[MemoryPackable]
public partial class RequiredType
{
    public required int MyProperty1 { get; set; }
    public required string MyProperty2 { get; set; }
}

[MemoryPackable]
public partial class RequiredInitOnlyType
{
    public required int MyProperty1 { get; init; }
    public required string MyProperty2 { get; init; }
    public required string MyProperty3 { get; set; }
    public string? MyProperty4 { get; set; }
    public string? MyProperty5 { get; init; }
}

[MemoryPackable]
public partial struct RequiredType2
{
    public required int MyProperty1 { get; set; }
    public required string MyProperty2 { get; set; }

    public void F()
    {
        // new MyRecord()
    }
}

[MemoryPackable]
public partial struct RequiredInitOnlyType2
{
    public required int MyProperty1 { get; init; }
    public required string MyProperty2 { get; init; }
    public required string MyProperty3 { get; set; }
    public string? MyProperty4 { get; set; }
    public string? MyProperty5 { get; init; }

    public void F()
    {
        // new MyRecord()
    }
}

#endif

[MemoryPackable]
public partial struct StructWithConstructor1
{
    public string MyProperty { get; set; }

    public StructWithConstructor1(string myProperty)
    {
        this.MyProperty = myProperty;
    }
}

[MemoryPackable]
public partial record MyRecord(int foo, int bar, string baz);

[MemoryPackable]
public partial record struct StructRecordUnmanaged(int foo, int bar);


[MemoryPackable]
public partial record struct StructRecordWithReference(int foo, string bar);
