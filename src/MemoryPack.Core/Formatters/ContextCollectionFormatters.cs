#if NET7_0_OR_GREATER

using MemoryPack.Internal;
using System.Buffers;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace MemoryPack.Formatters;

[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class ContextArrayFormatter<T> : MemoryPackFormatter<T?[]>
{
    readonly MemoryPackFormatter<T> elementFormatter;

    public ContextArrayFormatter(MemoryPackFormatter<T> elementFormatter)
    {
        this.elementFormatter = elementFormatter;
    }

    public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref T?[]? value)
    {
        if (!RuntimeHelpers.IsReferenceOrContainsReferences<T>())
        {
            writer.DangerousWriteUnmanagedArray(value);
            return;
        }

        if (value is null)
        {
            writer.WriteNullCollectionHeader();
            return;
        }

        writer.WriteCollectionHeader(value.Length);
        for (var i = 0; i < value.Length; i++)
        {
            elementFormatter.Serialize(ref writer, ref value[i]);
        }
    }

    public override void Deserialize(ref MemoryPackReader reader, scoped ref T?[]? value)
    {
        if (!RuntimeHelpers.IsReferenceOrContainsReferences<T>())
        {
            reader.DangerousReadUnmanagedArray(ref value);
            return;
        }

        if (!reader.TryReadCollectionHeader(out var length))
        {
            value = null;
            return;
        }

        if (value is null || value.Length != length)
        {
            value = new T?[length];
        }

        for (var i = 0; i < length; i++)
        {
            elementFormatter.Deserialize(ref reader, ref value[i]);
        }
    }
}

[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class ContextTwoDimensionalArrayFormatter<T> : MemoryPackFormatter<T?[,]>
{
    readonly MemoryPackFormatter<T> elementFormatter;

    public ContextTwoDimensionalArrayFormatter(MemoryPackFormatter<T> elementFormatter) => this.elementFormatter = elementFormatter;

    public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref T?[,]? value)
    {
        if (value is null) { writer.WriteNullObjectHeader(); return; }
        writer.WriteObjectHeader(3);
        writer.WriteUnmanaged(value.GetLength(0), value.GetLength(1));
        writer.WriteCollectionHeader(value.Length);
        foreach (var item in value) { var local = item; elementFormatter.Serialize(ref writer, ref local); }
    }

    public override void Deserialize(ref MemoryPackReader reader, scoped ref T?[,]? value)
    {
        if (!reader.TryReadObjectHeader(out var count)) { value = null; return; }
        if (count != 3) MemoryPackSerializationException.ThrowInvalidPropertyCount(3, count);
        reader.ReadUnmanaged(out int length0, out int length1);
        if (!reader.TryReadCollectionHeader(out var length) || length != length0 * length1) MemoryPackSerializationException.ThrowInvalidCollection();
        if (value is null || value.GetLength(0) != length0 || value.GetLength(1) != length1) value = new T?[length0, length1];
        for (var i = 0; i < length0; i++) for (var j = 0; j < length1; j++) elementFormatter.Deserialize(ref reader, ref value[i, j]);
    }
}

[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class ContextThreeDimensionalArrayFormatter<T> : MemoryPackFormatter<T?[,,]>
{
    readonly MemoryPackFormatter<T> elementFormatter;

    public ContextThreeDimensionalArrayFormatter(MemoryPackFormatter<T> elementFormatter) => this.elementFormatter = elementFormatter;

    public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref T?[,,]? value)
    {
        if (value is null) { writer.WriteNullObjectHeader(); return; }
        writer.WriteObjectHeader(4);
        writer.WriteUnmanaged(value.GetLength(0), value.GetLength(1), value.GetLength(2));
        writer.WriteCollectionHeader(value.Length);
        foreach (var item in value) { var local = item; elementFormatter.Serialize(ref writer, ref local); }
    }

    public override void Deserialize(ref MemoryPackReader reader, scoped ref T?[,,]? value)
    {
        if (!reader.TryReadObjectHeader(out var count)) { value = null; return; }
        if (count != 4) MemoryPackSerializationException.ThrowInvalidPropertyCount(4, count);
        reader.ReadUnmanaged(out int length0, out int length1, out int length2);
        if (!reader.TryReadCollectionHeader(out var length) || length != length0 * length1 * length2) MemoryPackSerializationException.ThrowInvalidCollection();
        if (value is null || value.GetLength(0) != length0 || value.GetLength(1) != length1 || value.GetLength(2) != length2) value = new T?[length0, length1, length2];
        for (var i = 0; i < length0; i++) for (var j = 0; j < length1; j++) for (var k = 0; k < length2; k++) elementFormatter.Deserialize(ref reader, ref value[i, j, k]);
    }
}

