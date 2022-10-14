using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace MemoryPack.Generator;

// share context for SourceGenerator and IncrementalGenerator
public interface IGeneratorContext
{
    CancellationToken CancellationToken { get; }
    void ReportDiagnostic(Diagnostic diagnostic);
    void AddSource(string hintName, string source);
    LanguageVersion LanguageVersion { get; }
    bool IsNet7OrGreater { get; }
}

public static class GeneratorContextExtensions
{

    public static bool IsCSharp11OrGreater(this IGeneratorContext context)
    {
        return (int)context.LanguageVersion >= 1100; // C# 11 == 1100
    }
}
