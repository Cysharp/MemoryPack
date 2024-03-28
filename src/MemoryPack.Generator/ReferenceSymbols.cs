using Microsoft.CodeAnalysis;
using System.Reflection;
using System.Runtime.InteropServices;

namespace MemoryPack.Generator;

public class ReferenceSymbols
{
    public Compilation Compilation { get; }

    public INamedTypeSymbol MemoryPackableAttribute { get; }
    public INamedTypeSymbol MemoryPackUnionAttribute { get; }
    public INamedTypeSymbol MemoryPackUnionFormatterAttribute { get; }
    public INamedTypeSymbol MemoryPackConstructorAttribute { get; }
    public INamedTypeSymbol MemoryPackAllowSerializeAttribute { get; }
    public INamedTypeSymbol MemoryPackOrderAttribute { get; }
    public INamedTypeSymbol? MemoryPackCustomFormatterAttribute { get; } // Unity is null.
    public INamedTypeSymbol? MemoryPackCustomFormatter2Attribute { get; } // Unity is null.
    public INamedTypeSymbol MemoryPackIgnoreAttribute { get; }
    public INamedTypeSymbol MemoryPackIncludeAttribute { get; }
    public INamedTypeSymbol MemoryPackOnSerializingAttribute { get; }
    public INamedTypeSymbol MemoryPackOnSerializedAttribute { get; }
    public INamedTypeSymbol MemoryPackOnDeserializingAttribute { get; }
    public INamedTypeSymbol MemoryPackOnDeserializedAttribute { get; }
    public INamedTypeSymbol SkipOverwriteDefaultAttribute { get; }
    public INamedTypeSymbol GenerateTypeScriptAttribute { get; }
    public INamedTypeSymbol IMemoryPackable { get; }

    public WellKnownTypes KnownTypes { get; }

