using System;
using System.Linq;
using MemoryPack.Tests.Utils;

namespace MemoryPack.Tests.SourceGeneratorTests;

public class NativeAotRegistrationTest
{
    [Fact]
    public void EmitsAotRegistrationForNongenericModels()
    {
        var (compilation, diagnostics) = CSharpGeneratorRunner.RunGenerator("""
using MemoryPack;

[MemoryPackable]
public partial record Item(string Name);
""");

        diagnostics.Should().BeEmpty();
        var generatedCode = compilation.SyntaxTrees
            .Select(x => x.GetText().ToString())
            .First(x => x.Contains("__MemoryPackAotRegistration_", StringComparison.Ordinal));
        generatedCode.Should().Contain("RuntimeFeature.IsDynamicCodeSupported")
            .And.Contain("Register(new global::MemoryPack.Formatters.MemoryPackableFormatter<global::Item>())")
            .And.Contain("new global::MemoryPack.Formatters.ArrayFormatter<global::Item>()");
    }

    [Fact]
    public void DoesNotEmitAotRegistrationForGenericModels()
    {
        var (compilation, diagnostics) = CSharpGeneratorRunner.RunGenerator("""
using MemoryPack;

[MemoryPackable]
public partial record Item<T>(T Value);
""");

        diagnostics.Should().BeEmpty();
        compilation.SyntaxTrees
            .Select(x => x.GetText().ToString())
            .Should()
            .NotContain(x => x.Contains("__MemoryPackAotRegistration_", StringComparison.Ordinal));
    }
}
