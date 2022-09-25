using MemoryPack.Formatters;
using MemoryPack.Internal;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// Clear support collection formatters
// List, Stack, Queue, LinkedList, HashSet, PriorityQueue,
// ObservableCollection, Collection
// ConcurrentQueue, ConcurrentStack, ConcurrentBag
// Dictionary, SortedDictionary, SortedList, ConcurrentDictionary

// Not supported clear
// ReadOnlyCollection, ReadOnlyObservableCollection, BlockingCollection

namespace MemoryPack
{
    public static partial class MemoryPackFormatterProvider
    {
        static readonly Dictionary<Type, Type> CollectionFormatters = new Dictionary<Type, Type>(18)
        {
            { typeof(List<>), typeof(ListFormatter<>) },
            { typeof(Stack<>), typeof(StackFormatter<>) },
            { typeof(Queue<>), typeof(QueueFormatter<>) },
            { typeof(LinkedList<>), typeof(LinkedListFormatter<>) },
            { typeof(HashSet<>), typeof(HashSetFormatter<>) },
            { typeof(PriorityQueue<,>), typeof(PriorityQueueFormatter<,>) },
            { typeof(ObservableCollection<>), typeof(ObservableCollectionFormatter<>) },
            { typeof(Collection<>), typeof(CollectionFormatter<>) },
            { typeof(ConcurrentQueue<>), typeof(ConcurrentQueueFormatter<>) },
            { typeof(ConcurrentStack<>), typeof(ConcurrentStackFormatter<>) },
            { typeof(ConcurrentBag<>), typeof(ConcurrentBagFormatter<>) },
            { typeof(Dictionary<,>), typeof(DictionaryFormatter<,>) },
            { typeof(SortedDictionary<,>), typeof(SortedDictionaryFormatter<,>) },
            { typeof(SortedList<,>), typeof(SortedListFormatter<,>) },
            { typeof(ConcurrentDictionary<,>), typeof(ConcurrentDictionaryFormatter<,>) },
            { typeof(ReadOnlyCollection<>), typeof(ReadOnlyCollectionFormatter<>) },
            { typeof(ReadOnlyObservableCollection<>), typeof(ReadOnlyObservableCollectionFormatter<>) },
            { typeof(BlockingCollection<>), typeof(BlockingCollectionFormatter<>) },
        };
    }
}

namespace MemoryPack.Formatters
{
    public sealed class ListFormatter<T> : MemoryPackFormatter<List<T?>>
    {
        public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref List<T?>? value)
        {
            if (value == null)
            {
                writer.WriteNullLengthHeader();
                return;
            }

            writer.WriteSpan(CollectionsMarshal.AsSpan(value));
        }

