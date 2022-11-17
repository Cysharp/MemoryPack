using Benchmark.Benchmarks;
using Benchmark.Micro;
using Benchmark.Models;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using BinaryPack.Models;
using BinaryPack.Models.Helpers;
using BinaryPack.Models.Interfaces;
using Iced.Intel;
using MemoryPack;
using MemoryPack.Compression;
using MemoryPack.Formatters;
using System.Reflection;

#if !DEBUG

var config = ManualConfig.CreateMinimumViable()
    .AddDiagnoser(MemoryDiagnoser.Default)
    // .AddColumn(StatisticColumn.OperationsPerSecond)
    .AddExporter(DefaultExporters.Plain)
    .AddExporter(MarkdownExporter.Default)
    .AddJob(Job.Default.WithWarmupCount(1).WithIterationCount(1)); // .AddJob(Job.ShortRun);

//BenchmarkSwitcher.FromAssembly(Assembly.GetEntryAssembly()!).Run(args, config);


//BenchmarkRunner.Run<Utf8Decoding>(config, args);

//BenchmarkSwitcher.FromAssembly(Assembly.GetEntryAssembly()!).RunAllJoined(config);


// BenchmarkRunner.Run(Assembly.GetEntryAssembly()!, config, args);



//BenchmarkRunner.Run<SerializeInt>(config, args);
//BenchmarkRunner.Run<SerializeTest<MyClass>>(config, args);


//BenchmarkRunner.Run<RawSerialize>(config, args);

// BenchmarkRunner.Run<ConcurrentQueueVsStack>(config, args);

BenchmarkRunner.Run<ListFormatterVsDirect>(config, args);


//BenchmarkRunner.Run<Utf16VsUtf8>(config, args);

//BenchmarkRunner.Run<SerializeTest<NeuralNetworkLayerModel>>(config, args);

// BenchmarkRunner.Run<DeserializeTest<NeuralNetworkLayerModel>>(config, args);


//BenchmarkRunner.Run<StaticDictionaryFormatterCheck>(config, args);
//BenchmarkRunner.Run<SerializeTest<JsonResponseModel>>(config, args);
//BenchmarkRunner.Run<DeserializeTest<JsonResponseModel>>(config, args);
//BenchmarkRunner.Run<SerializeTest<Vector3[]>>(config, args);
//BenchmarkRunner.Run<DeserializeTest<Vector3[]>>(config, args);

//BenchmarkRunner.Run<StaticAbstractVsFormatter>(config, args);

//BenchmarkRunner.Run<Compression<JsonResponseModel>>(config, args);
//BenchmarkRunner.Run<Compression<Vector3[]>>(config, args);
//BenchmarkRunner.Run<Compression<NeuralNetworkLayerModel>>(config, args);


//BenchmarkRunner.Run<GetLocalVsStaticField>(config, args);

//BenchmarkRunner.Run<VersionTolerant>(config, args);

//BenchmarkSwitcher.FromTypes(new[]{
//    typeof(SerializeTest<>),
//    typeof(DeserializeTest<>) })
//    .RunAllJoined(config);

#endif

#if DEBUG

var c = new StaticDictionaryFormatterCheck();
c.DeserializeCurrent();
c.DeserializeImprovement();

c.DeserializeCurrent();
c.DeserializeImprovement();


var model = new JsonResponseModel(true);
var model2 = Enumerable.Repeat(new Vector3 { X = 10.3f, Y = 40.5f, Z = 13411.3f }, 1000).ToArray();

using var compressor = new BrotliCompressor();
MemoryPackSerializer.Serialize(compressor, model2);
var foo = compressor.ToArray();

using var decompressor = new BrotliDecompressor();

var foo2 = decompressor.Decompress(foo);

Check<JsonResponseModel>();
Check<NeuralNetworkLayerModel>();

void Check<T>()
    where T : IInitializable, IEquatable<T>, new()
{
    var model = new T();
    model.Initialize();
    var bin = MemoryPackSerializer.Serialize(model);
    var model2 = MemoryPackSerializer.Deserialize<T>(bin);
    var ok = model.Equals(model2);
    Console.WriteLine(typeof(T) + " is " + (ok ? "ok" : "ng"));
}

[MemoryPackable]
public partial class Test
{
    public float[] F = default!;

    public Test()
    {
        // _ = new TestFormatter();
    }
}

#endif
