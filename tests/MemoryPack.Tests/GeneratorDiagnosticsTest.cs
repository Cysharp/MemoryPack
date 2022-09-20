using MemoryPack.Generator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MemoryPack.Tests;

public class GeneratorDiagnosticsTest
{
    void Compile(int id, string code)
    {
        // note: when doesn't detect code-generator error(succeeded code generation)
        // compiler will show many errors(because compilation does not reference dependent assemblies(System.Memory.dll, etc...)
        var diagnostics = CSharpGeneratorRunner.RunGenerator(code);
        diagnostics.Length.Should().Be(1);
        diagnostics[0].Id.Should().Be("MEMPACK" + id.ToString("000"));
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


}
