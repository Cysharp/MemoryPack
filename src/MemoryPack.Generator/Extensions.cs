using Microsoft.CodeAnalysis;

namespace MemoryPack.Generator;

internal static class Extensions
{
    private const string UnderScorePrefix = "_";

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

    public static IEnumerable<ISymbol> GetParentMembers(this INamedTypeSymbol symbol)
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
    }

    public static bool TryGetMemoryPackableType(this ITypeSymbol symbol, ReferenceSymbols references, out GenerateType generateType, out SerializeLayout serializeLayout)
    {
        var memPackAttr = symbol.GetAttribute(references.MemoryPackableAttribute);
        var packableCtorArgs = memPackAttr?.ConstructorArguments;
        generateType = GenerateType.Object;
        serializeLayout = SerializeLayout.Sequential;
        if (memPackAttr == null || packableCtorArgs == null)
        {
            generateType = GenerateType.NoGenerate;
            serializeLayout = SerializeLayout.Sequential;
            return false;
        }
        else if (packableCtorArgs.Value.Length != 0)
        {
            // MemoryPackable has three attribtues
            // [GenerateType generateType]
            // [SerializeLayout serializeLayout]
            // [GenerateType generateType, SerializeLayout serializeLayout]

            if (packableCtorArgs.Value.Length == 1)
            {
                var ctorValue = packableCtorArgs.Value[0];

                // check which construcotr was used
                var attrConstructor = memPackAttr.AttributeConstructor;
                var isSerializeLayout = attrConstructor!.Parameters[0].Type.Name == nameof(SerializeLayout);
                if (isSerializeLayout)
                {
                    generateType = GenerateType.Object;
                    serializeLayout = (SerializeLayout)(ctorValue.Value!);
                }
                else
                {
                    generateType = (GenerateType)(ctorValue.Value!);
                    serializeLayout = SerializeLayout.Sequential;
                    if (generateType is GenerateType.VersionTolerant or GenerateType.CircularReference)
                    {
                        serializeLayout = SerializeLayout.Explicit;
                    }
                }
            }
            else
            {
                generateType = (GenerateType)(packableCtorArgs.Value[0].Value ?? GenerateType.Object);
                serializeLayout = (SerializeLayout)(packableCtorArgs.Value[1].Value ?? SerializeLayout.Sequential);
            }
        }

        if (generateType == GenerateType.Object && (symbol.IsStatic || symbol.IsAbstract))
        {
            // static or abstract class is Union, set as NoGenerate
            generateType = GenerateType.Union;
            serializeLayout = SerializeLayout.Sequential;
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

    public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector) => DistinctBy(source, keySelector, null);

    public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey>? comparer)
    {
        return DistinctByIterator(source, keySelector, comparer);
    }

    private static IEnumerable<TSource> DistinctByIterator<TSource, TKey>(IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey>? comparer)
    {
        using IEnumerator<TSource> enumerator = source.GetEnumerator();

        if (enumerator.MoveNext())
        {
            var set = new HashSet<TKey>(comparer);
            do
            {
                TSource element = enumerator.Current;
                if (set.Add(keySelector(element)))
                {
                    yield return element;
                }
            }
            while (enumerator.MoveNext());
        }
    }

    public static bool TryGetConstructorParameter(this IMethodSymbol constructor, ISymbol member, out string? constructorParameterName)
    {
        var constructorParameter = GetConstructorParameter(constructor, member.Name);
        if (constructorParameter == null && member.Name.StartsWith(UnderScorePrefix))
        {
            constructorParameter = GetConstructorParameter(constructor, member.Name.Substring(UnderScorePrefix.Length));
        }

        constructorParameterName = constructorParameter?.Name;
        return constructorParameter != null;

        static IParameterSymbol? GetConstructorParameter(IMethodSymbol constructor, string name) => constructor.Parameters.FirstOrDefault(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    public static bool ContainsConstructorParameter(this IEnumerable<MemberMeta> members, IParameterSymbol constructorParameter) =>
        members.Any(x =>
            x.IsConstructorParameter &&
            string.Equals(constructorParameter.Name, x.ConstructorParameterName, StringComparison.OrdinalIgnoreCase));
}
