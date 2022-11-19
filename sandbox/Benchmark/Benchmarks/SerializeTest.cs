using Benchmark.BenchmarkNetUtilities;
using Benchmark.Micro;
using Benchmark.Models;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Order;
using BenchmarkDotNet.Running;
using BinaryPack.Models;
using MemoryPack;
using MemoryPack.Formatters;
using MessagePack;
using Microsoft.Extensions.DependencyInjection;
using Orleans;
using Orleans.Serialization;
using Orleans.Serialization.Buffers;
using Orleans.Serialization.Session;
using ProtoBuf;
using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;

namespace Benchmark.Benchmarks;

//[GenericTypeArguments(typeof(int))]
//[GenericTypeArguments(typeof(Vector3[]))]
//[GenericTypeArguments(typeof(JsonResponseModel))]
//[GenericTypeArguments(typeof(NeuralNetworkLayerModel))]
[CategoriesColumn]
[PayloadColumn]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
public class SerializeTest<T> : SerializerTestBase<T>
{
    ArrayBufferWriter<byte> writer;
    MemoryStream stream;
    Utf8JsonWriter jsonWriter;
    SerializerSession session;
    Serializer<T> orleansSerializer;

    public SerializeTest()
        : base()
    {
        // Orleans
        var serviceProvider = new ServiceCollection()
            .AddSerializer(builder => builder.AddAssembly(typeof(SerializeTest<>).Assembly))
            .BuildServiceProvider();
        session = serviceProvider.GetRequiredService<SerializerSessionPool>().GetSession();
        orleansSerializer = serviceProvider.GetRequiredService<Serializer<T>>();

        // create buffers
        stream = new MemoryStream();

        var serialize1 = orleansSerializer.SerializeToArray(value);
        var serialize2 = MessagePackSerializer.Serialize(value);
        ProtoBuf.Serializer.Serialize(stream, value);
        var serialize3 = stream.ToArray();
        stream.Position = 0;
        var serialize4 = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(value));
        var serialize5 = MemoryPackSerializer.Serialize(value);

        writer = new ArrayBufferWriter<byte>(new[] { /* serialize1, */ serialize2, serialize3, serialize4, serialize5 }.Max(x => x.Length));
        jsonWriter = new Utf8JsonWriter(writer);
    }

    [Benchmark, BenchmarkCategory(Categories.Bytes)]
    public byte[] MessagePackSerialize()
    {
        return MessagePackSerializer.Serialize(value);
    }

    [Benchmark(Baseline = true), BenchmarkCategory(Categories.Bytes)]
    public byte[] MemoryPackSerialize()
    {
        return MemoryPackSerializer.Serialize(value, MemoryPackSerializerOptions.Default);
    }

    [Benchmark, BenchmarkCategory(Categories.Bytes)]
    public byte[] MemoryPackSerializeUtf16()
    {
        return MemoryPackSerializer.Serialize(value, MemoryPackSerializerOptions.Utf16);
    }

    // requires T:new(), can't test it.
    //[Benchmark]
    //public byte[] BinaryPackSerialize()
    //{
    //    return BinaryPack.BinaryConverter.Serialize(value);
    //}

    [Benchmark, BenchmarkCategory(Categories.Bytes)]
    public byte[] ProtobufNetSerialize()
    {
        ProtoBuf.Serializer.Serialize(stream, value);
        var array = stream.ToArray();
        stream.Position = 0;
        return array;
    }

    [Benchmark, BenchmarkCategory(Categories.Bytes)]
    public byte[] SystemTextJsonSerialize()
    {
        System.Text.Json.JsonSerializer.Serialize(stream, value);
        var array = stream.ToArray();
        stream.Position = 0;
        return array;
    }

    [Benchmark, BenchmarkCategory(Categories.Bytes)]
    public byte[] OrleansSerialize()
    {
        return orleansSerializer.SerializeToArray(value);
    }

    [Benchmark, BenchmarkCategory(Categories.BufferWriter)]
    public void MessagePackBufferWriter()
    {
        MessagePackSerializer.Serialize(writer, value);
        writer.Clear();
    }

    [Benchmark(Baseline = true), BenchmarkCategory(Categories.BufferWriter)]
    public void MemoryPackBufferWriter()
    {
        MemoryPackSerializer.Serialize(writer, value, MemoryPackSerializerOptions.Default);
        writer.Clear();
    }

    [Benchmark, BenchmarkCategory(Categories.BufferWriter)]
    public void MemoryPackBufferWriterUtf16()
    {
        MemoryPackSerializer.Serialize(writer, value, MemoryPackSerializerOptions.Utf16);
        writer.Clear();
    }

    //[Benchmark]
    //public void BinaryPackStream()
    //{
    //    BinaryPack.BinaryConverter.Serialize(value, stream);
    //    stream.Position = 0;
    //}

    [Benchmark, BenchmarkCategory(Categories.BufferWriter)]
    public void ProtobufNetBufferWriter()
    {
        ProtoBuf.Serializer.Serialize(writer, value);
        writer.Clear();
    }

    [Benchmark, BenchmarkCategory(Categories.BufferWriter)]
    public void SystemTextJsonBufferWriter()
    {
        System.Text.Json.JsonSerializer.Serialize(jsonWriter, value);
        jsonWriter.Flush();
        writer.Clear();
        jsonWriter.Reset(writer);
    }

    // benchmark code is used orleans one. https://github.com/dotnet/orleans/pull/7984/

    [Benchmark, BenchmarkCategory(Categories.BufferWriter)]
    public void OrleansWriterPooledArrayBufferWriter()
    {
        var writer = Writer.CreatePooled(session);
        try
        {
            orleansSerializer.Serialize(value, ref writer);
        }
        finally
        {
            writer.Dispose();
            session.PartialReset();
        }
    }

    [Benchmark, BenchmarkCategory(Categories.BufferWriter)]
    public void OrleansWriterArrayBufferWriter()
    {
        var writer = this.writer.CreateWriter(session);
        try
        {
            orleansSerializer.Serialize(value, ref writer);
        }
        finally
        {
            writer.Dispose();
            session.PartialReset();
        }

        this.writer.Clear(); // clear ArrayBufferWriter<byte>
    }
}
