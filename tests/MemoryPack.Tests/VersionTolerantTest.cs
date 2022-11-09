using MemoryPack.Tests.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemoryPack.Tests;

public class VersionTolerantTest
{
    private void ConvertEqual<T>(T value)
    {
        MemoryPackSerializer.Deserialize<T>(MemoryPackSerializer.Serialize(value))
            .Should().BeEquivalentTo(value);
    }

    [Fact]
    public void Zero()
    {
        var zero = MemoryPackSerializer.Deserialize<VersionTolerant0>(MemoryPackSerializer.Serialize(new VersionTolerant0()));
        zero.Should().BeOfType<VersionTolerant0>();

        var wrapper = new VTWrapper<VersionTolerant0>()
        {
            Values = new[] { 1, 10, 100 },
            Versioned = new VersionTolerant0()
        };

        var v2 = MemoryPackSerializer.Deserialize<VTWrapper<VersionTolerant0>>(MemoryPackSerializer.Serialize(wrapper));
        v2!.Versioned!.Should().BeOfType<VersionTolerant0>();
        v2!.Values!.Should().Equal(1, 10, 100);
    }

    [Fact]
    public void Standard()
    {
        // ConvertEqual(new VersionTolerant0());
        ConvertEqual(new VersionTolerant1());
        ConvertEqual(new VersionTolerant2());
        ConvertEqual(new VersionTolerant3());
        ConvertEqual(new VersionTolerant4());
        ConvertEqual(new VersionTolerant5());
    }

    VTWrapper<T> MakeWrapper<T>(T v)
    {
        return new VTWrapper<T> { Versioned = v, Values = new[] { 1, 2, 10 } };
    }

    void CheckArray<T>(VTWrapper<T> value)
    {
        value.Values.Should().Equal(1, 2, 10);
    }
#pragma warning disable CS8602
#pragma warning disable CS8604

    [Fact]
    public void Version()
    {
        var v0 = new VersionTolerant0();
        var v1 = new VersionTolerant1() { MyProperty1 = 1000 };
        var v2 = new VersionTolerant2() { MyProperty1 = 3000, MyProperty2 = 9999 };
        var v3 = new VersionTolerant3() { MyProperty1 = 444, MyProperty2 = 2452, MyProperty3 = 32 };
        var v4 = new VersionTolerant4() { MyProperty1 = 99, MyProperty3 = 13 };
        var v5 = new VersionTolerant5() { MyProperty3 = 5000, MyProperty6 = "takoyaki" };

        var bin0 = MemoryPackSerializer.Serialize(MakeWrapper(v0));
        var bin1 = MemoryPackSerializer.Serialize(MakeWrapper(v1));
        var bin2 = MemoryPackSerializer.Serialize(MakeWrapper(v2));
        var bin3 = MemoryPackSerializer.Serialize(MakeWrapper(v3));
        var bin4 = MemoryPackSerializer.Serialize(MakeWrapper(v4));
        var bin5 = MemoryPackSerializer.Serialize(MakeWrapper(v5));


        var a = MemoryPackSerializer.Deserialize<VTWrapper<VersionTolerant2>>(bin1);
        CheckArray(a);

        a.Versioned.MyProperty1.Should().Be(1000);
        a.Versioned.MyProperty2.Should().Be(0);

        var b = MemoryPackSerializer.Deserialize<VTWrapper<VersionTolerant2>>(bin3);
        CheckArray(b);
        b.Versioned.MyProperty1.Should().Be(444);
        b.Versioned.MyProperty2.Should().Be(2452);

        var c = MemoryPackSerializer.Deserialize<VTWrapper<VersionTolerant4>>(bin3);
        CheckArray(c);

        c.Versioned.MyProperty1.Should().Be(444);
        c.Versioned.MyProperty3.Should().Be(32);

        var d = MemoryPackSerializer.Deserialize<VTWrapper<VersionTolerant5>>(bin3);
        CheckArray(d);
        d.Versioned.MyProperty3.Should().Be(32);
    }
}
