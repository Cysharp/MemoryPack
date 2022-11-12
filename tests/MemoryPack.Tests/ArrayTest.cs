using MemoryPack.Tests.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemoryPack.Tests;

public class ArrayTest
{
    [Fact]
    public void Check()
    {
        var checker = new ArrayCheck
        {
            Array1 = new[] { 1, 10, -1000, int.MaxValue, int.MinValue },
            Array2 = new int?[] { 300, null, -99999, null, 234242 },
            Array3 = new[] { "foo", "bar", "baz", "", "t" },
            Array4 = new[] { "zzzz", null, "", "あいうえお" }
        };

        var bin = MemoryPackSerializer.Serialize(checker);
        var v2 = MemoryPackSerializer.Deserialize<ArrayCheck>(bin);

        v2!.Array1.Should().Equal(checker.Array1);
        v2!.Array2.Should().Equal(checker.Array2);
        v2!.Array3.Should().Equal(checker.Array3);
        v2!.Array4.Should().Equal(checker.Array4);
    }
}
