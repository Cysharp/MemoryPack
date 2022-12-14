using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using MemoryPack;

var config = ManualConfig.CreateMinimumViable()
    .AddDiagnoser(MemoryDiagnoser.Default)
    // .AddColumn(StatisticColumn.OperationsPerSecond)
    //.AddExporter(DefaultExporters.Plain)
    .AddExporter(MarkdownExporter.Default)
    .AddJob(Job.Default.WithWarmupCount(1).WithIterationCount(1).WithRuntime(CoreRuntime.Core60))
    .AddJob(Job.Default.WithWarmupCount(1).WithIterationCount(1).WithRuntime(CoreRuntime.Core70));

BenchmarkRunner.Run<Net6Net7>(config);

[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByMethod)]
public class Net6Net7
{
    private Sample s = new();

    byte[] bin;

    public Net6Net7()
    {
        bin = MemoryPackSerializer.Serialize(s);
    }

    //[Benchmark]
    //public void SerializeDeserialize()
    //{
    //    MemoryPackSerializer.Deserialize<Sample>(MemoryPackSerializer.Serialize(s));
    //}

    [Benchmark]
    public byte[] Serialize()
    {
        return MemoryPackSerializer.Serialize(s);
    }

    [Benchmark]
    public Sample? Deserialize()
    {
        return MemoryPackSerializer.Deserialize<Sample>(bin);
    }
}

[MemoryPackable]
public partial class Sample
{
    // these types are serialized by default
    public int PublicField;
    public readonly int PublicReadOnlyField;
    public int PublicProperty { get; set; }
    public int PrivateSetPublicProperty { get; private set; }
    public int ReadOnlyPublicProperty { get; }
    public int InitProperty { get; init; }
    // public required int RequiredInitProperty { get; init; }

    // these types are not serialized by default
    int privateProperty { get; set; }
    int privateField;
    readonly int privateReadOnlyField;

    // use [MemoryPackIgnore] to remove target of a public member
    [MemoryPackIgnore]
    public int PublicProperty2 => PublicProperty + PublicField;

    // use [MemoryPackInclude] to promote a private member to serialization target
    [MemoryPackInclude]
    int privateField2;
    [MemoryPackInclude]
    int privateProperty2 { get; set; }
}
