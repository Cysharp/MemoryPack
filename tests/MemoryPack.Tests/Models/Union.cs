using Microsoft.CodeAnalysis.Operations;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MemoryPack.Tests.Models;


[MemoryPackable]
public partial class StandardBase
{
    public int MyProperty1 { get; set; }
    public int MyProperty2 { get; set; }
}

[MemoryPackable]
public partial class Derived1 : StandardBase
{
    public int DerivedProp1 { get; set; }
    public int DerivedProp2 { get; set; }
}

[MemoryPackable]
public partial class Derived2 : Derived1
{
    public int Derived2Prop1 { get; set; }
    public int Derived2Prop2 { get; set; }
}


[MemoryPackable]
[MemoryPackUnion(0, typeof(Impl1))]
[MemoryPackUnion(1, typeof(Impl2))]
public partial interface IUnionInterface
{
    int MyProperty { get; }
}

[MemoryPackable]
public partial class Impl1 : IUnionInterface
{
    public int MyProperty { get; set; }
    public long Foo { get; set; }
}

[MemoryPackable]
public partial class Impl2 : IUnionInterface
{
    public int MyProperty { get; set; }
    public string? Bar { get; set; }
}

//[MemoryPackable]
//public abstract partial class UnionAbstract
//{
//}
// TODO: Union can't be IMemoryPackable<T>!, only implements formatter!

public interface IFooTwo : IMemoryPackable
{
    static IFooTwo()
    {
    }
}
