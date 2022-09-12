// See https://aka.ms/new-console-template for more information
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using MemoryPack;
using MessagePack;
using System.Buffers;

var config = ManualConfig.CreateMinimumViable()
    .AddDiagnoser(MemoryDiagnoser.Default)
    .AddExporter(DefaultExporters.Plain)
    .AddJob(Job.Default.WithWarmupCount(1).WithIterationCount(1)); // .AddJob(Job.ShortRun);

BenchmarkRunner.Run<DefaultRun>(config, args);

public class DefaultRun
{
    Vector3[] v3;
    ArrayBufferWriter<byte> writer;

    public DefaultRun()
    {
        v3 = Enumerable.Repeat(new Vector3 { X = 10.3f, Y = 40.5f, Z = 13411.3f }, 1000).ToArray();
        var serialize1 = MessagePackSerializer.Serialize(v3);
        var serialize2 = MemoryPackSerializer.Serialize(v3);
        writer = new ArrayBufferWriter<byte>(Math.Max(serialize1.Length, serialize2.Length));
    }

    [Benchmark(Baseline = true)]
    public byte[] MessagePackSerialize()
    {
        return MessagePackSerializer.Serialize(v3);
    }

    [Benchmark]
    public byte[] MemoryPackSerialize()
    {
        return MemoryPackSerializer.Serialize(v3);
    }

    [Benchmark]
    public void MessagePackBufferWriter()
    {
        MessagePackSerializer.Serialize(writer, v3);
        writer.Clear();
    }

    [Benchmark]
    public void MemoryPackBufferWriter()
    {
        MemoryPackSerializer.Serialize(ref writer, v3);
        writer.Clear();
    }

    //[BenchmarkDotNet.Attributes.IterationCleanup]
    //public void ClearWriter()
    //{
    //    writer.Clear();
    //}
}

[MessagePackObject]
public struct Vector3
{
    [Key(0)]
    public float X;
    [Key(1)]
    public float Y;
    [Key(2)]
    public float Z;
}
