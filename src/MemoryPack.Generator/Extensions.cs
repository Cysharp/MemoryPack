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

    public static IEnumerable<ISymbol> GetAllMembers(this INamedTypeSymbol symbol)
    {
        // Iterate Parent -> Derived
        if (symbol.BaseType != null)
        {
            foreach (var item in GetAllMembers(symbol.BaseType))
            {
                // override item already iterated in parent type
                if (!item.IsOverride)
                {
                    yield return item;
                }
            }
        }

        foreach (var item in symbol.GetMembers())
        {
            if (!item.IsOverride)
            {
                yield return item;
            }
        }
    }

    public static bool IsWillImplementIMemoryPackable(this INamedTypeSymbol symbol, ReferenceSymbols reference)
    {
        // [MemoryPackable] and not interface/abstract, generator will implmement IMemoryPackable<T>
        return !symbol.IsAbstract && symbol.ContainsAttribute(reference.MemoryPackableAttribute);
    }
}
