using Benchmark.Models;
using BinaryPack.Models;
using MemoryPack.Formatters;

namespace Benchmark.Benchmarks;

// Value common setup.

public abstract class SerializerTestBase<T>
{
    protected T value { get; set; }


    public SerializerTestBase()
    {
        if (typeof(T) == typeof(int))
        {
            value = (T)(object)999999;
        }
        else if (typeof(T) == typeof(Vector3[]))
        {
            value = (T)(object)Enumerable.Repeat(new Vector3 { X = 10.3f, Y = 40.5f, Z = 13411.3f }, 1000).ToArray();
        }
        else if (typeof(T) == typeof(MyClass))
        {
            value = (T)(object)new MyClass { X = 100, Y = 99999999, Z = 4444, FirstName = "Hoge Huga Tako", LastName = "あいうえおかきくけこ" };
        }
        else if (typeof(T) == typeof(JsonResponseModel))
        {
            var model = new JsonResponseModel();
            model.Initialize();
            value = (T)(object)model;
        }
        else if (typeof(T) == typeof(NeuralNetworkLayerModel))
        {
            var model = new NeuralNetworkLayerModel();
            model.Initialize();
            value = (T)(object)model;
        }
        else
        {
            throw new InvalidOperationException($"Type {typeof(T)} is not registered create value.");
        }

        // TODO:prepare
        MemoryPack.MemoryPackFormatterProvider.Register(new ListFormatter<ApiModelContainer>());
        MemoryPack.MemoryPackFormatterProvider.Register(new ListFormatter<ImageModel>());
    }
}
