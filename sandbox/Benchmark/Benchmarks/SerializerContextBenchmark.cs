using System.Buffers;
using MemoryPack;

namespace Benchmark.Benchmarks;

[MemoryDiagnoser]
public class SerializerContextBenchmark
{
    readonly BenchmarkSimpleDto simple = new()
    {
        Id = 42,
        Name = "MemoryPack"
    };

    readonly BenchmarkDto graph = new()
    {
        Id = 42,
        Name = "MemoryPack",
        Child = new BenchmarkChild { Value = 10 },
        Children = Enumerable.Range(0, 32).Select(x => new BenchmarkChild { Value = x }).ToArray(),
        ChildList = Enumerable.Range(0, 32).Select(x => (BenchmarkChild?)new BenchmarkChild { Value = x }).ToList(),
        ChildMap = Enumerable.Range(0, 32).ToDictionary(x => x, x => (BenchmarkChild?)new BenchmarkChild { Value = x })
    };

    readonly BenchmarkMemoryPackContext context = BenchmarkMemoryPackContext.Default;
    readonly BenchmarkChild[] array = Enumerable.Range(0, 32).Select(x => new BenchmarkChild { Value = x }).ToArray();
    readonly List<BenchmarkChild> list = Enumerable.Range(0, 32).Select(x => new BenchmarkChild { Value = x }).ToList();
    readonly Dictionary<int, BenchmarkChild> dictionary = Enumerable.Range(0, 32).ToDictionary(x => x, x => new BenchmarkChild { Value = x });
    readonly BenchmarkUnion union = new BenchmarkUnionValue { Value = 42 };
    readonly ArrayBufferWriter<byte> staticBuffer = new(4096);
    readonly ArrayBufferWriter<byte> contextBuffer = new(4096);
    byte[] simpleBytes = null!;
    byte[] graphBytes = null!;
    byte[] arrayBytes = null!;
    byte[] listBytes = null!;
    byte[] dictionaryBytes = null!;
    byte[] unionBytes = null!;
    ReadOnlySequence<byte> graphSequence;
    MemoryStream staticSerializeStream = null!;
    MemoryStream contextSerializeStream = null!;
    MemoryStream staticDeserializeStream = null!;
    MemoryStream contextDeserializeStream = null!;

    [GlobalSetup]
    public void Setup()
    {
        simpleBytes = MemoryPackSerializer.Serialize<BenchmarkSimpleDto, BenchmarkMemoryPackContext>(simple, context);
        graphBytes = MemoryPackSerializer.Serialize<BenchmarkDto, BenchmarkMemoryPackContext>(graph, context);
        arrayBytes = MemoryPackSerializer.Serialize<BenchmarkChild[], BenchmarkMemoryPackContext>(array, context);
        listBytes = MemoryPackSerializer.Serialize<List<BenchmarkChild>, BenchmarkMemoryPackContext>(list, context);
        dictionaryBytes = MemoryPackSerializer.Serialize<Dictionary<int, BenchmarkChild>, BenchmarkMemoryPackContext>(dictionary, context);
        unionBytes = MemoryPackSerializer.Serialize<BenchmarkUnion, BenchmarkMemoryPackContext>(union, context);
        graphSequence = new ReadOnlySequence<byte>(graphBytes);
        staticSerializeStream = new MemoryStream(graphBytes.Length);
        contextSerializeStream = new MemoryStream(graphBytes.Length);
        staticDeserializeStream = new MemoryStream(graphBytes, writable: false);
        contextDeserializeStream = new MemoryStream(graphBytes, writable: false);

        // Ensure first-use registration and context construction are excluded from steady-state measurements.
        _ = MemoryPackSerializer.Serialize(simple);
        _ = MemoryPackSerializer.Deserialize<BenchmarkSimpleDto>(simpleBytes);
        _ = MemoryPackSerializer.Serialize(graph);
        _ = MemoryPackSerializer.Serialize(array);
        _ = MemoryPackSerializer.Serialize(list);
        _ = MemoryPackSerializer.Serialize(dictionary);
        _ = MemoryPackSerializer.Serialize<BenchmarkUnion>(union);
        _ = MemoryPackSerializer.Deserialize<BenchmarkDto>(graphBytes);
        _ = MemoryPackSerializer.Deserialize<BenchmarkChild[]>(arrayBytes);
        _ = MemoryPackSerializer.Deserialize<List<BenchmarkChild>>(listBytes);
        _ = MemoryPackSerializer.Deserialize<Dictionary<int, BenchmarkChild>>(dictionaryBytes);
        _ = MemoryPackSerializer.Deserialize<BenchmarkUnion>(unionBytes);
    }

