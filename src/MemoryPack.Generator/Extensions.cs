using Microsoft.CodeAnalysis;

namespace MemoryPack.Generator;

internal static class Extensions
{
    public static string NewLine(this IEnumerable<string> source)
    {
        return string.Join(Environment.NewLine, source);
    }

    public static bool ContainsAttribute(this ISymbol symbol, INamedTypeSymbol attribtue)
    {
        return symbol.GetAttributes().Any(x => SymbolEqualityComparer.Default.Equals(x.AttributeClass, attribtue));
    }
}