[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class ContextFourDimensionalArrayFormatter<T> : MemoryPackFormatter<T?[,,,]>
{
    readonly MemoryPackFormatter<T> elementFormatter;

    public ContextFourDimensionalArrayFormatter(MemoryPackFormatter<T> elementFormatter) => this.elementFormatter = elementFormatter;

    public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref T?[,,,]? value)
    {
        if (value is null) { writer.WriteNullObjectHeader(); return; }
        writer.WriteObjectHeader(5);
        writer.WriteUnmanaged(value.GetLength(0), value.GetLength(1), value.GetLength(2), value.GetLength(3));
        writer.WriteCollectionHeader(value.Length);
        foreach (var item in value) { var local = item; elementFormatter.Serialize(ref writer, ref local); }
    }

    public override void Deserialize(ref MemoryPackReader reader, scoped ref T?[,,,]? value)
    {
        if (!reader.TryReadObjectHeader(out var count)) { value = null; return; }
        if (count != 5) MemoryPackSerializationException.ThrowInvalidPropertyCount(5, count);
        reader.ReadUnmanaged(out int length0, out int length1, out int length2, out int length3);
        if (!reader.TryReadCollectionHeader(out var length) || length != length0 * length1 * length2 * length3) MemoryPackSerializationException.ThrowInvalidCollection();
        if (value is null || value.GetLength(0) != length0 || value.GetLength(1) != length1 || value.GetLength(2) != length2 || value.GetLength(3) != length3) value = new T?[length0, length1, length2, length3];
        for (var i = 0; i < length0; i++) for (var j = 0; j < length1; j++) for (var k = 0; k < length2; k++) for (var l = 0; l < length3; l++) elementFormatter.Deserialize(ref reader, ref value[i, j, k, l]);
    }
}

[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class ContextListFormatter<T> : MemoryPackFormatter<List<T?>>
{
    readonly MemoryPackFormatter<T> elementFormatter;

    public ContextListFormatter(MemoryPackFormatter<T> elementFormatter)
    {
        this.elementFormatter = elementFormatter;
    }

    public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref List<T?>? value)
    {
        if (value is null)
        {
            writer.WriteNullCollectionHeader();
            return;
        }

        var span = CollectionsMarshal.AsSpan(value);
        if (!RuntimeHelpers.IsReferenceOrContainsReferences<T>())
        {
            writer.DangerousWriteUnmanagedSpan(span);
            return;
        }

        writer.WriteCollectionHeader(value.Count);
        for (var i = 0; i < span.Length; i++)
        {
            elementFormatter.Serialize(ref writer, ref span[i]);
        }
    }

    public override void Deserialize(ref MemoryPackReader reader, scoped ref List<T?>? value)
    {
        if (!reader.TryReadCollectionHeader(out var length))
        {
            value = null;
            return;
        }

        value ??= new List<T?>(length);
        value.Clear();
        var span = CollectionsMarshalEx.CreateSpan(value, length);
        if (!RuntimeHelpers.IsReferenceOrContainsReferences<T>())
        {
            reader.ReadSpanWithoutReadLengthHeader(length, ref span);
            return;
        }

        for (var i = 0; i < span.Length; i++)
        {
            elementFormatter.Deserialize(ref reader, ref span[i]);
        }
    }
}

[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class ContextArraySegmentFormatter<T> : MemoryPackFormatter<ArraySegment<T?>>
{
    readonly MemoryPackFormatter<T> elementFormatter;
    public ContextArraySegmentFormatter(MemoryPackFormatter<T> elementFormatter) => this.elementFormatter = elementFormatter;

    public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref ArraySegment<T?> value)
    {
        writer.WriteCollectionHeader(value.Count);
        foreach (ref var item in value.AsSpan()) elementFormatter.Serialize(ref writer, ref item);
    }

    public override void Deserialize(ref MemoryPackReader reader, scoped ref ArraySegment<T?> value)
    {
        if (!reader.TryReadCollectionHeader(out var length)) { value = default; return; }
        var array = new T?[length];
        for (var i = 0; i < length; i++) elementFormatter.Deserialize(ref reader, ref array[i]);
        value = array;
    }
}

