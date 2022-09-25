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

    public static AttributeData? GetAttribute(this ISymbol symbol, INamedTypeSymbol attribtue)
    {
        return symbol.GetAttributes().FirstOrDefault(x => SymbolEqualityComparer.Default.Equals(x.AttributeClass, attribtue));
    }

    public static IEnumerable<ISymbol> GetAllMembers(this INamedTypeSymbol symbol, bool withoutOverride = true)
    {
        // Iterate Parent -> Derived
        if (symbol.BaseType != null)
        {
            foreach (var item in GetAllMembers(symbol.BaseType))
            {
                // override item already iterated in parent type
                if (!withoutOverride || !item.IsOverride)
                {
                    yield return item;
                }
            }
        }

        foreach (var item in symbol.GetMembers())
        {
            if (!withoutOverride || !item.IsOverride)
            {
                yield return item;
            }
        }
    }

    public static bool IsWillImplementIMemoryPackable(this ITypeSymbol symbol, ReferenceSymbols references)
    {
        // [MemoryPackable] and not interface/abstract, generator will implmement IMemoryPackable<T>
        return !symbol.IsAbstract && symbol.ContainsAttribute(references.MemoryPackableAttribute);
    }

    public static bool IsWillImplementMemoryPackUnion(this ITypeSymbol symbol, ReferenceSymbols references)
    {
        return symbol.IsAbstract && symbol.ContainsAttribute(references.MemoryPackUnionAttribute);
    }

    public static bool HasDuplicate<T>(this IEnumerable<T> source)
    {
        var set = new HashSet<T>();
        foreach (var item in source)
        {
            if (!set.Add(item))
            {
                return true;
            }
        }
        return false;
    }

    public static IEnumerable<INamedTypeSymbol> GetAllBaseTypes(this INamedTypeSymbol symbol)
    {
        var t = symbol.BaseType;
        while (t != null)
        {
            yield return t;
            t = t.BaseType;
        }
    }

    public static string FullyQualifiedToString(this ISymbol symbol)
    {
        return symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
    }

    public static bool EqualsUnconstructedGenericType(this INamedTypeSymbol left, INamedTypeSymbol right)
    {
        var l = left.IsGenericType ? left.ConstructUnboundGenericType() : left;
        var r = right.IsGenericType ? right.ConstructUnboundGenericType() : right;
        return SymbolEqualityComparer.Default.Equals(l, r);
    }
}
