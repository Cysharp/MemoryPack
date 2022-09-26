using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MemoryPack.Generator;

// dotnet/runtime generators.

// https://github.com/dotnet/runtime/blob/main/src/libraries/System.Text.RegularExpressions/gen/
// https://github.com/dotnet/runtime/tree/main/src/libraries/System.Text.Json/gen
// https://github.com/dotnet/runtime/tree/main/src/libraries/System.Private.CoreLib/gen
// https://github.com/dotnet/runtime/tree/main/src/libraries/Microsoft.Extensions.Logging.Abstractions/gen
// https://github.com/dotnet/runtime/tree/main/src/libraries/System.Runtime.InteropServices.JavaScript/gen/JSImportGenerator
// https://github.com/dotnet/runtime/tree/main/src/libraries/System.Runtime.InteropServices/gen/LibraryImportGenerator
// https://github.com/dotnet/runtime/tree/main/src/tests/Common/XUnitWrapperGenerator

// documents, blogs.

// https://github.com/dotnet/roslyn/blob/main/docs/features/incremental-generators.md
// https://andrewlock.net/creating-a-source-generator-part-1-creating-an-incremental-source-generator/
// https://qiita.com/WiZLite/items/48f37278cf13be899e40
// https://zenn.dev/pcysl5edgo/articles/6d9be0dd99c008
// https://neue.cc/2021/05/08_600.html
// https://www.thinktecture.com/en/net/roslyn-source-generators-introduction/

// for check generated file
// <EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles>
// <CompilerGeneratedFilesOutputPath>Generated</CompilerGeneratedFilesOutputPath>

[Generator(LanguageNames.CSharp)]
public partial class MemoryPackGenerator : IIncrementalGenerator
{
    public const string MemoryPackableAttributeFullName = "MemoryPack.MemoryPackableAttribute";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // no need RegisterPostInitializationOutput

        // return dir of info output or null .
        var logProvider = context.AnalyzerConfigOptionsProvider
            .Select((configOptions, token) =>
            {
                if (configOptions.GlobalOptions.TryGetValue("build_property.MemoryPackGenerator_SerializationInfoOutputDirectory", out var path))
                {
                    return path;
                }

                return (string?)null;
            });

        var typeDeclarations = context.SyntaxProvider.ForAttributeWithMetadataName(
                MemoryPackableAttributeFullName,
                predicate: static (node, token) =>
                {
                    // search [MemoryPackable] class or struct or interface or record
                    return (node is ClassDeclarationSyntax
                                 or StructDeclarationSyntax
                                 or RecordDeclarationSyntax
                                 or InterfaceDeclarationSyntax);
                },
                transform: static (context, token) =>
                {
                    return (TypeDeclarationSyntax)context.TargetNode;
                });

        var source = typeDeclarations
            .Combine(context.CompilationProvider)
            .WithComparer(Comparer.Instance)
            .Combine(logProvider);

        context.RegisterSourceOutput(source, static (context, source) =>
        {
            var typeDeclaration = source.Left.Item1;
            var compilation = source.Left.Item2;
            var logPath = source.Right;

            Generate(typeDeclaration, compilation, logPath, context);
        });
    }

    class Comparer : IEqualityComparer<(TypeDeclarationSyntax, Compilation)>
    {
        public static readonly Comparer Instance = new Comparer();

        public bool Equals((TypeDeclarationSyntax, Compilation) x, (TypeDeclarationSyntax, Compilation) y)
        {
            return x.Item1.Equals(y.Item1);
        }

        public int GetHashCode((TypeDeclarationSyntax, Compilation) obj)
        {
            return obj.Item1.GetHashCode();
        }
    }
}
