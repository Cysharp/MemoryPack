using MemoryPack.Formatters;
using MemoryPack.Internal;
using System.Buffers;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// interface collection formatters
// IEnumerable, ICollection, IReadOnlyCollection, IList, IReadOnlyList
// IDictionary, IReadOnlyDictionary, ILookup, IGrouping, ISet, IReadOnlySet

namespace MemoryPack
{
    public static partial class MemoryPackFormatterProvider
    {
        static readonly Dictionary<Type, Type> InterfaceCollectionFormatters = new(11)
        {
            { typeof(IEnumerable<>), typeof(InterfaceEnumerableFormatter<>) },
            { typeof(ICollection<>), typeof(InterfaceCollectionFormatter<>) },
            { typeof(IReadOnlyCollection<>), typeof(InterfaceReadOnlyCollectionFormatter<>) },
            { typeof(IList<>), typeof(InterfaceListFormatter<>) },
            { typeof(IReadOnlyList<>), typeof(InterfaceReadOnlyListFormatter<>) },
            { typeof(IDictionary<,>), typeof(InterfaceDictionaryFormatter<,>) },
            { typeof(IReadOnlyDictionary<,>), typeof(InterfaceReadOnlyDictionaryFormatter<,>) },
            { typeof(ILookup<,>), typeof(InterfaceLookupFormatter<,>) },
            { typeof(IGrouping<,>), typeof(InterfaceGroupingFormatter<,>) },
            { typeof(ISet<>), typeof(InterfaceSetFormatter<>) },
#if NET7_0_OR_GREATER
            { typeof(IReadOnlySet<>), typeof(InterfaceReadOnlySetFormatter<>) },
#endif
        };
    }
}

namespace MemoryPack.Formatters
{
    using static InterfaceCollectionFormatterUtils;

    file static class InterfaceCollectionFormatterUtils
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TrySerializeOptimized<TBufferWriter, TCollection, TElement>(ref MemoryPackWriter<TBufferWriter> writer, [NotNullWhen(false)] scoped ref TCollection? value)
            where TCollection : IEnumerable<TElement>
#if NET7_0_OR_GREATER
            where TBufferWriter : IBufferWriter<byte>
#else
            where TBufferWriter : class, IBufferWriter<byte>
#endif
        {
            if (value == null)
            {
                writer.WriteNullCollectionHeader();
                return true;
            }

            // optimize for list or array

            if (value is TElement?[] array)
            {
                writer.WriteArray(array);
                return true;
            }

#if NET7_0_OR_GREATER
            if (value is List<TElement?> list)
            {
                writer.WriteSpan(CollectionsMarshal.AsSpan(list));
                return true;
            }
#endif

            return false;
        }

        public static void SerializeCollection<TBufferWriter, TCollection, TElement>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref TCollection? value)
            where TCollection : ICollection<TElement>
#if NET7_0_OR_GREATER
            where TBufferWriter : IBufferWriter<byte>
#else
            where TBufferWriter : class, IBufferWriter<byte>
#endif
        {
            if (TrySerializeOptimized<TBufferWriter, TCollection, TElement>(ref writer, ref value)) return;

            var formatter = writer.GetFormatter<TElement>();
            writer.WriteCollectionHeader(value.Count);
            foreach (var item in value)
            {
                var v = item;
                formatter.Serialize(ref writer, ref v!);
            }
        }

        public static void SerializeReadOnlyCollection<TBufferWriter, TCollection, TElement>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref TCollection? value)
            where TCollection : IReadOnlyCollection<TElement>
#if NET7_0_OR_GREATER
            where TBufferWriter : IBufferWriter<byte>
#else
            where TBufferWriter : class, IBufferWriter<byte>
