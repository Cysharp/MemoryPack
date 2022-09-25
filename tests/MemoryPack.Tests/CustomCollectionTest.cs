using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemoryPack.Tests;

public class CustomCollectionTest
{
    T Convert<T>(T value)
    {
        var bin = MemoryPackSerializer.Serialize(value);
        return MemoryPackSerializer.Deserialize<T>(bin)!;
    }

    [Fact]
    public void NonGenerics()
    {
        var l = new ListInt { 1, 2, 3, 4, 5, 6, 7 };
        Convert(l).Should().Equal(l);

        var s = new SetInt { 1, 10, 20, 30 };
        Convert(s).Should().Equal(s);

        var d = new DictionaryIntInt { { 1, 10 }, { 2, 30 }, { 65, 2342 } };
        Convert(d).Should().Equal(d);
    }

    [Fact]
    public void Generics()
    {
        var l = new ListGenerics<int> { 1, 2, 3, 4, 5, 6, 7 };
        Convert(l).Should().Equal(l);

        var s = new SetGenerics<int> { 1, 10, 20, 30 };
        Convert(s).Should().Equal(s);

        var d = new DictionaryGenerics<int, int> { { 1, 10 }, { 2, 30 }, { 65, 2342 } };
        Convert(d).Should().Equal(d);
    }

}

[MemoryPackable(GenerateType.Collection)]
public partial class ListInt : List<int>
{
}

[MemoryPackable(GenerateType.Collection)]
public partial class SetInt : HashSet<int>
{
}

[MemoryPackable(GenerateType.Collection)]
public partial class DictionaryIntInt : Dictionary<int, int>
{
}


[MemoryPackable(GenerateType.Collection)]
public partial class ListGenerics<T> : List<T>
{
}

[MemoryPackable(GenerateType.Collection)]
public partial class SetGenerics<T> : HashSet<T>
{
}

[MemoryPackable(GenerateType.Collection)]
public partial class DictionaryGenerics<TK, TV> : Dictionary<TK, TV>
    where TK : notnull
{
}
