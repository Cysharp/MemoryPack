using System;
using System.Buffers.Binary;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace MemoryPack.Tests;

// List, Stack, ImmutableArray and BitArray formatters only use the public API(do not depend on the private field layout), binary format must not be changed.
// https://github.com/Cysharp/MemoryPack/issues/460
public class CollectionPublicApiTest
{
    static byte[] Int32Collection(params int[] values)
    {
        var bin = new byte[4 + values.Length * 4];
        BinaryPrimitives.WriteInt32LittleEndian(bin, values.Length);
        for (int i = 0; i < values.Length; i++)
        {
            BinaryPrimitives.WriteInt32LittleEndian(bin.AsSpan(4 + i * 4), values[i]);
        }
        return bin;
    }

    static Stack<T> CreateStack<T>(params T[] values)
    {
        var stack = new Stack<T>();
        foreach (var item in values)
        {
            stack.Push(item);
        }
        return stack;
    }

    [Fact]
    public void ListBinary()
    {
        var list = new List<int> { 1, 2, 3 };
        MemoryPackSerializer.Serialize(list).Should().Equal(Int32Collection(1, 2, 3));
        MemoryPackSerializer.Deserialize<List<int>>(Int32Collection(1, 2, 3)).Should().Equal(1, 2, 3);

        MemoryPackSerializer.Serialize(new List<int>()).Should().Equal(Int32Collection());
        MemoryPackSerializer.Deserialize<List<int>>(Int32Collection()).Should().BeEmpty();
        MemoryPackSerializer.Deserialize<List<int>>(MemoryPackSerializer.Serialize<List<int>>(null)).Should().BeNull();
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(0, 5)]
    [InlineData(5, 0)]
    [InlineData(5, 5)]
    [InlineData(3, 5)]
    [InlineData(5, 3)]
    [InlineData(1000, 3)]
    [InlineData(3, 1000)]
    public void ListOverwrite(int existingCount, int count)
    {
        // unmanaged
        {
            var source = Enumerable.Range(1, count).ToList();
            var bin = MemoryPackSerializer.Serialize(source);

            var list = Enumerable.Range(1, existingCount).Select(x => -x).ToList();
            var original = list;
            MemoryPackSerializer.Deserialize(bin, ref list);

            list.Should().BeSameAs(original);
            list.Should().Equal(source);

            // list is still valid.
            list!.Add(9999);
            list.Should().Equal(source.Append(9999));
            list.RemoveAt(list.Count - 1);
            list.Should().Equal(source);
        }
        // reference
        {
            var source = Enumerable.Range(1, count).Select(x => (string?)(x % 4 == 0 ? null : "v" + x)).ToList();
            var bin = MemoryPackSerializer.Serialize(source);

            var list = Enumerable.Range(1, existingCount).Select(x => (string?)("old" + x)).ToList();
            var original = list;
            MemoryPackSerializer.Deserialize(bin, ref list);

            list.Should().BeSameAs(original);
            list.Should().Equal(source);

            list!.Add("last");
            list.Should().Equal(source.Append("last"));
        }
        // packable
        {
            var source = new CollectionPublicApiData { List = Enumerable.Range(1, count).Select(x => new CollectionPublicApiItem { Id = x, Name = "v" + x }).ToList() };
            var bin = MemoryPackSerializer.Serialize(source);

            var value = new CollectionPublicApiData { List = Enumerable.Range(1, existingCount).Select(x => new CollectionPublicApiItem { Id = -x, Name = "old" + x }).ToList() };
            var original = value.List;
            MemoryPackSerializer.Deserialize(bin, ref value);

            value!.List.Should().BeSameAs(original);
            value.List!.Select(x => (x!.Id, x.Name)).Should().Equal(source.List.Select(x => (x!.Id, x.Name)));
        }
    }

