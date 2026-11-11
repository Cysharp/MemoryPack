using MemoryPack;
using MemoryPack.Tests.Models;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

// List, Stack, ImmutableArray and BitArray formatters only use the public API(do not depend on the private field layout), checks in Unity(Mono, IL2CPP).
// https://github.com/Cysharp/MemoryPack/issues/460
public class CollectionPublicApiTest
{
    static byte[] Int32Collection(params int[] values)
    {
        var bin = new byte[4 + values.Length * 4];
        BitConverter.GetBytes(values.Length).CopyTo(bin, 0);
        for (int i = 0; i < values.Length; i++)
        {
            BitConverter.GetBytes(values[i]).CopyTo(bin, 4 + i * 4);
        }
        return bin;
    }

    static byte[] BitArrayBinary(int length, params int[] array)
    {
        // (object header, int length, (int collection length, int[]))
        var bin = new byte[1 + 4 + 4 + array.Length * 4];
        bin[0] = 2;
        BitConverter.GetBytes(length).CopyTo(bin, 1);
        Int32Collection(array).CopyTo(bin, 5);
        return bin;
    }

    static Stack<T> CreateStack<T>(IEnumerable<T> values)
    {
        var stack = new Stack<T>();
        foreach (var item in values)
        {
            stack.Push(item);
        }
        return stack;
    }

    static CollectionPublicApiItem Item(int id)
    {
        return id % 4 == 0 ? null : new CollectionPublicApiItem { Id = id, Name = "v" + id };
    }

    static string ItemToString(CollectionPublicApiItem item)
    {
        return item == null ? "(null)" : item.Id + ":" + item.Name;
    }

    static CollectionPublicApiData Create(int count)
    {
        var ints = Enumerable.Range(1, count).ToArray();
        var strings = ints.Select(x => x % 4 == 0 ? null : "v" + x).ToArray();
        var items = ints.Select(x => Item(x)).ToArray();

        var bits = new BitArray(count);
        for (int i = 0; i < count; i += 3)
        {
            bits[i] = true;
        }

        return new CollectionPublicApiData
        {
            IntList = ints.ToList(),
            StringList = strings.ToList(),
            ItemList = items.ToList(),
            IntStack = CreateStack(ints),
            LongStack = CreateStack(ints.Select(x => (long)x * int.MaxValue)),
            StringStack = CreateStack(strings),
            ItemStack = CreateStack(items),
            IntImmutableArray = ImmutableArray.Create(ints),
            StringImmutableArray = ImmutableArray.Create(strings),
            ItemImmutableArray = ImmutableArray.Create(items),
            Bits = bits,
        };
    }

    static void AreSame(CollectionPublicApiData expected, CollectionPublicApiData actual)
    {
        CollectionAssert.AreEqual(expected.IntList, actual.IntList);
        CollectionAssert.AreEqual(expected.StringList, actual.StringList);
        CollectionAssert.AreEqual(expected.ItemList.Select(ItemToString).ToArray(), actual.ItemList.Select(ItemToString).ToArray());
        // enumerate Stack is the order of Pop
        CollectionAssert.AreEqual(expected.IntStack.ToArray(), actual.IntStack.ToArray());
        CollectionAssert.AreEqual(expected.LongStack.ToArray(), actual.LongStack.ToArray());
        CollectionAssert.AreEqual(expected.StringStack.ToArray(), actual.StringStack.ToArray());
        CollectionAssert.AreEqual(expected.ItemStack.Select(ItemToString).ToArray(), actual.ItemStack.Select(ItemToString).ToArray());
        CollectionAssert.AreEqual(expected.IntImmutableArray.ToArray(), actual.IntImmutableArray.ToArray());
        CollectionAssert.AreEqual(expected.StringImmutableArray.ToArray(), actual.StringImmutableArray.ToArray());
        CollectionAssert.AreEqual(expected.ItemImmutableArray.Select(ItemToString).ToArray(), actual.ItemImmutableArray.Select(ItemToString).ToArray());
        CollectionAssert.AreEqual(expected.Bits.Cast<bool>().ToArray(), actual.Bits.Cast<bool>().ToArray());
    }

