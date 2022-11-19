#nullable disable

using Benchmark.BenchmarkNetUtilities;
using Benchmark.Models;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Order;
using MemoryPack;
using MessagePack;
using Microsoft.Extensions.DependencyInjection;
using Orleans.Serialization;
using Orleans.Serialization.Buffers;
using Orleans.Serialization.Session;
using System.Buffers;
using System.Text;
using System.Text.Json;

namespace Benchmark.Benchmarks;

[GenericTypeArguments(typeof(Question))]
[GenericTypeArguments(typeof(Answer))]
[GenericTypeArguments(typeof(User))]
[GenericTypeArguments(typeof(List<Question>))]
[GenericTypeArguments(typeof(List<Answer>))]
[GenericTypeArguments(typeof(List<User>))]
[GenericTypeArguments(typeof(Dictionary<string, Question>))]
[GenericTypeArguments(typeof(Dictionary<string, Answer>))]
[GenericTypeArguments(typeof(Dictionary<string, User>))]
[CategoriesColumn]
[PayloadColumn]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
public class JilBenchmark<T>
{
    static Random Rand;

    static object MakeSingleObject(Type t)
    {
        var ret = Activator.CreateInstance(t);
        foreach (var p in t.GetProperties())
        {
            var propType = p.PropertyType;
            var val = propType.RandomValue(Rand);

            p.SetValue(ret, val);
        }

        return ret;
    }

    static object MakeListObject(Type t)
    {
        var asList = typeof(List<>).MakeGenericType(t);

        var ret = asList.RandomValue(Rand);

        // top level can't be null
        if (ret == null)
        {
            return MakeListObject(t);
        }

        return ret;
    }

    static object MakeDictionaryObject(Type t)
    {
        var asDictionary = typeof(Dictionary<,>).MakeGenericType(typeof(string), t);
        var ret = Activator.CreateInstance(asDictionary);
        var add = asDictionary.GetMethod("Add");

        var len = Rand.Next(30) + 20;
        for (var i = 0; i < len; i++)
        {
            var key = (string)typeof(string).RandomValue(Rand);
            if (key == null)
            {
                i--;
                continue;
            }

            var val = t.RandomValue(Rand);

            add.Invoke(ret, new object[] { key, val });
        }

        return ret;
    }


    static void ResetRand()
    {
        Rand = new Random(314159265);
    }

    T value;
    ArrayBufferWriter<byte> writer;
    MemoryStream stream;
    Utf8JsonWriter jsonWriter;
    SerializerSession session;
    Serializer<T> orleansSerializer;
    byte[] payloadMessagePack;
    byte[] payloadMemoryPack;
    byte[] payloadMemoryPackUtf16;
    byte[] payloadProtobuf;
    byte[] payloadJson;
    byte[] payloadOrleans;
    Stream payloadStreamMessagePack;
    Stream payloadStreamMemoryPack;
    Stream payloadStreamMemoryPackUtf16;
    Stream payloadStreamProtobuf;
    Stream payloadStreamJson;
    Stream payloadStreamOrleans;

    public JilBenchmark()
    {
        ResetRand();
        if (!typeof(T).IsGenericType)
        {
            value = (T)MakeSingleObject(typeof(T));
        }
        else
        {
            if (typeof(T).GetGenericTypeDefinition() == typeof(Dictionary<,>))
            {
                value = (T)MakeDictionaryObject(typeof(T).GetGenericArguments()[1]);
            }
            else if (typeof(T).GetGenericTypeDefinition() == typeof(List<>))
            {
                value = (T)MakeListObject(typeof(T).GetGenericArguments()[0]);
            }
        }

        // Orleans
        var serviceProvider = new ServiceCollection()
            .AddSerializer(builder => builder.AddAssembly(typeof(SerializeTest<>).Assembly))
            .BuildServiceProvider();
        session = serviceProvider.GetRequiredService<SerializerSessionPool>().GetSession();
        orleansSerializer = serviceProvider.GetRequiredService<Serializer<T>>();

        // create buffers
        stream = new MemoryStream();

        payloadOrleans = orleansSerializer.SerializeToArray(value);
        payloadMessagePack = MessagePackSerializer.Serialize(value);
        ProtoBuf.Serializer.Serialize(stream, value);
        payloadProtobuf = stream.ToArray();
        stream.Position = 0;
        payloadJson = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(value));
        payloadMemoryPack = MemoryPackSerializer.Serialize(value);
        payloadMemoryPackUtf16 = MemoryPackSerializer.Serialize(value, MemoryPackSerializerOptions.Utf16);

        writer = new ArrayBufferWriter<byte>(payloadJson.Length);
        jsonWriter = new Utf8JsonWriter(writer);

