using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Text;

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
    public const string MemoryPackUnionFormatterAttributeFullName = "MemoryPack.MemoryPackUnionFormatterAttribute";
    public const string GenerateTypeScriptAttributeFullName = "MemoryPack.GenerateTypeScriptAttribute";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // no need RegisterPostInitializationOutput

        RegisterMemoryPackable(context);
        RegisterTypeScript(context);
    }

    void RegisterMemoryPackable(IncrementalGeneratorInitializationContext context)
    {
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

        var typeDeclarations2 = context.SyntaxProvider.ForAttributeWithMetadataName(
                MemoryPackUnionFormatterAttributeFullName,
                predicate: static (node, token) =>
                {
                    return (node is ClassDeclarationSyntax);
                },
                transform: static (context, token) =>
                {
                    return (TypeDeclarationSyntax)context.TargetNode;
                });

        var parseOptions = context.ParseOptionsProvider.Select((parseOptions, token) =>
        {
            var csOptions = (CSharpParseOptions)parseOptions;
            var langVersion = csOptions.LanguageVersion;
            var net7 = csOptions.PreprocessorSymbolNames.Contains("NET7_0_OR_GREATER");
            return (langVersion, net7);
        });

        {
            var source = typeDeclarations
                .Combine(context.CompilationProvider)
                .WithComparer(Comparer.Instance)
                .Combine(logProvider)
                .Combine(parseOptions);

            context.RegisterSourceOutput(source, static (context, source) =>
            {
                var (typeDeclaration, compilation) = source.Left.Item1;
                var logPath = source.Left.Item2;
                var (langVersion, net7) = source.Right;

                Generate(typeDeclaration, compilation, logPath, new GeneratorContext(context, langVersion, net7));
            });
        }
        {
            var source = typeDeclarations2
                .Combine(context.CompilationProvider)
                .WithComparer(Comparer.Instance)
                .Combine(logProvider)
                .Combine(parseOptions);

            context.RegisterSourceOutput(source, static (context, source) =>
            {
                var (typeDeclaration, compilation) = source.Left.Item1;
                var logPath = source.Left.Item2;
                var (langVersion, net7) = source.Right;

                Generate(typeDeclaration, compilation, logPath, new GeneratorContext(context, langVersion, net7));
            });
        }
    }

    void RegisterTypeScript(IncrementalGeneratorInitializationContext context)
    {
        var typeScriptEnabled = context.AnalyzerConfigOptionsProvider
            .Select((configOptions, token) =>
            {
                // https://github.com/dotnet/project-system/blob/main/docs/design-time-builds.md
                var isDesignTimeBuild = configOptions.GlobalOptions.TryGetValue("build_property.DesignTimeBuild", out var designTimeBuild) && designTimeBuild == "true";

                string? path;
                if (!configOptions.GlobalOptions.TryGetValue("build_property.MemoryPackGenerator_TypeScriptOutputDirectory", out path))
                {
                    path = null;
                }
                string ext;
                if (!configOptions.GlobalOptions.TryGetValue("build_property.MemoryPackGenerator_TypeScriptImportExtension", out ext!))
                {
                    ext = ".js";
                }

                string convertProp;
                if (!configOptions.GlobalOptions.TryGetValue("build_property.MemoryPackGenerator_TypeScriptConvertPropertyName", out convertProp!))
                {
                    convertProp = "true";
                }

                if (!configOptions.GlobalOptions.TryGetValue("build_property.MemoryPackGenerator_TypeScriptEnableNullableTypes", out var enableNullableTypes))
                {
                    enableNullableTypes = "false";
                }

                if (!bool.TryParse(convertProp, out var convert)) convert = true;

                if (path == null) return null;

                return new TypeScriptGenerateOptions
                {
                    OutputDirectory = path,
                    ImportExtension = ext,
                    ConvertPropertyName = convert,
                    EnableNullableTypes = bool.TryParse(enableNullableTypes, out var enabledNullableTypesParsed) && enabledNullableTypesParsed,
                    IsDesignTimeBuild = isDesignTimeBuild
                };
            });

        var typeScriptDeclarations = context.SyntaxProvider.ForAttributeWithMetadataName(
                GenerateTypeScriptAttributeFullName,
                predicate: static (node, token) =>
                {
                    return (node is ClassDeclarationSyntax
                                 or RecordDeclarationSyntax
                                 or InterfaceDeclarationSyntax);
                },
                transform: static (context, token) =>
                {
                    return (TypeDeclarationSyntax)context.TargetNode;
                });

        var typeScriptGenerateSource = typeScriptDeclarations
            .Combine(context.CompilationProvider)
            .WithComparer(Comparer.Instance)
            .Combine(typeScriptEnabled)
            .Where(x => x.Right != null) // filter
            .Collect();

        context.RegisterSourceOutput(typeScriptGenerateSource, static (context, source) =>
        {
            ReferenceSymbols? reference = null;
            string? generatePath = null;

            var unionMap = new Dictionary<ITypeSymbol, ITypeSymbol>(SymbolEqualityComparer.Default); // <impl, base>
            foreach (var item in source)
            {
                var tsOptions = item.Right;
                if (tsOptions == null) continue;
                if (tsOptions.IsDesignTimeBuild) continue; // designtime build(in IDE), do nothing.

                var syntax = item.Left.Item1;
                var compilation = item.Left.Item2;
                var semanticModel = compilation.GetSemanticModel(syntax.SyntaxTree);
                var typeSymbol = semanticModel.GetDeclaredSymbol(syntax, context.CancellationToken) as ITypeSymbol;
                if (typeSymbol == null) continue;
                if (reference == null)
                {
                    reference = new ReferenceSymbols(compilation);
                }

                if (generatePath is null && item.Right is { } options)
                {
                    generatePath = options.OutputDirectory;
                }

                var isUnion = typeSymbol.ContainsAttribute(reference.MemoryPackUnionAttribute);

                if (isUnion)
                {
                    var unionTags = typeSymbol.GetAttributes()
                        .Where(x => SymbolEqualityComparer.Default.Equals(x.AttributeClass, reference.MemoryPackUnionAttribute))
                        .Where(x => x.ConstructorArguments.Length == 2)
                        .Select(x => (INamedTypeSymbol)x.ConstructorArguments[1].Value!);
                    foreach (var implType in unionTags)
                    {
                        unionMap[implType] = typeSymbol;
                    }
                }
            }

            if (generatePath != null)
            {
                var collector = new TypeCollector();
                foreach (var item in source)
                {
                    var typeDeclaration = item.Left.Item1;
                    var compilation = item.Left.Item2;

                    if (reference == null)
                    {
                        reference = new ReferenceSymbols(compilation);
                    }

                    var meta = GenerateTypeScript(typeDeclaration, compilation, item.Right!, context, reference, unionMap);
                    if (meta != null)
                    {
                        collector.Visit(meta, false);
                    }
                }

                GenerateEnums(collector.GetEnums(), generatePath);

                // generate runtime
                var runtime = new[]{
                    ("MemoryPackWriter.ts", TypeScriptRuntime.MemoryPackWriter),
                    ("MemoryPackReader.ts", TypeScriptRuntime.MemoryPackReader),
                };

                foreach (var item in runtime)
                {
                    var filePath = Path.Combine(generatePath, item.Item1);
                    if (!File.Exists(filePath))
                    {
                        File.WriteAllText(filePath, item.Item2, new UTF8Encoding(false));
                    }
                }
            }
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

    class GeneratorContext : IGeneratorContext
    {
        SourceProductionContext context;

        public GeneratorContext(SourceProductionContext context, LanguageVersion languageVersion, bool isNet70OrGreater)
        {
            this.context = context;
            this.LanguageVersion = languageVersion;
            this.IsNet7OrGreater = isNet70OrGreater;
        }

        public CancellationToken CancellationToken => context.CancellationToken;

        public Microsoft.CodeAnalysis.CSharp.LanguageVersion LanguageVersion { get; }

        public bool IsNet7OrGreater { get; }

        public bool IsForUnity => false;

        public void AddSource(string hintName, string source)
        {
            context.AddSource(hintName, source);
        }

        public void ReportDiagnostic(Diagnostic diagnostic)
        {
            context.ReportDiagnostic(diagnostic);
        }
    }
}
