using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace MemoryPack.Tests;

public partial class UnityDateTimeOffsetTest
{
    public static IEnumerable<object[]> Values() => Net10DateTimeOffsetTest.Values();

    static void ShouldBeSame(DateTimeOffset actual, DateTimeOffset expected)
    {
        // DateTimeOffset.Equals only compares UtcDateTime
        actual.UtcTicks.Should().Be(expected.UtcTicks);
        actual.Offset.Should().Be(expected.Offset);
        actual.Ticks.Should().Be(expected.Ticks);
    }

    // (long utcTicks, short offsetMinutes, 6 bytes zero)
    static byte[] ExpectedBinary(DateTimeOffset value)
    {
        var bin = new byte[16];
        BinaryPrimitives.WriteInt64LittleEndian(bin, value.UtcTicks);
        BinaryPrimitives.WriteInt16LittleEndian(bin.AsSpan(8), (short)value.Offset.TotalMinutes);
        return bin;
    }

    [Fact]
    public void Size()
    {
        Unsafe.SizeOf<UnityDateTimeOffset>().Should().Be(16);
    }

    [Theory]
    [MemberData(nameof(Values))]
    public void Convert(DateTimeOffset value)
    {
        UnityDateTimeOffset unity = value;
        DateTimeOffset value2 = unity;
        ShouldBeSame(value2, value);
        ShouldBeSame(new UnityDateTimeOffset(value).ToDateTimeOffset(), value);
        unity.ToString().Should().Be(value.ToString());

        // Net10DateTimeOffset <-> UnityDateTimeOffset via DateTimeOffset
        Net10DateTimeOffset net10 = unity.ToDateTimeOffset();
        UnityDateTimeOffset unity2 = net10.ToDateTimeOffset();
        ShouldBeSame(unity2, value);
    }

    [Theory]
    [MemberData(nameof(Values))]
    public void BinaryIsFixed(DateTimeOffset value)
    {
        var expected = ExpectedBinary(value);

        // serialized multiple times to check there is no uninitialized padding.
        for (int i = 0; i < 10; i++)
        {
            MemoryPackSerializer.Serialize<UnityDateTimeOffset>(value).Should().Equal(expected);
        }

        ShouldBeSame(MemoryPackSerializer.Deserialize<UnityDateTimeOffset>(expected), value);
    }

    [Fact]
    public void KnownBinary()
    {
        // https://github.com/Cysharp/MemoryPack/issues/192
        // class that has one DateTimeOffset member, value is new(2023, 10, 25, 17, 45, 00, TimeSpan.FromHours(8))
        var value = new DateTimeOffset(2023, 10, 25, 17, 45, 00, TimeSpan.FromHours(8));

        // serialized in Unity 2022.3.7f1
        var unity = new byte[] { 1, 0, 182, 36, 14, 63, 213, 219, 8, 224, 1, 0, 0, 0, 0, 0, 0 };
        // serialized in .NET 7
        var net7 = new byte[] { 1, 224, 1, 0, 0, 0, 0, 0, 0, 0, 182, 36, 14, 63, 213, 219, 8 };

        ShouldBeSame(MemoryPackSerializer.Deserialize<UnityDateTimeOffsetHolder>(unity)!.Value, value);
        ShouldBeSame(MemoryPackSerializer.Deserialize<Net10DateTimeOffsetHolder>(net7)!.Value, value);

        MemoryPackSerializer.Serialize(new UnityDateTimeOffsetHolder { Value = value }).Should().Equal(unity);
        MemoryPackSerializer.Serialize(new Net10DateTimeOffsetHolder { Value = value }).Should().Equal(net7); // positive offset, same as .NET 10
    }

