using System.Buffers;
using MemoryPack;

namespace Benchmark.Benchmarks;

[MemoryDiagnoser]
public class StaticPathBaselineBenchmark
{
    readonly StaticPathSimpleDto simple = new() { Id = 42, Name = "MemoryPack" };
    readonly StaticPathGraphDto graph = new()
    {
        Id = 42,
        Child = new StaticPathChild { Value = 10 },
        Children = Enumerable.Range(0, 32).Select(x => new StaticPathChild { Value = x }).ToArray(),
        ChildList = Enumerable.Range(0, 32).Select(x => (StaticPathChild?)new StaticPathChild { Value = x }).ToList(),
        ChildMap = Enumerable.Range(0, 32).ToDictionary(x => x, x => (StaticPathChild?)new StaticPathChild { Value = x }),
    };
    readonly ArrayBufferWriter<byte> buffer = new(4096);
    byte[] simpleBytes = null!;
    byte[] graphBytes = null!;
    ReadOnlySequence<byte> graphSequence;

    [GlobalSetup]
    public void Setup()
    {
        simpleBytes = MemoryPackSerializer.Serialize(simple);
        graphBytes = MemoryPackSerializer.Serialize(graph);
        graphSequence = new ReadOnlySequence<byte>(graphBytes);
        _ = MemoryPackSerializer.Deserialize<StaticPathSimpleDto>(simpleBytes);
        _ = MemoryPackSerializer.Deserialize<StaticPathGraphDto>(graphBytes);
    }

    [Benchmark]
    public byte[] SerializeSimple() => MemoryPackSerializer.Serialize(simple);

    [Benchmark]
    public StaticPathSimpleDto? DeserializeSimple() => MemoryPackSerializer.Deserialize<StaticPathSimpleDto>(simpleBytes);

    [Benchmark]
    public byte[] SerializeGraph() => MemoryPackSerializer.Serialize(graph);

    [Benchmark]
    public StaticPathGraphDto? DeserializeGraph() => MemoryPackSerializer.Deserialize<StaticPathGraphDto>(graphBytes);

    [Benchmark]
    public int SerializeBuffer()
    {
        buffer.Clear();
        MemoryPackSerializer.Serialize(buffer, graph);
        return buffer.WrittenCount;
    }

    [Benchmark]
    public StaticPathGraphDto? DeserializeSequence() => MemoryPackSerializer.Deserialize<StaticPathGraphDto>(graphSequence);
}

[MemoryPackable]
public partial class StaticPathSimpleDto
{
    public int Id { get; set; }
    public string? Name { get; set; }
}

[MemoryPackable]
public partial class StaticPathGraphDto
{
    public int Id { get; set; }
    public StaticPathChild? Child { get; set; }
    public StaticPathChild?[]? Children { get; set; }
    public List<StaticPathChild?>? ChildList { get; set; }
    public Dictionary<int, StaticPathChild?>? ChildMap { get; set; }
}

[MemoryPackable]
public partial class StaticPathChild
{
    public int Value { get; set; }
}
