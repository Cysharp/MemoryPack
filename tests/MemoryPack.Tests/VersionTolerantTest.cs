using MemoryPack.Tests.Models;
using System;

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
        var v5 = new VersionTolerant5() { MyProperty3 = 5000, MyProperty6 = new ushort[] { 1, 10, 100 } };

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

    [Fact]
    public void More()
    {
        var v3 = new VersionTolerant3 { MyProperty1 = 1000, MyProperty2 = 2000, MyProperty3 = 3000 };
        var v4 = new VersionTolerant4 { MyProperty1 = 4000, MyProperty3 = 5000 };

        var bin3 = MemoryPackSerializer.Serialize(v3);
        var bin4 = MemoryPackSerializer.Serialize(v4);

        var r_v4 = MemoryPackSerializer.Deserialize<VersionTolerant4>(bin3);
        r_v4.MyProperty1.Should().Be(1000);
        r_v4.MyProperty3.Should().Be(3000);

        var r_v3 = MemoryPackSerializer.Deserialize<VersionTolerant3>(bin4);
        r_v3.MyProperty1.Should().Be(4000);
        r_v3.MyProperty2.Should().Be(0);
        r_v3.MyProperty3.Should().Be(5000);
    }

    [Fact]
    public void More2()
    {
        var v1 = new Version1 { Id = 99, Name = "foo" };
        var v2 = new Version2 { Id = 9999, FirstName = "a", LastName = "b" };

        var bin1 = MemoryPackSerializer.Serialize(v1);
        var bin2 = MemoryPackSerializer.Serialize(v2);

        var r = MemoryPackSerializer.Deserialize<Version1>(bin2);
        r.Id.Should().Be(9999);
    }



    [Fact]
    public void Version2()
    {
        var v1 = new MoreVersionTolerant1() { MyProperty1 = new Version(10, 20, 4, 6) };
        var v2 = new MoreVersionTolerant2() { MyProperty1 = new Version(4, 23, 3, 99), MyProperty2 = 9999 };
        var v3 = new MoreVersionTolerant3() { MyProperty1 = new Version(6, 32, 425, 53), MyProperty2 = 2452, MyProperty3 = 32 };
        var v4 = new MoreVersionTolerant4() { MyProperty1 = new Version(11, 12, 13, 14), MyProperty3 = 13 };
        var v5 = new MoreVersionTolerant5() { MyProperty3 = 5000, MyProperty6 = new Version(1, 10, 100) };

        var bin1 = MemoryPackSerializer.Serialize(MakeWrapper(v1));
        var bin2 = MemoryPackSerializer.Serialize(MakeWrapper(v2));
        var bin3 = MemoryPackSerializer.Serialize(MakeWrapper(v3));
        var bin4 = MemoryPackSerializer.Serialize(MakeWrapper(v4));
        var bin5 = MemoryPackSerializer.Serialize(MakeWrapper(v5));


        var a = MemoryPackSerializer.Deserialize<VTWrapper<MoreVersionTolerant2>>(bin1);
        CheckArray(a);

        a.Versioned.MyProperty1.Should().Be(new Version(10, 20, 4, 6));
        a.Versioned.MyProperty2.Should().Be(0);

        var b = MemoryPackSerializer.Deserialize<VTWrapper<MoreVersionTolerant2>>(bin3);
        CheckArray(b);
        b.Versioned.MyProperty1.Should().Be(new Version(6, 32, 425, 53));
        b.Versioned.MyProperty2.Should().Be(2452);

        var c = MemoryPackSerializer.Deserialize<VTWrapper<MoreVersionTolerant4>>(bin3);
        CheckArray(c);

        c.Versioned.MyProperty1.Should().Be(new Version(6, 32, 425, 53));
        c.Versioned.MyProperty3.Should().Be(32);

        var d = MemoryPackSerializer.Deserialize<VTWrapper<MoreVersionTolerant5>>(bin3);
        CheckArray(d);
        d.Versioned.MyProperty3.Should().Be(32);
    }

    [Fact]
    public void More21()
    {
        var v3 = new MoreVersionTolerant3 { MyProperty1 = new Version(4, 23, 3, 99), MyProperty2 = 2000, MyProperty3 = 3000 };
        var v4 = new MoreVersionTolerant4 { MyProperty1 = new Version(5, 1, 2, 6), MyProperty3 = 5000 };

        var bin3 = MemoryPackSerializer.Serialize(v3);
        var bin4 = MemoryPackSerializer.Serialize(v4);

        var r_v4 = MemoryPackSerializer.Deserialize<MoreVersionTolerant4>(bin3);
        r_v4.MyProperty1.Should().Be(new Version(4, 23, 3, 99));
        r_v4.MyProperty3.Should().Be(3000);

        var r_v3 = MemoryPackSerializer.Deserialize<MoreVersionTolerant3>(bin4);
        r_v3.MyProperty1.Should().Be(new Version(5, 1, 2, 6));
        r_v3.MyProperty2.Should().Be(0);
        r_v3.MyProperty3.Should().Be(5000);
    }

    [Fact]
    public void More22()
    {
        var v1 = new MoreVersion1 { Id = new Version(4, 23, 3), Name = "foo" };
        var v2 = new MoreVersion2 { Id = new Version(5, 1, 2, 6), FirstName = "a", LastName = "b" };

        var bin1 = MemoryPackSerializer.Serialize(v1);
        var bin2 = MemoryPackSerializer.Serialize(v2);

        var r = MemoryPackSerializer.Deserialize<MoreVersion1>(bin2);
        r.Id.Should().Be(new Version(5, 1, 2, 6));
    }
}
