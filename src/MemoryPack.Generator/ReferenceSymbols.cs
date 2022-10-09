using Microsoft.CodeAnalysis;
using System.Reflection;

namespace MemoryPack.Generator;

public class ReferenceSymbols
{
    public Compilation Compilation { get; }

    public INamedTypeSymbol MemoryPackableAttribute { get; }
    public INamedTypeSymbol MemoryPackUnionAttribute { get; }
    public INamedTypeSymbol MemoryPackConstructorAttribute { get; }
    public INamedTypeSymbol MemoryPackAllowSerializeAttribute { get; }
    public INamedTypeSymbol MemoryPackOrderAttribute { get; }
    public INamedTypeSymbol MemoryPackIgnoreAttribute { get; }
    public INamedTypeSymbol MemoryPackIncludeAttribute { get; }
    public INamedTypeSymbol MemoryPackOnSerializingAttribute { get; }
    public INamedTypeSymbol MemoryPackOnSerializedAttribute { get; }
    public INamedTypeSymbol MemoryPackOnDeserializingAttribute { get; }
    public INamedTypeSymbol MemoryPackOnDeserializedAttribute { get; }
    public INamedTypeSymbol GenerateTypeScriptAttribute { get; }
    public INamedTypeSymbol IMemoryPackable { get; }

    public WellKnownTypes KnownTypes { get; }

