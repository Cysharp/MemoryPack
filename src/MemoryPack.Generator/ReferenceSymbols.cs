using Microsoft.CodeAnalysis;

namespace MemoryPack.Generator;

public class ReferenceSymbols
{
    public Compilation Compilation { get; }
    public INamedTypeSymbol MemoryPackableAttribute { get; }
    public INamedTypeSymbol MemoryPackUnionAttribute { get; }
    public INamedTypeSymbol MemoryPackConstructorAttribute { get; }
    // public INamedTypeSymbol MemoryPackGenerateAttribute { get; }
    public INamedTypeSymbol MemoryPackFormatterAttribute { get; }
    public INamedTypeSymbol MemoryPackIgnoreAttribute { get; }
    public INamedTypeSymbol MemoryPackIncludeAttribute { get; }
    public INamedTypeSymbol MemoryPackOnSerializingAttribute { get; }
    public INamedTypeSymbol MemoryPackOnSerializedAttribute { get; }
    public INamedTypeSymbol MemoryPackOnDeserializingAttribute { get; }
    public INamedTypeSymbol MemoryPackOnDeserializedAttribute { get; }
    public INamedTypeSymbol IMemoryPackable { get; }

    public WellKnownTypes KnownTypes { get; }

    public ReferenceSymbols(Compilation compilation)
    {
        Compilation = compilation;
        // MemoryPack
        MemoryPackableAttribute = GetTypeByMetadataName(MemoryPackGenerator.MemoryPackableAttributeFullName);
        MemoryPackUnionAttribute = GetTypeByMetadataName("MemoryPack.MemoryPackUnionAttribute");
        MemoryPackConstructorAttribute = GetTypeByMetadataName("MemoryPack.MemoryPackConstructorAttribute");
        //MemoryPackGenerateAttribute = GetTypeByMetadataName("MemoryPack.MemoryPackGenerateAttribute");
        MemoryPackFormatterAttribute = GetTypeByMetadataName("MemoryPack.MemoryPackFormatterAttribute");
        MemoryPackIgnoreAttribute = GetTypeByMetadataName("MemoryPack.MemoryPackIgnoreAttribute");
        MemoryPackIncludeAttribute = GetTypeByMetadataName("MemoryPack.MemoryPackIncludeAttribute");
        MemoryPackOnSerializingAttribute = GetTypeByMetadataName("MemoryPack.MemoryPackOnSerializing");
        MemoryPackOnSerializedAttribute = GetTypeByMetadataName("MemoryPack.MemoryPackOnSerialized");
        MemoryPackOnDeserializingAttribute = GetTypeByMetadataName("MemoryPack.MemoryPackOnDeserializing");
        MemoryPackOnDeserializedAttribute = GetTypeByMetadataName("MemoryPack.MemoryPackOnDeserialized");
        IMemoryPackable = GetTypeByMetadataName("MemoryPack.IMemoryPackable`1").ConstructUnboundGenericType();
        KnownTypes = new WellKnownTypes(this);
    }

    INamedTypeSymbol GetTypeByMetadataName(string metadataName)
    {
        var symbol = Compilation.GetTypeByMetadataName(metadataName);
        if (symbol == null)
        {
            throw new InvalidOperationException($"Type {metadataName} is not found in compilation.");
        }
        return symbol;
    }

    // UnamnaagedType no need.
    public class WellKnownTypes
    {
        readonly ReferenceSymbols parent;
        public INamedTypeSymbol System_Collections_Generic_IEnumerable_T { get; }
        public INamedTypeSymbol System_Version { get; }
        public INamedTypeSymbol System_Uri { get; }

        readonly HashSet<ITypeSymbol> knownTypes;

        public WellKnownTypes(ReferenceSymbols parent)
        {
            this.parent = parent;
            System_Collections_Generic_IEnumerable_T = GetTypeByMetadataName("System.Collections.Generic.IEnumerable`1").ConstructUnboundGenericType();
            System_Version = GetTypeByMetadataName("System.Version");
            System_Uri = GetTypeByMetadataName("System.Uri");

            knownTypes = new HashSet<ITypeSymbol>(new[]
            {
                System_Collections_Generic_IEnumerable_T,
                System_Version,
                System_Uri
            }, SymbolEqualityComparer.Default);
        }

        public bool Contains(ITypeSymbol symbol)
        {
            if (symbol is INamedTypeSymbol nts && nts.IsGenericType)
            {
                symbol = nts.ConstructUnboundGenericType();
            }

            return knownTypes.Contains(symbol);
        }

        INamedTypeSymbol GetTypeByMetadataName(string metadataName) => parent.GetTypeByMetadataName(metadataName);
    }
}

