#if NET7_0_OR_GREATER

using System.Collections;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MemoryPack.Formatters;

[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class ContextImmutableArrayFormatter<T> : MemoryPackFormatter<ImmutableArray<T?>>
{
    readonly MemoryPackFormatter<T> elementFormatter;

    public ContextImmutableArrayFormatter(MemoryPackFormatter<T> elementFormatter) => this.elementFormatter = elementFormatter;

    public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref ImmutableArray<T?> value)
    {
        if (value.IsDefault)
        {
            writer.WriteNullCollectionHeader();
            return;
        }

        writer.WriteCollectionHeader(value.Length);
        for (var i = 0; i < value.Length; i++)
        {
            var item = value[i];
            elementFormatter.Serialize(ref writer, ref item);
        }
    }

    public override void Deserialize(ref MemoryPackReader reader, scoped ref ImmutableArray<T?> value)
    {
        if (!reader.TryReadCollectionHeader(out var length))
        {
            value = default;
            return;
        }

        var items = new T?[length];
        for (var i = 0; i < length; i++)
        {
            elementFormatter.Deserialize(ref reader, ref items[i]);
        }
        value = ImmutableArray.Create(items);
    }
}

[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class ContextEnumerableFormatter<TCollection, TElement> : MemoryPackFormatter<TCollection>
    where TCollection : IEnumerable<TElement?>
{
    readonly MemoryPackFormatter<TElement> elementFormatter;
    readonly Func<IEnumerable<TElement?>, TCollection> materialize;
    readonly bool reverse;

    public ContextEnumerableFormatter(
        MemoryPackFormatter<TElement> elementFormatter,
        Func<IEnumerable<TElement?>, TCollection> materialize,
        bool reverse = false)
    {
        this.elementFormatter = elementFormatter;
        this.materialize = materialize;
        this.reverse = reverse;
    }

    public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref TCollection? value)
    {
        if (value is null)
        {
            writer.WriteNullCollectionHeader();
            return;
        }

        IEnumerable<TElement?> items = value;
        if (reverse)
        {
            items = items.Reverse();
        }

        if (!items.TryGetNonEnumeratedCount(out var count))
        {
            items = items.ToArray();
            count = ((TElement?[])items).Length;
        }

        writer.WriteCollectionHeader(count);
        foreach (var item in items)
        {
            var local = item;
            elementFormatter.Serialize(ref writer, ref local);
        }
    }

    public override void Deserialize(ref MemoryPackReader reader, scoped ref TCollection? value)
    {
        if (!reader.TryReadCollectionHeader(out var length))
        {
            value = default;
            return;
        }

        var items = new TElement?[length];
        for (var i = 0; i < length; i++)
        {
            elementFormatter.Deserialize(ref reader, ref items[i]);
        }

        value = materialize(items);
    }
}

[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class ContextMapFormatter<TMap, TKey, TValue> : MemoryPackFormatter<TMap>
    where TKey : notnull
    where TMap : IEnumerable<KeyValuePair<TKey, TValue?>>
{
    readonly MemoryPackFormatter<TKey> keyFormatter;
    readonly MemoryPackFormatter<TValue> valueFormatter;
    readonly Func<IEnumerable<KeyValuePair<TKey, TValue?>>, TMap> materialize;

    public ContextMapFormatter(
        MemoryPackFormatter<TKey> keyFormatter,
        MemoryPackFormatter<TValue> valueFormatter,
        Func<IEnumerable<KeyValuePair<TKey, TValue?>>, TMap> materialize)
    {
        this.keyFormatter = keyFormatter;
        this.valueFormatter = valueFormatter;
        this.materialize = materialize;
    }

    public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref TMap? value)
    {
        if (value is null)
        {
            writer.WriteNullCollectionHeader();
            return;
        }

        IEnumerable<KeyValuePair<TKey, TValue?>> items = value;
        if (!items.TryGetNonEnumeratedCount(out var count))
        {
            items = items.ToArray();
            count = ((KeyValuePair<TKey, TValue?>[])items).Length;
        }

        writer.WriteCollectionHeader(count);
        foreach (var item in items)
        {
            KeyValuePairFormatter.Serialize(keyFormatter, valueFormatter, ref writer, item!);
        }
    }

    public override void Deserialize(ref MemoryPackReader reader, scoped ref TMap? value)
    {
        if (!reader.TryReadCollectionHeader(out var length))
        {
            value = default;
            return;
        }

        var items = new KeyValuePair<TKey, TValue?>[length];
        for (var i = 0; i < length; i++)
        {
            KeyValuePairFormatter.Deserialize(keyFormatter, valueFormatter, ref reader, out var key, out var itemValue);
            items[i] = new KeyValuePair<TKey, TValue?>(key!, itemValue);
        }

        value = materialize(items);
    }
}