        public override void Deserialize(ref MemoryPackReader reader, scoped ref List<T?>? value)
        {
            if (!reader.TryReadLengthHeader(out var length))
            {
                value = null;
                return;
            }

            if (value == null)
            {
                value = new List<T?>(length);
            }
            else if (value.Count != length)
            {
                value.Clear();
            }

            var span = CollectionsMarshalEx.CreateSpan(value, length);
            reader.ReadSpanWithoutReadLengthHeader(length, ref span);
        }
    }

    public sealed class StackFormatter<T> : MemoryPackFormatter<Stack<T?>>
    {
        public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref Stack<T?>? value)
        {
            if (value == null)
            {
                writer.WriteNullLengthHeader();
                return;
            }

            writer.WriteSpan(CollectionsMarshalEx.AsSpan(value));
        }

        public override void Deserialize(ref MemoryPackReader reader, scoped ref Stack<T?>? value)
        {
            if (!reader.TryReadLengthHeader(out var length))
            {
                value = null;
                return;
            }

            if (value == null)
            {
                value = new Stack<T?>(length);
            }
            else if (value.Count != length)
            {
                value.Clear();
            }

            var span = CollectionsMarshalEx.CreateSpan(value, length);
            reader.ReadSpanWithoutReadLengthHeader(length, ref span);
        }
    }

    public sealed class QueueFormatter<T> : MemoryPackFormatter<Queue<T?>>
    {
        // Queue is circular buffer, can't optimize like List, Stack.

        public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref Queue<T?>? value)
        {
            if (value == null)
            {
                writer.WriteNullLengthHeader();
                return;
            }

            var formatter = writer.GetFormatter<T?>();
            writer.WriteLengthHeader(value.Count);
            foreach (var item in value)
            {
                var v = item;
                formatter.Serialize(ref writer, ref v);
            }
        }

        public override void Deserialize(ref MemoryPackReader reader, scoped ref Queue<T?>? value)
        {
            if (!reader.TryReadLengthHeader(out var length))
            {
                value = null;
                return;
            }

            if (value == null)
            {
                value = new Queue<T?>(length);
            }
            else
            {
                value.Clear();
                value.EnsureCapacity(length);
            }

            var formatter = reader.GetFormatter<T?>();
            for (int i = 0; i < length; i++)
            {
                T? v = default;
                formatter.Deserialize(ref reader, ref v);
                value.Enqueue(v);
            }
        }
    }

    public sealed class LinkedListFormatter<T> : MemoryPackFormatter<LinkedList<T?>>
    {
        public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref LinkedList<T?>? value)
        {
            if (value == null)
            {
                writer.WriteNullLengthHeader();
                return;
            }

            var formatter = writer.GetFormatter<T?>();
            writer.WriteLengthHeader(value.Count);
            foreach (var item in value)
            {
                var v = item;
                formatter.Serialize(ref writer, ref v);
            }
        }

        public override void Deserialize(ref MemoryPackReader reader, scoped ref LinkedList<T?>? value)
        {
            if (!reader.TryReadLengthHeader(out var length))
            {
                value = null;
                return;
            }

            if (value == null)
            {
                value = new LinkedList<T?>();
            }
            else
            {
                value.Clear();
            }

            var formatter = reader.GetFormatter<T?>();
            for (int i = 0; i < length; i++)
            {
                T? v = default;
                formatter.Deserialize(ref reader, ref v);
                value.AddLast(v);
            }
        }
    }

    public sealed class HashSetFormatter<T> : MemoryPackFormatter<HashSet<T?>>
    {
        public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref HashSet<T?>? value)
        {
            if (value == null)
            {
                writer.WriteNullLengthHeader();
                return;
            }

            var formatter = writer.GetFormatter<T?>();
            writer.WriteLengthHeader(value.Count);
            foreach (var item in value)
            {
                var v = item;
                formatter.Serialize(ref writer, ref v);
            }
        }

        public override void Deserialize(ref MemoryPackReader reader, scoped ref HashSet<T?>? value)
        {
            if (!reader.TryReadLengthHeader(out var length))
            {
                value = null;
                return;
            }

            if (value == null)
            {
                value = new HashSet<T?>(length);
            }
            else
            {
                value.Clear();
            }

            var formatter = reader.GetFormatter<T?>();
            for (int i = 0; i < length; i++)
            {
                T? v = default;
                formatter.Deserialize(ref reader, ref v);
                value.Add(v);
            }
        }
    }

    public sealed class PriorityQueueFormatter<TElement, TPriority> : MemoryPackFormatter<PriorityQueue<TElement?, TPriority?>>
    {
        static PriorityQueueFormatter()
        {
            if (!MemoryPackFormatterProvider.IsRegistered<(TElement?, TPriority?)>())
            {
                MemoryPackFormatterProvider.Register(new ValueTupleFormatter<TElement?, TPriority?>());
            }
        }

        public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref PriorityQueue<TElement?, TPriority?>? value)
        {
            if (value == null)
            {
                writer.WriteNullLengthHeader();
                return;
            }

            var formatter = writer.GetFormatter<(TElement?, TPriority?)>();

            writer.WriteLengthHeader(value.Count);
            foreach (var item in value.UnorderedItems)
            {
                var v = item;
                formatter.Serialize(ref writer, ref v);
            }
        }

        public override void Deserialize(ref MemoryPackReader reader, scoped ref PriorityQueue<TElement?, TPriority?>? value)
        {
            if (!reader.TryReadLengthHeader(out var length))
            {
                value = null;
                return;
            }

            if (value == null)
            {
                value = new PriorityQueue<TElement?, TPriority?>(length);
            }
            else
            {
                value.Clear();
            }

            var formatter = reader.GetFormatter<(TElement?, TPriority?)>();
            for (int i = 0; i < length; i++)
            {
                (TElement?, TPriority?) v = default;
                formatter.Deserialize(ref reader, ref v);
                value.Enqueue(v.Item1, v.Item2);
            }
        }
    }

    public sealed class CollectionFormatter<T> : MemoryPackFormatter<Collection<T?>>
    {
        public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref Collection<T?>? value)
        {
            if (value == null)
            {
                writer.WriteNullLengthHeader();
                return;
            }

            var formatter = writer.GetFormatter<T?>();
            writer.WriteLengthHeader(value.Count);
            foreach (var item in value)
            {
                var v = item;
                formatter.Serialize(ref writer, ref v);
            }
        }

        public override void Deserialize(ref MemoryPackReader reader, scoped ref Collection<T?>? value)
        {
            if (!reader.TryReadLengthHeader(out var length))
            {
                value = null;
                return;
            }

            if (value == null)
            {
                value = new Collection<T?>();
            }
            else
            {
                value.Clear();
            }

            var formatter = reader.GetFormatter<T?>();
            for (int i = 0; i < length; i++)
            {
                T? v = default;
                formatter.Deserialize(ref reader, ref v);
                value.Add(v);
            }
        }
    }

    public sealed class ObservableCollectionFormatter<T> : MemoryPackFormatter<ObservableCollection<T?>>
    {
        public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref ObservableCollection<T?>? value)
        {
            if (value == null)
            {
                writer.WriteNullLengthHeader();
                return;
            }

            var formatter = writer.GetFormatter<T?>();
            writer.WriteLengthHeader(value.Count);
            foreach (var item in value)
            {
                var v = item;
                formatter.Serialize(ref writer, ref v);
            }
        }

        public override void Deserialize(ref MemoryPackReader reader, scoped ref ObservableCollection<T?>? value)
        {
            if (!reader.TryReadLengthHeader(out var length))
            {
                value = null;
                return;
            }

            if (value == null)
            {
                value = new ObservableCollection<T?>();
            }
            else
            {
                value.Clear();
            }

            var formatter = reader.GetFormatter<T?>();
            for (int i = 0; i < length; i++)
            {
                T? v = default;
                formatter.Deserialize(ref reader, ref v);
                value.Add(v);
            }
        }
    }

    public sealed class ConcurrentQueueFormatter<T> : MemoryPackFormatter<ConcurrentQueue<T?>>
    {
        public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref ConcurrentQueue<T?>? value)
        {
            if (value == null)
            {
                writer.WriteNullLengthHeader();
                return;
            }

            // note: serializing ConcurretnCollection(Queue/Stack/Bag/Dictionary) is not thread-safe.
            // operate Add/Remove in iterating in other thread, not guranteed correct result

            var formatter = writer.GetFormatter<T?>();
            var count = value.Count;
            writer.WriteLengthHeader(count);
            var i = 0;
            foreach (var item in value)
            {
                i++;
                var v = item;
                formatter.Serialize(ref writer, ref v);
            }

            if (i != count) ThrowHelper.ThrowInvalidConcurrrentCollectionOperation();
        }

        public override void Deserialize(ref MemoryPackReader reader, scoped ref ConcurrentQueue<T?>? value)
        {
            if (!reader.TryReadLengthHeader(out var length))
            {
                value = null;
                return;
            }

            if (value == null)
            {
                value = new ConcurrentQueue<T?>();
            }
            else
            {
                value.Clear();
            }

            var formatter = reader.GetFormatter<T?>();
            for (int i = 0; i < length; i++)
            {
                T? v = default;
                formatter.Deserialize(ref reader, ref v);
                value.Enqueue(v);
            }
        }
    }

    public sealed class ConcurrentStackFormatter<T> : MemoryPackFormatter<ConcurrentStack<T?>>
    {
        public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref ConcurrentStack<T?>? value)
        {
            if (value == null)
            {
                writer.WriteNullLengthHeader();
                return;
            }

            // reverse order in serialize
            var count = value.Count;
            T?[] rentArray = ArrayPool<T?>.Shared.Rent(count);
            try
            {
                var i = 0;
                foreach (var item in value)
                {
                    rentArray[i++] = item;
                }
                if (i != count) ThrowHelper.ThrowInvalidConcurrrentCollectionOperation();

                var formatter = writer.GetFormatter<T?>();
                writer.WriteLengthHeader(count);
                for (i = i - 1; i >= 0; i--)
                {
                    formatter.Serialize(ref writer, ref rentArray[i]);
                }
            }
            finally
            {
                ArrayPool<T?>.Shared.Return(rentArray, clearArray: RuntimeHelpers.IsReferenceOrContainsReferences<T>());
            }
        }

        public override void Deserialize(ref MemoryPackReader reader, scoped ref ConcurrentStack<T?>? value)
        {
            if (!reader.TryReadLengthHeader(out var length))
            {
                value = null;
                return;
            }

            if (value == null)
            {
                value = new ConcurrentStack<T?>();
            }
            else
            {
                value.Clear();
            }

            var formatter = reader.GetFormatter<T?>();
            for (int i = 0; i < length; i++)
            {
                T? v = default;
                formatter.Deserialize(ref reader, ref v);
                value.Push(v);
            }
        }
    }

    public sealed class ConcurrentBagFormatter<T> : MemoryPackFormatter<ConcurrentBag<T?>>
    {
        public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref ConcurrentBag<T?>? value)
        {
            if (value == null)
            {
                writer.WriteNullLengthHeader();
                return;
            }

            var formatter = writer.GetFormatter<T?>();
            var count = value.Count;
            writer.WriteLengthHeader(count);
            var i = 0;
            foreach (var item in value)
            {
                i++;
                var v = item;
                formatter.Serialize(ref writer, ref v);
            }

            if (i != count) ThrowHelper.ThrowInvalidConcurrrentCollectionOperation();
        }

        public override void Deserialize(ref MemoryPackReader reader, scoped ref ConcurrentBag<T?>? value)
        {
            if (!reader.TryReadLengthHeader(out var length))
            {
                value = null;
                return;
            }

            if (value == null)
            {
                value = new ConcurrentBag<T?>();
            }
            else
            {
                value.Clear();
            }

            var formatter = reader.GetFormatter<T?>();
            for (int i = 0; i < length; i++)
            {
                T? v = default;
                formatter.Deserialize(ref reader, ref v);
                value.Add(v);
            }
        }
    }

    public sealed class DictionaryFormatter<TKey, TValue> : MemoryPackFormatter<Dictionary<TKey, TValue?>>
        where TKey : notnull
    {
        static DictionaryFormatter()
        {
            if (!MemoryPackFormatterProvider.IsRegistered<KeyValuePair<TKey, TValue?>>())
            {
                MemoryPackFormatterProvider.Register(new KeyValuePairFormatter<TKey, TValue?>());
            }
        }

        public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref Dictionary<TKey, TValue?>? value)
        {
            if (value == null)
            {
                writer.WriteNullLengthHeader();
                return;
            }

            var formatter = writer.GetFormatter<KeyValuePair<TKey, TValue?>>();

            writer.WriteLengthHeader(value.Count);
            foreach (var item in value)
            {
                var v = item;
                formatter.Serialize(ref writer, ref v);
            }
        }

        public override void Deserialize(ref MemoryPackReader reader, scoped ref Dictionary<TKey, TValue?>? value)
        {
            if (!reader.TryReadLengthHeader(out var length))
            {
                value = null;
                return;
            }

            if (value == null)
            {
                value = new Dictionary<TKey, TValue?>(length);
            }
            else
            {
                value.Clear();
            }

            var formatter = reader.GetFormatter<KeyValuePair<TKey, TValue?>>();
            for (int i = 0; i < length; i++)
            {
                KeyValuePair<TKey, TValue?> v = default;
                formatter.Deserialize(ref reader, ref v);
                value.Add(v.Key, v.Value);
            }
        }
    }

    public sealed class SortedDictionaryFormatter<TKey, TValue> : MemoryPackFormatter<SortedDictionary<TKey, TValue?>>
        where TKey : notnull
    {
        static SortedDictionaryFormatter()
        {
            if (!MemoryPackFormatterProvider.IsRegistered<KeyValuePair<TKey, TValue?>>())
            {
                MemoryPackFormatterProvider.Register(new KeyValuePairFormatter<TKey, TValue?>());
            }
        }

        public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref SortedDictionary<TKey, TValue?>? value)
        {
            if (value == null)
            {
                writer.WriteNullLengthHeader();
                return;
            }

            var formatter = writer.GetFormatter<KeyValuePair<TKey, TValue?>>();

            writer.WriteLengthHeader(value.Count);
            foreach (var item in value)
            {
                var v = item;
                formatter.Serialize(ref writer, ref v);
            }
        }

        public override void Deserialize(ref MemoryPackReader reader, scoped ref SortedDictionary<TKey, TValue?>? value)
        {
            if (!reader.TryReadLengthHeader(out var length))
            {
                value = null;
                return;
            }

            if (value == null)
            {
                value = new SortedDictionary<TKey, TValue?>();
            }
            else
            {
                value.Clear();
            }

            var formatter = reader.GetFormatter<KeyValuePair<TKey, TValue?>>();
            for (int i = 0; i < length; i++)
            {
                KeyValuePair<TKey, TValue?> v = default;
                formatter.Deserialize(ref reader, ref v);
                value.Add(v.Key, v.Value);
            }
        }
    }

    public sealed class SortedListFormatter<TKey, TValue> : MemoryPackFormatter<SortedList<TKey, TValue?>>
        where TKey : notnull
    {
        static SortedListFormatter()
        {
            if (!MemoryPackFormatterProvider.IsRegistered<KeyValuePair<TKey, TValue?>>())
            {
                MemoryPackFormatterProvider.Register(new KeyValuePairFormatter<TKey, TValue?>());
            }
        }

        public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref SortedList<TKey, TValue?>? value)
        {
            if (value == null)
            {
                writer.WriteNullLengthHeader();
                return;
            }

            var formatter = writer.GetFormatter<KeyValuePair<TKey, TValue?>>();

            writer.WriteLengthHeader(value.Count);
            foreach (var item in value)
            {
                var v = item;
                formatter.Serialize(ref writer, ref v);
            }
        }

        public override void Deserialize(ref MemoryPackReader reader, scoped ref SortedList<TKey, TValue?>? value)
        {
            if (!reader.TryReadLengthHeader(out var length))
            {
                value = null;
                return;
            }

            if (value == null)
            {
                value = new SortedList<TKey, TValue?>(length);
            }
            else
            {
                value.Clear();
            }

            var formatter = reader.GetFormatter<KeyValuePair<TKey, TValue?>>();
            for (int i = 0; i < length; i++)
            {
                KeyValuePair<TKey, TValue?> v = default;
                formatter.Deserialize(ref reader, ref v);
                value.Add(v.Key, v.Value);
            }
        }
    }

    public sealed class ConcurrentDictionaryFormatter<TKey, TValue> : MemoryPackFormatter<ConcurrentDictionary<TKey, TValue?>>
        where TKey : notnull
    {
        static ConcurrentDictionaryFormatter()
        {
            if (!MemoryPackFormatterProvider.IsRegistered<KeyValuePair<TKey, TValue?>>())
            {
                MemoryPackFormatterProvider.Register(new KeyValuePairFormatter<TKey, TValue?>());
            }
        }

        public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref ConcurrentDictionary<TKey, TValue?>? value)
        {
            if (value == null)
            {
                writer.WriteNullLengthHeader();
                return;
            }

            var formatter = writer.GetFormatter<KeyValuePair<TKey, TValue?>>();

            var count = value.Count;
            writer.WriteLengthHeader(count);
            var i = 0;
            foreach (var item in value)
            {
                i++;
                var v = item;
                formatter.Serialize(ref writer, ref v);
            }

            if (i != count) ThrowHelper.ThrowInvalidConcurrrentCollectionOperation();
        }

        public override void Deserialize(ref MemoryPackReader reader, scoped ref ConcurrentDictionary<TKey, TValue?>? value)
        {
            if (!reader.TryReadLengthHeader(out var length))
            {
                value = null;
                return;
            }

            if (value == null)
            {
                value = new ConcurrentDictionary<TKey, TValue?>();
            }
            else
            {
                value.Clear();
            }

            var formatter = reader.GetFormatter<KeyValuePair<TKey, TValue?>>();
            for (int i = 0; i < length; i++)
            {
                KeyValuePair<TKey, TValue?> v = default;
                formatter.Deserialize(ref reader, ref v);
                value.TryAdd(v.Key, v.Value);
            }
        }
    }

    public sealed class ReadOnlyCollectionFormatter<T> : MemoryPackFormatter<ReadOnlyCollection<T?>>
    {
        public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref ReadOnlyCollection<T?>? value)
        {
            if (value == null)
            {
                writer.WriteNullLengthHeader();
                return;
            }

            var formatter = writer.GetFormatter<T?>();
            writer.WriteLengthHeader(value.Count);
            foreach (var item in value)
            {
                var v = item;
                formatter.Serialize(ref writer, ref v);
            }
        }

        public override void Deserialize(ref MemoryPackReader reader, scoped ref ReadOnlyCollection<T?>? value)
        {
            var array = reader.ReadArray<T?>();

            if (array == null)
            {
                value = null;
            }
            else
            {
                value = new ReadOnlyCollection<T?>(array);
            }
        }
    }

    public sealed class ReadOnlyObservableCollectionFormatter<T> : MemoryPackFormatter<ReadOnlyObservableCollection<T?>>
    {
        public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref ReadOnlyObservableCollection<T?>? value)
        {
            if (value == null)
            {
                writer.WriteNullLengthHeader();
                return;
            }

            var formatter = writer.GetFormatter<T?>();
            writer.WriteLengthHeader(value.Count);
            foreach (var item in value)
            {
                var v = item;
                formatter.Serialize(ref writer, ref v);
            }
        }

        public override void Deserialize(ref MemoryPackReader reader, scoped ref ReadOnlyObservableCollection<T?>? value)
        {
            var array = reader.ReadArray<T?>();

            if (array == null)
            {
                value = null;
            }
            else
            {
                value = new ReadOnlyObservableCollection<T?>(new ObservableCollection<T?>(array));
            }
        }
    }

    public sealed class BlockingCollectionFormatter<T> : MemoryPackFormatter<BlockingCollection<T?>>
    {
        public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref BlockingCollection<T?>? value)
        {
            if (value == null)
            {
                writer.WriteNullLengthHeader();
                return;
            }

            var formatter = writer.GetFormatter<T?>();
            writer.WriteLengthHeader(value.Count);
            foreach (var item in value)
            {
                var v = item;
                formatter.Serialize(ref writer, ref v);
            }
        }

        public override void Deserialize(ref MemoryPackReader reader, scoped ref BlockingCollection<T?>? value)
        {
            if (!reader.TryReadLengthHeader(out var length))
            {
                value = null;
                return;
            }

            value = new BlockingCollection<T?>();

            var formatter = reader.GetFormatter<T?>();
            for (int i = 0; i < length; i++)
            {
                T? v = default;
                formatter.Deserialize(ref reader, ref v);
                value.Add(v);
            }
        }
    }
}
