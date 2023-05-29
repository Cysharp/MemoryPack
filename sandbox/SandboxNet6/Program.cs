// See https://aka.ms/new-console-template for more information
using MemoryPack;
using System.Buffers;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;


// System.Buffers.IBufferWriter<byte>
Console.WriteLine("Hello, World!");


[MemoryPackable(GenerateType.NoGenerate)]
public partial interface IForExternalUnion
{
    public int BaseValue { get; set; }
}

[MemoryPackable]
public partial class AForOne : IForExternalUnion
{
    public int BaseValue { get; set; }
    public int MyProperty { get; set; }
}

[MemoryPackable]
public partial class AForTwo : IForExternalUnion
{
    public int BaseValue { get; set; }
    public int MyProperty { get; set; }
}

[MemoryPackUnionFormatter(typeof(IForExternalUnion))]
[MemoryPackUnion(0, typeof(AForOne))]
[MemoryPackUnion(1, typeof(AForTwo))]
public partial class ForExternalUnionFormatter
{
}


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


[MemoryPackable]
public partial struct PartialStructOne
{
    public int X;
    public int Y;

    //[MemoryPackConstructor]
    public PartialStructOne(int x)
    {
        this.X = x;
        this.Y = 0;
    }

    //[MemoryPackConstructor]
    public PartialStructOne(int x, int y)
    {
        this.X = x;
        this.Y = y;
    }
}

//[MemoryPackable]
//public partial struct DateTimeParamDefault
//{
//    public DateTimeOffset DateTime; // short offset(2+padding) + dateTime/ulong(8) = 16
//    public long Timestamp;  // 8
//    public bool IsItInSeconds; // 1(+padding7) = 8
//}

//[StructLayout(LayoutKind.Sequential)]
//[MemoryPackable]
//public partial struct TesTest
//{
//    public ValueTuple<int, int> VTI;
//    public MyMessageHeader MyMsgHead;
//    public bool IsItInSeconds; // 1(+padding7) = 8
//}

//[StructLayout(LayoutKind.Sequential)]
//[MemoryPackable]
//public partial struct DateTimeParamSequential
//{
//    public DateTimeOffset DateTime; // short offset(2+padding) + dateTime/ulong(8) = 16
//    public long Timestamp;  // 8
//    public bool IsItInSeconds; // 1(+padding7) = 8
//}

//[StructLayout(LayoutKind.Auto)]
//[MemoryPackable]
//public partial struct DateTimeParamAuto
//{
//    public DateTimeOffset DateTime; // short offset(2+padding) + dateTime/ulong(8) = 16
//    public long Timestamp;  // 8
//    public bool IsItInSeconds; // 1(+padding7) = 8
//}

//[StructLayout(LayoutKind.Explicit, Size = 25)]
//[MemoryPackable]
//public partial struct DateTimeParamExplicit
//{
//    [FieldOffset(9)]
//    public DateTimeOffset DateTime;
//    [FieldOffset(1)]
//    public long Timestamp;  // 8
//    [FieldOffset(0)]
//    public bool IsItInSeconds; // 1
//}

//[StructLayout(LayoutKind.Auto)]
//public struct MyMessageHeader
//{
//}


public struct float2 { }
public struct quaternion { }

[Serializable]
[MemoryPackable]
public partial class PlayerInput
{
    public float2 move;
    public quaternion target;
    public List<KeyRecord> keyRecords = new();
}

public enum EAction : byte
{
    Ability1,
    Ability2,
    Ability3,
    Ability4,
    Ability5,
    Ability6,
}

public enum EActionStatus : byte
{
    KeyDown = 0,
    keyPressing = 1,
    KeyUp = 2,
}

public struct KeyRecord
{
    public EAction action;
    public EActionStatus status;
}
