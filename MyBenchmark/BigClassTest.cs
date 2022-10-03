using BenchmarkDotNet.Attributes;
using MemoryPack;

namespace MyBenchmark;


public class BigClassTest
{
    private BigClass bigClass;

    [GlobalSetup]
    public void Setup()
    {
        bigClass = new BigClass();
    }


    [Benchmark]
    public byte[] MessagePackSerialize()
    {
        return MemoryPackSerializer.Serialize(bigClass);
    }
}
