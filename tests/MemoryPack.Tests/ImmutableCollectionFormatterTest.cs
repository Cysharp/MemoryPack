using System.Collections.Generic;
using System.Collections.Immutable;

namespace MemoryPack.Tests;

public class ImmutableCollectionFormatterTest
{
    T? Convert<T>(T? value)
    {
        var bin = MemoryPackSerializer.Serialize(value);
        return MemoryPackSerializer.Deserialize<T>(bin);
    }

    TAs? ConvertAs<T, TAs>(T? value, TAs dummy)
        where T : TAs
    {
        var bin = MemoryPackSerializer.Serialize<TAs>(value);
        return MemoryPackSerializer.Deserialize<TAs>(bin);
    }

    [Fact]
    public void Collection()
    {
        {
            var value = ImmutableArray.Create(1, 10, 100, 2, 4, 530, 647, 73, 8, 42);
            Convert(value).Should().Equal(value);
        }
        {
            var value = ImmutableList.Create(1, 10, 100, 2, 4, 530, 647, 73, 8, 42);
            Convert(value).Should().Equal(value);
            ConvertAs(value, default(IImmutableList<int>)).Should().Equal(value);
        }
        {
            var value = ImmutableQueue.Create(1, 10, 100, 2, 4, 530, 647, 73, 8, 42);
            Convert(value).Should().Equal(value);
            ConvertAs(value, default(IImmutableQueue<int>)).Should().Equal(value);
        }
        {
            var value = ImmutableStack.Create(1, 10, 100, 2, 4, 530, 647, 73, 8, 42);
            Convert(value).Should().Equal(value);
            ConvertAs(value, default(IImmutableStack<int>)).Should().Equal(value);
        }
        {
            var value = ImmutableHashSet.Create(1, 10, 100, 2, 4, 530, 647, 73, 8, 42);
            Convert(value).Should().Equal(value);
            ConvertAs(value, default(IImmutableSet<int>)).Should().Equal(value);
        }
        {
            var value = ImmutableSortedSet.Create(1, 10, 100, 2, 4, 530, 647, 73, 8, 42);
            Convert(value).Should().Equal(value);
        }
    }

    [Fact]
    public void Dictionary()
    {
        {
            var value = ImmutableDictionary.CreateRange(new KeyValuePair<int, int>[]
            {
                new(1, 10),
                new(2, 20),
                new(3, 30),
                new(5, 50),
            });

            Convert(value).Should().Equal(value);
            ConvertAs(value, default(IImmutableDictionary<int,int>)).Should().Equal(value);
        }
        {
            var value = ImmutableSortedDictionary.CreateRange(new KeyValuePair<int, int>[]
            {
                new(1, 10),
                new(2, 20),
                new(3, 30),
                new(5, 50),
            });

            Convert(value).Should().Equal(value);
            ConvertAs(value, default(IImmutableDictionary<int, int>)).Should().Equal(value);
        }
    }
}
