using MemoryPack.Tests.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MemoryPack.Tests;

public class GeneratorTest
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    static void NoMember<T>(T? value)
    {
        var bin = MemoryPackSerializer.Serialize(value);
        T? value2 = MemoryPackSerializer.Deserialize<T>(bin);

        value2.Should().NotBeNull();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    static void VerifyEquivalent<T>(T? value)
    {
        var bin = MemoryPackSerializer.Serialize(value);
        T? value2 = MemoryPackSerializer.Deserialize<T>(bin);

        value.Should().BeEquivalentTo(value2);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    static T? Serdes<T>(T? value)
    {
        var bin = MemoryPackSerializer.Serialize(value);
        return MemoryPackSerializer.Deserialize<T>(bin);
    }

    [Fact]
    public void Standard()
    {
        NoMember(new StandardTypeZero());
        VerifyEquivalent(new StandardTypeOne() { One = 9999 });
        VerifyEquivalent(new StandardTypeTwo() { One = 9999, Two = 111 });
        VerifyEquivalent(new StandardUnmanagedStruct { MyProperty = 1111111 });
        VerifyEquivalent(new StandardStruct { MyProperty = "foobarbaz" });
        VerifyEquivalent(new MemoryPack.Tests.Models.More.StandardTypeTwo { One = "foo", Two = "bar" });
        VerifyEquivalent(new GlobalNamespaceType() { MyProperty = 10000 });
    }

    [Fact]
    public void Null()
    {
        var bin = MemoryPackSerializer.Serialize<StandardTypeOne>(null);
        MemoryPackSerializer.Deserialize<StandardTypeOne>(bin).Should().BeNull();
    }

    [Fact]
    public void Private()
    {
        {
            var v = new NoInclude();
            v.SetAll(10, 20, "foo", "bar", "baz", 99);

            var v2 = Serdes(v);

            v2.Should().NotBeNull();
            v2!.GetAll().Should().Be((10, 20, "foo", "bar", (string?)null, 0));
        }

        {
            var v = new Include();
            v.SetAll(10, 20, "foo", "bar", "baz", 99);

            var v2 = Serdes(v);

            v2.Should().NotBeNull();
            v2!.GetAll().Should().Be((10, 20, "foo", "bar", "baz", 99));
        }
    }
}
