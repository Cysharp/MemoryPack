using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

#nullable enable
namespace MemoryPack.Formatters {

public sealed class GenericCollectionFormatter<TCollection, TElement> : MemoryPackFormatter<TCollection?>
    where TCollection : ICollection<TElement?>, new()
{
    public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref TCollection? value)
    {
        if (value == null)
        {
            writer.WriteNullCollectionHeader();
            return;
        }

        var formatter = writer.GetFormatter<TElement?>();

        writer.WriteCollectionHeader(value.Count);
        foreach (var item in value)
        {
            var v = item;
            formatter.Serialize(ref writer, ref v);
        }
    }

    public override void Deserialize(ref MemoryPackReader reader, ref TCollection? value)
    {
        if (!reader.TryReadCollectionHeader(out var length))
        {
            value = default;
            return;
        }

        var formatter = reader.GetFormatter<TElement?>();

        var collection = new TCollection();
        for (int i = 0; i < length; i++)
        {
            TElement? v = default;
            formatter.Deserialize(ref reader, ref v);
            collection.Add(v);
        }

        value = collection;
    }
}

public sealed class GenericSetFormatter<TSet, TElement> : MemoryPackFormatter<TSet?>
    where TSet : ISet<TElement?>, new()
{
    public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref TSet? value)
    {
        if (value == null)
        {
            writer.WriteNullCollectionHeader();
            return;
        }

        var formatter = writer.GetFormatter<TElement?>();

        writer.WriteCollectionHeader(value.Count);
        foreach (var item in value)
        {
            var v = item;
            formatter.Serialize(ref writer, ref v);
        }
    }

    public override void Deserialize(ref MemoryPackReader reader, ref TSet? value)
    {
        if (!reader.TryReadCollectionHeader(out var length))
        {
            value = default;
            return;
        }

        var formatter = reader.GetFormatter<TElement?>();

        var collection = new TSet();
        for (int i = 0; i < length; i++)
        {
            TElement? v = default;
            formatter.Deserialize(ref reader, ref v);
            collection.Add(v);
        }

        value = collection;
    }
}

public sealed class GenericDictionaryFormatter<TDictionary, TKey, TValue> : MemoryPackFormatter<TDictionary?>
    where TKey : notnull
    where TDictionary : IDictionary<TKey, TValue?>, new()
{
    static GenericDictionaryFormatter()
    {
        if (!MemoryPackFormatterProvider.IsRegistered<KeyValuePair<TKey, TValue?>>())
        {
            MemoryPackFormatterProvider.Register(new KeyValuePairFormatter<TKey, TValue?>());
        }
    }

    public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref TDictionary? value)
    {
        if (value == null)
        {
            writer.WriteNullCollectionHeader();
            return;
        }

        var formatter = writer.GetFormatter<KeyValuePair<TKey, TValue?>>();

        writer.WriteCollectionHeader(value.Count);
        foreach (var item in value)
        {
            var v = item;
            formatter.Serialize(ref writer, ref v);
        }
    }

    public override void Deserialize(ref MemoryPackReader reader, ref TDictionary? value)
    {
        if (!reader.TryReadCollectionHeader(out var length))
        {
            value = default;
            return;
        }

        var formatter = reader.GetFormatter<KeyValuePair<TKey, TValue?>>();

        var collection = new TDictionary();
        for (int i = 0; i < length; i++)
        {
            KeyValuePair<TKey, TValue?> v = default;
            formatter.Deserialize(ref reader, ref v);
            collection.Add(v);
        }

        value = collection;
    }
}

}