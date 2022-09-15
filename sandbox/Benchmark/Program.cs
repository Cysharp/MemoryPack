using Benchmark.Benchmarks;
using Benchmark.Models;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;

var config = ManualConfig.CreateMinimumViable()
    .AddDiagnoser(MemoryDiagnoser.Default)
    .AddExporter(DefaultExporters.Plain)
    .AddJob(Job.Default.WithWarmupCount(1).WithIterationCount(1)); // .AddJob(Job.ShortRun);

//BenchmarkSwitcher.FromAssembly(Assembly.GetEntryAssembly()!).Run(args, config);
//BenchmarkSwitcher.FromAssembly(Assembly.GetEntryAssembly()!).RunAllJoined(config);
// BenchmarkRunner.Run(Assembly.GetEntryAssembly()!, config, args);

//BenchmarkRunner.Run<SerializeInt>(config, args);
BenchmarkRunner.Run<SerializeTest<MyClass>>(config, args);
