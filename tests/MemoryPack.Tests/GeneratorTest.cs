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

}
