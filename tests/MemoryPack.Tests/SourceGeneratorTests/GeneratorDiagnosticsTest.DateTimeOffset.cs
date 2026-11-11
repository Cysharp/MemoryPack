#if NET7_0_OR_GREATER

using Microsoft.CodeAnalysis;
using System.Linq;

namespace MemoryPack.Tests;

public partial class GeneratorDiagnosticsTest
{
    static readonly string[] Net9Symbols = new[] { "NET7_0_OR_GREATER", "NET8_0_OR_GREATER", "NET9_0_OR_GREATER" };
    static readonly string[] Net10Symbols = new[] { "NET7_0_OR_GREATER", "NET8_0_OR_GREATER", "NET9_0_OR_GREATER", "NET10_0_OR_GREATER" };
    static readonly string[] NetStandardSymbols = new[] { "NETSTANDARD2_1" };
    static readonly string[] UnitySymbols = new[] { "NETSTANDARD2_1", "UNITY_5_3_OR_NEWER", "UNITY_2022_3_OR_NEWER", "UNITY_EDITOR" };

    static string DateTimeOffsetMemberCode(string memberType)
    {
        return $$"""
using System;
using System.Collections.Generic;
using MemoryPack;

[MemoryPackable]
public partial class Hoge
{
    public int Id { get; set; }
    public {{memberType}} Target { get; set; }
}

public struct UnmanagedHasDateTimeOffset
{
    public int X;
    public DateTimeOffset Y;
}

public struct UnmanagedHasNoDateTimeOffset
{
    public int X;
    public DateTime Y;
    public Net10DateTimeOffset Z;
    public UnityDateTimeOffset W;
}
""";
    }

    [Theory]
    [InlineData("DateTimeOffset")]
    [InlineData("DateTimeOffset?")]
    [InlineData("DateTimeOffset[]")]
    [InlineData("DateTimeOffset?[]")]
    [InlineData("List<DateTimeOffset>")]
    [InlineData("Dictionary<int, DateTimeOffset>")]
    [InlineData("Dictionary<DateTimeOffset, string>")]
    [InlineData("(int, DateTimeOffset)")]
    [InlineData("List<DateTimeOffset[]>")]
    [InlineData("UnmanagedHasDateTimeOffset")]
    [InlineData("UnmanagedHasDateTimeOffset[]")]
    public void MEMPACK043_DateTimeOffsetLayoutDependsOnRuntime(string memberType)
    {
        var code = DateTimeOffsetMemberCode(memberType);

        foreach (var symbols in new[] { Net9Symbols, NetStandardSymbols, UnitySymbols })
        {
            var (_, diagnostics) = CSharpGeneratorRunner.RunGenerator(code, symbols);
            diagnostics.Length.Should().Be(1);
            diagnostics[0].Id.Should().Be("MEMPACK043");
            diagnostics[0].Severity.Should().Be(DiagnosticSeverity.Warning);

            // recommends both types, Unity is UnityDateTimeOffset first.
            var message = diagnostics[0].GetMessage();
            message.Should().Contain("'Hoge' member 'Target'");
            var net10Index = message.IndexOf("MemoryPack.Net10DateTimeOffset");
            var unityIndex = message.IndexOf("MemoryPack.UnityDateTimeOffset");
            net10Index.Should().BeGreaterThan(0);
            unityIndex.Should().BeGreaterThan(0);
            if (symbols == UnitySymbols)
            {
                unityIndex.Should().BeLessThan(net10Index);
            }
            else
            {
                net10Index.Should().BeLessThan(unityIndex);
            }
        }

        // .NET 10 or later, no warning.
        {
            var (_, diagnostics) = CSharpGeneratorRunner.RunGenerator(code, Net10Symbols);
            diagnostics.Length.Should().Be(0);
        }
    }

    [Theory]
    [InlineData("Net10DateTimeOffset")]
    [InlineData("Net10DateTimeOffset?")]
    [InlineData("Net10DateTimeOffset[]")]
    [InlineData("List<Net10DateTimeOffset>")]
    [InlineData("UnityDateTimeOffset")]
    [InlineData("UnityDateTimeOffset?")]
    [InlineData("UnityDateTimeOffset[]")]
    [InlineData("List<UnityDateTimeOffset>")]
    [InlineData("DateTime")]
    [InlineData("TimeSpan")]
    [InlineData("List<DateTime>")]
    [InlineData("UnmanagedHasNoDateTimeOffset")]
    public void MEMPACK043_NoWarning(string memberType)
    {
        var code = DateTimeOffsetMemberCode(memberType);

        foreach (var symbols in new[] { Net9Symbols, NetStandardSymbols, UnitySymbols, Net10Symbols })
        {
            var (_, diagnostics) = CSharpGeneratorRunner.RunGenerator(code, symbols);
            diagnostics.Length.Should().Be(0);
        }
    }

    [Fact]
    public void MEMPACK043_IgnoredMemberAndGeneratedCode()
    {
        // ignored member is not serialized, no warning.
        {
            var (_, diagnostics) = CSharpGeneratorRunner.RunGenerator("""
using System;
using MemoryPack;

[MemoryPackable]
public partial class Hoge
{
    public int Id { get; set; }
    [MemoryPackIgnore]
    public DateTimeOffset Target { get; set; }
}
""", Net9Symbols);
            diagnostics.Length.Should().Be(0);
        }

        // warning does not stop code generation, and Net10DateTimeOffset member is compilable.
        foreach (var memberType in new[] { "DateTimeOffset", "Net10DateTimeOffset", "UnityDateTimeOffset" })
        {
            var (compilation, _) = CSharpGeneratorRunner.RunGenerator(DateTimeOffsetMemberCode(memberType), Net9Symbols);
            compilation.SyntaxTrees.Any(x => x.FilePath.Contains("Hoge")).Should().BeTrue();
            compilation.GetDiagnostics().Where(x => x.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
        }
    }
}

#endif
