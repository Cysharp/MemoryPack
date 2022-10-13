using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MemoryPack.Generator;

[Generator(LanguageNames.CSharp)]
public partial class MemoryPackGenerator : ISourceGenerator
{
    public const string MemoryPackableAttributeFullName = "MemoryPack.MemoryPackableAttribute";
    public const string GenerateTypeScriptAttributeFullName = "MemoryPack.GenerateTypeScriptAttribute";

    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications(SyntaxContextReceiver.Create);
    }

    public void Execute(GeneratorExecutionContext context)
    {
        if (context.SyntaxContextReceiver is not SyntaxContextReceiver receiver || receiver.ClassDeclarations.Count == 0)
        {
            return;
        }


        // C#11 == 1100
        var langVersion = (int)(context.ParseOptions as CSharpParseOptions)!.LanguageVersion;
        var langVersion2 = (int)(context.ParseOptions as CSharpParseOptions)!.SpecifiedLanguageVersion;

        // exists NET7_OR_GREATER?
        var takoyaki = context.ParseOptions.PreprocessorSymbolNames.ToArray();

        string? logPath = null; //  TODO: get from options

        var compiation = context.Compilation;
        var generateContext = new GeneratorContext(context);

        foreach (var syntax in receiver.ClassDeclarations)
        {
            Generate(syntax, compiation, logPath, generateContext);
        }
    }

    class SyntaxContextReceiver : ISyntaxContextReceiver
    {
        internal static ISyntaxContextReceiver Create()
        {
            return new SyntaxContextReceiver();
        }

        public HashSet<TypeDeclarationSyntax> ClassDeclarations { get; } = new();

        public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
        {
            var node = context.Node;
            if (node is ClassDeclarationSyntax
                     or StructDeclarationSyntax
                     or RecordDeclarationSyntax
                     or InterfaceDeclarationSyntax)
            {
                var typeSyntax = (TypeDeclarationSyntax)node;
                if (typeSyntax.AttributeLists.Count > 0)
                {
                    var attr = typeSyntax.AttributeLists.SelectMany(x => x.Attributes)
                        .FirstOrDefault(x => x.Name.ToString() is "MemoryPackable" or "MemoryPackableAttribute" or "MemoryPack.MemoryPackable" or "MemoryPack.MemoryPackableAttribute");
                    if (attr != null)
                    {
                        ClassDeclarations.Add(typeSyntax);
                    }
                }
            }
        }
    }

    class GeneratorContext : IGeneratorContext
    {
        GeneratorExecutionContext context;

        public GeneratorContext(GeneratorExecutionContext context)
        {
            this.context = context;
        }

        public CancellationToken CancellationToken => context.CancellationToken;

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
