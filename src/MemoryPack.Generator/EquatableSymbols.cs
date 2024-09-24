using Microsoft.CodeAnalysis;

namespace MemoryPack.Generator;

public class EquatableTypeSymbol(ITypeSymbol symbol) : IEquatable<EquatableTypeSymbol>
{
    public ITypeSymbol Symbol { get; private set; } = symbol;

    // This Equality is not exactly correct
    // We have decided that it is almost certainly fine for internal use only, and are using the name as a key
    public bool Equals(EquatableTypeSymbol other)
    {
        return this.Symbol.EqualsNamespaceAndName(other.Symbol);
    }

    public override string ToString()
    {
        return Symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
    }
}

public class EquatableMethodSymbol(IMethodSymbol symbol) : IEquatable<EquatableMethodSymbol>
{
    public IMethodSymbol Symbol { get; private set; } = symbol;

    // not exactly correct, only check name and parameters name and type
    public bool Equals(EquatableMethodSymbol other)
    {
        if (!Symbol.ContainingType.EqualsNamespaceAndName(other.Symbol.ContainingType)) return false;
        if (Symbol.Name != other.Symbol.Name) return false;

        if (Symbol.Parameters.Length != other.Symbol.Parameters.Length) return false;
        for (int i = 0; i < Symbol.Parameters.Length; i++)
        {
            var left = Symbol.Parameters[i];
            var right = other.Symbol.Parameters[i];
            if (left.Name != right.Name) return false;
            if (!left.Type.EqualsNamespaceAndName(right.Type)) return false;
        }

        return true;
    }

    public override string ToString()
    {
        return Symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
    }
}
