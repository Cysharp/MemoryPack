using MemoryPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyBenchmark;

[MemoryPackable]
internal partial class BigClass
{
    public double[] doubles;

    public BigClass()
    {
        doubles = new double[65535];
        for (int i = 0; i < doubles.Length; i++)
        {
            doubles[i] = Random.Shared.NextDouble();
        }
    }
}
