using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

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

    public static AttributeData? GetImplAttribute(this ISymbol symbol, INamedTypeSymbol implAttribtue)
    {
        return symbol.GetAttributes().FirstOrDefault(x =>
        {
            if (x.AttributeClass == null) return false;
            if (x.AttributeClass.EqualsUnconstructedGenericType(implAttribtue)) return true;

            foreach (var item in x.AttributeClass.GetAllBaseTypes())
            {
                if (item.EqualsUnconstructedGenericType(implAttribtue))
                {
                    return true;
                }
            }
            return false;
        });
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

    public static bool TryGetMemoryPackableType(this ITypeSymbol symbol, ReferenceSymbols references, out GenerateType generateType, out SerializeLayout serializeLayout)
    {
        var packableCtorArgs = symbol.GetAttribute(references.MemoryPackableAttribute)?.ConstructorArguments;
        generateType = GenerateType.Object;
        serializeLayout = SerializeLayout.Sequential;
        if (packableCtorArgs == null)
        {
            generateType = GenerateType.NoGenerate;
            serializeLayout = SerializeLayout.Sequential;
            return false;
        }
        else if (packableCtorArgs.Value.Length != 0)
        {
            // MemoryPackable has two attribtue
            if (packableCtorArgs.Value.Length == 1)
            {
                // (SerializeLayout serializeLayout)
                var ctorValue = packableCtorArgs.Value[0];
                serializeLayout = (SerializeLayout)(ctorValue.Value ?? SerializeLayout.Sequential);
                generateType = GenerateType.Object;
            }
            else
            {
                // (GenerateType generateType = GenerateType.Object, SerializeLayout serializeLayout = SerializeLayout.Sequential)
                generateType = (GenerateType)(packableCtorArgs.Value[0].Value ?? GenerateType.Object);
                serializeLayout = (SerializeLayout)(packableCtorArgs.Value[1].Value ?? SerializeLayout.Sequential);
                if (generateType is GenerateType.VersionTolerant or GenerateType.CircularReference)
                {
                    serializeLayout = SerializeLayout.Explicit; // version-torelant, always explicit.
                }
            }
        }

        if (symbol.IsStatic || symbol.IsAbstract)
        {
            // static or abstract class is Union
            return false;
        }

        return true;
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
