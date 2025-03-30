using MemoryPack.Generator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;

namespace MemoryPack.Tests.Utils;

public static class CSharpGeneratorRunner
{
    static Compilation baseCompilation = default!;

    [ModuleInitializer]
    public static void InitializeCompilation()
    {
        var globalUsings = """
global using System;
global using System.Linq;
global using System.Collections;
global using System.Collections.Generic;
global using System.Threading;
global using System.Threading.Tasks;
global using System.ComponentModel.DataAnnotations;
global using MemoryPack;
""";

        var systemAssemblies = AppDomain.CurrentDomain.GetAssemblies()
            .Where(x => !x.IsDynamic && !string.IsNullOrWhiteSpace(x.Location));

        var references = systemAssemblies
            .Append(typeof(MemoryPackableAttribute).Assembly) // System Assemblies + MemoryPack.Core.dll
            .Select(x => MetadataReference.CreateFromFile(x.Location))
            .ToArray();

        var compilation = CSharpCompilation.Create("generatortest",
            references: references,
            syntaxTrees: [CSharpSyntaxTree.ParseText(globalUsings, path: "GlobalUsings.cs")],
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true));

        baseCompilation = compilation;
    }

    public static (Compilation, ImmutableArray<Diagnostic>) RunGenerator(string source, string[]? preprocessorSymbols = null, AnalyzerConfigOptionsProvider? options = null)
    {
        if (preprocessorSymbols == null)
        {
            preprocessorSymbols = new[] { "NET7_0_OR_GREATER" };
        }
        var parseOptions = new CSharpParseOptions(LanguageVersion.Latest, preprocessorSymbols: preprocessorSymbols);

        var driver = CSharpGeneratorDriver.Create(new MemoryPackGenerator()).WithUpdatedParseOptions(parseOptions);
        if (options != null)
        {
            driver = (Microsoft.CodeAnalysis.CSharp.CSharpGeneratorDriver)driver.WithUpdatedAnalyzerConfigOptions(options);
        }

        var compilation = baseCompilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText(source, parseOptions));

        driver.RunGeneratorsAndUpdateCompilation(compilation, out var newCompilation, out var diagnostics);

        return (newCompilation, diagnostics);
    }

    public static (string Key, string Reasons)[][] GetIncrementalGeneratorTrackedStepsReasons(string keyPrefixFilter, params string[] sources)
    {
        var parseOptions = new CSharpParseOptions(LanguageVersion.Latest);
        var driver = CSharpGeneratorDriver.Create(
            [new MemoryPackGenerator().AsSourceGenerator()],
            driverOptions: new GeneratorDriverOptions(IncrementalGeneratorOutputKind.None, trackIncrementalGeneratorSteps: true))
            .WithUpdatedParseOptions(parseOptions);

        var generatorResults = sources
            .Select(source =>
            {
                var compilation = baseCompilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText(source, parseOptions));
                driver = driver.RunGenerators(compilation);
                return driver.GetRunResult().Results[0];
            })
            .ToArray();

        var reasons = generatorResults
            .Select(x => x.TrackedSteps
                .Where(x => x.Key.StartsWith(keyPrefixFilter) || x.Key == "SourceOutput")
                .Select(x =>
                {
                    if (x.Key == "SourceOutput")
                    {
                        var values = x.Value.Where(x => x.Inputs[0].Source.Name?.StartsWith(keyPrefixFilter) ?? false);
                        return (
                            x.Key,
                            Reasons: string.Join(", ", values.SelectMany(x => x.Outputs).Select(x => x.Reason).ToArray())
                        );
                    }
                    else
                    {
                        return (
                            Key: x.Key.Substring(keyPrefixFilter.Length),
                            Reasons: string.Join(", ", x.Value.SelectMany(x => x.Outputs).Select(x => x.Reason).ToArray())
                        );
                    }
                })
                .OrderBy(x => x.Key)
                .ToArray())
            .ToArray();

        return reasons;
    }
}

public class VerifyHelper(ITestOutputHelper output, string idPrefix)
{
    // Diagnostics Verify

    public void Ok([StringSyntax("C#-test")] string code, [CallerArgumentExpression("code")] string? codeExpr = null)
    {
        output.WriteLine(codeExpr);

        var (compilation, diagnostics) = CSharpGeneratorRunner.RunGenerator(code);
        foreach (var item in diagnostics)
        {
            output.WriteLine(item.ToString());
        }
        OutputGeneratedCode(compilation);

        diagnostics.Length.Should().Be(0);
    }

    public void Verify(int id, [StringSyntax("C#-test")] string code, string diagnosticsCodeSpan, [CallerArgumentExpression("code")] string? codeExpr = null)
    {
        output.WriteLine(codeExpr);

        var (compilation, diagnostics) = CSharpGeneratorRunner.RunGenerator(code);
        foreach (var item in diagnostics)
        {
            output.WriteLine(item.ToString());
        }
        OutputGeneratedCode(compilation);

        diagnostics.Length.Should().Be(1);
        diagnostics[0].Id.Should().Be(idPrefix + id.ToString("000"));

        var text = GetLocationText(diagnostics[0], compilation.SyntaxTrees);
        text.Should().Be(diagnosticsCodeSpan);
    }

    public (string, string)[] Verify([StringSyntax("C#-test")] string code, [CallerArgumentExpression("code")] string? codeExpr = null)
    {
        output.WriteLine(codeExpr);

        var (compilation, diagnostics) = CSharpGeneratorRunner.RunGenerator(code);
        OutputGeneratedCode(compilation);
        return diagnostics.Select(x => (x.Id, GetLocationText(x, compilation.SyntaxTrees))).ToArray();
    }

    string GetLocationText(Diagnostic diagnostic, IEnumerable<SyntaxTree> syntaxTrees)
    {
        var location = diagnostic.Location;

        var textSpan = location.SourceSpan;
        var sourceTree = location.SourceTree;
        if (sourceTree == null)
        {
            var lineSpan = location.GetLineSpan();
            if (lineSpan.Path == null) return "";

            sourceTree = syntaxTrees.FirstOrDefault(x => x.FilePath == lineSpan.Path);
            if (sourceTree == null) return "";
        }

        var text = sourceTree.GetText().GetSubText(textSpan).ToString();
        return text;
    }

    void OutputGeneratedCode(Compilation compilation)
    {
        foreach (var syntaxTree in compilation.SyntaxTrees)
        {
            // only shows ConsoleApp.Run/Builder generated code
            if (!syntaxTree.FilePath.Contains("g.cs")) continue;
            output.WriteLine(syntaxTree.ToString());
        }
    }
}
