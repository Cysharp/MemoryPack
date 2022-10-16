using MemoryPack;
using MemoryPack.Formatters;
using Models;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;

public class BuiltinTest
{
    private T Convert<T>(T value)
    {
        return MemoryPackSerializer.Deserialize<T>(MemoryPackSerializer.Serialize(value));
    }

    private void ConvertEqual<T>(T value)
    {
        var v = MemoryPackSerializer.Deserialize<T>(MemoryPackSerializer.Serialize(value));
        Assert.AreEqual(value, v);
    }

    private void ConvertCollectionEqual<T>(T[] value)
    {
        var v = MemoryPackSerializer.Deserialize<T[]>(MemoryPackSerializer.Serialize(value));
        CollectionAssert.AreEqual(value, v);
    }

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
        var bin = MemoryPackSerializer.Serialize(new Takoyaki { MyProperty = 9999 });
        Debug.Log("Payload size:" + bin.Length);
        var v2 = MemoryPackSerializer.Deserialize<Takoyaki>(bin);
        Debug.Log("OK Deserialzie:" + v2.MyProperty);
    }

    [Test]
    public void TupleT()
    {
        _ = new TupleFormatter<int>();
        _ = new TupleFormatter<int, int>();
        _ = new TupleFormatter<int, int, int>();
        _ = new TupleFormatter<int, int, int, int>();
        _ = new TupleFormatter<int, int, int, int, int>();
        _ = new TupleFormatter<int, int, int, int, int, int>();
        _ = new TupleFormatter<int, int, int, int, int, int, int>();
        _ = new TupleFormatter<int, int, int, int, int, int, int, int>();

        ConvertEqual(Tuple.Create(1));
        ConvertEqual(Tuple.Create(1, 2));
        ConvertEqual(Tuple.Create(1, 2, 3));
        ConvertEqual(Tuple.Create(1, 2, 3, 4));
        ConvertEqual(Tuple.Create(1, 2, 3, 4, 5));
        ConvertEqual(Tuple.Create(1, 2, 3, 4, 5, 6));
        ConvertEqual(Tuple.Create(1, 2, 3, 4, 5, 6, 7));
        // ConvertEqual(Tuple.Create(1, 2, 3, 4, 5, 6, 7, 8));
    }

    [Test]
    public void ValueTupleT()
    {
        _ = new ValueTupleFormatter<int>();
        _ = new ValueTupleFormatter<int, int>();
        _ = new ValueTupleFormatter<int, int, int>();
        _ = new ValueTupleFormatter<int, int, int, int>();
        _ = new ValueTupleFormatter<int, int, int, int, int>();
        _ = new ValueTupleFormatter<int, int, int, int, int, int>();
        _ = new ValueTupleFormatter<int, int, int, int, int, int, int>();
        //_ = new ValueTupleFormatter<int, int, int, int, int, int, int, int>();

        ConvertEqual(ValueTuple.Create(1));
        ConvertEqual(ValueTuple.Create(1, 2));
        ConvertEqual(ValueTuple.Create(1, 2, 3));
        ConvertEqual(ValueTuple.Create(1, 2, 3, 4));
        ConvertEqual(ValueTuple.Create(1, 2, 3, 4, 5));
        ConvertEqual(ValueTuple.Create(1, 2, 3, 4, 5, 6));
        ConvertEqual(ValueTuple.Create(1, 2, 3, 4, 5, 6, 7));
        // ConvertEqual(ValueTuple.Create(1, 2, 3, 4, 5, 6, 7, 8));
    }

    [Test]
    public void EnumTes()
    {
        ConvertEqual(BEnum.B);
        ConvertEqual(NormalEnum.A);
        ConvertEqual(NotNotEnum.C);
    }

    public enum BEnum : byte
    {
        A, B, C
    }
    public enum NormalEnum
    {
        A, B, C
    }

    public enum NotNotEnum : long
    {
        A, B, C
    }
}
