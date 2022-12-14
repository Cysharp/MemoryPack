using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using MemoryPack;
using System.Buffers;
using System.Reflection;

var config = ManualConfig.CreateMinimumViable()
    .AddDiagnoser(MemoryDiagnoser.Default)
    // .AddColumn(StatisticColumn.OperationsPerSecond)
    //.AddExporter(DefaultExporters.Plain)
    .AddExporter(MarkdownExporter.Default)
    .AddJob(Job.Default.WithWarmupCount(1).WithIterationCount(1).WithRuntime(CoreRuntime.Core60))
    .AddJob(Job.Default.WithWarmupCount(1).WithIterationCount(1).WithRuntime(CoreRuntime.Core70));

#if DEBUG

#else
BenchmarkSwitcher.FromTypes(new[] { typeof(Net6Net7<>) }).RunAllJoined(config);
#endif

[GenericTypeArguments(typeof(Sample))]
[GenericTypeArguments(typeof(Sample2))]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByMethod)]
public class Net6Net7<T>
    where T : class, new()
{
    private T s;
    byte[] bin;

    public Net6Net7()
    {
        s = new();
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
    public T? Deserialize()
    {
        return MemoryPackSerializer.Deserialize<T>(bin);
    }
}

[MemoryPackable]
public partial class Sample
{
    public int PublicField;
    public readonly int PublicReadOnlyField;
    public int PublicProperty { get; set; }
    public int PrivateSetPublicProperty { get; private set; }
    public int ReadOnlyPublicProperty { get; }
    public int InitProperty { get; init; }

    [MemoryPackIgnore]
    public int PublicProperty2 => PublicProperty + PublicField;

    [MemoryPackInclude]
    int privateField2;
    [MemoryPackInclude]
    int privateProperty2 { get; set; }
}

[MemoryPackable]
public partial class Sample2
{
    public int PublicField;
    public readonly int PublicReadOnlyField;
    public int PublicProperty { get; set; }
    public int PrivateSetPublicProperty { get; private set; }
    public int ReadOnlyPublicProperty { get; }
    public int InitProperty { get; init; }

    public int[]? ArrayProp { get; set; }

    [MemoryPackIgnore]
    public int PublicProperty2 => PublicProperty + PublicField;

    [MemoryPackInclude]
    int privateField2;
    [MemoryPackInclude]
    int privateProperty2 { get; set; }
}
