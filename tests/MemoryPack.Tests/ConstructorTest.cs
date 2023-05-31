using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemoryPack.Tests;
public class ConstructorTest
{
    [Fact]
    public void SkipOrder()
    {
        var a = new Alpha { B1 = new Beta(10) };
        var bin = MemoryPackSerializer.Serialize(a);
        var v2 = MemoryPackSerializer.Deserialize<Alpha>(bin);
        v2!.B1!.Value1.Should().Be(10);
    }
}


[MemoryPackable(GenerateType.CircularReference)]
public partial class Alpha
{
    [MemoryPackOrder(1)]
    public Beta? B1 { get; set; }

    public Alpha()
    {

    }

}

// ctor for VersionTolerant, Skipped order

[MemoryPackable(GenerateType.VersionTolerant)]
public partial class Beta
{
    [MemoryPackOrder(1)]
    public int Value1 { get; set; }

    public Beta(int value1)
    {
        this.Value1 = value1;
    }
}

// support underscore private/internal convention

[MemoryPackable]
public partial class Gamma
{
    [MemoryPackInclude]
    private readonly string _test;

    public Gamma(string test)
    {
        _test = test;
    }
}
