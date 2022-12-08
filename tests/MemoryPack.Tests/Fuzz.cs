using RandomFixtureKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemoryPack.Tests;

public class Fuzz
{

    [Fact]
    public void Random()
    {
        var resolvers = new[] { StandardResolver.EdgeCase, StandardResolver.AllowNull, StandardResolver.NonNull, StandardResolver.RandomAll };
        foreach (var resolver in resolvers)
        {
            for (int i = 0; i < 100; i++)
            {
                var v = FixtureFactory.Create<Foo1>(resolver: resolver);
                var bin = MemoryPackSerializer.Serialize(v);
                var v2 = MemoryPackSerializer.Deserialize<Foo1>(bin);
                v.Should().BeEquivalentTo(v2);
            }
        }
    }
}

[MemoryPackable]
internal partial class Foo1
{
    public int MyProperty { get; set; }
#pragma warning disable CS8618
    public string MyProperty2 { get; set; }
    public Foo2 MyProperty3 { get; set; }
    public string MyProperty4 { get; set; }
    public List<Foo2> Foo22 { get; set; }
}

[MemoryPackable]
internal partial class Foo2
{
    public int MyProperty { get; set; }
    public string MyProperty2 { get; set; }
    public Foo3 MyProperty3 { get; set; }
    public string MyProperty4 { get; set; }
    public Dictionary<int, string> Dic { get; set; }
    public List<Foo3> Foo33 { get; set; }
}


[MemoryPackable]
internal partial class Foo3
{
    public int MyProperty { get; set; }
    public long MyPr3operty { get; set; }
    public bool MyProp42erty { get; set; }
    public string MyProperty2 { get; set; }
    public string MyProperty4 { get; set; }
}
