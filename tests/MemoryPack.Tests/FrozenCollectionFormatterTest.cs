#if NET8_0_OR_GREATER
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Frozen;

namespace MemoryPack.Tests;

public class FrozenCollectionFormatterTest
{
    [Fact]
    public void FrozenSet()
    {
        var set = new HashSet<int>();
        set.Add(1);
        set.Add(2);
        set.Add(3);
        set.Add(4);
        set.Add(5);

        var value = set.ToFrozenSet();
        var bin = MemoryPackSerializer.Serialize(value);
        var deserializedValue = MemoryPackSerializer.Deserialize<FrozenSet<int>>(bin);
        deserializedValue.Should().Equal(value);
    }

    [Fact]
    public void FrozenDictionary()
    {
        var dict = new Dictionary<int, int>()
        {
            { 1, 2 }, { 3, 4 }, { 4, 5 }, { 6, 7 }, { 8, 9 }
        };
        var value = dict.ToFrozenDictionary();
        var bin = MemoryPackSerializer.Serialize(value);
        MemoryPackSerializer.Deserialize<FrozenDictionary<int, int>>(bin).Should().BeEquivalentTo(value);
    }
}
#endif