    [Fact]
    public void StackBinary()
    {
        // serialize as the order of Push(bottom to top)
        var stack = CreateStack(1, 2, 3);
        var bin = MemoryPackSerializer.Serialize(stack);
        bin.Should().Equal(Int32Collection(1, 2, 3));

        // source is not changed
        stack.Should().Equal(3, 2, 1);

        var stack2 = MemoryPackSerializer.Deserialize<Stack<int>>(Int32Collection(1, 2, 3))!;
        stack2.Should().Equal(3, 2, 1);
        stack2.Pop().Should().Be(3);
        stack2.Pop().Should().Be(2);
        stack2.Pop().Should().Be(1);

        MemoryPackSerializer.Serialize(new Stack<int>()).Should().Equal(Int32Collection());
        MemoryPackSerializer.Deserialize<Stack<int>>(Int32Collection()).Should().BeEmpty();
        MemoryPackSerializer.Deserialize<Stack<int>>(MemoryPackSerializer.Serialize<Stack<int>>(null)).Should().BeNull();

        // same binary as List and array
        MemoryPackSerializer.Serialize(CreateStack("a", null, "c")).Should().Equal(MemoryPackSerializer.Serialize(new[] { "a", null, "c" }));
        MemoryPackSerializer.Deserialize<Stack<string?>>(MemoryPackSerializer.Serialize(new[] { "a", null, "c" })).Should().Equal("c", null, "a");
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(0, 5)]
    [InlineData(5, 0)]
    [InlineData(5, 5)]
    [InlineData(3, 5)]
    [InlineData(5, 3)]
    [InlineData(1000, 3)]
    [InlineData(3, 1000)]
    [InlineData(0, 100000)]
    public void StackOverwrite(int existingCount, int count)
    {
        // unmanaged
        {
            var source = CreateStack(Enumerable.Range(1, count).ToArray());
            var bin = MemoryPackSerializer.Serialize(source);
            MemoryPackSerializer.Deserialize<Stack<int>>(bin).Should().Equal(source);

            var stack = CreateStack(Enumerable.Range(1, existingCount).Select(x => -x).ToArray());
            var original = stack;
            MemoryPackSerializer.Deserialize(bin, ref stack);

            stack.Should().BeSameAs(original);
            stack.Should().Equal(source);

            // stack is still valid.
            stack!.Push(9999);
            stack.Pop().Should().Be(9999);
            stack.Should().Equal(source);
        }
        // unmanaged struct(larger than int)
        {
            var source = CreateStack(Enumerable.Range(1, count).Select(x => (x, (long)x * 2, Guid.NewGuid())).ToArray());
            var bin = MemoryPackSerializer.Serialize(source);

            var stack = CreateStack(Enumerable.Range(1, existingCount).Select(x => (-x, 0L, Guid.Empty)).ToArray());
            MemoryPackSerializer.Deserialize(bin, ref stack);
            stack.Should().Equal(source);
        }
        // reference
        {
            var source = CreateStack(Enumerable.Range(1, count).Select(x => (string?)(x % 4 == 0 ? null : "v" + x)).ToArray());
            var bin = MemoryPackSerializer.Serialize(source);
            MemoryPackSerializer.Deserialize<Stack<string?>>(bin).Should().Equal(source);

            var stack = CreateStack(Enumerable.Range(1, existingCount).Select(x => (string?)("old" + x)).ToArray());
            var original = stack;
            MemoryPackSerializer.Deserialize(bin, ref stack);

            stack.Should().BeSameAs(original);
            stack.Should().Equal(source);
        }
    }

    [Fact]
    public void StackMembers()
    {
        var value = new CollectionPublicApiData
        {
            Stack = CreateStack(new CollectionPublicApiItem { Id = 1, Name = "a" }, null, new CollectionPublicApiItem { Id = 3, Name = "c" }),
            NestedStack = CreateStack(CreateStack(1, 2), CreateStack<int>(), CreateStack(3, 4, 5)),
            ImmutableArray = ImmutableArray.Create("a", null, "c"),
            Bits = new BitArray(new[] { true, false, true }),
        };

        var value2 = MemoryPackSerializer.Deserialize<CollectionPublicApiData>(MemoryPackSerializer.Serialize(value))!;

        value2.Stack!.Select(x => x?.Id).Should().Equal(3, null, 1);
        value2.Stack!.Select(x => x?.Name).Should().Equal("c", null, "a");
        value2.NestedStack!.Select(x => x!.ToArray()).Should().BeEquivalentTo(new[] { new[] { 5, 4, 3 }, Array.Empty<int>(), new[] { 2, 1 } }, x => x.WithStrictOrdering());
        value2.ImmutableArray.Should().Equal("a", null, "c");
        value2.Bits!.Cast<bool>().Should().Equal(true, false, true);
    }

    [Fact]
    public void ImmutableArrayBinary()
    {
        MemoryPackSerializer.Serialize(ImmutableArray.Create(1, 2, 3)).Should().Equal(Int32Collection(1, 2, 3));
        MemoryPackSerializer.Deserialize<ImmutableArray<int>>(Int32Collection(1, 2, 3)).Should().Equal(1, 2, 3);

        MemoryPackSerializer.Deserialize<ImmutableArray<int>>(Int32Collection()).IsEmpty.Should().BeTrue();
        MemoryPackSerializer.Deserialize<ImmutableArray<int>>(MemoryPackSerializer.Serialize(default(ImmutableArray<int>))).IsDefault.Should().BeTrue();

        var strings = ImmutableArray.Create("a", null, "c");
        MemoryPackSerializer.Deserialize<ImmutableArray<string?>>(MemoryPackSerializer.Serialize(strings)).Should().Equal(strings);

        var large = Enumerable.Range(1, 100000).ToImmutableArray();
        MemoryPackSerializer.Deserialize<ImmutableArray<int>>(MemoryPackSerializer.Serialize(large)).Should().Equal(large);

        var largeStrings = Enumerable.Range(1, 1000).Select(x => "v" + x).ToImmutableArray();
        MemoryPackSerializer.Deserialize<ImmutableArray<string>>(MemoryPackSerializer.Serialize(largeStrings)).Should().Equal(largeStrings);
    }

