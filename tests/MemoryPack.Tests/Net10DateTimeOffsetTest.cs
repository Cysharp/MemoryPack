using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace MemoryPack.Tests;

public partial class Net10DateTimeOffsetTest
{
    public static IEnumerable<object[]> Values()
    {
        yield return new object[] { new DateTimeOffset(2026, 11, 30, 16, 48, 36, TimeSpan.FromMinutes(-300)) };
        yield return new object[] { new DateTimeOffset(2026, 11, 30, 16, 48, 36, TimeSpan.FromMinutes(540)) };
        yield return new object[] { new DateTimeOffset(2026, 11, 30, 16, 48, 36, 123, TimeSpan.FromMinutes(345)) };
        yield return new object[] { new DateTimeOffset(2026, 11, 30, 16, 48, 36, TimeSpan.Zero) };
        yield return new object[] { new DateTimeOffset(2000, 1, 1, 0, 0, 0, TimeSpan.FromHours(14)) };
        yield return new object[] { new DateTimeOffset(2000, 1, 1, 0, 0, 0, TimeSpan.FromHours(-14)) };
        yield return new object[] { DateTimeOffset.MinValue };
        yield return new object[] { DateTimeOffset.MaxValue };
        yield return new object[] { new DateTimeOffset(DateTime.MinValue.Ticks + TimeSpan.FromHours(14).Ticks, TimeSpan.FromHours(14)) };
        yield return new object[] { new DateTimeOffset(DateTime.MaxValue.Ticks - TimeSpan.FromHours(14).Ticks, TimeSpan.FromHours(-14)) };
    }

    static void ShouldBeSame(DateTimeOffset actual, DateTimeOffset expected)
    {
        // DateTimeOffset.Equals only compares UtcDateTime
        actual.UtcTicks.Should().Be(expected.UtcTicks);
        actual.Offset.Should().Be(expected.Offset);
        actual.Ticks.Should().Be(expected.Ticks);
    }

    // (int offsetMinutes, int zero, long utcTicks)
    static byte[] ExpectedBinary(DateTimeOffset value)
    {
        var bin = new byte[16];
        BinaryPrimitives.WriteInt32LittleEndian(bin, (int)value.Offset.TotalMinutes);
        BinaryPrimitives.WriteInt64LittleEndian(bin.AsSpan(8), value.UtcTicks);
        return bin;
    }

    [Fact]
    public void Size()
    {
        Unsafe.SizeOf<Net10DateTimeOffset>().Should().Be(16);
    }

    [Theory]
    [MemberData(nameof(Values))]
    public void Convert(DateTimeOffset value)
    {
        Net10DateTimeOffset net10 = value;
        DateTimeOffset value2 = net10;
        ShouldBeSame(value2, value);
        ShouldBeSame(new Net10DateTimeOffset(value).ToDateTimeOffset(), value);
        net10.ToString().Should().Be(value.ToString());
    }

    [Theory]
    [MemberData(nameof(Values))]
    public void BinaryIsFixed(DateTimeOffset value)
    {
        var expected = ExpectedBinary(value);

        // serialized multiple times to check there is no uninitialized padding.
        for (int i = 0; i < 10; i++)
        {
            MemoryPackSerializer.Serialize<Net10DateTimeOffset>(value).Should().Equal(expected);
        }

        ShouldBeSame(MemoryPackSerializer.Deserialize<Net10DateTimeOffset>(expected), value);
    }

    [Fact]
    public void KnownBinary()
    {
        // 2026-11-30T16:48:36-05:00
        var value = new DateTimeOffset(2026, 11, 30, 16, 48, 36, TimeSpan.FromMinutes(-300));

        // memory dump of DateTimeOffset in .NET 10, (int offsetMinutes, 4 bytes padding, long utcTicks)
        var net10 = System.Convert.FromHexString("D4FEFFFF000000000092B5AF2B4FDF08");
        // memory dump of DateTimeOffset in .NET 9, (short offsetMinutes, 6 bytes padding, long utcTicks)
        var net9 = System.Convert.FromHexString("D4FE0000000000000092B5AF2B4FDF08");

        MemoryPackSerializer.Serialize<Net10DateTimeOffset>(value).Should().Equal(net10);

        ShouldBeSame(MemoryPackSerializer.Deserialize<Net10DateTimeOffset>(net10), value);
        ShouldBeSame(MemoryPackSerializer.Deserialize<Net10DateTimeOffset>(net9), value);
    }