#endif
        {
            if (TrySerializeOptimized<TBufferWriter, TCollection, TElement>(ref writer, ref value)) return;

            var formatter = writer.GetFormatter<TElement>();
            writer.WriteCollectionHeader(value.Count);
            foreach (var item in value)
            {
                var v = item;
                formatter.Serialize(ref writer, ref v!);
            }
        }

        public static List<T?>? ReadList<T>(ref MemoryPackReader reader)
        {
            var formatter = reader.GetFormatter<List<T?>>();
            List<T?>? v = default;
            formatter.Deserialize(ref reader, ref v);
            return v;
        }
    }

    public sealed class InterfaceEnumerableFormatter<T> : MemoryPackFormatter<IEnumerable<T?>>
    {
        public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref IEnumerable<T?>? value)
        {
            if (TrySerializeOptimized<TBufferWriter, IEnumerable<T?>, T?>(ref writer, ref value)) return;

            if (value.TryGetNonEnumeratedCountEx(out var count))
            {
                var formatter = writer.GetFormatter<T?>();
                writer.WriteCollectionHeader(count);
                foreach (var item in value)
                {
                    var v = item;
                    formatter.Serialize(ref writer, ref v);
                }
            }
            else
            {
                // write to tempbuffer(because we don't know length so can't write header)
                var tempBuffer = ReusableLinkedArrayBufferWriterPool.Rent();
                try
                {
                    var tempWriter = new MemoryPackWriter<ReusableLinkedArrayBufferWriter>(ref tempBuffer, writer.Options);

                    count = 0;
                    var formatter = writer.GetFormatter<T?>();
                    foreach (var item in value)
                    {
                        count++;
                        var v = item;
                        formatter.Serialize(ref tempWriter, ref v);
                    }

                    tempWriter.Flush();

                    // write to parameter writer.
                    writer.WriteCollectionHeader(count);
                    tempBuffer.WriteToAndReset(ref writer);
                }
                finally
                {
                    ReusableLinkedArrayBufferWriterPool.Return(tempBuffer);
                }
            }
        }

        public override void Deserialize(ref MemoryPackReader reader, scoped ref IEnumerable<T?>? value)
        {
            value = reader.ReadArray<T?>();
        }
    }

    public sealed class InterfaceCollectionFormatter<T> : MemoryPackFormatter<ICollection<T?>>
    {
        static InterfaceCollectionFormatter()
        {
            if (!MemoryPackFormatterProvider.IsRegistered<List<T?>>())
            {
                MemoryPackFormatterProvider.Register(new ListFormatter<T>());
            }
        }

        public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref ICollection<T?>? value)
        {
            SerializeCollection<TBufferWriter, ICollection<T?>, T?>(ref writer, ref value);
        }

        public override void Deserialize(ref MemoryPackReader reader, scoped ref ICollection<T?>? value)
        {
            value = ReadList<T?>(ref reader);
        }
    }

    public sealed class InterfaceReadOnlyCollectionFormatter<T> : MemoryPackFormatter<IReadOnlyCollection<T?>>
    {
        static InterfaceReadOnlyCollectionFormatter()
        {
            if (!MemoryPackFormatterProvider.IsRegistered<List<T?>>())
            {
                MemoryPackFormatterProvider.Register(new ListFormatter<T>());
            }
        }

        public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref IReadOnlyCollection<T?>? value)
        {
            SerializeReadOnlyCollection<TBufferWriter, IReadOnlyCollection<T?>, T?>(ref writer, ref value);
        }

        public override void Deserialize(ref MemoryPackReader reader, scoped ref IReadOnlyCollection<T?>? value)
        {
            value = ReadList<T?>(ref reader);
        }
    }

    public sealed class InterfaceListFormatter<T> : MemoryPackFormatter<IList<T?>>
    {
        static InterfaceListFormatter()
        {
            if (!MemoryPackFormatterProvider.IsRegistered<List<T?>>())
            {
                MemoryPackFormatterProvider.Register(new ListFormatter<T>());
            }
        }

        public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref IList<T?>? value)
        {
            SerializeCollection<TBufferWriter, IList<T?>, T?>(ref writer, ref value);
        }

        public override void Deserialize(ref MemoryPackReader reader, scoped ref IList<T?>? value)
        {
            value = ReadList<T?>(ref reader);
        }
    }

    public sealed class InterfaceReadOnlyListFormatter<T> : MemoryPackFormatter<IReadOnlyList<T?>>
    {
        static InterfaceReadOnlyListFormatter()
        {
            if (!MemoryPackFormatterProvider.IsRegistered<List<T?>>())
            {
                MemoryPackFormatterProvider.Register(new ListFormatter<T>());
            }
        }

        public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref IReadOnlyList<T?>? value)
        {
            SerializeReadOnlyCollection<TBufferWriter, IReadOnlyList<T?>, T?>(ref writer, ref value);
        }

        public override void Deserialize(ref MemoryPackReader reader, scoped ref IReadOnlyList<T?>? value)
        {
            value = ReadList<T?>(ref reader);
        }
    }

    public sealed class InterfaceDictionaryFormatter<TKey, TValue> : MemoryPackFormatter<IDictionary<TKey, TValue>>
        where TKey : notnull
    {
        static InterfaceDictionaryFormatter()
        {
            if (!MemoryPackFormatterProvider.IsRegistered<KeyValuePair<TKey, TValue>>())
            {
                MemoryPackFormatterProvider.Register(new KeyValuePairFormatter<TKey, TValue>());
            }
        }

        public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref IDictionary<TKey, TValue>? value)
        {
            if (value == null)
            {
                writer.WriteNullCollectionHeader();
                return;
            }

            var formatter = writer.GetFormatter<KeyValuePair<TKey, TValue>>();
            writer.WriteCollectionHeader(value.Count);
            foreach (var item in value)
            {
                var v = item;
                formatter.Serialize(ref writer, ref v);
            }
        }

        public override void Deserialize(ref MemoryPackReader reader, scoped ref IDictionary<TKey, TValue>? value)
        {
            if (!reader.TryReadCollectionHeader(out var length))
            {
                value = null;
                return;
            }

            var dict = new Dictionary<TKey, TValue>();

            var formatter = reader.GetFormatter<KeyValuePair<TKey, TValue>>();
            for (int i = 0; i < length; i++)
            {
                KeyValuePair<TKey, TValue> item = default;
                formatter.Deserialize(ref reader, ref item);
                dict.Add(item.Key, item.Value);
            }

            value = dict;
        }
    }

    public sealed class InterfaceReadOnlyDictionaryFormatter<TKey, TValue> : MemoryPackFormatter<IReadOnlyDictionary<TKey, TValue>>
        where TKey : notnull
    {
        static InterfaceReadOnlyDictionaryFormatter()
        {
            if (!MemoryPackFormatterProvider.IsRegistered<KeyValuePair<TKey, TValue>>())
            {
                MemoryPackFormatterProvider.Register(new KeyValuePairFormatter<TKey, TValue>());
            }
        }

        public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref IReadOnlyDictionary<TKey, TValue>? value)
        {
            if (value == null)
            {
                writer.WriteNullCollectionHeader();
                return;
            }

            var formatter = writer.GetFormatter<KeyValuePair<TKey, TValue>>();
            writer.WriteCollectionHeader(value.Count);
            foreach (var item in value)
            {
                var v = item;
                formatter.Serialize(ref writer, ref v);
            }
        }

        public override void Deserialize(ref MemoryPackReader reader, scoped ref IReadOnlyDictionary<TKey, TValue>? value)
        {
            if (!reader.TryReadCollectionHeader(out var length))
            {
                value = null;
                return;
            }

            var dict = new Dictionary<TKey, TValue>();

            var formatter = reader.GetFormatter<KeyValuePair<TKey, TValue>>();
            for (int i = 0; i < length; i++)
            {
                KeyValuePair<TKey, TValue> item = default;
                formatter.Deserialize(ref reader, ref item);
                dict.Add(item.Key, item.Value);
            }

            value = dict;
        }
    }

    public sealed class InterfaceLookupFormatter<TKey, TElement> : MemoryPackFormatter<ILookup<TKey, TElement>>
        where TKey : notnull
    {
        static InterfaceLookupFormatter()
        {
            if (!MemoryPackFormatterProvider.IsRegistered<IGrouping<TKey, TElement>>())
            {
                MemoryPackFormatterProvider.Register(new InterfaceGroupingFormatter<TKey, TElement>());
            }
        }

        public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref ILookup<TKey, TElement>? value)
        {
            if (value == null)
            {
                writer.WriteNullCollectionHeader();
                return;
            }

            var formatter = writer.GetFormatter<IGrouping<TKey, TElement>>();
            writer.WriteCollectionHeader(value.Count);
            foreach (var item in value)
            {
                var v = item;
                formatter.Serialize(ref writer, ref v);
            }
        }

        public override void Deserialize(ref MemoryPackReader reader, scoped ref ILookup<TKey, TElement>? value)
        {
            if (!reader.TryReadCollectionHeader(out var length))
            {
                value = null;
                return;
            }

            var dict = new Dictionary<TKey, IGrouping<TKey, TElement>>();

            var formatter = reader.GetFormatter<IGrouping<TKey, TElement>>();
            for (int i = 0; i < length; i++)
            {
                IGrouping<TKey, TElement>? item = default;
                formatter.Deserialize(ref reader, ref item);
                if (item != null)
                {
                    dict.Add(item.Key, item);
                }
            }
            value = new Lookup<TKey, TElement>(dict);
        }
    }

    public sealed class InterfaceGroupingFormatter<TKey, TElement> : MemoryPackFormatter<IGrouping<TKey, TElement>>
        where TKey : notnull
    {
        // serialize as {key, [collection]}

        static InterfaceGroupingFormatter()
        {
            if (!MemoryPackFormatterProvider.IsRegistered<IEnumerable<TElement>>())
            {
                MemoryPackFormatterProvider.Register(new InterfaceEnumerableFormatter<TElement>());
            }
        }

        public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref IGrouping<TKey, TElement>? value)
        {
            if (value == null)
            {
                writer.WriteNullObjectHeader();
                return;
            }

            writer.WriteObjectHeader(2);
            writer.WriteValue(value.Key);
            writer.WriteValue<IEnumerable<TElement>>(value); // write as IEnumerable<TElement>
        }

        public override void Deserialize(ref MemoryPackReader reader, scoped ref IGrouping<TKey, TElement>? value)
        {
            if (!reader.TryReadObjectHeader(out var count))
            {
                value = null;
                return;
            }

            if (count != 2) MemoryPackSerializationException.ThrowInvalidPropertyCount(2, count);

            var key = reader.ReadValue<TKey>();
            var values = reader.ReadArray<TElement>() as IEnumerable<TElement>;

            if (key == null) MemoryPackSerializationException.ThrowDeserializeObjectIsNull(nameof(key));
            if (values == null) MemoryPackSerializationException.ThrowDeserializeObjectIsNull(nameof(values));

            value = new Grouping<TKey, TElement>(key, values);

        }
    }

    public sealed class InterfaceSetFormatter<T> : MemoryPackFormatter<ISet<T?>>
    {
        static InterfaceSetFormatter()
        {
            if (!MemoryPackFormatterProvider.IsRegistered<HashSet<T>>())
            {
                MemoryPackFormatterProvider.Register(new HashSetFormatter<T>());
            }
        }

        public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref ISet<T?>? value)
        {
            if (value == null)
            {
                writer.WriteNullCollectionHeader();
                return;
            }

            var formatter = writer.GetFormatter<T>();
            writer.WriteCollectionHeader(value.Count);
            foreach (var item in value)
            {
                var v = item;
                formatter.Serialize(ref writer, ref v);
            }
        }

        public override void Deserialize(ref MemoryPackReader reader, scoped ref ISet<T?>? value)
        {
            if (!reader.TryReadCollectionHeader(out var length))
            {
                value = null;
                return;
            }

            var set = new HashSet<T?>(length);

            var formatter = reader.GetFormatter<T>();
            for (int i = 0; i < length; i++)
            {
                T? item = default;
                formatter.Deserialize(ref reader, ref item);
                set.Add(item);
            }

            value = set;
        }
    }