[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class ContextMemoryFormatter<T> : MemoryPackFormatter<Memory<T?>>
{
    readonly MemoryPackFormatter<T> elementFormatter;
    public ContextMemoryFormatter(MemoryPackFormatter<T> elementFormatter) => this.elementFormatter = elementFormatter;

    public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref Memory<T?> value)
    {
        writer.WriteCollectionHeader(value.Length);
        foreach (ref var item in value.Span) elementFormatter.Serialize(ref writer, ref item);
    }

    public override void Deserialize(ref MemoryPackReader reader, scoped ref Memory<T?> value)
    {
        if (!reader.TryReadCollectionHeader(out var length)) { value = default; return; }
        var array = new T?[length];
        for (var i = 0; i < length; i++) elementFormatter.Deserialize(ref reader, ref array[i]);
        value = array;
    }
}

[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class ContextReadOnlyMemoryFormatter<T> : MemoryPackFormatter<ReadOnlyMemory<T?>>
{
    readonly MemoryPackFormatter<T> elementFormatter;
    public ContextReadOnlyMemoryFormatter(MemoryPackFormatter<T> elementFormatter) => this.elementFormatter = elementFormatter;

    public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref ReadOnlyMemory<T?> value)
    {
        writer.WriteCollectionHeader(value.Length);
        foreach (ref readonly var item in value.Span)
        {
            var local = item;
            elementFormatter.Serialize(ref writer, ref local);
        }
    }

    public override void Deserialize(ref MemoryPackReader reader, scoped ref ReadOnlyMemory<T?> value)
    {
        if (!reader.TryReadCollectionHeader(out var length)) { value = default; return; }
        var array = new T?[length];
        for (var i = 0; i < length; i++) elementFormatter.Deserialize(ref reader, ref array[i]);
        value = array;
    }
}

[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class ContextReadOnlySequenceFormatter<T> : MemoryPackFormatter<ReadOnlySequence<T?>>
{
    readonly MemoryPackFormatter<T> elementFormatter;
    public ContextReadOnlySequenceFormatter(MemoryPackFormatter<T> elementFormatter) => this.elementFormatter = elementFormatter;

    public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref ReadOnlySequence<T?> value)
    {
        writer.WriteCollectionHeader(checked((int)value.Length));
        foreach (var memory in value)
        {
            foreach (ref readonly var item in memory.Span)
            {
                var local = item;
                elementFormatter.Serialize(ref writer, ref local);
            }
        }
    }

    public override void Deserialize(ref MemoryPackReader reader, scoped ref ReadOnlySequence<T?> value)
    {
        if (!reader.TryReadCollectionHeader(out var length)) { value = default; return; }
        var array = new T?[length];
        for (var i = 0; i < length; i++) elementFormatter.Deserialize(ref reader, ref array[i]);
        value = new ReadOnlySequence<T?>(array);
    }
}

[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class ContextKeyValuePairFormatter<TKey, TValue> : MemoryPackFormatter<KeyValuePair<TKey?, TValue?>>
{
    readonly MemoryPackFormatter<TKey> keyFormatter;
    readonly MemoryPackFormatter<TValue> valueFormatter;

    public ContextKeyValuePairFormatter(MemoryPackFormatter<TKey> keyFormatter, MemoryPackFormatter<TValue> valueFormatter)
    {
        this.keyFormatter = keyFormatter;
        this.valueFormatter = valueFormatter;
    }

    public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref KeyValuePair<TKey?, TValue?> value)
        => KeyValuePairFormatter.Serialize(keyFormatter, valueFormatter, ref writer, value);

    public override void Deserialize(ref MemoryPackReader reader, scoped ref KeyValuePair<TKey?, TValue?> value)
    {
        KeyValuePairFormatter.Deserialize(keyFormatter, valueFormatter, ref reader, out var key, out var itemValue);
        value = new KeyValuePair<TKey?, TValue?>(key, itemValue);
    }
}