    public ReferenceSymbols(Compilation compilation)
    {
        Compilation = compilation;

        // MemoryPack
        MemoryPackableAttribute = GetTypeByMetadataName(MemoryPackGenerator.MemoryPackableAttributeFullName);
        MemoryPackUnionAttribute = GetTypeByMetadataName("MemoryPack.MemoryPackUnionAttribute");
        MemoryPackConstructorAttribute = GetTypeByMetadataName("MemoryPack.MemoryPackConstructorAttribute");
        MemoryPackAllowSerializeAttribute = GetTypeByMetadataName("MemoryPack.MemoryPackAllowSerializeAttribute");
        MemoryPackOrderAttribute = GetTypeByMetadataName("MemoryPack.MemoryPackOrderAttribute");
        MemoryPackIgnoreAttribute = GetTypeByMetadataName("MemoryPack.MemoryPackIgnoreAttribute");
        MemoryPackIncludeAttribute = GetTypeByMetadataName("MemoryPack.MemoryPackIncludeAttribute");
        MemoryPackOnSerializingAttribute = GetTypeByMetadataName("MemoryPack.MemoryPackOnSerializingAttribute");
        MemoryPackOnSerializedAttribute = GetTypeByMetadataName("MemoryPack.MemoryPackOnSerializedAttribute");
        MemoryPackOnDeserializingAttribute = GetTypeByMetadataName("MemoryPack.MemoryPackOnDeserializingAttribute");
        MemoryPackOnDeserializedAttribute = GetTypeByMetadataName("MemoryPack.MemoryPackOnDeserializedAttribute");
        GenerateTypeScriptAttribute = GetTypeByMetadataName(MemoryPackGenerator.GenerateTypeScriptAttributeFullName);
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
        public INamedTypeSymbol System_Collections_Generic_ICollection_T { get; }
        public INamedTypeSymbol System_Collections_Generic_ISet_T { get; }
        public INamedTypeSymbol System_Collections_Generic_IDictionary_T { get; }

        public INamedTypeSymbol System_Version { get; }
        public INamedTypeSymbol System_Uri { get; }

        public INamedTypeSymbol System_Numerics_BigInteger { get; }
        public INamedTypeSymbol System_TimeZoneInfo { get; }
        public INamedTypeSymbol System_Collections_BitArray { get; }
        public INamedTypeSymbol System_Text_StringBuilder { get; }
        public INamedTypeSymbol System_Type { get; }
        public INamedTypeSymbol System_Lazy_T { get; }
        public INamedTypeSymbol System_Collections_Generic_KeyValuePair_T { get; }
        public INamedTypeSymbol System_Nullable_T { get; }

        // netstandard2.0 source generator has there reference so use string instead...
        //public INamedTypeSymbol System_Memory_T { get; }
        //public INamedTypeSymbol System_ReadOnlyMemory_T { get; }
        //public INamedTypeSymbol System_Buffers_ReadOnlySequence_T { get; }
        //public INamedTypeSymbol System_Collections_Generic_PriorityQueue_T { get; }
        const string System_Memory_T = "global::System.Memory<>";
        const string System_ReadOnlyMemory_T = "global::System.ReadOnlyMemory<>";
        const string System_Buffers_ReadOnlySequence_T = "global::System.Buffers.ReadOnlySequence<>";
        const string System_Collections_Generic_PriorityQueue_T = "global::System.Collections.Generic.PriorityQueue<,>";

        readonly HashSet<ITypeSymbol> knownTypes;

        public WellKnownTypes(ReferenceSymbols parent)
        {
            this.parent = parent;
            System_Collections_Generic_IEnumerable_T = GetTypeByMetadataName("System.Collections.Generic.IEnumerable`1").ConstructUnboundGenericType();
            System_Collections_Generic_ICollection_T = GetTypeByMetadataName("System.Collections.Generic.ICollection`1").ConstructUnboundGenericType();
            System_Collections_Generic_ISet_T = GetTypeByMetadataName("System.Collections.Generic.ISet`1").ConstructUnboundGenericType();
            System_Collections_Generic_IDictionary_T = GetTypeByMetadataName("System.Collections.Generic.IDictionary`2").ConstructUnboundGenericType();
            System_Version = GetTypeByMetadataName("System.Version");
            System_Uri = GetTypeByMetadataName("System.Uri");
            System_Numerics_BigInteger = GetTypeByMetadataName("System.Numerics.BigInteger");
            System_TimeZoneInfo = GetTypeByMetadataName("System.TimeZoneInfo");
            System_Collections_BitArray = GetTypeByMetadataName("System.Collections.BitArray");
            System_Text_StringBuilder = GetTypeByMetadataName("System.Text.StringBuilder");
            System_Type = GetTypeByMetadataName("System.Type");
            System_Lazy_T = GetTypeByMetadataName("System.Lazy`1").ConstructUnboundGenericType();
            System_Collections_Generic_KeyValuePair_T = GetTypeByMetadataName("System.Collections.Generic.KeyValuePair`2").ConstructUnboundGenericType();
            System_Nullable_T = GetTypeByMetadataName("System.Nullable`1").ConstructUnboundGenericType();
            //System_Memory_T = GetTypeByMetadataName("System.Memory").ConstructUnboundGenericType();
            //System_ReadOnlyMemory_T = GetTypeByMetadataName("System.ReadOnlyMemory").ConstructUnboundGenericType();
            //System_Buffers_ReadOnlySequence_T = GetTypeByMetadataName("System.Buffers.ReadOnlySequence").ConstructUnboundGenericType();
            //System_Collections_Generic_PriorityQueue_T = GetTypeByMetadataName("System.Collections.Generic.PriorityQueue").ConstructUnboundGenericType();

            knownTypes = new HashSet<ITypeSymbol>(new[]
            {
                System_Collections_Generic_IEnumerable_T,
                System_Collections_Generic_ICollection_T,
                System_Collections_Generic_ISet_T,
                System_Collections_Generic_IDictionary_T,
                System_Version,
                System_Uri,
                System_Numerics_BigInteger,
                System_TimeZoneInfo,
                System_Collections_BitArray,
                System_Text_StringBuilder,
                System_Type,
                System_Lazy_T,
                System_Collections_Generic_KeyValuePair_T,
                System_Nullable_T,
                //System_Memory_T,
                //System_ReadOnlyMemory_T,
                //System_Buffers_ReadOnlySequence_T,
                //System_Collections_Generic_PriorityQueue_T
            }, SymbolEqualityComparer.Default);
        }

        public bool Contains(ITypeSymbol symbol)
        {
            var constructedSymbol = symbol;
            if (symbol is INamedTypeSymbol nts && nts.IsGenericType)
            {
                symbol = nts.ConstructUnboundGenericType();
            }

            var contains1 = knownTypes.Contains(symbol);
            if (contains1) return true;

            var fullyQualifiedString = symbol.FullyQualifiedToString();
            if (fullyQualifiedString is System_Memory_T or System_ReadOnlyMemory_T or System_Buffers_ReadOnlySequence_T or System_Collections_Generic_PriorityQueue_T)
            {
                return true;
            }

            // tuple
            if (fullyQualifiedString.StartsWith("global::System.Tuple<") || fullyQualifiedString.StartsWith("global::System.ValueTuple<"))
            {
                return true;
            }

            // Most collections are basically serializable, wellknown
            var isIterable = constructedSymbol.AllInterfaces.Any(x => x.EqualsUnconstructedGenericType(System_Collections_Generic_IEnumerable_T));
            if (isIterable)
            {
                return true;
            }

            return false;
        }

        INamedTypeSymbol GetTypeByMetadataName(string metadataName) => parent.GetTypeByMetadataName(metadataName);
    }
}