#if NET7_0_OR_GREATER

    public sealed class InterfaceReadOnlySetFormatter<T> : MemoryPackFormatter<IReadOnlySet<T?>>
    {
        static InterfaceReadOnlySetFormatter()
        {
            if (!MemoryPackFormatterProvider.IsRegistered<HashSet<T>>())
            {
                MemoryPackFormatterProvider.Register(new HashSetFormatter<T>());
            }
        }

        public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref IReadOnlySet<T?>? value)
        {
            if (value == null)
            {
                writer.WriteNullCollectionHeader();
                return;
            }

            var formatter = writer.GetFormatter<T>();
            writer.WriteCollectionHeader(value.Count);
            foreach (var item in value)
            {
                var v = item;
                formatter.Serialize(ref writer, ref v);
            }
        }

        public override void Deserialize(ref MemoryPackReader reader, scoped ref IReadOnlySet<T?>? value)
        {
            if (!reader.TryReadCollectionHeader(out var length))
            {
                value = null;
                return;
            }

            var set = new HashSet<T?>(length);

            var formatter = reader.GetFormatter<T>();
            for (int i = 0; i < length; i++)
            {
                T? item = default;
                formatter.Deserialize(ref reader, ref item);
                set.Add(item);
            }

            value = set;
        }
    }

