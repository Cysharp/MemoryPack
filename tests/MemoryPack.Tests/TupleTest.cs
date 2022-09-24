using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemoryPack.Tests;

public class TupleTEst
{

    private void ConvertEqual<T>(T value)
    {
        MemoryPackSerializer.Deserialize<T>(MemoryPackSerializer.Serialize(value))
            .Should().Be(value);
    }

    [Fact]
    public void TupleT()
    {
        ConvertEqual(Tuple.Create(1));
        ConvertEqual(Tuple.Create(1, 2));
        ConvertEqual(Tuple.Create(1, 2, 3));
        ConvertEqual(Tuple.Create(1, 2, 3, 4));
        ConvertEqual(Tuple.Create(1, 2, 3, 4, 5));
        ConvertEqual(Tuple.Create(1, 2, 3, 4, 5, 6));
        ConvertEqual(Tuple.Create(1, 2, 3, 4, 5, 6, 7));
        ConvertEqual(Tuple.Create(1, 2, 3, 4, 5, 6, 7, 8));
    }

    [Fact]
    public void ValueTupleT()
    {
        ConvertEqual(ValueTuple.Create(1));
        ConvertEqual(ValueTuple.Create(1, 2));
        ConvertEqual(ValueTuple.Create(1, 2, 3));
        ConvertEqual(ValueTuple.Create(1, 2, 3, 4));
        ConvertEqual(ValueTuple.Create(1, 2, 3, 4, 5));
        ConvertEqual(ValueTuple.Create(1, 2, 3, 4, 5, 6));
        ConvertEqual(ValueTuple.Create(1, 2, 3, 4, 5, 6, 7));
        ConvertEqual(ValueTuple.Create(1, 2, 3, 4, 5, 6, 7, 8));
    }
}