        payloadStreamMessagePack = new MemoryStream(payloadMessagePack, 0, payloadMessagePack.Length, writable: false, publiclyVisible: false);
        payloadStreamMemoryPack = new MemoryStream(payloadMemoryPack, 0, payloadMemoryPack.Length, writable: false, publiclyVisible: false);
        payloadStreamMemoryPackUtf16 = new MemoryStream(payloadMemoryPackUtf16, 0, payloadMemoryPackUtf16.Length, writable: false, publiclyVisible: false);
        payloadStreamProtobuf = new MemoryStream(payloadProtobuf, 0, payloadProtobuf.Length, writable: false, publiclyVisible: false);
        payloadStreamJson = new MemoryStream(payloadJson, 0, payloadJson.Length, writable: false, publiclyVisible: false);
        payloadStreamOrleans = new MemoryStream(payloadOrleans, 0, payloadOrleans.Length, writable: false, publiclyVisible: false);
    }

    [Benchmark, BenchmarkCategory(Categories.Serialize, Categories.Bytes)]
    public byte[] MessagePackSerialize()
    {
        return MessagePackSerializer.Serialize(value);
    }

    [Benchmark, BenchmarkCategory(Categories.Serialize, Categories.Bytes)]
    public byte[] MemoryPackSerialize()
    {
        return MemoryPackSerializer.Serialize(value, MemoryPackSerializerOptions.Default);
    }

    [Benchmark, BenchmarkCategory(Categories.Serialize, Categories.Bytes)]
    public byte[] MemoryPackSerializeUtf16()
    {
        return MemoryPackSerializer.Serialize(value, MemoryPackSerializerOptions.Utf16);
    }

    [Benchmark, BenchmarkCategory(Categories.Serialize, Categories.Bytes)]
    public byte[] ProtobufNetSerialize()
    {
        ProtoBuf.Serializer.Serialize(stream, value);
        var array = stream.ToArray();
        stream.Position = 0;
        return array;
    }

    [Benchmark, BenchmarkCategory(Categories.Serialize, Categories.Bytes)]
    public byte[] SystemTextJsonSerialize()
    {
        System.Text.Json.JsonSerializer.Serialize(stream, value);
        var array = stream.ToArray();
        stream.Position = 0;
        return array;
    }

    [Benchmark, BenchmarkCategory(Categories.Serialize, Categories.Bytes)]
    public byte[] OrleansSerialize()
    {
        return orleansSerializer.SerializeToArray(value);
    }

    [Benchmark, BenchmarkCategory(Categories.Serialize, Categories.BufferWriter)]
    public void MessagePackBufferWriter()
    {
        MessagePackSerializer.Serialize(writer, value);
        writer.Clear();
    }

    [Benchmark, BenchmarkCategory(Categories.Serialize, Categories.BufferWriter)]
    public void MemoryPackBufferWriter()
    {
        MemoryPackSerializer.Serialize(writer, value);
        writer.Clear();
    }

    [Benchmark, BenchmarkCategory(Categories.Serialize, Categories.BufferWriter)]
    public void MemoryPackBufferWriterUtf16()
    {
        MemoryPackSerializer.Serialize(writer, value, MemoryPackSerializerOptions.Utf16);
        writer.Clear();
    }

    [Benchmark, BenchmarkCategory(Categories.Serialize, Categories.BufferWriter)]
    public void ProtobufNetBufferWriter()
    {
        ProtoBuf.Serializer.Serialize(writer, value);
        writer.Clear();
    }

    [Benchmark, BenchmarkCategory(Categories.Serialize, Categories.BufferWriter)]
    public void SystemTextJsonBufferWriter()
    {
        System.Text.Json.JsonSerializer.Serialize(jsonWriter, value);
        jsonWriter.Flush();
        writer.Clear();
        jsonWriter.Reset(writer);
    }

    [Benchmark, BenchmarkCategory(Categories.Serialize, Categories.BufferWriter)]
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

    [Benchmark, BenchmarkCategory(Categories.Serialize, Categories.BufferWriter)]
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

    [Benchmark, BenchmarkCategory(Categories.Serialize, Categories.Stream)]
    public void MessagePackStream()
    {
        MessagePackSerializer.Serialize(stream, value);
        stream.Position = 0;
    }

    [Benchmark, BenchmarkCategory(Categories.Serialize, Categories.Stream)]
    public void MemoryPackStream()
    {
        MemoryPackSerializer.SerializeAsync(stream, value, MemoryPackSerializerOptions.Default).GetAwaiter().GetResult();
        stream.Position = 0;
    }

    [Benchmark, BenchmarkCategory(Categories.Serialize, Categories.Stream)]
    public void MemoryPackStreamUtf16()
    {
        MemoryPackSerializer.SerializeAsync(stream, value, MemoryPackSerializerOptions.Utf16).GetAwaiter().GetResult();
        stream.Position = 0;
    }

    [Benchmark, BenchmarkCategory(Categories.Serialize, Categories.Stream)]
    public void ProtobufNetStram()
    {
        ProtoBuf.Serializer.Serialize(stream, value);
        stream.Position = 0;
    }

    [Benchmark, BenchmarkCategory(Categories.Serialize, Categories.Stream)]
    public void SystemTextJsonStream()
    {
        System.Text.Json.JsonSerializer.Serialize(stream, value);
        stream.Position = 0;
    }

    [Benchmark, BenchmarkCategory(Categories.Serialize, Categories.Stream)]
    public void OrleansWriterStream()
    {
        var writer = Writer.Create(stream, session);
        try
        {
            orleansSerializer.Serialize(value, ref writer);
        }
        finally
        {
            writer.Dispose();
            session.PartialReset();
        }
        stream.Position = 0;
    }

    // deserialize

    [Benchmark, BenchmarkCategory(Categories.Deserialize, Categories.Bytes)]
    public T MessagePackDeserialize()
    {
        return MessagePackSerializer.Deserialize<T>(payloadMessagePack);
    }

    [Benchmark, BenchmarkCategory(Categories.Deserialize, Categories.Bytes)]
    public T MemoryPackDeserialize()
    {
        return MemoryPackSerializer.Deserialize<T>(payloadMemoryPack);
    }

    [Benchmark, BenchmarkCategory(Categories.Deserialize, Categories.Bytes)]
    public T MemoryPackDeserializeUtf16()
    {
        return MemoryPackSerializer.Deserialize<T>(payloadMemoryPackUtf16);
    }

    [Benchmark, BenchmarkCategory(Categories.Deserialize, Categories.Bytes)]
    public T ProtobufNetDeserialize()
    {
        return ProtoBuf.Serializer.Deserialize<T>(payloadProtobuf.AsSpan());
    }

    [Benchmark, BenchmarkCategory(Categories.Deserialize, Categories.Bytes)]
    public T SystemTextJsonDeserialize()
    {
        return System.Text.Json.JsonSerializer.Deserialize<T>(payloadJson);
    }

    [Benchmark, BenchmarkCategory(Categories.Deserialize, Categories.Bytes)]
    public T OrleansDeserialize()
    {
        try
        {
            var reader = Reader.Create(payloadOrleans, session);
            return orleansSerializer.Deserialize(ref reader);
        }
        finally
        {
            session.PartialReset();
        }
    }

    [Benchmark, BenchmarkCategory(Categories.Deserialize, Categories.Stream)]
    public T MessagePackDeserializeStream()
    {
        payloadStreamMessagePack.Position = 0;
        return MessagePackSerializer.DeserializeAsync<T>(payloadStreamMessagePack).GetAwaiter().GetResult();
    }

    [Benchmark, BenchmarkCategory(Categories.Deserialize, Categories.Stream)]
    public T MemoryPackDeserializeStream()
    {
        payloadStreamMemoryPack.Position = 0;
        return MemoryPackSerializer.DeserializeAsync<T>(payloadStreamMemoryPack).GetAwaiter().GetResult();
    }

    [Benchmark, BenchmarkCategory(Categories.Deserialize, Categories.Stream)]
    public T MemoryPackDeserializeStreamUtf16()
    {
        payloadStreamMemoryPackUtf16.Position = 0;
        return MemoryPackSerializer.DeserializeAsync<T>(payloadStreamMemoryPackUtf16).GetAwaiter().GetResult();
    }

    [Benchmark, BenchmarkCategory(Categories.Deserialize, Categories.Stream)]
    public T ProtobufNetDeserializeStream()
    {
        payloadStreamProtobuf.Position = 0;
        return ProtoBuf.Serializer.Deserialize<T>(payloadStreamProtobuf);
    }

    [Benchmark, BenchmarkCategory(Categories.Deserialize, Categories.Stream)]
    public T SystemTextJsonDeserializeStream()
    {
        payloadStreamJson.Position = 0;
        return System.Text.Json.JsonSerializer.DeserializeAsync<T>(payloadStreamJson).GetAwaiter().GetResult();
    }

    [Benchmark, BenchmarkCategory(Categories.Deserialize, Categories.Stream)]
    public T OrleansDeserializeStream()
    {
        try
        {
            payloadStreamOrleans.Position = 0;
            var reader = Reader.Create(payloadStreamOrleans, session);
            return orleansSerializer.Deserialize(ref reader);
        }
        finally
        {
            session.PartialReset();
        }
    }
}