#endif

    internal sealed class Grouping<TKey, TElement> : IGrouping<TKey, TElement>
    {
        readonly TKey key;
        readonly IEnumerable<TElement> elements;

        public Grouping(TKey key, IEnumerable<TElement> elements)
        {
            this.key = key;
            this.elements = elements;
        }

        public TKey Key
        {
            get
            {
                return this.key;
            }
        }

        public IEnumerator<TElement> GetEnumerator()
        {
            return this.elements.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.elements.GetEnumerator();
        }
    }

    internal sealed class Lookup<TKey, TElement> : ILookup<TKey, TElement>
        where TKey : notnull
    {
        readonly Dictionary<TKey, IGrouping<TKey, TElement>> groupings;

        public Lookup(Dictionary<TKey, IGrouping<TKey, TElement>> groupings)
        {
            this.groupings = groupings;
        }

        public IEnumerable<TElement> this[TKey key]
        {
            get
            {
                return this.groupings[key];
            }
        }

        public int Count
        {
            get
            {
                return this.groupings.Count;
            }
        }

        public bool Contains(TKey key)
        {
            return this.groupings.ContainsKey(key);
        }

        public IEnumerator<IGrouping<TKey, TElement>> GetEnumerator()
        {
            return this.groupings.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.groupings.Values.GetEnumerator();
        }
    }
}
