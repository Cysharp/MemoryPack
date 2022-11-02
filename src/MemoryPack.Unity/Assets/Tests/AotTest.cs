using MemoryPack;
using MemoryPack.Formatters;
using Models;
using NUnit.Framework;
using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AotTest
{
    // https://github.com/Cysharp/MemoryPack/issues/34

    [Test]
    public void Dict()
    {
        var container = new MyContainer();

        container.Dict[1] = "One";
        container.Dict[2] = "Two";
        container.Dict[3] = "Three";

        var bin = MemoryPackSerializer.Serialize(container);
        var val = MemoryPackSerializer.Deserialize<MyContainer>(bin);

        Assert.AreEqual(val.Dict.Count, 3);
        Assert.AreEqual(val.Dict[1], "One");
        Assert.AreEqual(val.Dict[2], "Two");
        Assert.AreEqual(val.Dict[3], "Three");
    }

    [Test]
    public void List()
    {
        var container = new MyContainer2();

        container.List.Add(1);
        container.List.Add(2);
        container.List.Add(3);

        var bin = MemoryPackSerializer.Serialize(container);
        var val = MemoryPackSerializer.Deserialize<MyContainer2>(bin);

        Assert.AreEqual(val.List.Count, 3);
        Assert.AreEqual(val.List[0], 1);
        Assert.AreEqual(val.List[1], 2);
        Assert.AreEqual(val.List[2], 3);
    }

    [Test]
    public void Set()
    {
        var container = new MyContainer3();

        container.Set.Add(1);
        container.Set.Add(2);
        container.Set.Add(3);

        var bin = MemoryPackSerializer.Serialize(container);
        var val = MemoryPackSerializer.Deserialize<MyContainer3>(bin);

        Assert.AreEqual(val.Set.Count, 3);
        Assert.AreEqual(val.Set.Contains(1), true);
        Assert.AreEqual(val.Set.Contains(2), true);
        Assert.AreEqual(val.Set.Contains(3), true);
    }

    [Test]
    public void CheckIsRegistered()
    {
        //if (!MemoryPackFormatterProvider.IsRegistered<KeyValuePair<int, string>>())
        //{
        //    var f = new KeyValuePairFormatter<int, string>();
        //    var bufferWriter = new NullBufferWriter();
        //    var writer = new MemoryPackWriter<NullBufferWriter>(ref bufferWriter, MemoryPackSerializeOptions.Default);
        //    var value = default(KeyValuePair<int, string>);
        //    f.Serialize(ref writer, ref value);
        //}

        //var isRegistered = MemoryPackFormatterProvider.IsRegistered<KeyValuePair<int, string>>();
        //UnityEngine.Debug.Log("KVP<int,string> is registered?: " + isRegistered);
        //var f = MemoryPackFormatterProvider.GetFormatter<KeyValuePair<int, string>>();
        //UnityEngine.Debug.Log("f is not null: " + (f != null));

        ////var array = new ArrayBufferWriter<byte>();
        ////var writer = new MemoryPackWriter<ArrayBufferWriter<byte>>(ref array, MemoryPackSerializeOptions.Default);

        ////f.Serialize(ref writer, ref kvp);


        //var kvp = new KeyValuePair<int, string>(1, "One");
        // この行を入れると動く
        //MemoryPackSerializer.Serialize(kvp);
    }
}

sealed class NullBufferWriter : IBufferWriter<byte>
{
    byte[] dummyBuffer;

    public void Advance(int count)
    {
    }

    public Memory<byte> GetMemory(int sizeHint = 0)
    {
        if (dummyBuffer == null || dummyBuffer.Length < sizeHint)
        {
            dummyBuffer = new byte[sizeHint];
        }
        return dummyBuffer;
    }

    public Span<byte> GetSpan(int sizeHint = 0)
    {
        return GetMemory(sizeHint).Span;
    }
}
