using Benchmark.BenchmarkNetUtilities;
using Benchmark.Micro;
using Benchmark.Models;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Jobs;
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

//[GenericTypeArguments(typeof(MyClass))]


[GenericTypeArguments(typeof(int))]
[GenericTypeArguments(typeof(Vector3[]))]
[GenericTypeArguments(typeof(JsonResponseModel))]
[GenericTypeArguments(typeof(NeuralNetworkLayerModel))]
[CategoriesColumn]
[PayloadColumn]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public class SerializeTest<T> : SerializerTestBase<T>
{
    ArrayBufferWriter<byte> writer;
    MemoryStream stream;
    Utf8JsonWriter jsonWriter;
    //SerializerSessionPool pool;
    //Serializer<T> orleansSerializer;

    public SerializeTest()
        : base()
    {
        // Orleans
        var serviceProvider = new ServiceCollection()
            .AddSerializer(builder => builder.AddAssembly(typeof(SerializeTest<>).Assembly))
            .BuildServiceProvider();
        //pool = serviceProvider.GetRequiredService<SerializerSessionPool>();
        //orleansSerializer = serviceProvider.GetRequiredService<Serializer<T>>();

        // create buffers
        stream = new MemoryStream();

        //var serialize1 = orleansSerializer.SerializeToArray(value);
        var serialize2 = MessagePackSerializer.Serialize(value);
        ProtoBuf.Serializer.Serialize(stream, value);
        var serialize3 = stream.ToArray();
        stream.Position = 0;
        var serialize4 = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(value));
        var serialize5 = MemoryPackSerializer.Serialize(value);

        writer = new ArrayBufferWriter<byte>(new[] { /* serialize1, */ serialize2, serialize3, serialize4, serialize5 }.Max(x => x.Length));
        jsonWriter = new Utf8JsonWriter(writer);
    }

    [Benchmark(Baseline = true), BenchmarkCategory(Categories.Bytes)]
    public byte[] MessagePackSerialize()
    {
        return MessagePackSerializer.Serialize(value);
    }

    [Benchmark, BenchmarkCategory(Categories.Bytes)]
    public byte[] MemoryPackSerialize()
    {
        return MemoryPackSerializer.Serialize(value);
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

    //[Benchmark, BenchmarkCategory(Categories.Bytes)]
    //public byte[] OrleansSerialize()
    //{
    //    return orleansSerializer.SerializeToArray(value);
    //}

    [Benchmark(Baseline = true), BenchmarkCategory(Categories.BufferWriter)]
    public void MessagePackBufferWriter()
    {
        MessagePackSerializer.Serialize(writer, value);
        writer.Clear();
    }

    [Benchmark, BenchmarkCategory(Categories.BufferWriter)]
    public void MemoryPackBufferWriter()
    {
        MemoryPackSerializer.Serialize(writer, value);
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

    // https://github.com/dotnet/orleans/pull/7984/
    // should use `session.PartialReset();`, not with using ?

    //[Benchmark, BenchmarkCategory(Categories.BufferWriter)]
    //public void OrleansBufferWriter()
    //{
    //    using (var session = pool.GetSession())
    //    {
    //        var writer = Writer.CreatePooled(session);
    //        try
    //        {
    //            orleansSerializer.Serialize(value, ref writer);
    //            writer.Commit();
    //        }
    //        finally
    //        {
    //            writer.Dispose();
    //        }
    //    }
    //}

    //[Benchmark, BenchmarkCategory(Categories.BufferWriter)]
    //public void OrleansBufferWriter2()
    //{
    //    using (var session = pool.GetSession())
    //    {
    //        // writer is ArrayBufferWriter<byte>
    //        var writer2 = writer.CreateWriter(session);
    //        try
    //        {
    //            orleansSerializer.Serialize(value, ref writer2);
    //            writer2.Commit();
    //        }
    //        finally
    //        {
    //            writer2.Dispose();
    //        }
    //        writer.Clear(); // reuse ArrayBufferWriter<byte>
    //    }
    //}
}
