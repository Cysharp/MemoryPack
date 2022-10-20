using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace MemoryPack.Generator;

public class TypeCollector
{
    HashSet<ITypeSymbol> types = new(SymbolEqualityComparer.Default);

    public void Visit(TypeMeta typeMeta, bool visitInterface)
    {
        Visit(typeMeta.Symbol, visitInterface);
        foreach (var item in typeMeta.Members)
        {
            Visit(item.MemberType, visitInterface);
        }
    }

    public void Visit(ISymbol symbol, bool visitInterface)
    {
        if (symbol is ITypeSymbol typeSymbol)
        {
            // 7~20 is primitive
            if ((int)typeSymbol.SpecialType is >= 7 and <= 20)
            {
                return;
            }

            if (!types.Add(typeSymbol))
            {
                return;
            }

            if (typeSymbol is IArrayTypeSymbol array)
            {
                Visit(array.ElementType, visitInterface);
            }
            else if (typeSymbol is INamedTypeSymbol namedTypeSymbol)
            {
                if (visitInterface)
                {
                    foreach (var item in namedTypeSymbol.AllInterfaces)
                    {
                        Visit(item, visitInterface);
                    }

                    foreach (var item in namedTypeSymbol.GetAllBaseTypes())
                    {
                        Visit(item, visitInterface);
                    }
                }

                if (namedTypeSymbol.IsGenericType)
                {
                    foreach (var item in namedTypeSymbol.TypeArguments)
                    {
                        Visit(item, visitInterface);
                    }
                }
            }
        }
    }

    public IEnumerable<ITypeSymbol> GetEnums()
    {
        foreach (var typeSymbol in types)
        {
            if (typeSymbol.TypeKind == TypeKind.Enum)
            {
                yield return typeSymbol;
            }
        }
    }

    public IEnumerable<ITypeSymbol> GetMemoryPackableTypes(ReferenceSymbols reference)
    {
        foreach (var typeSymbol in types)
        {
            if (typeSymbol.ContainsAttribute(reference.MemoryPackableAttribute))
            {
                yield return typeSymbol;
            }
        }
    }

    public IEnumerable<ITypeSymbol> GetTypes()
    {
        return types.OfType<ITypeSymbol>();
    }
}
