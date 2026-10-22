using Microsoft.CodeAnalysis;
using System;
using System.IO;
using System.Linq;

namespace MemoryPack.Tests;

public class SerializerContextGeneratorTest
{
    [Fact]
    public void ContextMustBePartial()
    {
        var (_, diagnostics) = CSharpGeneratorRunner.RunGenerator("""
using MemoryPack;

[MemoryPackable]
public partial class Dto { public int Id { get; set; } }

[MemoryPackSerializable(typeof(Dto))]
public class InvalidContext : MemoryPackSerializerContext { }
""");

        diagnostics.Should().ContainSingle(x => x.Id == "MEMPACK043");
    }

    [Fact]
    public void ContextRequiresNet7OrLater()
    {
        var (_, diagnostics) = CSharpGeneratorRunner.RunGenerator("""
using MemoryPack;

[MemoryPackable]
public partial class Dto { public int Id { get; set; } }

[MemoryPackSerializable(typeof(Dto))]
public partial class InvalidContext : MemoryPackSerializerContext { }
""", preprocessorSymbols: []);

        diagnostics.Should().ContainSingle(x => x.Id == "MEMPACK044");
    }

    [Fact]
    public void ContextMustDeriveFromSerializerContextBase()
    {
        var (_, diagnostics) = CSharpGeneratorRunner.RunGenerator("""
using MemoryPack;

[MemoryPackable]
public partial class Dto { public int Id { get; set; } }

[MemoryPackSerializable(typeof(Dto))]
public partial class InvalidContext { }
""");

        diagnostics.Should().ContainSingle(x => x.Id == "MEMPACK047");
    }

    [Fact]
    public void ContextCannotBeGeneric()
    {
        var (_, diagnostics) = CSharpGeneratorRunner.RunGenerator("""
using MemoryPack;

[MemoryPackable]
public partial class Dto { public int Id { get; set; } }

[MemoryPackSerializable(typeof(Dto))]
public partial class InvalidContext<T> : MemoryPackSerializerContext { }
""");

        diagnostics.Should().ContainSingle(x => x.Id == "MEMPACK046");
    }

    [Fact]
    public void UnsupportedRootProducesContextDiagnostic()
    {
        var (_, diagnostics) = CSharpGeneratorRunner.RunGenerator("""
using MemoryPack;

[MemoryPackSerializable(typeof(object))]
public partial class InvalidContext : MemoryPackSerializerContext { }
""");

        diagnostics.Should().ContainSingle(x => x.Id == "MEMPACK045");
    }

    [Fact]
    public void ExistingNullAndOptionsCallsRemainUnambiguous()
    {
        var (compilation, generatorDiagnostics) = CSharpGeneratorRunner.RunGenerator("""
using MemoryPack;

[MemoryPackable]
public partial class Dto { public int Id { get; set; } }

[MemoryPackSerializable(typeof(Dto))]
public partial class DtoContext : MemoryPackSerializerContext { }

public static class Usage
{
    public delegate byte[] SerializeDelegate(in Dto? value, MemoryPackSerializerOptions? options);
    public delegate Dto? DeserializeDelegate(System.ReadOnlySpan<byte> bytes, MemoryPackSerializerOptions? options);

    public static readonly SerializeDelegate SerializeMethodGroup = MemoryPackSerializer.Serialize;
    public static readonly DeserializeDelegate DeserializeMethodGroup = MemoryPackSerializer.Deserialize<Dto>;
    public static byte[] SerializeNull(Dto value) => MemoryPackSerializer.Serialize(value, null);
    public static byte[] SerializeOptions(Dto value) => MemoryPackSerializer.Serialize(value, MemoryPackSerializerOptions.Utf8);
    public static Dto? DeserializeNull(byte[] bytes) => MemoryPackSerializer.Deserialize<Dto>(bytes, null);
    public static Dto? DeserializeOptions(byte[] bytes) => MemoryPackSerializer.Deserialize<Dto>(bytes, MemoryPackSerializerOptions.Utf16);
}
""");

        generatorDiagnostics.Where(x => x.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
        compilation.GetDiagnostics().Where(x => x.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
    }

    [Fact]
    public void UnrelatedContextDoesNotChangeExistingGeneratedFormatterOutput()
    {
        const string dto = """
using MemoryPack;

[MemoryPackable]
public partial class ExistingDto
{
    public int Id { get; set; }
    public string? Name { get; set; }
}
""";
        var (baseline, baselineDiagnostics) = CSharpGeneratorRunner.RunGenerator(dto);
        var (withContext, contextDiagnostics) = CSharpGeneratorRunner.RunGenerator(dto + """

[MemoryPackable]
public partial class ContextOnlyDto { public int Value { get; set; } }

[MemoryPackSerializable(typeof(ContextOnlyDto))]
public partial class ContextOnly : MemoryPackSerializerContext { }
""");

        baselineDiagnostics.Where(x => x.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
        contextDiagnostics.Where(x => x.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
        var baselineSource = baseline.SyntaxTrees.Single(x => x.FilePath.EndsWith("ExistingDto.MemoryPackFormatter.g.cs", StringComparison.Ordinal)).ToString();
        var contextSource = withContext.SyntaxTrees.Single(x => x.FilePath.EndsWith("ExistingDto.MemoryPackFormatter.g.cs", StringComparison.Ordinal)).ToString();
        contextSource.Should().Be(baselineSource);
    }

    [Fact]
    public void ExternalMemoryPackableWithoutContextFactoryProducesDiagnostic()
    {
        var (externalCompilation, externalGeneratorDiagnostics) = CSharpGeneratorRunner.RunGenerator("""
using MemoryPack;

namespace ExternalLibrary;

[MemoryPackable]
public partial class ExternalDto { public int Id { get; set; } }
""", assemblyName: "ExternalLibrary");
        externalGeneratorDiagnostics.Where(x => x.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
        using var stream = new MemoryStream();
        externalCompilation.Emit(stream).Success.Should().BeTrue();

        var reference = MetadataReference.CreateFromImage(stream.ToArray());
        var (_, diagnostics) = CSharpGeneratorRunner.RunGenerator("""
using ExternalLibrary;
using MemoryPack;

[MemoryPackSerializable(typeof(ExternalDto))]
public partial class InvalidContext : MemoryPackSerializerContext { }
""", additionalReferences: [reference]);

        diagnostics.Should().ContainSingle(x => x.Id == "MEMPACK045");
    }
}
