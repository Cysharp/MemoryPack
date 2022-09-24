using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemoryPack.Tests;

public class ArrayFormatterTest
{
    private T Convert<T>(T value)
    {
        return MemoryPackSerializer.Deserialize<T>(MemoryPackSerializer.Serialize(value))!;
    }

    [Theory]
    [InlineData(100, 100, 10, 5)]
    [InlineData(10, 20, 15, 5)]
    [InlineData(3, 5, 10, 15)]
    public void MultiDimensional(int dataI, int dataJ, int dataK, int dataL)
    {
        var two = new ValueTuple<int, int>[dataI, dataJ];
        var three = new ValueTuple<int, int, int>[dataI, dataJ, dataK];
        var four = new ValueTuple<int, int, int, int>[dataI, dataJ, dataK, dataL];

        for (int i = 0; i < dataI; i++)
        {
            for (int j = 0; j < dataJ; j++)
            {
                two[i, j] = (i, j);
                for (int k = 0; k < dataK; k++)
                {
                    three[i, j, k] = (i, j, k);
                    for (int l = 0; l < dataL; l++)
                    {
                        four[i, j, k, l] = (i, j, k, l);
                    }
                }
            }
        }

        (int, int)[,] cTwo = this.Convert(two);
        (int, int, int)[,,] cThree = this.Convert(three);
        (int, int, int, int)[,,,] cFour = this.Convert(four);

        cTwo.Length.Should().Be(two.Length);
        cThree.Length.Should().Be(three.Length);
        cFour.Length.Should().Be(four.Length);

        for (int i = 0; i < dataI; i++)
        {
            for (int j = 0; j < dataJ; j++)
            {
                cTwo[i, j].Should().Be(two[i, j]);
                for (int k = 0; k < dataK; k++)
                {
                    cThree[i, j, k].Should().Be(three[i, j, k]);
                    for (int l = 0; l < dataL; l++)
                    {
                        cFour[i, j, k, l].Should().Be(four[i, j, k, l]);
                    }
                }
            }
        }
    }

    [Theory]
    [InlineData(100, 100, 10, 5)]
    [InlineData(10, 20, 15, 5)]
    [InlineData(3, 5, 10, 15)]
    public void MultiDimensional2(int dataI, int dataJ, int dataK, int dataL)
    {
        var two = new ValueTuple<ObjectValue, ObjectValue>[dataI, dataJ];
        var three = new ValueTuple<ObjectValue, ObjectValue, ObjectValue>[dataI, dataJ, dataK];
        var four = new ValueTuple<ObjectValue, ObjectValue, ObjectValue, ObjectValue>[dataI, dataJ, dataK, dataL];

        for (int i = 0; i < dataI; i++)
        {
            for (int j = 0; j < dataJ; j++)
            {
                two[i, j] = (i, j);
                for (int k = 0; k < dataK; k++)
                {
                    three[i, j, k] = (i, j, k);
                    for (int l = 0; l < dataL; l++)
                    {
                        four[i, j, k, l] = (i, j, k, l);
                    }
                }
            }
        }

        (ObjectValue, ObjectValue)[,] cTwo = this.Convert(two);
        (ObjectValue, ObjectValue, ObjectValue)[,,] cThree = this.Convert(three);
        (ObjectValue, ObjectValue, ObjectValue, ObjectValue)[,,,] cFour = this.Convert(four);

        cTwo.Length.Should().Be(two.Length);
        cThree.Length.Should().Be(three.Length);
        cFour.Length.Should().Be(four.Length);

        for (int i = 0; i < dataI; i++)
        {
            for (int j = 0; j < dataJ; j++)
            {
                cTwo[i, j].Should().Be(two[i, j]);
                for (int k = 0; k < dataK; k++)
                {
                    cThree[i, j, k].Should().Be(three[i, j, k]);
                    for (int l = 0; l < dataL; l++)
                    {
                        cFour[i, j, k, l].Should().Be(four[i, j, k, l]);
                    }
                }
            }
        }
    }

    [Fact]
    public void MultiDimentionalOverwrite()
    {
        var two = new int[3, 3];
        two[0, 0] = 0;
        two[0, 1] = 1;
        two[0, 2] = 2;
        two[1, 0] = 3;
        two[1, 1] = 4;
        two[1, 2] = 5;
        two[2, 0] = 6;
        two[2, 1] = 7;
        two[2, 2] = 8;

        var bin = MemoryPackSerializer.Serialize(two);
        var refArray = two;

        Array.Clear(two);
        MemoryPackSerializer.Deserialize(bin, ref two);
        Debug.Assert(two != null);
        two.Should().BeSameAs(refArray);
        two[0, 0].Should().Be(0);
        two[0, 1].Should().Be(1);
        two[0, 2].Should().Be(2);
        two[1, 0].Should().Be(3);
        two[1, 1].Should().Be(4);
        two[1, 2].Should().Be(5);
        two[2, 0].Should().Be(6);
        two[2, 1].Should().Be(7);
        two[2, 2].Should().Be(8);
    }
}

[MemoryPackable]
public partial class ObjectValue : IEquatable<ObjectValue>
{
    public int Value { get; }

    public ObjectValue(int value)
    {
        this.Value = value;
    }

    public static implicit operator ObjectValue(int value) { return new ObjectValue(value); }
    public static implicit operator int(ObjectValue value) { return value.Value; }

    public override string ToString()
    {
        return Value.ToString();
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public bool Equals(ObjectValue? other)
    {
        if (other == null) return false;
        return Value == other;
    }
}
