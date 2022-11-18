using MemoryPack;

namespace Benchmark.Models;

public enum TestEnum
{
    one, two, three
}

[MemoryPackable]
public partial class HyperTest
{
    public Guid? Gn { get; set; }
    public int A { get; set; }
    public long B { get; set; }
    public DateTime C { get; set; }
    public uint D { get; set; }
    public decimal E { get; set; }
    public TimeSpan F { get; set; }
    public Guid G { get; set; }
    public TestEnum H { get; set; }
}
