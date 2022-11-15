using Benchmark.BenchmarkNetUtilities;
using BenchmarkDotNet.Configs;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Microsoft.Diagnostics.Tracing;
using Orleans.Serialization.Buffers;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Benchmark.Benchmarks;

[CategoriesColumn]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public class StaticDictionaryFormatterCheck
{
    Dictionary<string, int> target;
    IMemoryPackFormatter<Dictionary<string, int>> current;
    IMemoryPackFormatter<Dictionary<string, int>> improvement;

    Dictionary<string, int> dict;
    ArrayBufferWriter<byte> buffer;
    MemoryPackWriterOptionalState state;
    MemoryPackReaderOptionalState state2;
    byte[] bytes;

    public StaticDictionaryFormatterCheck()
    {
        target = Enumerable.Range(1, 100)
            .ToDictionary(x => x.ToString(), x => x);

        current = new DictionaryFormatter<string, int>();
        improvement = new DictionaryFormatter2<string, int>();

        dict = new Dictionary<string, int>(100);

        bytes = MemoryPackSerializer.Serialize(target);

        buffer = new ArrayBufferWriter<byte>(bytes.Length);

        state = MemoryPackWriterOptionalStatePool.Rent(null);
        state2 = MemoryPackReaderOptionalStatePool.Rent(null);
    }

    //[Benchmark, BenchmarkCategory(Categories.Serialize)]
    //public void SerializeCurrent()
    //{
    //    var writer = new MemoryPackWriter<ArrayBufferWriter<byte>>(ref buffer, state);
    //    current.Serialize(ref writer, ref target!);
    //    writer.Flush();
    //    buffer.Clear();
    //}

    //[Benchmark, BenchmarkCategory(Categories.Serialize)]
    //public void SerializeImprovement()
    //{
    //    var writer = new MemoryPackWriter<ArrayBufferWriter<byte>>(ref buffer, state);
    //    improvement.Serialize(ref writer, ref target!);
    //    writer.Flush();
    //    buffer.Clear();
    //}

    [Benchmark, BenchmarkCategory(Categories.Deserialize)]
    public void DeserializeCurrent()
    {
        var reader = new MemoryPackReader(bytes, state2);
        current.Deserialize(ref reader, ref dict!);
        reader.Dispose();
    }

    [Benchmark, BenchmarkCategory(Categories.Deserialize)]
    public void DeserializeImprovement()
    {
        var reader = new MemoryPackReader(bytes, state2);

        improvement.Deserialize(ref reader, ref dict!);
        reader.Dispose();
    }
}


[Preserve]
sealed class DictionaryFormatter<TKey, TValue> : MemoryPackFormatter<Dictionary<TKey, TValue?>>
    where TKey : notnull
{
    static DictionaryFormatter()
    {
        if (!MemoryPackFormatterProvider.IsRegistered<KeyValuePair<TKey, TValue?>>())
        {
            MemoryPackFormatterProvider.Register(new KeyValuePairFormatter<TKey, TValue?>());
        }
    }

    readonly IEqualityComparer<TKey>? equalityComparer;

    public DictionaryFormatter()
        : this(null)
    {

    }

    public DictionaryFormatter(IEqualityComparer<TKey>? equalityComparer)
    {
        this.equalityComparer = equalityComparer;
    }

    [Preserve]
    public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref Dictionary<TKey, TValue?>? value)
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

    [Preserve]
    public override void Deserialize(ref MemoryPackReader reader, scoped ref Dictionary<TKey, TValue?>? value)
    {
        if (!reader.TryReadCollectionHeader(out var length))
        {
            value = null;
            return;
        }

        if (value == null)
        {
            value = new Dictionary<TKey, TValue?>(length, equalityComparer);
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

[Preserve]
sealed class DictionaryFormatter2<TKey, TValue> : MemoryPackFormatter<Dictionary<TKey, TValue?>>
    where TKey : notnull
{
    readonly IEqualityComparer<TKey>? equalityComparer;

    public DictionaryFormatter2()
        : this(null)
    {

    }

    public DictionaryFormatter2(IEqualityComparer<TKey>? equalityComparer)
    {
        this.equalityComparer = equalityComparer;
    }

    [Preserve]
    public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref Dictionary<TKey, TValue?>? value)
    {
        if (value == null)
        {
            writer.WriteNullCollectionHeader();
            return;
        }

        var keyFormatter = writer.GetFormatter<TKey>();
        var valueFormatter = writer.GetFormatter<TValue>();

        writer.WriteCollectionHeader(value.Count);
        foreach (var item in value)
        {
            KeyValuePairFormatter.Serialize(keyFormatter!, valueFormatter!, ref writer, item!);
        }
    }

    [Preserve]
    public override void Deserialize(ref MemoryPackReader reader, scoped ref Dictionary<TKey, TValue?>? value)
    {
        if (!reader.TryReadCollectionHeader(out var length))
        {
            value = null;
            return;
        }

        if (value == null)
        {
            value = new Dictionary<TKey, TValue?>(length, equalityComparer);
        }
        else
        {
            value.Clear();
        }

        var keyFormatter = reader.GetFormatter<TKey>();
        var valueFormatter = reader.GetFormatter<TValue>();
        for (int i = 0; i < length; i++)
        {
            KeyValuePairFormatter.Deserialize(keyFormatter, valueFormatter, ref reader, out var k, out var v);
            value.Add(k!, v);
        }
    }
}