[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class ContextLazyFormatter<T> : MemoryPackFormatter<Lazy<T?>>
{
    readonly MemoryPackFormatter<T> valueFormatter;
    public ContextLazyFormatter(MemoryPackFormatter<T> valueFormatter) => this.valueFormatter = valueFormatter;

    public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref Lazy<T?>? value)
    {
        if (value is null) { writer.WriteNullObjectHeader(); return; }
        writer.WriteObjectHeader(1);
        var item = value.Value;
        valueFormatter.Serialize(ref writer, ref item);
    }

    public override void Deserialize(ref MemoryPackReader reader, scoped ref Lazy<T?>? value)
    {
        if (!reader.TryReadObjectHeader(out var count)) { value = null; return; }
        if (count != 1) MemoryPackSerializationException.ThrowInvalidPropertyCount(1, count);
        T? item = default;
        valueFormatter.Deserialize(ref reader, ref item);
        value = new Lazy<T?>(item);
    }
}

[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class ContextDictionaryFormatter<TKey, TValue> : MemoryPackFormatter<Dictionary<TKey, TValue?>>
    where TKey : notnull
{
    readonly MemoryPackFormatter<TKey> keyFormatter;
    readonly MemoryPackFormatter<TValue> valueFormatter;

    public ContextDictionaryFormatter(MemoryPackFormatter<TKey> keyFormatter, MemoryPackFormatter<TValue> valueFormatter)
    {
        this.keyFormatter = keyFormatter;
        this.valueFormatter = valueFormatter;
    }

    public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref Dictionary<TKey, TValue?>? value)
    {
        if (value is null)
        {
            writer.WriteNullCollectionHeader();
            return;
        }

        writer.WriteCollectionHeader(value.Count);
        foreach (var item in value)
        {
            KeyValuePairFormatter.Serialize(keyFormatter, valueFormatter, ref writer, item!);
        }
    }

    public override void Deserialize(ref MemoryPackReader reader, scoped ref Dictionary<TKey, TValue?>? value)
    {
        if (!reader.TryReadCollectionHeader(out var length))
        {
            value = null;
            return;
        }

        value ??= new Dictionary<TKey, TValue?>(length);
        value.Clear();
        for (var i = 0; i < length; i++)
        {
            KeyValuePairFormatter.Deserialize(keyFormatter, valueFormatter, ref reader, out var key, out var itemValue);
            value.Add(key!, itemValue);
        }
    }
}

[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class ContextGenericCollectionFormatter<TCollection, TElement> : MemoryPackFormatter<TCollection>
    where TCollection : ICollection<TElement?>, new()
{
    readonly MemoryPackFormatter<TElement> elementFormatter;

    public ContextGenericCollectionFormatter(MemoryPackFormatter<TElement> elementFormatter)
    {
        this.elementFormatter = elementFormatter;
    }

    public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref TCollection? value)
    {
        if (value is null)
        {
            writer.WriteNullCollectionHeader();
            return;
        }

        writer.WriteCollectionHeader(value.Count);
        foreach (var item in value)
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

        var collection = new TCollection();
        for (var i = 0; i < length; i++)
        {
            TElement? item = default;
            elementFormatter.Deserialize(ref reader, ref item);
            collection.Add(item);
        }
        value = collection;
    }
}

[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class ContextGenericSetFormatter<TSet, TElement> : MemoryPackFormatter<TSet>
    where TSet : ISet<TElement?>, new()
{
    readonly MemoryPackFormatter<TElement> elementFormatter;

    public ContextGenericSetFormatter(MemoryPackFormatter<TElement> elementFormatter)
    {
        this.elementFormatter = elementFormatter;
    }

    public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref TSet? value)
    {
        if (value is null)
        {
            writer.WriteNullCollectionHeader();
            return;
        }

        writer.WriteCollectionHeader(value.Count);
        foreach (var item in value)
        {
            var local = item;
            elementFormatter.Serialize(ref writer, ref local);
        }
    }

    public override void Deserialize(ref MemoryPackReader reader, scoped ref TSet? value)
    {
        if (!reader.TryReadCollectionHeader(out var length))
        {
            value = default;
            return;
        }

        var set = new TSet();
        for (var i = 0; i < length; i++)
        {
            TElement? item = default;
            elementFormatter.Deserialize(ref reader, ref item);
            set.Add(item);
        }
        value = set;
    }
}

