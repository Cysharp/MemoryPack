using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace MemoryPack.Generator;

public class TypeCollector
{
    HashSet<ITypeSymbol> types = new(SymbolEqualityComparer.Default);

    public void Visit(TypeMeta typeMeta)
    {
        Visit(typeMeta.Symbol);
        foreach (var item in typeMeta.Members)
        {
            Visit(item.MemberType);
        }
    }

    public void Visit(ISymbol symbol)
    {
        if (symbol is ITypeSymbol typeSymbol)
        {
            if (!types.Add(typeSymbol))
            {
                return;
            }

            if (typeSymbol is IArrayTypeSymbol array)
            {
                Visit(array.ElementType);
            }
            else if (typeSymbol is INamedTypeSymbol namedTypeSymbol)
            {
                foreach (var item in namedTypeSymbol.AllInterfaces)
                {
                    Visit(item);
                }

                foreach (var item in namedTypeSymbol.GetAllBaseTypes())
                {
                    Visit(item);
                }

                if (namedTypeSymbol.IsGenericType)
                {
                    foreach (var item in namedTypeSymbol.TypeArguments)
                    {
                        Visit(item);
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

    public IEnumerable<INamedTypeSymbol> GetSerializableGenericTypes(ReferenceSymbols reference)
    {
        foreach (var typeSymbol in types.OfType<INamedTypeSymbol>())
        {
            if (typeSymbol.IsGenericType && reference.KnownTypes.Contains(typeSymbol))
            {
                yield return typeSymbol;
            }
        }
    }
}