    [Test]
    public void Roundtrip()
    {
        foreach (var count in new[] { 0, 1, 3, 5, 100, 1000, 100000 })
        {
            var value = Create(count);
            var bin = MemoryPackSerializer.Serialize(value);
            AreSame(value, MemoryPackSerializer.Deserialize<CollectionPublicApiData>(bin));

            // source is not changed, same binary in every serialize.
            AreSame(Create(count), value);
            CollectionAssert.AreEqual(bin, MemoryPackSerializer.Serialize(value));
        }

        // null and default
        var empty = MemoryPackSerializer.Deserialize<CollectionPublicApiData>(MemoryPackSerializer.Serialize(new CollectionPublicApiData()));
        Assert.IsNull(empty.IntList);
        Assert.IsNull(empty.IntStack);
        Assert.IsNull(empty.StringStack);
        Assert.IsTrue(empty.IntImmutableArray.IsDefault);
        Assert.IsTrue(empty.StringImmutableArray.IsDefault);
        Assert.IsNull(empty.Bits);
    }

    [Test]
    public void Overwrite()
    {
        foreach (var existingCount in new[] { 0, 3, 5, 1000 })
        {
            foreach (var count in new[] { 0, 3, 5, 1000 })
            {
                var source = Create(count);
                var bin = MemoryPackSerializer.Serialize(source);

                var value = Create(existingCount);
                var intList = value.IntList;
                var stringList = value.StringList;
                var intStack = value.IntStack;
                var stringStack = value.StringStack;

                MemoryPackSerializer.Deserialize(bin, ref value);
                AreSame(source, value);

                // List and Stack are reused.
                Assert.AreSame(intList, value.IntList);
                Assert.AreSame(stringList, value.StringList);
                Assert.AreSame(intStack, value.IntStack);
                Assert.AreSame(stringStack, value.StringStack);

                // collections are still valid.
                value.IntList.Add(9999);
                value.IntStack.Push(9999);
                value.StringStack.Push("last");
                Assert.AreEqual(count + 1, value.IntList.Count);
                Assert.AreEqual(9999, value.IntList[count]);
                Assert.AreEqual(9999, value.IntStack.Pop());
                Assert.AreEqual("last", value.StringStack.Pop());
                Assert.AreEqual(count, value.IntStack.Count);
            }
        }
    }

    [Test]
    public void BinaryIsSameAsDotNet()
    {
        // register formatters
        MemoryPackSerializer.Serialize(new CollectionPublicApiData());

        // Stack is serialized as the order of Push(bottom to top)
        var stack = CreateStack(new[] { 1, 2, 3 });
        CollectionAssert.AreEqual(Int32Collection(1, 2, 3), MemoryPackSerializer.Serialize(stack));
        CollectionAssert.AreEqual(new[] { 3, 2, 1 }, stack.ToArray());
        CollectionAssert.AreEqual(new[] { 3, 2, 1 }, MemoryPackSerializer.Deserialize<Stack<int>>(Int32Collection(1, 2, 3)).ToArray());

        CollectionAssert.AreEqual(Int32Collection(1, 2, 3), MemoryPackSerializer.Serialize(new List<int> { 1, 2, 3 }));
        CollectionAssert.AreEqual(new[] { 1, 2, 3 }, MemoryPackSerializer.Deserialize<List<int>>(Int32Collection(1, 2, 3)).ToArray());

        CollectionAssert.AreEqual(Int32Collection(1, 2, 3), MemoryPackSerializer.Serialize(ImmutableArray.Create(1, 2, 3)));
        CollectionAssert.AreEqual(new[] { 1, 2, 3 }, MemoryPackSerializer.Deserialize<ImmutableArray<int>>(Int32Collection(1, 2, 3)).ToArray());
        Assert.IsTrue(MemoryPackSerializer.Deserialize<ImmutableArray<int>>(Int32Collection()).IsEmpty);

        // same binary as array
        var strings = new[] { "a", null, "c" };
        var stringsBin = MemoryPackSerializer.Serialize(strings);
        CollectionAssert.AreEqual(stringsBin, MemoryPackSerializer.Serialize(CreateStack(strings)));
        CollectionAssert.AreEqual(stringsBin, MemoryPackSerializer.Serialize(ImmutableArray.Create(strings)));
        CollectionAssert.AreEqual(new[] { "c", null, "a" }, MemoryPackSerializer.Deserialize<Stack<string>>(stringsBin).ToArray());
        CollectionAssert.AreEqual(strings, MemoryPackSerializer.Deserialize<ImmutableArray<string>>(stringsBin).ToArray());
    }

