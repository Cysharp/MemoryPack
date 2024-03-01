using Benchmark.BenchmarkNetUtilities;
using Benchmark.Models;
using MemoryPack;
using MemoryPack.Formatters;
using Orleans.Serialization.Buffers;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Benchmark.Benchmarks;

public class ListFormatterVsDirect
{
    List<MyClass> value;
    byte[] bytes;
    IMemoryPackFormatter<List<MyClass?>> formatter;
    ArrayBufferWriter<byte> buffer;
    MemoryPackWriterOptionalState state;
    MemoryPackReaderOptionalState state2;

    public ListFormatterVsDirect()
    {
        value = Enumerable.Range(0, 100)
            .Select(_ => new MyClass { X = 100, Y = 99999999, Z = 4444, FirstName = "Hoge Huga Tako", LastName = "あいうえおかきくけこ" })
            .ToList();
        bytes = MemoryPackSerializer.Serialize(value);
        formatter = new ListFormatter<MyClass>();
        buffer = new ArrayBufferWriter<byte>(bytes.Length);

        state = MemoryPackWriterOptionalStatePool.Rent(null);
        state2 = MemoryPackReaderOptionalStatePool.Rent(null);
    }

    [Benchmark, BenchmarkCategory(Categories.Serialize)]
    public void SerializeFormatter()
    {
        var writer = new MemoryPackWriter<ArrayBufferWriter<byte>>(ref buffer, state);
        formatter.Serialize(ref writer, ref value!);
        writer.Flush();
        buffer.Clear();
    }

    [Benchmark, BenchmarkCategory(Categories.Serialize)]
    public void SerializePackable()
    {
        var writer = new MemoryPackWriter<ArrayBufferWriter<byte>>(ref buffer, state);
        MemoryPack.Formatters.ListFormatter.SerializePackable(ref writer, value!);
        writer.Flush();
        buffer.Clear();
    }


    [Benchmark, BenchmarkCategory(Categories.Deserialize)]
    public void DeserializeFormatter()
    {
        List<MyClass?>? list = null;
        var reader = new MemoryPackReader(bytes, state2);
        //reader.ReadPackableArray
        // var a = MemoryPack.Formatters.ListFormatter.DeserializePackable<(ref reader);
        formatter.Deserialize(ref reader, ref list);
        reader.Dispose();
    }

    [Benchmark, BenchmarkCategory(Categories.Deserialize)]
    public void DeserializePackable()
    {
        List<MyClass?>? list = null;
        var reader = new MemoryPackReader(bytes, state2);
        ListFormatter.DeserializePackable(ref reader, ref list!);
        reader.Dispose();
    }
}
