using MemoryPack;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;

public class BuiltinTest
{
    [Test]
    public void Primitive()
    {
        var bin = MemoryPackSerializer.Serialize(100);
        var v = MemoryPackSerializer.Deserialize<int>(bin);
        Debug.Log("100 is ok: " + v);
    }

    [Test]
    public void StringArray()
    {
        var bin = MemoryPackSerializer.Serialize(new[] { "foo", "bar", "あいうえお" });
        var v = MemoryPackSerializer.Deserialize<string[]>(bin);
        Debug.Log("foo bar あいうえお is ok: " + string.Join(" ", v));
    }

    [Test]
    public void SerializeSimpleClass()
    {
        MyClass.RegisterFormatter(); // register myself:)

        var bin = MemoryPackSerializer.Serialize(new MyClass { MyProperty = 9999 });
        Debug.Log("Payload size:" + bin.Length);
        var v2 = MemoryPackSerializer.Deserialize<MyClass>(bin);
        Debug.Log("OK Deserialzie:" + v2.MyProperty);
    }
}

[MemoryPackable]
public partial class MyClass
{
    public int MyProperty { get; set; }
}
