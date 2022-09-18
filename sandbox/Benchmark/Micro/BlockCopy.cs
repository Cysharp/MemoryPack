using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Benchmark.Micro;

public class BlockCopy
{
    byte[] buffer = default!;
    byte[] dest = default!;

    [Params(10, 100, 1000, 10000, 65536, 100000, 1000000)]
    public int BufferCount { get; set; }

    [GlobalSetup]
    public void Init()
    {
        buffer = new byte[BufferCount];
        buffer.AsSpan().Fill(99);
        dest = new byte[buffer.Length];
    }

    [Benchmark]
    public void MemoryCopy()
    {
        // Span CopyTo using Buffer.MemoryCopy
        buffer.AsSpan().CopyTo(dest);
    }

    [Benchmark]
    public void CopyBlockUnaligned()
    {
        Unsafe.CopyBlockUnaligned(ref MemoryMarshal.GetArrayDataReference(dest), ref MemoryMarshal.GetArrayDataReference(buffer), (uint)buffer.Length);
    }
}
