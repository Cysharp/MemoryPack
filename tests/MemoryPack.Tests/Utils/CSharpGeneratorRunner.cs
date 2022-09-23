using MemoryPack.Generator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

namespace MemoryPack.Tests.Utils;

public static class CSharpGeneratorRunner
{
    static Compilation baseCompilation = default!;

    [ModuleInitializer]
    public static void InitializeCompilation()
    {
        // running .NET Core system assemblies dir path
        var baseAssemblyPath = Path.GetDirectoryName(typeof(object).Assembly.Location)!;
        var systemAssemblies = Directory.GetFiles(baseAssemblyPath)
            .Where(x =>
            {
                var fileName = Path.GetFileName(x);
                if (fileName.EndsWith("Native.dll")) return false;
                return fileName.StartsWith("System") || (fileName is "mscorlib.dll" or "netstandard.dll");
            });

        var references = systemAssemblies
            .Append(typeof(MemoryPackableAttribute).Assembly.Location) // System Assemblies + MemoryPack.Core.dll
            .Select(x => MetadataReference.CreateFromFile(x))
            .ToArray();

        var compilation = CSharpCompilation.Create("generatortest",
            references: references,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        baseCompilation = compilation;
    }

    public static Diagnostic[] RunGenerator(string source)
    {
        var driver = CSharpGeneratorDriver.Create(new MemoryPackGenerator());

        var compilation = baseCompilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.CSharp11))); // use C#11

        driver.RunGeneratorsAndUpdateCompilation(compilation, out var newCompilation, out var diagnostics);

        // combine diagnostics as result.
        var compilationDiagnostics = newCompilation.GetDiagnostics();
        return diagnostics.Concat(compilationDiagnostics).ToArray();
    }
}
