using Benchmark.BenchmarkNetUtilities;
using Benchmark.Models;
using BenchmarkDotNet.Configs;
using MemoryPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Benchmark.Benchmarks;

[PayloadColumn]
[CategoriesColumn]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public class VersionTolerant
{
    MyClass value1;
    VersionTolerantMyClass value2;
    byte[] serializedNormal;
    byte[] serializedVT;

    public VersionTolerant()
    {
        value1 = new MyClass
        {
            X = 100,
            Y = 332,
            Z = 942524,
            FirstName = "hogehoge",
            LastName = "ふがふが"
        };

        value2 = new VersionTolerantMyClass
        {
            X = 100,
            Y = 332,
            Z = 942524,
            FirstName = "hogehoge",
            LastName = "ふがふが"
        };

        serializedNormal = MemoryPackSerializer.Serialize(value1);
        serializedVT = MemoryPackSerializer.Serialize(value2);
    }

    [Benchmark, BenchmarkCategory(Categories.Serialize)]
    public byte[] DefaultSerialzie()
    {
        return MemoryPackSerializer.Serialize(value1);
    }

    [Benchmark, BenchmarkCategory(Categories.Serialize)]
    public byte[] VersionTolerantSerialize()
    {
        return MemoryPackSerializer.Serialize(value2);
    }


    [Benchmark, BenchmarkCategory(Categories.Serialize)]
    public MyClass? DefaultDeserialize()
    {
        return MemoryPackSerializer.Deserialize<MyClass>(serializedNormal);
    }

    [Benchmark, BenchmarkCategory(Categories.Serialize)]
    public VersionTolerantMyClass? VersionTolerantDeserialzie()
    {
        return MemoryPackSerializer.Deserialize<VersionTolerantMyClass>(serializedVT);
    }
}


