using MemoryPack.Formatters;
using MemoryPack.Tests.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemoryPack.Tests;

public class UnionTest
{
    [Fact]
    public void Foo()
    {
        {
            var one = new AForOne { BaseValue = 10, MyProperty = 99 };
            var two = new AForTwo { BaseValue = 99, MyProperty = 10000 };

            var bin1 = MemoryPackSerializer.Serialize((IForExternalUnion)one);
            var bin2 = MemoryPackSerializer.Serialize((IForExternalUnion)two);

            var one2 = MemoryPackSerializer.Deserialize<IForExternalUnion>(bin1);
            var two2 = MemoryPackSerializer.Deserialize<IForExternalUnion>(bin2);

            one2.Should().BeAssignableTo<AForOne>().Subject.Should().BeEquivalentTo(one);
            two2.Should().BeAssignableTo<AForTwo>().Subject.Should().BeEquivalentTo(two);
        }
        {
            var one = new BForOne<DateTime> { NoValue = DateTime.Now, MyProperty = 99 };
            var two = new BForTwo<string> { NoValue = "aaaa", MyProperty = 10000 };

            var bin1 = MemoryPackSerializer.Serialize((IGenericsUnion<DateTime>)one);
            var bin2 = MemoryPackSerializer.Serialize((IGenericsUnion<string>)two);

            var one2 = MemoryPackSerializer.Deserialize<IGenericsUnion<DateTime>>(bin1);
            var two2 = MemoryPackSerializer.Deserialize<IGenericsUnion<string>>(bin2);

            one2.Should().BeAssignableTo<BForOne<DateTime>>().Subject.Should().BeEquivalentTo(one);
            two2.Should().BeAssignableTo<BForTwo<string>>().Subject.Should().BeEquivalentTo(two);
        }
    }

    [Fact]
    public void Dynamic()
    {
        var f = new DynamicUnionFormatter<IDynamicBase>(
            (0, typeof(Gen1)),
            (1, typeof(Gen2)));

        MemoryPackFormatterProvider.Register(f);

        var one = new Gen1() { MyProperty = 999 };
        var two = new Gen2() { MyProperty = "aabbbC" };

        var bin1 = MemoryPackSerializer.Serialize<IDynamicBase>(one);
        var bin2 = MemoryPackSerializer.Serialize<IDynamicBase>(two);

        var d1 = MemoryPackSerializer.Deserialize<IDynamicBase>(bin1);
        var d2 = MemoryPackSerializer.Deserialize<IDynamicBase>(bin2);

        (d1 as Gen1)!.MyProperty.Should().Be(999);
        (d2 as Gen2)!.MyProperty.Should().Be("aabbbC");
    }
}

[MemoryPackable(GenerateType.NoGenerate)]
public partial class IDynamicBase
{
}


[MemoryPackable(GenerateType.Object)]
public partial class Gen1 : IDynamicBase
{
    public int MyProperty { get; set; }
}

[MemoryPackable(GenerateType.Object)]
public partial class Gen2 : IDynamicBase
{
    public string? MyProperty { get; set; }
}