    [Test]
    public void BitArrayUnusedBitsAreCleared()
    {
        // inner array of BitArray(100, true) is filled by 0xFFFFFFFF, unused bits of the last element are not written.
        CollectionAssert.AreEqual(BitArrayBinary(100, -1, -1, -1, 15), MemoryPackSerializer.Serialize(new BitArray(100, true)));
        CollectionAssert.AreEqual(BitArrayBinary(100, -1, -1, -1, 15), MemoryPackSerializer.Serialize(new BitArray(100).Not()));
        CollectionAssert.AreEqual(BitArrayBinary(64, -1, -1), MemoryPackSerializer.Serialize(new BitArray(64, true)));
        CollectionAssert.AreEqual(BitArrayBinary(31, int.MaxValue), MemoryPackSerializer.Serialize(new BitArray(31, true)));
        CollectionAssert.AreEqual(BitArrayBinary(0), MemoryPackSerializer.Serialize(new BitArray(0)));

        var shrink = new BitArray(1000, true);
        shrink.Length = 10;
        CollectionAssert.AreEqual(BitArrayBinary(10, 1023), MemoryPackSerializer.Serialize(shrink));

        // binary of the older version can have garbage in unused bits and longer array.
        var value = MemoryPackSerializer.Deserialize<BitArray>(BitArrayBinary(100, -1, -1, -1, -1));
        Assert.AreEqual(100, value.Length);
        Assert.IsTrue(value.Cast<bool>().All(x => x));

        var longer = MemoryPackSerializer.Deserialize<BitArray>(BitArrayBinary(10, Enumerable.Repeat(-1, 32).ToArray()));
        Assert.AreEqual(10, longer.Length);
        CollectionAssert.AreEqual(BitArrayBinary(10, 1023), MemoryPackSerializer.Serialize(longer));

        // older version in Unity(Mono) wrote the length as 0, https://github.com/Cysharp/MemoryPack/issues/399
        Assert.AreEqual(0, MemoryPackSerializer.Deserialize<BitArray>(BitArrayBinary(0, 1, 2, 3, 4)).Length);
    }

    [Test]
    public void BitArrayLengthIsNotMatchedToArray()
    {
        Assert.Throws<MemoryPackSerializationException>(() => MemoryPackSerializer.Deserialize<BitArray>(BitArrayBinary(1)));
        Assert.Throws<MemoryPackSerializationException>(() => MemoryPackSerializer.Deserialize<BitArray>(BitArrayBinary(33, 1)));
        Assert.Throws<MemoryPackSerializationException>(() => MemoryPackSerializer.Deserialize<BitArray>(BitArrayBinary(int.MaxValue)));
        Assert.Throws<MemoryPackSerializationException>(() => MemoryPackSerializer.Deserialize<BitArray>(BitArrayBinary(-1)));
        Assert.Throws<MemoryPackSerializationException>(() => MemoryPackSerializer.Deserialize<BitArray>(BitArrayBinary(int.MinValue, 1, 2)));
    }
}
