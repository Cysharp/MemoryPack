using BenchmarkDotNet.Attributes;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Benchmark.Micro;

public class SerializeInt
{
    int value;

    public SerializeInt()
    {
        value = int.Parse("9999");
    }

    [Benchmark]
    [SkipLocalsInit]
    public byte[] UseSpan()
    {
        Span<byte> buffer = stackalloc byte[sizeof(int)];
        Unsafe.WriteUnaligned(ref MemoryMarshal.GetReference(buffer), value);
        return buffer.ToArray();
    }

    [Benchmark]
    public unsafe byte[] NoCopy()
    {
        var array = GC.AllocateUninitializedArray<byte>(sizeof(int));
        Unsafe.WriteUnaligned(ref MemoryMarshal.GetArrayDataReference(array), value);
        return array;
    }
}