    [Theory]
    [MemberData(nameof(Values))]
    public void ReadDateTimeOffsetBinaryOfNet9OrLower(DateTimeOffset value)
    {
        // (short offsetMinutes, 6 bytes padding(not initialized), long utcTicks)
        var bin = new byte[16];
        var random = new Random(value.GetHashCode());
        for (int i = 0; i < 20; i++)
        {
            random.NextBytes(bin.AsSpan(2, 6)); // garbage padding
            BinaryPrimitives.WriteInt16LittleEndian(bin, (short)value.Offset.TotalMinutes);
            BinaryPrimitives.WriteInt64LittleEndian(bin.AsSpan(8), value.UtcTicks);

            ShouldBeSame(MemoryPackSerializer.Deserialize<Net10DateTimeOffset>(bin), value);
        }
    }

    [Theory]
    [MemberData(nameof(Values))]
    public void ReadDateTimeOffsetBinaryOfNet10OrLater(DateTimeOffset value)
    {
        // (int offsetMinutes, 4 bytes padding(not initialized), long utcTicks)
        var bin = new byte[16];
        var random = new Random(value.GetHashCode());
        for (int i = 0; i < 20; i++)
        {
            random.NextBytes(bin.AsSpan(4, 4)); // garbage padding
            BinaryPrimitives.WriteInt32LittleEndian(bin, (int)value.Offset.TotalMinutes);
            BinaryPrimitives.WriteInt64LittleEndian(bin.AsSpan(8), value.UtcTicks);

            ShouldBeSame(MemoryPackSerializer.Deserialize<Net10DateTimeOffset>(bin), value);
        }
    }

    [Theory]
    [MemberData(nameof(Values))]
    public void CompatibleWithDateTimeOffsetOfCurrentRuntime(DateTimeOffset value)
    {
        // Net10DateTimeOffset -> DateTimeOffset, works in all runtimes.
        ShouldBeSame(MemoryPackSerializer.Deserialize<DateTimeOffset>(MemoryPackSerializer.Serialize<Net10DateTimeOffset>(value)), value);

        // DateTimeOffset -> Net10DateTimeOffset, works in all runtimes.
        ShouldBeSame(MemoryPackSerializer.Deserialize<Net10DateTimeOffset>(MemoryPackSerializer.Serialize(value)), value);

        // in .NET 10 or later, binary is same(exclude padding).
        if (Environment.Version.Major >= 10)
        {
            var native = MemoryPackSerializer.Serialize(value);
            native.AsSpan(4, 4).Clear();
            native.Should().Equal(MemoryPackSerializer.Serialize<Net10DateTimeOffset>(value));
        }
    }

    [Fact]
    public void Members()
    {
        var value = new Net10DateTimeOffsetData
        {
            Value = new DateTimeOffset(2026, 11, 30, 16, 48, 36, TimeSpan.FromMinutes(-300)),
            Nullable = new DateTimeOffset(2026, 11, 30, 16, 48, 36, TimeSpan.FromMinutes(540)),
            NullableNull = null,
            Array = Values().Select(x => (Net10DateTimeOffset)(DateTimeOffset)x[0]).ToArray(),
            List = Values().Select(x => (Net10DateTimeOffset)(DateTimeOffset)x[0]).ToList(),
        };

        var bin = MemoryPackSerializer.Serialize(value);
        var value2 = MemoryPackSerializer.Deserialize<Net10DateTimeOffsetData>(bin)!;

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

        // array is (int length, Net10DateTimeOffset[])
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
            BinaryPrimitives.WriteInt32LittleEndian(bin, 900);
            BinaryPrimitives.WriteInt64LittleEndian(bin.AsSpan(8), new DateTime(2026, 11, 30).Ticks);
            var value = MemoryPackSerializer.Deserialize<Net10DateTimeOffset>(bin);
            Assert.Throws<ArgumentOutOfRangeException>(() => value.ToDateTimeOffset());
        }
        // ticks is out of range
        foreach (var ticks in new[] { -1L, long.MinValue, long.MaxValue, DateTime.MaxValue.Ticks + 1 })
        {
            var bin = new byte[16];
            BinaryPrimitives.WriteInt64LittleEndian(bin.AsSpan(8), ticks);
            var value = MemoryPackSerializer.Deserialize<Net10DateTimeOffset>(bin);
            Assert.Throws<ArgumentOutOfRangeException>(() => value.ToDateTimeOffset());
        }
    }
}

[MemoryPackable]
public partial class Net10DateTimeOffsetData
{
    public Net10DateTimeOffset Value { get; set; }
    public Net10DateTimeOffset? Nullable { get; set; }
    public Net10DateTimeOffset? NullableNull { get; set; }
    public Net10DateTimeOffset[]? Array { get; set; }
    public List<Net10DateTimeOffset>? List { get; set; }
}
