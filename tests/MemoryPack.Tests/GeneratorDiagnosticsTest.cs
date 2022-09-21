using MemoryPack.Generator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace MemoryPack.Tests;

public class GeneratorDiagnosticsTest
{
    void Compile(int id, string code, bool allowMultipleError = false)
    {
        // note: when doesn't detect code-generator error(succeeded code generation)
        // compiler will show many errors(because compilation does not reference dependent assemblies(System.Memory.dll, etc...)
        var diagnostics = CSharpGeneratorRunner.RunGenerator(code);
        if (!allowMultipleError)
        {
            diagnostics.Length.Should().Be(1);
            diagnostics[0].Id.Should().Be("MEMPACK" + id.ToString("000"));
        }
        else
        {
            diagnostics.Select(x => x.Id).Should().Contain("MEMPACK" + id.ToString("000"));
        }
    }

    [Fact]
    public void MEMPACK001_MuestBePartial()
    {
        Compile(1, """
using MemoryPack;

[MemoryPackable]
public class Hoge
{
}
""");
    }

    [Fact]
    public void MEMPACK002_NestedNotAllow()
    {
        Compile(2, """
using MemoryPack;

public partial class Hoge
{
    [MemoryPackable]
    public partial class Huga
    {
    }
}
""");
    }

    [Fact]
    public void MEMPACK003_AbstractMustUnion()
    {
        Compile(3, """
using MemoryPack;

[MemoryPackable]
public abstract partial class Hoge
{
}
""");

        Compile(3, """
using MemoryPack;

[MemoryPackable]
public partial interface IHoge
{
}
""");
    }

    [Fact]
    public void MEMPACK004_MultipleCtorWithoutAttribute()
    {
        Compile(4, """
using MemoryPack;

[MemoryPackable]
public partial class Hoge
{
    public Hoge()
    {
    }

    public Hoge(int x)
    {
    }
}
""");
    }

    [Fact]
    public void MEMPACK005_MultipleCtorAttribute()
    {
        Compile(5, """
using MemoryPack;

[MemoryPackable]
public partial class Hoge
{
    [MemoryPackConstructor]
    public Hoge()
    {
    }

    [MemoryPackConstructor]
    public Hoge(int x)
    {
    }
}
""");
    }

    [Fact]
    public void MEMPACK006_ConstructorHasNoMatchedParameter()
    {
        Compile(6, """
using MemoryPack;

[MemoryPackable]
public partial class Hoge
{
    public int Foo { get; set;}

    [MemoryPackConstructor]
    public Hoge(int hhogee)
    {
        this.Foo = hhogee;
    }
}
""");
    }

    [Fact]
    public void MEMPACK007_OnMethodHasParameter()
    {
        Compile(7, """
using MemoryPack;

[MemoryPackable]
public partial class Hoge
{
    [MemoryPackOnSerializing]
    void Foo(int x)
    {
    }
}
""");
    }

    [Fact]
    public void MEMPACK008_OnMethodInUnamannagedType()
    {
        Compile(8, """
using MemoryPack;

[MemoryPackable]
public partial struct Hoge
{
    [MemoryPackOnSerializing]
    void Foo()
    {
    }
}
""");
    }

    [Fact]
    public void MEMPACK009_OverrideMemberCantAddAnnotation()
    {
        Compile(9, """
using MemoryPack;

public abstract class MyClass
{
    public abstract int MyProperty { get; set; }
}

[MemoryPackable]
public partial class MyClass2 : MyClass
{
    [MemoryPackIgnore]
    public override int MyProperty { get; set; }
}
""");

        Compile(9, """
using MemoryPack;

public abstract class MyClass
{
    public abstract int MyProperty { get; set; }
}

[MemoryPackable]
public partial class MyClass3 : MyClass
{
    [MemoryPackInclude]
    public override int MyProperty { get; set; }
}

""");
    }

    [Fact]
    public void MEMPACK010_015_Union()
    {
        Compile(10, """
using MemoryPack;

[MemoryPackable]
[MemoryPackUnion(0, typeof(string))]
public sealed partial class MyClass
{
}
""", allowMultipleError: true);

        Compile(11, """
using MemoryPack;

[MemoryPackable]
[MemoryPackUnion(0, typeof(string))]
public partial class MyClass
{
}
""", allowMultipleError: true);

        Compile(12, """
using MemoryPack;

[MemoryPackable]
[MemoryPackUnion(1, typeof(MyClass1))]
[MemoryPackUnion(1, typeof(MyClass2))]
public partial interface IMyClass
{
}

[MemoryPackable]
public partial class MyClass1 : IMyClass
{
}

[MemoryPackable]
public partial class MyClass2 : IMyClass
{
}
""", allowMultipleError: true);

        Compile(13, """
using MemoryPack;

[MemoryPackable]
[MemoryPackUnion(1, typeof(MyClass1))]
[MemoryPackUnion(2, typeof(MyClass2))]
public partial interface IMyClass
{
}

[MemoryPackable]
public partial class MyClass1 : IMyClass
{
}

[MemoryPackable]
public partial class MyClass2
{
}
""", allowMultipleError: true);

        Compile(14, """
using MemoryPack;

[MemoryPackable]
[MemoryPackUnion(1, typeof(MyClass1))]
[MemoryPackUnion(2, typeof(MyClass2))]
public abstract partial class MyClassBase
{
}

[MemoryPackable]
public partial class MyClass1 : MyClassBase
{
}

[MemoryPackable]
public partial class MyClass2
{
}
""", allowMultipleError: true);

        Compile(15, """
using MemoryPack;

[MemoryPackable]
[MemoryPackUnion(1, typeof(MyClass1))]
[MemoryPackUnion(2, typeof(MyClass2))]
public partial interface IMyClass
{
}

[MemoryPackable]
public partial class MyClass1 : IMyClass
{
}

[MemoryPackable]
public partial struct MyClass2 : IMyClass
{
}
""", allowMultipleError: true);

        Compile(16, """
using MemoryPack;

[MemoryPackable]
[MemoryPackUnion(1, typeof(MyClass1))]
[MemoryPackUnion(2, typeof(MyClass2))]
public partial interface IMyClass
{
}

[MemoryPackable]
public partial class MyClass1 : IMyClass
{
}

// [MemoryPackable]
public partial class MyClass2 : IMyClass
{
}
""", allowMultipleError: true);

    }
}