    [Benchmark(Baseline = true)]
    public byte[] StaticSerializeSimple() => MemoryPackSerializer.Serialize(simple);

    [Benchmark]
    public byte[] ContextSerializeSimple() => MemoryPackSerializer.Serialize<BenchmarkSimpleDto, BenchmarkMemoryPackContext>(simple, context);

    [Benchmark]
    public BenchmarkSimpleDto? StaticDeserializeSimple() => MemoryPackSerializer.Deserialize<BenchmarkSimpleDto>(simpleBytes);

    [Benchmark]
    public BenchmarkSimpleDto? ContextDeserializeSimple() => MemoryPackSerializer.Deserialize<BenchmarkSimpleDto, BenchmarkMemoryPackContext>(simpleBytes, context);

    [Benchmark]
    public byte[] StaticSerializeGraph() => MemoryPackSerializer.Serialize(graph);

    [Benchmark]
    public byte[] ContextSerializeGraph() => MemoryPackSerializer.Serialize<BenchmarkDto, BenchmarkMemoryPackContext>(graph, context);

    [Benchmark]
    public BenchmarkDto? StaticDeserializeGraph() => MemoryPackSerializer.Deserialize<BenchmarkDto>(graphBytes);

    [Benchmark]
    public BenchmarkDto? ContextDeserializeGraph() => MemoryPackSerializer.Deserialize<BenchmarkDto, BenchmarkMemoryPackContext>(graphBytes, context);

    [Benchmark]
    public int StaticSerializeBuffer()
    {
        staticBuffer.Clear();
        MemoryPackSerializer.Serialize(staticBuffer, graph);
        return staticBuffer.WrittenCount;
    }

    [Benchmark]
    public int ContextSerializeBuffer()
    {
        contextBuffer.Clear();
        MemoryPackSerializer.Serialize(contextBuffer, graph, context);
        return contextBuffer.WrittenCount;
    }

    [Benchmark]
    public byte[] StaticSerializeArray() => MemoryPackSerializer.Serialize(array);

    [Benchmark]
    public byte[] ContextSerializeArray() => MemoryPackSerializer.Serialize<BenchmarkChild[], BenchmarkMemoryPackContext>(array, context);

    [Benchmark]
    public byte[] StaticSerializeList() => MemoryPackSerializer.Serialize(list);

    [Benchmark]
    public byte[] ContextSerializeList() => MemoryPackSerializer.Serialize<List<BenchmarkChild>, BenchmarkMemoryPackContext>(list, context);

    [Benchmark]
    public byte[] StaticSerializeDictionary() => MemoryPackSerializer.Serialize(dictionary);

    [Benchmark]
    public byte[] ContextSerializeDictionary() => MemoryPackSerializer.Serialize<Dictionary<int, BenchmarkChild>, BenchmarkMemoryPackContext>(dictionary, context);

    [Benchmark]
    public byte[] StaticSerializeUnion() => MemoryPackSerializer.Serialize<BenchmarkUnion>(union);

    [Benchmark]
    public byte[] ContextSerializeUnion() => MemoryPackSerializer.Serialize<BenchmarkUnion, BenchmarkMemoryPackContext>(union, context);

    [Benchmark]
    public BenchmarkChild[]? StaticDeserializeArray() => MemoryPackSerializer.Deserialize<BenchmarkChild[]>(arrayBytes);

    [Benchmark]
    public BenchmarkChild[]? ContextDeserializeArray() => MemoryPackSerializer.Deserialize<BenchmarkChild[], BenchmarkMemoryPackContext>(arrayBytes, context);

    [Benchmark]
    public List<BenchmarkChild>? StaticDeserializeList() => MemoryPackSerializer.Deserialize<List<BenchmarkChild>>(listBytes);