    static byte[] BitArrayBinary(int length, params int[] array)
    {
        // (object header, int length, (int collection length, int[]))
        var bin = new byte[1 + 4 + 4 + array.Length * 4];
        bin[0] = 2;
        BinaryPrimitives.WriteInt32LittleEndian(bin.AsSpan(1), length);
        Int32Collection(array).CopyTo(bin, 5);
        return bin;
    }

    [Fact]
    public void BitArrayUnusedBitsAreCleared()
    {
        // inner array of BitArray(100, true) is filled by 0xFFFFFFFF, unused bits of the last element are not written.
        MemoryPackSerializer.Serialize(new BitArray(100, true)).Should().Equal(BitArrayBinary(100, -1, -1, -1, 0b1111));
        MemoryPackSerializer.Serialize(new BitArray(100).Not()).Should().Equal(BitArrayBinary(100, -1, -1, -1, 0b1111));
        MemoryPackSerializer.Serialize(new BitArray(64, true)).Should().Equal(BitArrayBinary(64, -1, -1));
        MemoryPackSerializer.Serialize(new BitArray(1, true)).Should().Equal(BitArrayBinary(1, 1));
        MemoryPackSerializer.Serialize(new BitArray(31, true)).Should().Equal(BitArrayBinary(31, int.MaxValue));
        MemoryPackSerializer.Serialize(new BitArray(0)).Should().Equal(BitArrayBinary(0));

        // shrink, inner array is larger than required but only writes required length.
        var shrink = new BitArray(1000, true);
        shrink.Length = 10;
        MemoryPackSerializer.Serialize(shrink).Should().Equal(BitArrayBinary(10, 0b1111111111));
    }

    [Fact]
    public void BitArrayReadsUnusedBitsAndLongerArray()
    {
        // binary of the older version can have garbage in unused bits and longer array.
        var value = MemoryPackSerializer.Deserialize<BitArray>(BitArrayBinary(100, -1, -1, -1, -1))!;
        value.Length.Should().Be(100);
        value.Cast<bool>().Should().OnlyContain(x => x);
        value.Length = 128;
        value.Cast<bool>().Skip(100).Should().OnlyContain(x => !x); // garbage does not appear

        var longer = MemoryPackSerializer.Deserialize<BitArray>(BitArrayBinary(10, Enumerable.Repeat(-1, 32).ToArray()))!;
        longer.Length.Should().Be(10);
        longer.Cast<bool>().Should().OnlyContain(x => x);
        MemoryPackSerializer.Serialize(longer).Should().Equal(BitArrayBinary(10, 0b1111111111));

        // older version in Unity(Mono) wrote the length as 0, https://github.com/Cysharp/MemoryPack/issues/399
        MemoryPackSerializer.Deserialize<BitArray>(BitArrayBinary(0, 1, 2, 3, 4))!.Length.Should().Be(0);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(32)]
    [InlineData(33)]
    [InlineData(int.MaxValue)]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    public void BitArrayLengthIsNotMatchedToArray(int length)
    {
        // must not allocate by the length that is not backed by the data.
        var array = length == 33 ? new[] { 1 } : Array.Empty<int>();
        Assert.Throws<MemoryPackSerializationException>(() => MemoryPackSerializer.Deserialize<BitArray>(BitArrayBinary(length, array)));

        // null array
        var bin = new byte[] { 2, 0, 0, 0, 0, 255, 255, 255, 255 };
        BinaryPrimitives.WriteInt32LittleEndian(bin.AsSpan(1), length);
        Assert.Throws<MemoryPackSerializationException>(() => MemoryPackSerializer.Deserialize<BitArray>(bin));
    }

    [Fact]
    public void BitArrayNullArray()
    {
        MemoryPackSerializer.Deserialize<BitArray>(new byte[] { 2, 0, 0, 0, 0, 255, 255, 255, 255 })!.Length.Should().Be(0);
    }
}

[MemoryPackable]
public partial class CollectionPublicApiItem
{
    public int Id { get; set; }
    public string? Name { get; set; }
}

[MemoryPackable]
public partial class CollectionPublicApiData
{
    public List<CollectionPublicApiItem?>? List { get; set; }
    public Stack<CollectionPublicApiItem?>? Stack { get; set; }
    public Stack<Stack<int>?>? NestedStack { get; set; }
    public ImmutableArray<string?> ImmutableArray { get; set; }
    public BitArray? Bits { get; set; }
}