    public ReferenceSymbols(Compilation compilation)
    {
        Compilation = compilation;

        // MemoryPack
        MemoryPackableAttribute = GetTypeByMetadataName(MemoryPackGenerator.MemoryPackableAttributeFullName);
        MemoryPackUnionAttribute = GetTypeByMetadataName("MemoryPack.MemoryPackUnionAttribute");
        MemoryPackUnionFormatterAttribute = GetTypeByMetadataName("MemoryPack.MemoryPackUnionFormatterAttribute");
        MemoryPackConstructorAttribute = GetTypeByMetadataName("MemoryPack.MemoryPackConstructorAttribute");
        MemoryPackAllowSerializeAttribute = GetTypeByMetadataName("MemoryPack.MemoryPackAllowSerializeAttribute");
        MemoryPackOrderAttribute = GetTypeByMetadataName("MemoryPack.MemoryPackOrderAttribute");
        MemoryPackCustomFormatterAttribute = compilation.GetTypeByMetadataName("MemoryPack.MemoryPackCustomFormatterAttribute`1")?.ConstructUnboundGenericType();
        MemoryPackCustomFormatter2Attribute = compilation.GetTypeByMetadataName("MemoryPack.MemoryPackCustomFormatterAttribute`2")?.ConstructUnboundGenericType();
        MemoryPackIgnoreAttribute = GetTypeByMetadataName("MemoryPack.MemoryPackIgnoreAttribute");
        MemoryPackIncludeAttribute = GetTypeByMetadataName("MemoryPack.MemoryPackIncludeAttribute");
        MemoryPackOnSerializingAttribute = GetTypeByMetadataName("MemoryPack.MemoryPackOnSerializingAttribute");
        MemoryPackOnSerializedAttribute = GetTypeByMetadataName("MemoryPack.MemoryPackOnSerializedAttribute");
        MemoryPackOnDeserializingAttribute = GetTypeByMetadataName("MemoryPack.MemoryPackOnDeserializingAttribute");
        MemoryPackOnDeserializedAttribute = GetTypeByMetadataName("MemoryPack.MemoryPackOnDeserializedAttribute");
        SkipOverwriteDefaultAttribute = GetTypeByMetadataName("MemoryPack.SuppressDefaultInitialization");
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
        public INamedTypeSymbol System_Collections_Generic_List_T { get; }

        public INamedTypeSymbol System_Guid { get; }
        public INamedTypeSymbol System_Version { get; }
        public INamedTypeSymbol System_Uri { get; }

        public INamedTypeSymbol System_Numerics_BigInteger { get; }
        public INamedTypeSymbol System_TimeZoneInfo { get; }
        public INamedTypeSymbol System_Collections_BitArray { get; }
        public INamedTypeSymbol System_Text_StringBuilder { get; }
        public INamedTypeSymbol System_Type { get; }
        public INamedTypeSymbol System_Globalization_CultureInfo { get; }
        public INamedTypeSymbol System_Lazy_T { get; }
        public INamedTypeSymbol System_Collections_Generic_KeyValuePair_T { get; }
        public INamedTypeSymbol System_Nullable_T { get; }

        public INamedTypeSymbol System_DateTime { get; }
        public INamedTypeSymbol System_DateTimeOffset { get; }
        public INamedTypeSymbol System_Runtime_InteropServices_StructLayout { get; }

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

        static readonly Dictionary<string, string> knownGenericTypes = new()
        {
            // ArrayFormatters
            { "System.ArraySegment<>", "global::MemoryPack.Formatters.ArraySegmentFormatter<TREPLACE>" },
            { "System.Memory<>", "global::MemoryPack.Formatters.MemoryFormatter<TREPLACE>" },
            { "System.ReadOnlyMemory<>", "global::MemoryPack.Formatters.ReadOnlyMemoryFormatter<TREPLACE>" },
            { "System.Buffers.ReadOnlySequence<>", "global::MemoryPack.Formatters.ReadOnlySequenceFormatter<TREPLACE>" },

            // CollectionFormatters
            { "System.Collections.Generic.List<>", "global::MemoryPack.Formatters.ListFormatter<TREPLACE>" },
            { "System.Collections.Generic.Stack<>", "global::MemoryPack.Formatters.StackFormatter<TREPLACE>" },
            { "System.Collections.Generic.Queue<>", "global::MemoryPack.Formatters.QueueFormatter<TREPLACE>" },
            { "System.Collections.Generic.LinkedList<>", "global::MemoryPack.Formatters.LinkedListFormatter<TREPLACE>" },
            { "System.Collections.Generic.HashSet<>", "global::MemoryPack.Formatters.HashSetFormatter<TREPLACE>" },
            { "System.Collections.Generic.SortedSet<>", "global::MemoryPack.Formatters.SortedSetFormatter<TREPLACE>" },
            { "System.Collections.Generic.PriorityQueue<,>", "global::MemoryPack.Formatters.PriorityQueueFormatter<TREPLACE>" },
            { "System.Collections.ObjectModel.ObservableCollection<>", "global::MemoryPack.Formatters.ObservableCollectionFormatter<TREPLACE>" },
            { "System.Collections.ObjectModel.Collection<>", "global::MemoryPack.Formatters.CollectionFormatter<TREPLACE>" },
            { "System.Collections.Concurrent.ConcurrentQueue<>", "global::MemoryPack.Formatters.ConcurrentQueueFormatter<TREPLACE>" },
            { "System.Collections.Concurrent.ConcurrentStack<>", "global::MemoryPack.Formatters.ConcurrentStackFormatter<TREPLACE>" },
            { "System.Collections.Concurrent.ConcurrentBag<>", "global::MemoryPack.Formatters.ConcurrentBagFormatter<TREPLACE>" },
            { "System.Collections.Generic.Dictionary<,>", "global::MemoryPack.Formatters.DictionaryFormatter<TREPLACE>" },
            { "System.Collections.Generic.SortedDictionary<,>", "global::MemoryPack.Formatters.SortedDictionaryFormatter<TREPLACE>" },
            { "System.Collections.Generic.SortedList<,>", "global::MemoryPack.Formatters.SortedListFormatter<TREPLACE>" },
            { "System.Collections.Concurrent.ConcurrentDictionary<,>", "global::MemoryPack.Formatters.ConcurrentDictionaryFormatter<TREPLACE>" },
            { "System.Collections.ObjectModel.ReadOnlyCollection<>", "global::MemoryPack.Formatters.ReadOnlyCollectionFormatter<TREPLACE>" },
            { "System.Collections.ObjectModel.ReadOnlyObservableCollection<>", "global::MemoryPack.Formatters.ReadOnlyObservableCollectionFormatter<TREPLACE>" },
            { "System.Collections.Concurrent.BlockingCollection<>", "global::MemoryPack.Formatters.BlockingCollectionFormatter<TREPLACE>" },

            // ImmutableCollectionFormatters
            { "System.Collections.Immutable.ImmutableArray<>", "global::MemoryPack.Formatters.ImmutableArrayFormatter<TREPLACE>" },
            { "System.Collections.Immutable.ImmutableList<>", "global::MemoryPack.Formatters.ImmutableListFormatter<TREPLACE>" },
            { "System.Collections.Immutable.ImmutableQueue<>", "global::MemoryPack.Formatters.ImmutableQueueFormatter<TREPLACE>" },
            { "System.Collections.Immutable.ImmutableStack<>", "global::MemoryPack.Formatters.ImmutableStackFormatter<TREPLACE>" },
            { "System.Collections.Immutable.ImmutableDictionary<,>", "global::MemoryPack.Formatters.ImmutableDictionaryFormatter<TREPLACE>" },
            { "System.Collections.Immutable.ImmutableSortedDictionary<,>", "global::MemoryPack.Formatters.ImmutableSortedDictionaryFormatter<TREPLACE>" },
            { "System.Collections.Immutable.ImmutableSortedSet<>", "global::MemoryPack.Formatters.ImmutableSortedSetFormatter<TREPLACE>" },
            { "System.Collections.Immutable.ImmutableHashSet<>", "global::MemoryPack.Formatters.ImmutableHashSetFormatter<TREPLACE>" },
            { "System.Collections.Immutable.IImmutableList<>", "global::MemoryPack.Formatters.InterfaceImmutableListFormatter<TREPLACE>" },
            { "System.Collections.Immutable.IImmutableQueue<>", "global::MemoryPack.Formatters.InterfaceImmutableQueueFormatter<TREPLACE>" },
            { "System.Collections.Immutable.IImmutableStack<>", "global::MemoryPack.Formatters.InterfaceImmutableStackFormatter<TREPLACE>" },
            { "System.Collections.Immutable.IImmutableDictionary<,>", "global::MemoryPack.Formatters.InterfaceImmutableDictionaryFormatter<TREPLACE>" },
            { "System.Collections.Immutable.IImmutableSet<>", "global::MemoryPack.Formatters.InterfaceImmutableSetFormatter<TREPLACE>" },

            // InterfaceCollectionFormatters
            { "System.Collections.Generic.IEnumerable<>", "global::MemoryPack.Formatters.InterfaceEnumerableFormatter<TREPLACE>" },
            { "System.Collections.Generic.ICollection<>", "global::MemoryPack.Formatters.InterfaceCollectionFormatter<TREPLACE>" },
            { "System.Collections.Generic.IReadOnlyCollection<>", "global::MemoryPack.Formatters.InterfaceReadOnlyCollectionFormatter<TREPLACE>" },
            { "System.Collections.Generic.IList<>", "global::MemoryPack.Formatters.InterfaceListFormatter<TREPLACE>" },
            { "System.Collections.Generic.IReadOnlyList<>", "global::MemoryPack.Formatters.InterfaceReadOnlyListFormatter<TREPLACE>" },
            { "System.Collections.Generic.IDictionary<,>", "global::MemoryPack.Formatters.InterfaceDictionaryFormatter<TREPLACE>" },
            { "System.Collections.Generic.IReadOnlyDictionary<,>", "global::MemoryPack.Formatters.InterfaceReadOnlyDictionaryFormatter<TREPLACE>" },
            { "System.Linq.ILookup<,>", "global::MemoryPack.Formatters.InterfaceLookupFormatter<TREPLACE>" },
            { "System.Linq.IGrouping<,>", "global::MemoryPack.Formatters.InterfaceGroupingFormatter<TREPLACE>" },
            { "System.Collections.Generic.ISet<>", "global::MemoryPack.Formatters.InterfaceSetFormatter<TREPLACE>" },
            { "System.Collections.Generic.IReadOnlySet<>", "global::MemoryPack.Formatters.InterfaceReadOnlySetFormatter<TREPLACE>" },

            { "System.Collections.Generic.KeyValuePair<,>", "global::MemoryPack.Formatters.KeyValuePairFormatter<TREPLACE>" },
            { "System.Lazy<>", "global::MemoryPack.Formatters.LazyFormatter<TREPLACE>" },

            // TupleFormatters
            { "System.Tuple<>", "global::MemoryPack.Formatters.TupleFormatter<TREPLACE>" },
            { "System.Tuple<,>", "global::MemoryPack.Formatters.TupleFormatter<TREPLACE>" },
            { "System.Tuple<,,>", "global::MemoryPack.Formatters.TupleFormatter<TREPLACE>" },
            { "System.Tuple<,,,>", "global::MemoryPack.Formatters.TupleFormatter<TREPLACE>" },
            { "System.Tuple<,,,,>", "global::MemoryPack.Formatters.TupleFormatter<TREPLACE>" },
            { "System.Tuple<,,,,,>", "global::MemoryPack.Formatters.TupleFormatter<TREPLACE>" },
            { "System.Tuple<,,,,,,>", "global::MemoryPack.Formatters.TupleFormatter<TREPLACE>" },
            { "System.Tuple<,,,,,,,>", "global::MemoryPack.Formatters.TupleFormatter<TREPLACE>" },
            { "System.ValueTuple<>", "global::MemoryPack.Formatters.ValueTupleFormatter<TREPLACE>" },
            { "System.ValueTuple<,>", "global::MemoryPack.Formatters.ValueTupleFormatter<TREPLACE>" },
            { "System.ValueTuple<,,>", "global::MemoryPack.Formatters.ValueTupleFormatter<TREPLACE>" },
            { "System.ValueTuple<,,,>", "global::MemoryPack.Formatters.ValueTupleFormatter<TREPLACE>" },
            { "System.ValueTuple<,,,,>", "global::MemoryPack.Formatters.ValueTupleFormatter<TREPLACE>" },
            { "System.ValueTuple<,,,,,>", "global::MemoryPack.Formatters.ValueTupleFormatter<TREPLACE>" },
            { "System.ValueTuple<,,,,,,>", "global::MemoryPack.Formatters.ValueTupleFormatter<TREPLACE>" },
            { "System.ValueTuple<,,,,,,,>", "global::MemoryPack.Formatters.ValueTupleFormatter<TREPLACE>" },
        };

        public WellKnownTypes(ReferenceSymbols parent)
        {
            this.parent = parent;
            System_Collections_Generic_IEnumerable_T = GetTypeByMetadataName("System.Collections.Generic.IEnumerable`1").ConstructUnboundGenericType();
            System_Collections_Generic_ICollection_T = GetTypeByMetadataName("System.Collections.Generic.ICollection`1").ConstructUnboundGenericType();
            System_Collections_Generic_ISet_T = GetTypeByMetadataName("System.Collections.Generic.ISet`1").ConstructUnboundGenericType();
            System_Collections_Generic_IDictionary_T = GetTypeByMetadataName("System.Collections.Generic.IDictionary`2").ConstructUnboundGenericType();
            System_Collections_Generic_List_T = GetTypeByMetadataName("System.Collections.Generic.List`1").ConstructUnboundGenericType();
            System_Guid = GetTypeByMetadataName("System.Guid");
            System_Version = GetTypeByMetadataName("System.Version");
            System_Uri = GetTypeByMetadataName("System.Uri");
            System_Numerics_BigInteger = GetTypeByMetadataName("System.Numerics.BigInteger");
            System_TimeZoneInfo = GetTypeByMetadataName("System.TimeZoneInfo");
            System_Collections_BitArray = GetTypeByMetadataName("System.Collections.BitArray");
            System_Text_StringBuilder = GetTypeByMetadataName("System.Text.StringBuilder");
            System_Type = GetTypeByMetadataName("System.Type");
            System_Globalization_CultureInfo = GetTypeByMetadataName("System.Globalization.CultureInfo");
            System_Lazy_T = GetTypeByMetadataName("System.Lazy`1").ConstructUnboundGenericType();
            System_Collections_Generic_KeyValuePair_T = GetTypeByMetadataName("System.Collections.Generic.KeyValuePair`2").ConstructUnboundGenericType();
            System_Nullable_T = GetTypeByMetadataName("System.Nullable`1").ConstructUnboundGenericType();
            //System_Memory_T = GetTypeByMetadataName("System.Memory").ConstructUnboundGenericType();
            //System_ReadOnlyMemory_T = GetTypeByMetadataName("System.ReadOnlyMemory").ConstructUnboundGenericType();
            //System_Buffers_ReadOnlySequence_T = GetTypeByMetadataName("System.Buffers.ReadOnlySequence").ConstructUnboundGenericType();
            //System_Collections_Generic_PriorityQueue_T = GetTypeByMetadataName("System.Collections.Generic.PriorityQueue").ConstructUnboundGenericType();

            System_DateTime = GetTypeByMetadataName("System.DateTime");
            System_DateTimeOffset = GetTypeByMetadataName("System.DateTimeOffset");
            System_Runtime_InteropServices_StructLayout = GetTypeByMetadataName("System.Runtime.InteropServices.StructLayoutAttribute");

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
                System_Globalization_CultureInfo,
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

        public string? GetNonDefaultFormatterName(ITypeSymbol? type)
        {
            if (type == null) return null;

            if (type.TypeKind == TypeKind.Enum)
            {
                return $"global::MemoryPack.Formatters.UnmanagedFormatter<{type.FullyQualifiedToString()}>";
            }

            if (type.TypeKind == TypeKind.Array)
            {
                if (type is IArrayTypeSymbol array)
                {
                    if (array.IsSZArray)
                    {
                        return $"global::MemoryPack.Formatters.ArrayFormatter<{array.ElementType.FullyQualifiedToString()}>";
                    }
                    else
                    {
                        if (array.Rank == 2)
                        {
                            return $"global::MemoryPack.Formatters.TwoDimensionalArrayFormatter<{array.ElementType.FullyQualifiedToString()}>";
                        }
                        else if (array.Rank == 3)
                        {
                            return $"global::MemoryPack.Formatters.ThreeDimensionalArrayFormatter<{array.ElementType.FullyQualifiedToString()}>";
                        }
                        else if (array.Rank == 4)
                        {
                            return $"global::MemoryPack.Formatters.FourDimensionalArrayFormatter<{array.ElementType.FullyQualifiedToString()}>";
                        }
                    }
                }

                return null;
            }

            if (type is not INamedTypeSymbol named) return null;

            if (!named.IsGenericType) return null;

            var genericType = named.ConstructUnboundGenericType();
            var genericTypeString = genericType.ToDisplayString();
            var fullName = type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            // var isOpenGenericType = type.TypeArguments.Any(x => x is ITypeParameterSymbol);

            // nullable
            if (genericTypeString == "T?")
            {
                var firstTypeArgument = named.TypeArguments[0];
                var f = "global::MemoryPack.Formatters.NullableFormatter<" + firstTypeArgument.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) + ">";
                return f;
            }

            // known types
            if (knownGenericTypes.TryGetValue(genericTypeString, out var formatter))
            {
                var typeArgs = string.Join(", ", named.TypeArguments.Select(x => x.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)));
                var f = formatter.Replace("TREPLACE", typeArgs);
                return f;
            }

            return null;
        }

        INamedTypeSymbol GetTypeByMetadataName(string metadataName) => parent.GetTypeByMetadataName(metadataName);
    }
}