    [Benchmark]
    public List<BenchmarkChild>? ContextDeserializeList() => MemoryPackSerializer.Deserialize<List<BenchmarkChild>, BenchmarkMemoryPackContext>(listBytes, context);

    [Benchmark]
    public Dictionary<int, BenchmarkChild>? StaticDeserializeDictionary() => MemoryPackSerializer.Deserialize<Dictionary<int, BenchmarkChild>>(dictionaryBytes);

    [Benchmark]
    public Dictionary<int, BenchmarkChild>? ContextDeserializeDictionary() => MemoryPackSerializer.Deserialize<Dictionary<int, BenchmarkChild>, BenchmarkMemoryPackContext>(dictionaryBytes, context);

    [Benchmark]
    public BenchmarkUnion? StaticDeserializeUnion() => MemoryPackSerializer.Deserialize<BenchmarkUnion>(unionBytes);

    [Benchmark]
    public BenchmarkUnion? ContextDeserializeUnion() => MemoryPackSerializer.Deserialize<BenchmarkUnion, BenchmarkMemoryPackContext>(unionBytes, context);

    [Benchmark]
    public BenchmarkDto? StaticDeserializeSequence() => MemoryPackSerializer.Deserialize<BenchmarkDto>(graphSequence);

    [Benchmark]
    public BenchmarkDto? ContextDeserializeSequence() => MemoryPackSerializer.Deserialize<BenchmarkDto, BenchmarkMemoryPackContext>(graphSequence, context);

    [Benchmark]
    public async ValueTask<long> StaticSerializeStream()
    {
        staticSerializeStream.Position = 0;
        staticSerializeStream.SetLength(0);
        await MemoryPackSerializer.SerializeAsync(staticSerializeStream, graph);
        return staticSerializeStream.Length;
    }

    [Benchmark]
    public async ValueTask<long> ContextSerializeStream()
    {
        contextSerializeStream.Position = 0;
        contextSerializeStream.SetLength(0);
        await MemoryPackSerializer.SerializeAsync(contextSerializeStream, graph, context);
        return contextSerializeStream.Length;
    }

    [Benchmark]
    public async ValueTask<BenchmarkDto?> StaticDeserializeStream()
    {
        staticDeserializeStream.Position = 0;
        return await MemoryPackSerializer.DeserializeAsync<BenchmarkDto>(staticDeserializeStream);
    }

    [Benchmark]
    public async ValueTask<BenchmarkDto?> ContextDeserializeStream()
    {
        contextDeserializeStream.Position = 0;
        return await MemoryPackSerializer.DeserializeAsync<BenchmarkDto, BenchmarkMemoryPackContext>(contextDeserializeStream, context);
    }
}

[MemoryDiagnoser]
public class SerializerContextConstructionBenchmark
{
    [Benchmark]
    public MemoryPackSerializerContext Construct() => new BenchmarkMemoryPackContext();
}

[MemoryPackable]
public partial class BenchmarkSimpleDto
{
    public int Id { get; set; }
    public string? Name { get; set; }
}

[MemoryPackable]
public partial class BenchmarkDto
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public BenchmarkChild? Child { get; set; }
    public BenchmarkChild?[]? Children { get; set; }
    public List<BenchmarkChild?>? ChildList { get; set; }
    public Dictionary<int, BenchmarkChild?>? ChildMap { get; set; }
}

[MemoryPackable]
public partial class BenchmarkChild
{
    public int Value { get; set; }
}

[MemoryPackable]
[MemoryPackUnion(0, typeof(BenchmarkUnionValue))]
public partial interface BenchmarkUnion
{
}

[MemoryPackable]
public partial class BenchmarkUnionValue : BenchmarkUnion
{
    public int Value { get; set; }
}

[MemoryPackSerializable(typeof(BenchmarkSimpleDto))]
[MemoryPackSerializable(typeof(BenchmarkDto))]
[MemoryPackSerializable(typeof(BenchmarkChild[]))]
[MemoryPackSerializable(typeof(List<BenchmarkChild>))]
[MemoryPackSerializable(typeof(Dictionary<int, BenchmarkChild>))]
[MemoryPackSerializable(typeof(BenchmarkUnion))]
public partial class BenchmarkMemoryPackContext : MemoryPackSerializerContext
{
}