[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class ContextPriorityQueueFormatter<TElement, TPriority> : MemoryPackFormatter<PriorityQueue<TElement?, TPriority?>>
{
    readonly MemoryPackFormatter<TElement> elementFormatter;
    readonly MemoryPackFormatter<TPriority> priorityFormatter;

    public ContextPriorityQueueFormatter(MemoryPackFormatter<TElement> elementFormatter, MemoryPackFormatter<TPriority> priorityFormatter)
    {
        this.elementFormatter = elementFormatter;
        this.priorityFormatter = priorityFormatter;
    }

    public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref PriorityQueue<TElement?, TPriority?>? value)
    {
        if (value is null)
        {
            writer.WriteNullCollectionHeader();
            return;
        }

        writer.WriteCollectionHeader(value.Count);
        foreach (var item in value.UnorderedItems)
        {
            var element = item.Element;
            var priority = item.Priority;
            if (!RuntimeHelpers.IsReferenceOrContainsReferences<(TElement?, TPriority?)>())
            {
                writer.DangerousWriteUnmanaged((element, priority));
                continue;
            }
            elementFormatter.Serialize(ref writer, ref element);
            priorityFormatter.Serialize(ref writer, ref priority);
        }
    }

    public override void Deserialize(ref MemoryPackReader reader, scoped ref PriorityQueue<TElement?, TPriority?>? value)
    {
        if (!reader.TryReadCollectionHeader(out var length))
        {
            value = null;
            return;
        }

        value = new PriorityQueue<TElement?, TPriority?>(length);
        for (var i = 0; i < length; i++)
        {
            if (!RuntimeHelpers.IsReferenceOrContainsReferences<(TElement?, TPriority?)>())
            {
                reader.DangerousReadUnmanaged(out ValueTuple<TElement?, TPriority?> item);
                value.Enqueue(item.Item1, item.Item2);
                continue;
            }

            TElement? element = default;
            TPriority? priority = default;
            elementFormatter.Deserialize(ref reader, ref element);
            priorityFormatter.Deserialize(ref reader, ref priority);
            value.Enqueue(element, priority);
        }
    }
}

