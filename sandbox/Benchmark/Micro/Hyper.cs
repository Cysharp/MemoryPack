using Benchmark.Models;
using BenchmarkDotNet.Order;
using MemoryPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Benchmark.Micro;

// [Orderer(SummaryOrderPolicy.FastestToSlowest)]
public class Hyper
{
    HyperTest test;
    byte[] bin;

    public Hyper()
    {
        var i = 0;
        test = new HyperTest()
        {
            // Gn
            A = i,
            B = i,
            C = DateTime.Now.Date,
            D = (uint)i,
            E = i,
            F = DateTime.Now - DateTime.Now.AddDays(-1),
            G = Guid.NewGuid(),
            H = TestEnum.three,
            //  I = i.ToString()
        };

        bin = MemoryPackSerializer.Serialize(test);
    }

    [Benchmark]
    public byte[] Serialize()
    {
        return MemoryPackSerializer.Serialize(test);
    }

    [Benchmark]
    public HyperTest? Deserialize()
    {
        return MemoryPackSerializer.Deserialize<HyperTest>(bin);
    }
}

public struct Foo
{
    public int X;
    public string Y;
}
