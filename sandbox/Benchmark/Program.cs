using Benchmark.Benchmarks;
using Benchmark.Micro;
using Benchmark.Models;
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
using MemoryPack.Formatters;
using MessagePack;
using MyBenchmark;
using System.Reflection;

var config = ManualConfig.CreateMinimumViable()
    .AddDiagnoser(MemoryDiagnoser.Default)
    .AddExporter(DefaultExporters.Plain)
    .AddExporter(MarkdownExporter.Default)
    .AddJob(Job.Default.WithWarmupCount(1).WithIterationCount(1)); // .AddJob(Job.ShortRun);

//BenchmarkSwitcher.FromAssembly(Assembly.GetEntryAssembly()!).Run(args, config);


//BenchmarkSwitcher.FromAssembly(Assembly.GetEntryAssembly()!).RunAllJoined(config);


// BenchmarkRunner.Run(Assembly.GetEntryAssembly()!, config, args);



//BenchmarkRunner.Run<SerializeInt>(config, args);
//BenchmarkRunner.Run<SerializeTest<MyClass>>(config, args);


//BenchmarkRunner.Run<RawSerialize>(config, args);

// BenchmarkRunner.Run<ConcurrentQueueVsStack>(config, args);

//BenchmarkRunner.Run<SerializeTest<JsonResponseModel>>(config, args);

//BenchmarkRunner.Run<SerializeTest<NeuralNetworkLayerModel>>(config, args);

// BenchmarkRunner.Run<DeserializeTest<NeuralNetworkLayerModel>>(config, args);
//BenchmarkRunner.Run<DeserializeTest<JsonResponseModel>>(config, args);


//BenchmarkRunner.Run<GetLocalVsStaticField>(config, args);

BenchmarkSwitcher.FromTypes(new[]{
    typeof(SerializeTest<>),
    //typeof(DeserializeTest<>)
})
    .RunAllJoined(config);

//BenchmarkRunner.Run<SerializeTest<NeuralNetworkLayerModel>>(config, args);

//var model = new NeuralNetworkLayerModel();
//model.Initialize();
//var value = (NeuralNetworkLayerModel)(object)model;
//var bytes = MessagePackSerializer.Serialize(value);
//var m2 = MessagePackSerializer.Deserialize<NeuralNetworkLayerModel>(bytes);
#if DEBUG




Check<JsonResponseModel>();
//Check<NeuralNetworkLayerModel>();

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