[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class ContextGroupingFormatter<TKey, TElement> : MemoryPackFormatter<IGrouping<TKey, TElement>>
    where TKey : notnull
{
    readonly MemoryPackFormatter<TKey> keyFormatter;
    readonly MemoryPackFormatter<TElement> elementFormatter;

    public ContextGroupingFormatter(MemoryPackFormatter<TKey> keyFormatter, MemoryPackFormatter<TElement> elementFormatter)
    {
        this.keyFormatter = keyFormatter;
        this.elementFormatter = elementFormatter;
    }

    public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref IGrouping<TKey, TElement>? value)
    {
        if (value is null)
        {
            writer.WriteNullObjectHeader();
            return;
        }

        writer.WriteObjectHeader(2);
        var key = value.Key;
        keyFormatter.Serialize(ref writer, ref key);
        IEnumerable<TElement> items = value;
        if (!items.TryGetNonEnumeratedCount(out var count))
        {
            items = items.ToArray();
            count = ((TElement[])items).Length;
        }
        writer.WriteCollectionHeader(count);
        foreach (var item in items)
        {
            var local = item;
            elementFormatter.Serialize(ref writer, ref local);
        }
    }

    public override void Deserialize(ref MemoryPackReader reader, scoped ref IGrouping<TKey, TElement>? value)
    {
        if (!reader.TryReadObjectHeader(out var count))
        {
            value = null;
            return;
        }
        if (count != 2)
        {
            MemoryPackSerializationException.ThrowInvalidPropertyCount(2, count);
        }

        TKey? key = default;
        keyFormatter.Deserialize(ref reader, ref key);
        if (!reader.TryReadCollectionHeader(out var length))
        {
            MemoryPackSerializationException.ThrowInvalidCollection();
        }

        var items = new TElement[length];
        for (var i = 0; i < length; i++)
        {
            TElement? item = default;
            elementFormatter.Deserialize(ref reader, ref item);
            items[i] = item!;
        }
        value = new ContextGrouping<TKey, TElement>(key!, items);
    }
}

[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class ContextLookupFormatter<TKey, TElement> : MemoryPackFormatter<ILookup<TKey, TElement>>
    where TKey : notnull
{
    readonly ContextGroupingFormatter<TKey, TElement> groupingFormatter;

    public ContextLookupFormatter(MemoryPackFormatter<TKey> keyFormatter, MemoryPackFormatter<TElement> elementFormatter)
    {
        groupingFormatter = new ContextGroupingFormatter<TKey, TElement>(keyFormatter, elementFormatter);
    }

    public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref ILookup<TKey, TElement>? value)
    {
        if (value is null)
        {
            writer.WriteNullCollectionHeader();
            return;
        }

        writer.WriteCollectionHeader(value.Count);
        foreach (var item in value)
        {
            IGrouping<TKey, TElement>? local = item;
            groupingFormatter.Serialize(ref writer, ref local);
        }
    }

    public override void Deserialize(ref MemoryPackReader reader, scoped ref ILookup<TKey, TElement>? value)
    {
        if (!reader.TryReadCollectionHeader(out var length))
        {
            value = null;
            return;
        }

        var groups = new Dictionary<TKey, ContextGrouping<TKey, TElement>>(length);
        for (var i = 0; i < length; i++)
        {
            IGrouping<TKey, TElement>? item = null;
            groupingFormatter.Deserialize(ref reader, ref item);
            if (item is ContextGrouping<TKey, TElement> group)
            {
                groups.Add(group.Key, group);
            }
        }
        value = new ContextLookup<TKey, TElement>(groups);
    }
}

sealed class ContextGrouping<TKey, TElement> : IGrouping<TKey, TElement>
{
    readonly IEnumerable<TElement> values;
    public TKey Key { get; }

    public ContextGrouping(TKey key, IEnumerable<TElement> values)
    {
        Key = key;
        this.values = values;
    }

    public IEnumerator<TElement> GetEnumerator() => values.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

sealed class ContextLookup<TKey, TElement> : ILookup<TKey, TElement>
    where TKey : notnull
{
    readonly IReadOnlyDictionary<TKey, ContextGrouping<TKey, TElement>> groups;

    public ContextLookup(IReadOnlyDictionary<TKey, ContextGrouping<TKey, TElement>> groups) => this.groups = groups;

    public int Count => groups.Count;
    public IEnumerable<TElement> this[TKey key] => groups.TryGetValue(key, out var group) ? group : Array.Empty<TElement>();
    public bool Contains(TKey key) => groups.ContainsKey(key);
    public IEnumerator<IGrouping<TKey, TElement>> GetEnumerator() => groups.Values.Cast<IGrouping<TKey, TElement>>().GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

#endif
