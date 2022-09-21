using MemoryPack.Tests.Models;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
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
    static T? VerifyEquivalent<T>(T? value)
    {
        var bin = MemoryPackSerializer.Serialize(value);
        T? value2 = MemoryPackSerializer.Deserialize<T>(bin);

        value.Should().BeEquivalentTo(value2);
        return value2;
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

    [Fact]
    public void Ctor()
    {
        VerifyEquivalent(new NoCtor { X = 10 });
        VerifyEquivalent(new OneCtor { X = 10 });
        VerifyEquivalent(new OneCtor2(10, 100));
        VerifyEquivalent(new ExplicitlyCtor(10, 100));

        var v = VerifyEquivalent(new ParameterCheck("foobarbaz") { MyProperty2 = "ttt" });
        v!.IsProp1SetCalled().Should().BeFalse();
    }

    [Fact]
    public void MethodInvoke()
    {
        var mc = new MethodCall();
        MethodCall.Log.Clear();
        var bin = MemoryPackSerializer.Serialize(mc);
        MethodCall.Log.Should().Equal("OnSerializing1", "OnSerializing2", "Get", "OnSerialized1", "OnSerialized2");

        MethodCall.Log.Clear();
        MemoryPackSerializer.Deserialize<MethodCall>(bin, ref mc);
        MethodCall.Log.Should().Equal("OnDeserializing1", "OnDeserializing2", "Constructor", "Set", "OnDeserialized1", "OnDeserialized2");

        MethodCall.Log.Clear();
        MemoryPackSerializer.Deserialize<MethodCall>(bin);
        MethodCall.Log.Should().Equal("OnDeserializing1", "Constructor", "Set", "OnDeserialized1", "OnDeserialized2");

        // allow null
        MethodCall.Log.Clear();
        var bin2 = MemoryPackSerializer.Serialize((MethodCall?)null);
        MethodCall.Log.Should().Equal("OnSerializing1", "OnSerialized1");

        MethodCall.Log.Clear();
        MemoryPackSerializer.Deserialize<MethodCall>(bin2);
        MethodCall.Log.Should().Equal("OnDeserializing1", "OnDeserialized1");
    }

    [Fact]
    public void Records()
    {
        VerifyEquivalent(new UnmanagedStruct { X = 9, Y = 3, Z = 2222 });
        VerifyEquivalent(new IncludesReferenceStruct { X = 9, Y = "foobarbaz" });
        VerifyEquivalent(new RequiredType { MyProperty1 = 10, MyProperty2 = "hogemogehuga" });
        VerifyEquivalent(new RequiredType2 { MyProperty1 = 10, MyProperty2 = "hogemogehuga" });
        VerifyEquivalent(new StructWithConstructor1("foo"));
        VerifyEquivalent(new MyRecord(10, 20, "haa"));
        VerifyEquivalent(new StructRecordUnmanaged(10, 20));
        VerifyEquivalent(new StructRecordWithReference(10, "zzz"));
    }

    [Fact]
    public void Derived()
    {
        VerifyEquivalent(new StandardBase { MyProperty1 = 1, MyProperty2 = 2 });
        VerifyEquivalent(new Derived1 { MyProperty1 = 1, MyProperty2 = 2, DerivedProp1 = 3, DerivedProp2 = 4 });
        VerifyEquivalent(new Derived2 { MyProperty1 = 1, MyProperty2 = 2, DerivedProp1 = 3, DerivedProp2 = 4, Derived2Prop1 = 5, Derived2Prop2 = 6 });
    }

    [Fact]
    public void Union()
    {
        // interface
        {
            IUnionInterface a = new Impl1 { MyProperty = 10, Foo = 999 };
            IUnionInterface b = new Impl2 { MyProperty = 1000, Bar = "foobarbaz" };
            {
                var bin = MemoryPackSerializer.Serialize(a);
                var a1 = MemoryPackSerializer.Deserialize<IUnionInterface>(bin);

                a1.Should().NotBeNull();
                a1!.MyProperty.Should().Be(10);
                (a1 as Impl1)!.Foo.Should().Be(999);
            }
            {
                var bin = MemoryPackSerializer.Serialize(b);
                var b1 = MemoryPackSerializer.Deserialize<IUnionInterface>(bin);

                b1.Should().NotBeNull();
                b1!.MyProperty.Should().Be(1000);
                (b1 as Impl2)!.Bar.Should().Be("foobarbaz");
            }
        }
        // abstract
        {
            UnionAbstractClass a = new ImplA1 { MyProperty = 10, Foo = 999 };
            UnionAbstractClass b = new ImplA2 { MyProperty = 1000, Bar = "foobarbaz" };
            {
                var bin = MemoryPackSerializer.Serialize(a);
                var a1 = MemoryPackSerializer.Deserialize<UnionAbstractClass>(bin);

                a1.Should().NotBeNull();
                a1!.MyProperty.Should().Be(10);
                (a1 as ImplA1)!.Foo.Should().Be(999);
            }
            {
                var bin = MemoryPackSerializer.Serialize(b);
                var b1 = MemoryPackSerializer.Deserialize<UnionAbstractClass>(bin);

                b1.Should().NotBeNull();
                b1!.MyProperty.Should().Be(1000);
                (b1 as ImplA2)!.Bar.Should().Be("foobarbaz");
            }
        }
    }

    [Fact]
    public void Versioning()
    {
        var v0 = new Versioning0();
        var v1 = new Versioning1() { MyProperty1 = 999 };
        var v2 = new Versioning2() { MyProperty1 = 10, MyProperty2 = 33333 };
        var v3 = new Versioning3() { MyProperty1 = 4, MyProperty2 = 55, MyProperty3 = 24442 };
        var v4 = new Versioning4() { MyProperty1 = 10000, MyProperty2 = 3, MyProperty3 = 4252, MyProperty4 = 99999 };

        var bin0 = MemoryPackSerializer.Serialize(v0);
        var bin1 = MemoryPackSerializer.Serialize(v1);
        var bin2 = MemoryPackSerializer.Serialize(v2);
        var bin3 = MemoryPackSerializer.Serialize(v3);
        var bin4 = MemoryPackSerializer.Serialize(v4);

        // small -> large is ok.
        var v0Tov4 = MemoryPackSerializer.Deserialize<Versioning4>(bin0);
        var v1Tov4 = MemoryPackSerializer.Deserialize<Versioning4>(bin1);
        var v2Tov4 = MemoryPackSerializer.Deserialize<Versioning4>(bin2);
        var v3Tov4 = MemoryPackSerializer.Deserialize<Versioning4>(bin3);
        var v4Tov4 = MemoryPackSerializer.Deserialize<Versioning4>(bin4);

        v0Tov4.Should().NotBeNull();
        v1Tov4.Should().BeEquivalentTo(v1);
        v2Tov4.Should().BeEquivalentTo(v2);
        v3Tov4.Should().BeEquivalentTo(v3);
        v4Tov4.Should().BeEquivalentTo(v4);

        // large -> small is ng
        Assert.Throws<InvalidOperationException>(() => MemoryPackSerializer.Deserialize<Versioning2>(bin3));

        // check wrapped
        var w2 = new WrappedVersioning2 { Before = "BF", V2 = v2, After = "AF" };
        var binw2 = MemoryPackSerializer.Serialize(w2);

        var w4 = MemoryPackSerializer.Deserialize<WrappedVersioning4>(binw2);

        w4!.Before.Should().Be("BF");
        w4!.V4!.MyProperty1.Should().Be(v2.MyProperty1);
        w4!.V4!.MyProperty2.Should().Be(v2.MyProperty2);
        w4!.V4!.MyProperty3.Should().Be(0);
        w4!.V4!.MyProperty4.Should().Be(0);
        w4!.After.Should().Be("AF");
    }
}
