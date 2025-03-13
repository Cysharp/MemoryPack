using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MemoryPack.Generator;

internal class FullyQualifiedNameRewriter : CSharpSyntaxRewriter
{
    private readonly SemanticModel _semanticModel;

    public FullyQualifiedNameRewriter(SemanticModel semanticModel)
    {
        _semanticModel = semanticModel;
    }

    public override SyntaxNode? VisitObjectCreationExpression(ObjectCreationExpressionSyntax node)
    {
        var typeInfo = _semanticModel.GetTypeInfo(node.Type);
        var typeSymbol = typeInfo.Type ?? _semanticModel.GetSymbolInfo(node.Type).Symbol as ITypeSymbol;
        if (typeSymbol != null)
        {
            var fullyQualifiedName = typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

            var newTypeSyntax = SyntaxFactory.ParseTypeName(fullyQualifiedName)
                .WithLeadingTrivia(node.Type.GetLeadingTrivia())
                .WithTrailingTrivia(node.Type.GetTrailingTrivia());

            node = node.WithType(newTypeSyntax);
        }

        return base.VisitObjectCreationExpression(node);
    }
}
