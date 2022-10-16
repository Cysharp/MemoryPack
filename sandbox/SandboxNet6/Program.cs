// See https://aka.ms/new-console-template for more information
using MemoryPack;
using System.Buffers;
using System.Runtime.CompilerServices;


// System.Buffers.IBufferWriter<byte>
Console.WriteLine("Hello, World!");

[MemoryPackable]
public partial class HelloMemoryPackable
{
    public int MyProperty { get; set; }
}


[MemoryPackable]
public partial class HelloMemoryPackable2
{
    public HelloMemoryPackable? MyProperty { get; set; }
    // public TypeAccessException My3Property { get; set; }
}


[MemoryPackable]
[MemoryPackUnion(0, typeof(FooClass))]
[MemoryPackUnion(249, typeof(BarClass))]
public partial interface IUnionSample
{
}

[MemoryPackable]
public partial class FooClass : IUnionSample
{
    public int XYZ { get; set; }
}

[MemoryPackable]
public partial class BarClass : IUnionSample
{
    public string? OPQ { get; set; }
}


[MemoryPackable]
public partial struct IncludesReferenceStruct
{
    public int X;
    public string? Y;
}


[MemoryPackable]
[MemoryPackUnion(0, typeof(GenricUnionA<>))]
[MemoryPackUnion(1, typeof(GenricUnionB<>))]
public partial interface IGenericUnion<ToaruHoge>
{
    ToaruHoge? Value { get; set; }
}


[MemoryPackable]
public partial class GenricUnionA<T> : IGenericUnion<T>
{
    public T? Value { get; set; }
    public int MyProperty { get; set; }
}

[MemoryPackable]
public partial class GenricUnionB<T> : IGenericUnion<T>
{
    public T? Value { get; set; }
    public double MyProperty { get; set; }
}
