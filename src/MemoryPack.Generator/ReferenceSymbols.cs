using Microsoft.CodeAnalysis;

namespace MemoryPack.Generator;

public class ReferenceSymbols
{
    public Compilation Compilation { get; }
    public INamedTypeSymbol MemoryPack_MemoryPackableAttribute { get; }
    public INamedTypeSymbol MemoryPack_MemoryPackUnionAttribute { get; }
    public INamedTypeSymbol MemoryPack_MemoryPackConstructorAttribute { get; }
    public INamedTypeSymbol MemoryPack_MemoryPackIgnoreAttribute { get; }
    public INamedTypeSymbol MemoryPack_MemoryPackOnSerializingAttribute { get; }
    public INamedTypeSymbol MemoryPack_MemoryPackOnSerializedAttribute { get; }
    public INamedTypeSymbol MemoryPack_MemoryPackOnDeserializingAttribute { get; }
    public INamedTypeSymbol MemoryPack_MemoryPackOnDeserializedAttribute { get; }
    public INamedTypeSymbol MemoryPack_IMemoryPackable { get; }

    public ReferenceSymbols(Compilation compilation)
    {
        Compilation = compilation;
        // MemoryPack
        MemoryPack_MemoryPackableAttribute = GetTypeByMetadataName(MemoryPackGenerator.MemoryPackableAttributeFullName);
        MemoryPack_MemoryPackUnionAttribute = GetTypeByMetadataName("MemoryPack.MemoryPackUnionAttribute");
        MemoryPack_MemoryPackConstructorAttribute = GetTypeByMetadataName("MemoryPack.MemoryPackConstructorAttribute");
        MemoryPack_MemoryPackIgnoreAttribute = GetTypeByMetadataName("MemoryPack.MemoryPackIgnoreAttribute");
        MemoryPack_MemoryPackOnSerializingAttribute = GetTypeByMetadataName("MemoryPack.MemoryPackOnSerializing");
        MemoryPack_MemoryPackOnSerializedAttribute = GetTypeByMetadataName("MemoryPack.MemoryPackOnSerialized");
        MemoryPack_MemoryPackOnDeserializingAttribute = GetTypeByMetadataName("MemoryPack.MemoryPackOnDeserializing");
        MemoryPack_MemoryPackOnDeserializedAttribute = GetTypeByMetadataName("MemoryPack.MemoryPackOnDeserialized");
        MemoryPack_IMemoryPackable = GetTypeByMetadataName("MemoryPack.IMemoryPackable`1").ConstructUnboundGenericType();
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

}
