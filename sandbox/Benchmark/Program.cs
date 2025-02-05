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
using MessagePack;
using PolyType;
using System.Reflection;

#if !DEBUG

var config = ManualConfig.CreateMinimumViable()
    .AddDiagnoser(MemoryDiagnoser.Default)
    // .AddColumn(StatisticColumn.OperationsPerSecond)
    //.AddExporter(DefaultExporters.Plain)
    .AddExporter(MarkdownExporter.Default)
    .AddJob(Job.Default.WithWarmupCount(1).WithIterationCount(1)); // .AddJob(Job.ShortRun);

//BenchmarkSwitcher.FromAssembly(Assembly.GetEntryAssembly()!).Run(args, config);


//BenchmarkRunner.Run<Hyper>(config, args);

//BenchmarkSwitcher.FromAssembly(Assembly.GetEntryAssembly()!).RunAllJoined(config);


// BenchmarkRunner.Run(Assembly.GetEntryAssembly()!, config, args);



//BenchmarkRunner.Run<SerializeInt>(config, args);
//BenchmarkRunner.Run<SerializeTest<MyClass>>(config, args);


//BenchmarkRunner.Run<RawSerialize>(config, args);

// BenchmarkRunner.Run<ConcurrentQueueVsStack>(config, args);

//BenchmarkRunner.Run<ListFormatterVsDirect>(config, args);


//BenchmarkRunner.Run<Utf16VsUtf8>(config, args);

//BenchmarkRunner.Run<SerializeTest<NeuralNetworkLayerModel>>(config, args);

// BenchmarkRunner.Run<DeserializeTest<NeuralNetworkLayerModel>>(config, args);



var serializer = new Nerdbank.MessagePack.MessagePackSerializer();


//ITypeShapeProvider
//PolyType.SourceGenerator.ShapeProvider_Benchmark.Default


//BenchmarkRunner.Run<StaticDictionaryFormatterCheck>(config, args);
BenchmarkRunner.Run<SerializeTest<JsonResponseModel>>(config, args);
//BenchmarkRunner.Run<DeserializeTest<JsonResponseModel>>(config, args);
//BenchmarkRunner.Run<DeserializeTest<JsonResponseModel>>(config, args);
//BenchmarkRunner.Run<SerializeTest<Vector3[]>>(config, args);
//BenchmarkRunner.Run<DeserializeTest<Vector3[]>>(config, args);

//BenchmarkRunner.Run<StaticAbstractVsFormatter>(config, args);

//BenchmarkRunner.Run<Compression<JsonResponseModel>>(config, args);
//BenchmarkRunner.Run<Compression<Vector3[]>>(config, args);
//BenchmarkRunner.Run<Compression<NeuralNetworkLayerModel>>(config, args);
// Nerdbank.MessagePack.
//Nerdbank.MessagePack.MessagePackSerializer.SerializeDefaultValues
//BenchmarkRunner.Run<GetLocalVsStaticField>(config, args);

//BenchmarkRunner.Run<VersionTolerant>(config, args);

//BenchmarkRunner.Run(typeof(JilBenchmark<>), config, args);

//BenchmarkSwitcher.FromTypes(new[]{
//    typeof(SerializeTest<>),
//    typeof(DeserializeTest<>),
//});
//    .RunAllJoined(config);

#endif

#if DEBUG


var shape = PolyType.SourceGenerator.ShapeProvider_Benchmark.Default;
shape.GetShape(typeof(int));
var i = 100;

var ss = new Nerdbank.MessagePack.MessagePackSerializer();
var bin = ss.Serialize(i, PolyType.SourceGenerator.ShapeProvider_Benchmark.Default);
ss.Deserialize<int>(bin, PolyType.SourceGenerator.ShapeProvider_Benchmark.Default);






//var model = new RestApiModel();
//model.Initialize();
//model.Info = new MediaInfoModel();
//model.Info = null;
// model.Info = null;

var model = new Foo();
model.Bar = "aiu";
model.Bar = "tako";
model.Boz = "foobarbaz";
//model.Boz = null;

//var nb = new Nerdbank.MessagePack.MessagePackSerializer();
//var bin = nb.Serialize(model);
//nb.Deserialize<Foo>(bin);
var bin2 = MessagePackSerializer.Serialize(model);
MessagePackSerializer.Deserialize<Foo>(bin2);
//var a = MessagePackSerializer.ConvertToJson(bin);
//var b = MessagePackSerializer.ConvertToJson(bin2);
//Console.WriteLine(bin);

//MessagePack.MessagePackSerializerOptions
//MemoryPack.MemoryPackSerializerOptions
//System.Text.Json.JsonSerializerOptions

//new JilBenchmark<Question>().OrleansDeserializeStream();
//var jil = new JilBenchmark<Question>();
//var bin = jil.MemoryPackSerializeUtf16();
//var q2 = MemoryPackSerializer.Deserialize<Question>(bin);


//new Hyper().Serialize();

//var c = new StaticDictionaryFormatterCheck();
//c.DeserializeCurrent();
//c.DeserializeImprovement();

//c.DeserializeCurrent();
//c.DeserializeImprovement();


//var model = new JsonResponseModel(true);
//var model2 = Enumerable.Repeat(new Vector3 { X = 10.3f, Y = 40.5f, Z = 13411.3f }, 1000).ToArray();

//using var compressor = new BrotliCompressor();
//MemoryPackSerializer.Serialize(compressor, model2);
//var foo = compressor.ToArray();

//using var decompressor = new BrotliDecompressor();

//var foo2 = decompressor.Decompress(foo);

//Check<JsonResponseModel>();
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


[GenerateShape, MessagePackObject]
public partial class Foo
{
    [Key(0), Nerdbank.MessagePack.Key(0)]
    public string? Bar { get; set; }
    [Key(1), Nerdbank.MessagePack.Key(1)]
    public string? Baz { get; set; }
    [Key(2), Nerdbank.MessagePack.Key(2)]
    public string? Boz { get; set; }
}

#endif
