using MemoryPack.Tests.Models;
using Newtonsoft.Json;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Collections.Specialized;
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

    [Fact]
    public void Check2()
    {
        var checker = new ArrayOptimizeCheck()
        {
            Array1 = new[] { new StandardTypeTwo { One = 9, Two = 2 }, new StandardTypeTwo { One = 999, Two = 444 } },
            List1 = new List<StandardTypeTwo?> { new StandardTypeTwo { One = 93, Two = 12 }, new StandardTypeTwo { One = 9499, Two = 45344 } }
        };

        var bin = MemoryPackSerializer.Serialize(checker);
        var v2 = MemoryPackSerializer.Deserialize<ArrayOptimizeCheck>(bin);
#pragma warning disable CS8602
        v2!.Array1[0].One.Should().Be(checker.Array1[0].One);
        v2!.Array1[0].Two.Should().Be(checker.Array1[0].Two);
        v2!.Array1[1].One.Should().Be(checker.Array1[1].One);
        v2!.Array1[1].Two.Should().Be(checker.Array1[1].Two);

        v2!.List1[0].One.Should().Be(checker.List1[0].One);
        v2!.List1[0].Two.Should().Be(checker.List1[0].Two);
        v2!.List1[1].One.Should().Be(checker.List1[1].One);
        v2!.List1[1].Two.Should().Be(checker.List1[1].Two);
    }

    [Fact]
    public void BoolArray()
    {
        var rand = new Random();
        for (int i = 0; i < 1000; i++)
        {
            var data = Enumerable.Range(0, i).Select(_ => rand.Next(0, 2) == 0).ToArray();
            var value = new BitPackSingleData { Data = data };

            var bin = MemoryPackSerializer.Serialize(value);
            var value2 = MemoryPackSerializer.Deserialize<BitPackSingleData>(bin);

            value2.Data.Should().Equal(data);
        }
        for (int i = 0; i < 1000; i++)
        {
            var data = Enumerable.Range(0, i).Select(_ => rand.Next(0, 2) == 0).ToArray();
            var value = new BitPackData { Data = data, AAA = i };

            var bin = MemoryPackSerializer.Serialize(value);
            var value2 = MemoryPackSerializer.Deserialize<BitPackData>(bin);

            value2.Data.Should().Equal(data);
            value2.AAA.Should().Be(i);
        }
    }

    [Fact]
    public void MemoryOwnerTest()
    {
        var memow = MemoryPool<byte>.Shared.Rent(100);



        var bin = MemoryPackSerializer.Serialize(memow);
        var value2 = MemoryPackSerializer.Deserialize<IMemoryOwner<byte>>(bin);



    }
}





[MemoryPackable]
public partial class BitPackData
{
    [BitPackFormatter]
    public bool[]? Data { get; set; }
    public int AAA { get; set; }
}

[MemoryPackable]
public partial class BitPackSingleData
{
    [BitPackFormatter]
    public bool[]? Data { get; set; }
}