    [Theory]
    [MemberData(nameof(Values))]
    public void ReadDateTimeOffsetBinaryOfUnity(DateTimeOffset value)
    {
        // (long utcTicks, short offsetMinutes, 6 bytes padding(not initialized))
        var bin = new byte[16];
        var random = new Random(value.GetHashCode());
        for (int i = 0; i < 20; i++)
        {
            random.NextBytes(bin.AsSpan(10, 6)); // garbage padding
            BinaryPrimitives.WriteInt64LittleEndian(bin, value.UtcTicks);
            BinaryPrimitives.WriteInt16LittleEndian(bin.AsSpan(8), (short)value.Offset.TotalMinutes);

            ShouldBeSame(MemoryPackSerializer.Deserialize<UnityDateTimeOffset>(bin), value);
        }
    }

    [Fact]
    public void Members()
    {
        var value = new UnityDateTimeOffsetData
        {
            Value = new DateTimeOffset(2026, 11, 30, 16, 48, 36, TimeSpan.FromMinutes(-300)),
            Nullable = new DateTimeOffset(2026, 11, 30, 16, 48, 36, TimeSpan.FromMinutes(540)),
            NullableNull = null,
            Array = Values().Select(x => (UnityDateTimeOffset)(DateTimeOffset)x[0]).ToArray(),
            List = Values().Select(x => (UnityDateTimeOffset)(DateTimeOffset)x[0]).ToList(),
        };

        var bin = MemoryPackSerializer.Serialize(value);
        var value2 = MemoryPackSerializer.Deserialize<UnityDateTimeOffsetData>(bin)!;

        ShouldBeSame(value2.Value, value.Value);
        ShouldBeSame(value2.Nullable!.Value, value.Nullable!.Value);
        value2.NullableNull.Should().BeNull();
        value2.Array!.Length.Should().Be(value.Array.Length);
        value2.List!.Count.Should().Be(value.List.Count);
        for (int i = 0; i < value.Array.Length; i++)
        {
            ShouldBeSame(value2.Array[i], value.Array[i]);
            ShouldBeSame(value2.List[i], value.List[i]);
        }

        // array is (int length, UnityDateTimeOffset[])
        var arrayBin = MemoryPackSerializer.Serialize(value.Array);
        arrayBin.Length.Should().Be(4 + value.Array.Length * 16);
        arrayBin.AsSpan(4).ToArray().Should().Equal(value.Array.SelectMany(x => ExpectedBinary(x)).ToArray());
    }

    [Fact]
    public void MalformedData()
    {
        // offset is out of range(900 minutes)
        {
            var bin = new byte[16];
            BinaryPrimitives.WriteInt64LittleEndian(bin, new DateTime(2026, 11, 30).Ticks);
            BinaryPrimitives.WriteInt16LittleEndian(bin.AsSpan(8), 900);
            var value = MemoryPackSerializer.Deserialize<UnityDateTimeOffset>(bin);
            Assert.Throws<ArgumentOutOfRangeException>(() => value.ToDateTimeOffset());
        }
        // ticks is out of range
        foreach (var ticks in new[] { -1L, long.MinValue, long.MaxValue, DateTime.MaxValue.Ticks + 1 })
        {
            var bin = new byte[16];
            BinaryPrimitives.WriteInt64LittleEndian(bin, ticks);
            var value = MemoryPackSerializer.Deserialize<UnityDateTimeOffset>(bin);
            Assert.Throws<ArgumentOutOfRangeException>(() => value.ToDateTimeOffset());
        }
    }
}

[MemoryPackable]
public partial class UnityDateTimeOffsetHolder
{
    public UnityDateTimeOffset Value { get; set; }
}

[MemoryPackable]
public partial class Net10DateTimeOffsetHolder
{
    public Net10DateTimeOffset Value { get; set; }
}

[MemoryPackable]
public partial class UnityDateTimeOffsetData
{
    public UnityDateTimeOffset Value { get; set; }
    public UnityDateTimeOffset? Nullable { get; set; }
    public UnityDateTimeOffset? NullableNull { get; set; }
    public UnityDateTimeOffset[]? Array { get; set; }
    public List<UnityDateTimeOffset>? List { get; set; }
}
