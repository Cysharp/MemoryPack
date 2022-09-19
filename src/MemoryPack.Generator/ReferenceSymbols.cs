using Microsoft.CodeAnalysis;

namespace MemoryPack.Generator;

public class ReferenceSymbols
{
    public Compilation Compilation { get; }
    public INamedTypeSymbol MemoryPackableAttribute { get; }
    public INamedTypeSymbol MemoryPackUnionAttribute { get; }
    public INamedTypeSymbol MemoryPackConstructorAttribute { get; }
    public INamedTypeSymbol MemoryPackIgnoreAttribute { get; }
    public INamedTypeSymbol MemoryPackIncludeAttribute { get; }
    public INamedTypeSymbol MemoryPackOnSerializingAttribute { get; }
    public INamedTypeSymbol MemoryPackOnSerializedAttribute { get; }
    public INamedTypeSymbol MemoryPackOnDeserializingAttribute { get; }
    public INamedTypeSymbol MemoryPackOnDeserializedAttribute { get; }
    public INamedTypeSymbol IMemoryPackable { get; }

    public ReferenceSymbols(Compilation compilation)
    {
        Compilation = compilation;
        // MemoryPack
        MemoryPackableAttribute = GetTypeByMetadataName(MemoryPackGenerator.MemoryPackableAttributeFullName);
        MemoryPackUnionAttribute = GetTypeByMetadataName("MemoryPack.MemoryPackUnionAttribute");
        MemoryPackConstructorAttribute = GetTypeByMetadataName("MemoryPack.MemoryPackConstructorAttribute");
        MemoryPackIgnoreAttribute = GetTypeByMetadataName("MemoryPack.MemoryPackIgnoreAttribute");
        MemoryPackIncludeAttribute = GetTypeByMetadataName("MemoryPack.MemoryPackIncludeAttribute");
        MemoryPackOnSerializingAttribute = GetTypeByMetadataName("MemoryPack.MemoryPackOnSerializing");
        MemoryPackOnSerializedAttribute = GetTypeByMetadataName("MemoryPack.MemoryPackOnSerialized");
        MemoryPackOnDeserializingAttribute = GetTypeByMetadataName("MemoryPack.MemoryPackOnDeserializing");
        MemoryPackOnDeserializedAttribute = GetTypeByMetadataName("MemoryPack.MemoryPackOnDeserialized");
        IMemoryPackable = GetTypeByMetadataName("MemoryPack.IMemoryPackable`1").ConstructUnboundGenericType();
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
