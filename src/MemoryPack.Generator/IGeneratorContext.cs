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
    bool IsNet7OrGreater { get; } // for the struct layout of the runtime(MEMPACK034), not for the build of MemoryPack
    bool IsNet8OrGreater { get; } // uses the net* build of MemoryPack(net8.0 is the lowest net* target), otherwise the netstandard2.1 build
    bool IsNet10OrGreater { get; } // for the DateTimeOffset layout of the runtime(MEMPACK043)
    bool IsUnityCompilation { get; } // has UNITY_*** symbol
    bool IsForUnity { get; }
}

public static class GeneratorContextExtensions
{
    public static bool IsCSharp9OrGreater(this IGeneratorContext context)
    {
        return (int)context.LanguageVersion >= 900; // C# 9 == 900
    }

    public static bool IsCSharp10OrGreater(this IGeneratorContext context)
    {
        return (int)context.LanguageVersion >= 1000; // C# 10 == 1000
    }

    public static bool IsCSharp11OrGreater(this IGeneratorContext context)
    {
        return (int)context.LanguageVersion >= 1100; // C# 11 == 1100
    }
}
