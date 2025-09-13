using MemoryPack.Formatters;
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
        VerifyEquivalent(new GlobalNamespaceType() { MyProperty = 10000 });
    }

    [Fact]
    public void Nested()
    {
        VerifyEquivalent(new NestedContainer.StandardTypeNested() { One = 9999 });
        VerifyEquivalent(new NestedStructContainer.StandardTypeNested() { One = 9999 });
        VerifyEquivalent(new NestedRecordStructContainer.StandardTypeNested() { One = 9999 });
        VerifyEquivalent(new NestedInterfaceContainer.StandardTypeNested() { One = 9999 });
        VerifyEquivalent(new NestedAbstractClassContainer.StandardTypeNested() { One = 9999 });
        VerifyEquivalent(new DoublyNestedContainer.DoublyNestedContainerInner.StandardTypeDoublyNested() { One = 9999 });
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
        MethodCall.Log.Should().Equal("OnSerializing1", "OnSerializing2", "OnSerializing_M1", "OnSerializing_M2", "Get", "OnSerialized1", "OnSerialized2", "OnSerialized_M1", "OnSerialized_M2");

        MethodCall.Log.Clear();
        MemoryPackSerializer.Deserialize<MethodCall>(bin, ref mc);
        MethodCall.Log.Should().Equal("OnDeserializing1", "OnDeserializing2", "OnDeserializing_M1", "OnDeserializing_M2", "Get", "Set", "OnDeserialized1", "OnDeserialized2", "OnDeserialized_M1", "OnDeserialized_M2");

        MethodCall.Log.Clear();
        MemoryPackSerializer.Deserialize<MethodCall>(bin);
        MethodCall.Log.Should().Equal("OnDeserializing1", "OnDeserializing_M1", "Constructor", "Set", "OnDeserialized1", "OnDeserialized2", "OnDeserialized_M1", "OnDeserialized_M2");

        // allow null
        MethodCall.Log.Clear();
        var bin2 = MemoryPackSerializer.Serialize((MethodCall?)null);
        MethodCall.Log.Should().Equal("OnSerializing1", "OnSerializing_M1", "OnSerialized1", "OnSerialized_M1");

        MethodCall.Log.Clear();
        MemoryPackSerializer.Deserialize<MethodCall>(bin2);
        MethodCall.Log.Should().Equal("OnDeserializing1", "OnDeserializing_M1", "OnDeserialized1", "OnDeserialized_M1");
    }

    [Fact]
    public void Records()
    {
        VerifyEquivalent(new UnmanagedStruct { X = 9, Y = 3, Z = 2222 });
        VerifyEquivalent(new IncludesReferenceStruct { X = 9, Y = "foobarbaz" });
#if NET7_0_OR_GREATER
        VerifyEquivalent(new RequiredType { MyProperty1 = 10, MyProperty2 = "hogemogehuga" });
        VerifyEquivalent(new RequiredType2 { MyProperty1 = 10, MyProperty2 = "hogemogehuga" });
#endif
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
        Assert.Throws<MemoryPackSerializationException>(() => MemoryPackSerializer.Deserialize<Versioning2>(bin3));

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

    [Fact]
    public void Recursive()
    {
        var rec = new Recursive() { MyProperty = 99 };
        var rec2 = new Recursive() { MyProperty = 1000 };
        rec.Rec = rec2;

        // ok to serialize
        var bin = MemoryPackSerializer.Serialize(rec);
        var newRec = MemoryPackSerializer.Deserialize<Recursive>(bin);

        Debug.Assert(newRec != null);
        newRec.MyProperty.Should().Be(99);
        newRec.Rec!.MyProperty.Should().Be(1000);
        newRec.Rec!.Rec.Should().BeNull();

        //set rec
        rec.Rec = rec;
        Assert.Throws<MemoryPackSerializationException>(() => MemoryPackSerializer.Serialize(rec));
    }

    [Fact]
    public void ManyMemebrs()
    {
        MemoryPackSerializer.Serialize(new ManyMembers());
    }

    [Fact]
    public void Generics()
    {
        var gt = new GenericsType<string>()
        {
            MyProperty1 = 2,
            MyProperty2 = "hogehoge"
        };

        VerifyEquivalent(gt);

        var comp = new MoreComplecsGenerics<int, string>()
        {
            Dict = new Dictionary<int, GenericsType<string>>{
                { 3, new GenericsType<string>{ MyProperty1 = 10, MyProperty2 = "tako" } },
                { 9, new GenericsType<string>{ MyProperty1 = 99, MyProperty2 = "yaki" } },
            }
        };

        var bin = MemoryPackSerializer.Serialize(comp);
        var two = MemoryPackSerializer.Deserialize<MoreComplecsGenerics<int, string>>(bin);

        Debug.Assert(two != null);
        (two.Dict![3] is { MyProperty1: 10, MyProperty2: "tako" }).Should().BeTrue();
        (two.Dict![9] is { MyProperty1: 99, MyProperty2: "yaki" }).Should().BeTrue();

        // union
        var a = new GenricUnionA<long>() { Value = 9999999, MyProperty = 10000 };
        var b = new GenricUnionB<long>() { Value = 1111111, MyProperty = 99.9932 };

        var binA = MemoryPackSerializer.Serialize<IGenericUnion<long>>(a);
        var binB = MemoryPackSerializer.Serialize<IGenericUnion<long>>(b);

        var a2 = MemoryPackSerializer.Deserialize<IGenericUnion<long>>(binA);
        var b2 = MemoryPackSerializer.Deserialize<IGenericUnion<long>>(binB);

        a2.Should().BeOfType<GenricUnionA<long>>().Subject.Should().BeEquivalentTo(new { Value = (long)9999999, MyProperty = 10000 });
        b2.Should().BeOfType<GenricUnionB<long>>().Subject.Should().BeEquivalentTo(new { Value = (long)1111111, MyProperty = 99.9932 });
    }

    [Fact]
    public void Overwrite()
    {
        var v1 = new Overwrite()
        {
            MyProperty1 = 10,
            MyProperty2 = 100,
            MyProperty3 = "foo",
            MyProperty4 = "bar"
        };

        var v2 = new Overwrite2()
        {
            MyProperty1 = 11,
            MyProperty2 = 101,
            MyProperty3 = "foz",
            MyProperty4 = "baz"
        };

        var v3 = new Overwrite3(14, 130)
        {
            MyProperty3 = "fzo",
            MyProperty4 = "bzr"
        };

        var v4 = new Overwrite4()
        {
            MyProperty1 = 19,
            MyProperty2 = v1
        };

        var bin = MemoryPackSerializer.Serialize(v1);

        v1.MyProperty1 = 999;
        v1.MyProperty2 = 100000;
        v1.MyProperty3 = "foooooo";
        v1.MyProperty4 = "barrrrrrrrr";

        var v1_original = v1;
        MemoryPackSerializer.Deserialize(bin, ref v1);
        Debug.Assert(v1 != null);
        v1.MyProperty1.Should().Be(10);
        v1.MyProperty2.Should().Be(100);
        v1.MyProperty3.Should().Be("foo");
        v1.MyProperty4.Should().Be("bar");

        v1.Should().BeSameAs(v1_original);

        VerifyEquivalent(v2);
        VerifyEquivalent(v3).Should().NotBeSameAs(v3);

        var bin2 = MemoryPackSerializer.Serialize(v4);
        var v4_original = v4;
        v4.MyProperty1 = 9999;

        MemoryPackSerializer.Deserialize(bin2, ref v4);
        Debug.Assert(v4 != null);
        v4.MyProperty1.Should().Be(19);
        v4.MyProperty2.Should().BeSameAs(v1);

        v1.Should().BeSameAs(v1_original);

    }
}