[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class ContextGenericDictionaryFormatter<TDictionary, TKey, TValue> : MemoryPackFormatter<TDictionary>
    where TKey : notnull
    where TDictionary : IDictionary<TKey, TValue?>, new()
{
    readonly MemoryPackFormatter<TKey> keyFormatter;
    readonly MemoryPackFormatter<TValue> valueFormatter;

    public ContextGenericDictionaryFormatter(MemoryPackFormatter<TKey> keyFormatter, MemoryPackFormatter<TValue> valueFormatter)
    {
        this.keyFormatter = keyFormatter;
        this.valueFormatter = valueFormatter;
    }

    public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref TDictionary? value)
    {
        if (value is null)
        {
            writer.WriteNullCollectionHeader();
            return;
        }

        writer.WriteCollectionHeader(value.Count);
        foreach (var item in value)
        {
            KeyValuePairFormatter.Serialize(keyFormatter, valueFormatter, ref writer, item!);
        }
    }

    public override void Deserialize(ref MemoryPackReader reader, scoped ref TDictionary? value)
    {
        if (!reader.TryReadCollectionHeader(out var length))
        {
            value = default;
            return;
        }

        var dictionary = new TDictionary();
        for (var i = 0; i < length; i++)
        {
            KeyValuePairFormatter.Deserialize(keyFormatter, valueFormatter, ref reader, out var key, out var itemValue);
            dictionary.Add(key!, itemValue);
        }
        value = dictionary;
    }
}

[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class ContextNullableFormatter<T> : MemoryPackFormatter<T?>
    where T : struct
{
    readonly MemoryPackFormatter<T> valueFormatter;

    public ContextNullableFormatter(MemoryPackFormatter<T> valueFormatter)
    {
        this.valueFormatter = valueFormatter;
    }

    public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref T? value)
    {
        if (!value.HasValue)
        {
            writer.WriteNullObjectHeader();
            return;
        }

        writer.WriteObjectHeader(1);
        var item = value.GetValueOrDefault();
        valueFormatter.Serialize(ref writer, ref item);
    }

    public override void Deserialize(ref MemoryPackReader reader, scoped ref T? value)
    {
        if (!reader.TryReadObjectHeader(out var count))
        {
            value = null;
            return;
        }

        if (count != 1)
        {
            MemoryPackSerializationException.ThrowInvalidPropertyCount(1, count);
        }

        T item = default;
        valueFormatter.Deserialize(ref reader, ref item);
        value = item;
    }
}

[EditorBrowsable(EditorBrowsableState.Never)]
public sealed partial class ContextTypeFormatter : MemoryPackFormatter<Type>
{
    readonly IReadOnlyDictionary<string, Type> types;

    public ContextTypeFormatter(IReadOnlyDictionary<string, Type> types)
    {
        this.types = types;
    }

    [GeneratedRegex(@", Version=\d+.\d+.\d+.\d+, Culture=[\w-]+, PublicKeyToken=(?:null|[a-f0-9]{16})")]
    private static partial Regex ShortTypeNameRegex();

    internal static string GetWireName(Type type)
    {
        return ShortTypeNameRegex().Replace(type.AssemblyQualifiedName!, "");
    }

    public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref Type? value)
    {
        if (value is null)
        {
            writer.WriteNullCollectionHeader();
            return;
        }

        var typeName = GetWireName(value);
        if (!types.TryGetValue(typeName, out var registered) || registered != value)
        {
            MemoryPackSerializationException.ThrowMessage($"Type is not registered in this MemoryPackSerializerContext: {typeName}");
        }
        writer.WriteString(typeName);
    }

    public override void Deserialize(ref MemoryPackReader reader, scoped ref Type? value)
    {
        var typeName = reader.ReadString();
        if (typeName is null)
        {
            value = null;
            return;
        }

        if (!types.TryGetValue(typeName, out value))
        {
            MemoryPackSerializationException.ThrowMessage($"Type is not registered in this MemoryPackSerializerContext: {typeName}");
        }
    }
}

#endif
