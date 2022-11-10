using Benchmark.BenchmarkNetUtilities;
using Benchmark.Models;
using BinaryPack.Models;
using MemoryPack;
using MessagePack;
using Microsoft.Extensions.DependencyInjection;
using Orleans.Serialization;
using Orleans.Serialization.Buffers;
using Orleans.Serialization.Session;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Benchmark.Benchmarks;

//[GenericTypeArguments(typeof(int))]
//[GenericTypeArguments(typeof(Vector3[]))]
//[GenericTypeArguments(typeof(JsonResponseModel))]
//[GenericTypeArguments(typeof(NeuralNetworkLayerModel))]
public class DeserializeTest<T> : SerializerTestBase<T>
{
    Serializer<T> orleansSerializer;

    SerializerSession session;
    byte[] payloadMessagePack;
    byte[] payloadMemoryPack;
    byte[] payloadProtobuf;
    byte[] payloadJson;
    byte[] payloadOrleans;

    public DeserializeTest()
        : base()
    {
        // Orleans
        var serviceProvider = new ServiceCollection()
            .AddSerializer(builder => builder.AddAssembly(typeof(SerializeTest<>).Assembly))
            .BuildServiceProvider();
        session = serviceProvider.GetRequiredService<SerializerSessionPool>().GetSession();
        orleansSerializer = serviceProvider.GetRequiredService<Serializer<T>>();

        payloadOrleans = orleansSerializer.SerializeToArray(value);
        payloadMessagePack = MessagePackSerializer.Serialize(value);
        payloadMemoryPack = MemoryPackSerializer.Serialize(value);
        using var stream = new MemoryStream();
        ProtoBuf.Serializer.Serialize(stream, value);
        payloadProtobuf = stream.ToArray();
        payloadJson = JsonSerializer.SerializeToUtf8Bytes(value);
    }

    [Benchmark]
    public T MessagePackDeserialize()
    {
        return MessagePackSerializer.Deserialize<T>(payloadMessagePack);
    }

    [Benchmark(Baseline = true)]
    public T? MemoryPackDeserialize()
    {
        return MemoryPackSerializer.Deserialize<T>(payloadMemoryPack);
    }

    [Benchmark]
    public T ProtobufNetDeserialize()
    {
        return ProtoBuf.Serializer.Deserialize<T>(payloadProtobuf.AsSpan());
    }

    [Benchmark]
    public T? SystemTextJsonDeserialize()
    {
        return System.Text.Json.JsonSerializer.Deserialize<T>(payloadJson);
    }

    [Benchmark]
    public T OrleansDeserialize()
    {
        session.PartialReset();
        var reader = Reader.Create(payloadOrleans, session);
        return orleansSerializer.Deserialize(ref reader);
    }
}
