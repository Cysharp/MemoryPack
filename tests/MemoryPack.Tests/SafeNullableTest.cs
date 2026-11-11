using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MemoryPack.Tests;

public partial class SafeNullableTest
{
    public enum ByteEnum : byte { A, B, C = 255 }
    public enum IntEnum { A, B, C = int.MinValue }
    public enum LongEnum : long { A, B, C = long.MaxValue }

    // primitives and enum are compatible with Nullable<T>
    [Fact]
    public void CompatibleWithNullable()
    {
        Compatible<bool>(true, false);
        Compatible<byte>(0, 1, 255);
        Compatible<sbyte>(0, 1, -1, sbyte.MinValue);
        Compatible<short>(0, 1, -1, short.MinValue, short.MaxValue);
        Compatible<ushort>(0, 1, ushort.MaxValue);
        Compatible<int>(0, 1, -1, int.MinValue, int.MaxValue);
        Compatible<uint>(0, 1, uint.MaxValue);
        Compatible<long>(0, 1, -1, long.MinValue, long.MaxValue);
        Compatible<ulong>(0, 1, ulong.MaxValue);
        Compatible<float>(0, 1.5f, -1.5f, float.NaN, float.MaxValue);
        Compatible<double>(0, 1.5, -1.5, double.NaN, double.MaxValue);
        Compatible<char>('\0', 'a', char.MaxValue);
        Compatible<ByteEnum>(ByteEnum.A, ByteEnum.B, ByteEnum.C);
        Compatible<IntEnum>(IntEnum.A, IntEnum.B, IntEnum.C);
        Compatible<LongEnum>(LongEnum.A, LongEnum.B, LongEnum.C);
        Compatible<DateTime>(default, new DateTime(2026, 11, 30, 16, 48, 36, DateTimeKind.Utc), DateTime.MaxValue);
        Compatible<TimeSpan>(default, TimeSpan.FromMinutes(-300), TimeSpan.MaxValue);
    }

    static void Compatible<T>(params T[] values)
        where T : unmanaged
    {
        var size = Unsafe.SizeOf<T>();

        // same size as Nullable<T>, and always twice of T(no padding).
        Unsafe.SizeOf<SafeNullable<T>>().Should().Be(Unsafe.SizeOf<T?>(), typeof(T).Name);
        Unsafe.SizeOf<SafeNullable<T>>().Should().Be(size * 2, typeof(T).Name);

        foreach (var value in values)
        {
            // (hasValue = (1, 0...), value)
            var expected = new byte[size * 2];
            expected[0] = 1;
            MemoryMarshal.Write(expected.AsSpan(size), ref Unsafe.AsRef(in value));

            // serialized multiple times, always same.
            for (int i = 0; i < 10; i++)
            {
                MemoryPackSerializer.Serialize(new SafeNullable<T>(value)).Should().Equal(expected);
                MemoryPackSerializer.Serialize<SafeNullable<T>>(value).Should().Equal(expected);
            }

            // SafeNullable<T> -> Nullable<T>
            var nullable = MemoryPackSerializer.Deserialize<T?>(expected);
            nullable.HasValue.Should().BeTrue();
            AsBytes(nullable.GetValueOrDefault()).Should().Equal(AsBytes(value));

            // Nullable<T> -> SafeNullable<T>
            var safe = MemoryPackSerializer.Deserialize<SafeNullable<T>>(MemoryPackSerializer.Serialize<T?>(value));
            safe.HasValue.Should().BeTrue();
            AsBytes(safe.Value).Should().Equal(AsBytes(value));

            // Nullable<T> that padding is not initialized -> SafeNullable<T>
            var garbage = (byte[])expected.Clone();
            for (int i = 1; i < size; i++)
            {
                garbage[i] = (byte)(200 + i);
            }
            safe = MemoryPackSerializer.Deserialize<SafeNullable<T>>(garbage);
            safe.HasValue.Should().BeTrue();
            AsBytes(safe.Value).Should().Equal(AsBytes(value));
        }

        // null is all zero
        {
            var expected = new byte[size * 2];
            for (int i = 0; i < 10; i++)
            {
                MemoryPackSerializer.Serialize(new SafeNullable<T>(null)).Should().Equal(expected);
                MemoryPackSerializer.Serialize(default(SafeNullable<T>)).Should().Equal(expected);
                MemoryPackSerializer.Serialize<SafeNullable<T>>(null).Should().Equal(expected);
            }

            MemoryPackSerializer.Deserialize<T?>(expected).HasValue.Should().BeFalse();
            MemoryPackSerializer.Deserialize<SafeNullable<T>>(MemoryPackSerializer.Serialize<T?>(null)).HasValue.Should().BeFalse();

            // Nullable<T>(null) that padding is not initialized
            var garbage = (byte[])expected.Clone();
            for (int i = 1; i < size; i++)
            {
                garbage[i] = (byte)(200 + i);
            }
            MemoryPackSerializer.Deserialize<SafeNullable<T>>(garbage).HasValue.Should().BeFalse();
        }
    }

    static byte[] AsBytes<T>(T value)
        where T : unmanaged
    {
        return MemoryMarshal.AsBytes(MemoryMarshal.CreateReadOnlySpan(ref value, 1)).ToArray();
    }

    // not compatible with Nullable<T> but can roundtrip itself.
    [Fact]
    public void NotCompatibleTypes()
    {
        Unsafe.SizeOf<SafeNullable<Guid>>().Should().Be(32);
        Unsafe.SizeOf<SafeNullable<Guid>>().Should().NotBe(Unsafe.SizeOf<Guid?>());

        var guid = Guid.NewGuid();
        var bin = MemoryPackSerializer.Serialize<SafeNullable<Guid>>(guid);
        bin.Length.Should().Be(32);
        bin.AsSpan(0, 16).ToArray().Should().Equal(new byte[] { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 });
        MemoryPackSerializer.Deserialize<SafeNullable<Guid>>(bin).Value.Should().Be(guid);

        MemoryPackSerializer.Deserialize<SafeNullable<decimal>>(MemoryPackSerializer.Serialize<SafeNullable<decimal>>(123.456m)).Value.Should().Be(123.456m);
        MemoryPackSerializer.Deserialize<SafeNullable<decimal>>(MemoryPackSerializer.Serialize<SafeNullable<decimal>>(null)).HasValue.Should().BeFalse();
    }

    [Fact]
    public void Api()
    {
        SafeNullable<int> hasValue = 10;
        SafeNullable<int> isNull = null;
        SafeNullable<int> isDefault = default;
        int? nullable = hasValue;
        int? nullableNull = isNull;

        hasValue.HasValue.Should().BeTrue();
        hasValue.Value.Should().Be(10);
        hasValue.GetValueOrDefault().Should().Be(10);
        hasValue.GetValueOrDefault(99).Should().Be(10);
        hasValue.ToNullable().Should().Be(10);
        hasValue.ToString().Should().Be("10");
        nullable.Should().Be(10);
        ((int)hasValue).Should().Be(10);
        (hasValue == 10).Should().BeTrue();
        (hasValue == null).Should().BeFalse();

        foreach (var item in new[] { isNull, isDefault, new SafeNullable<int>(null) })
        {
            item.HasValue.Should().BeFalse();
            Assert.Throws<InvalidOperationException>(() => item.Value);
            item.GetValueOrDefault().Should().Be(0);
            item.GetValueOrDefault(99).Should().Be(99);
            item.ToNullable().Should().BeNull();
            item.ToString().Should().Be("");
            (item == null).Should().BeTrue();
        }
        nullableNull.Should().BeNull();

        // equality
        hasValue.Equals(new SafeNullable<int>(10)).Should().BeTrue();
        hasValue.Equals(new SafeNullable<int>(11)).Should().BeFalse();
        hasValue.Equals(isNull).Should().BeFalse();
        isNull.Equals(hasValue).Should().BeFalse();
        isNull.Equals(isDefault).Should().BeTrue();
        hasValue.Equals((object)10).Should().BeTrue();
        hasValue.Equals((object)new SafeNullable<int>(10)).Should().BeTrue();
        isNull.Equals((object?)null).Should().BeTrue();
        hasValue.GetHashCode().Should().Be(10.GetHashCode());
        isNull.GetHashCode().Should().Be(0);

        // deserialized from the binary of Nullable<T> that has garbage padding is equal to the fresh value.
        var fromGarbage = MemoryPackSerializer.Deserialize<SafeNullable<int>>(new byte[] { 1, 201, 202, 203, 10, 0, 0, 0 });
        fromGarbage.Equals(hasValue).Should().BeTrue();
        fromGarbage.GetHashCode().Should().Be(hasValue.GetHashCode());
    }

    [Fact]
    public void Members()
    {
        var value = new SafeNullableData
        {
            Int = 10,
            IntNull = null,
            Long = long.MinValue,
            Bool = true,
            Enum = IntEnum.C,
            Double = 1.5,
            Array = new SafeNullable<int>[] { 1, null, 3 },
            List = new List<SafeNullable<long>> { null, 2, 3 },
            Text = "foo",
        };

        var bin = MemoryPackSerializer.Serialize(value);
        var value2 = MemoryPackSerializer.Deserialize<SafeNullableData>(bin)!;

        value2.Int.Should().Be(value.Int);
        value2.IntNull.HasValue.Should().BeFalse();
        value2.Long.Should().Be(value.Long);
        value2.Bool.Should().Be(value.Bool);
        value2.Enum.Should().Be(value.Enum);
        value2.Double.Should().Be(value.Double);
        value2.Array.Should().Equal(value.Array);
        value2.List.Should().Equal(value.List);
        value2.Text.Should().Be("foo");

        // same binary as the object that uses Nullable<T>(when padding is zero), and can read each other.
        var nullableValue = MemoryPackSerializer.Deserialize<NullableData>(bin)!;
        nullableValue.Int.Should().Be(10);
        nullableValue.IntNull.Should().BeNull();
        nullableValue.Long.Should().Be(long.MinValue);
        nullableValue.Bool.Should().Be(true);
        nullableValue.Enum.Should().Be(IntEnum.C);
        nullableValue.Double.Should().Be(1.5);
        nullableValue.Array.Should().Equal(1, null, 3);
        nullableValue.List.Should().Equal(null, 2, 3);
        nullableValue.Text.Should().Be("foo");

        var value3 = MemoryPackSerializer.Deserialize<SafeNullableData>(MemoryPackSerializer.Serialize(nullableValue))!;
        value3.Int.Should().Be(value.Int);
        value3.IntNull.HasValue.Should().BeFalse();
        value3.Long.Should().Be(value.Long);
        value3.Bool.Should().Be(value.Bool);
        value3.Enum.Should().Be(value.Enum);
        value3.Double.Should().Be(value.Double);
        value3.Array.Should().Equal(value.Array);
        value3.List.Should().Equal(value.List);
        value3.Text.Should().Be("foo");
    }

    [Fact]
    public void BinaryIsDeterministic()
    {
        // https://github.com/Cysharp/MemoryPack/issues/381
        // https://github.com/Cysharp/MemoryPack/issues/475
        byte[]? first = null;
        for (int i = 0; i < 100; i++)
        {
            PoisonStack((byte)i);
            var bin = Serialize(i);
            if (first == null)
            {
                first = bin;
            }
            else
            {
                bin.Should().Equal(first);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        static byte[] Serialize(int i)
        {
            var value = new SafeNullableData
            {
                Int = 10,
                IntNull = (i < 0) ? 1 : null,
                Long = 20,
                Bool = true,
                Enum = IntEnum.B,
                Double = 1.5,
                Array = new SafeNullable<int>[] { 1, null, 3 },
                List = new List<SafeNullable<long>> { null, 2, 3 },
                Text = "foo",
            };
            return MemoryPackSerializer.Serialize(value);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        static void PoisonStack(byte value)
        {
            Span<byte> s = stackalloc byte[2048];
            s.Fill(value);
            Consume(s);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        static void Consume(Span<byte> _)
        {
        }
    }
}

[MemoryPackable]
public partial class SafeNullableData
{
    public SafeNullable<int> Int { get; set; }
    public SafeNullable<int> IntNull { get; set; }
    public SafeNullable<long> Long { get; set; }
    public SafeNullable<bool> Bool { get; set; }
    public SafeNullable<SafeNullableTest.IntEnum> Enum { get; set; }
    public SafeNullable<double> Double { get; set; }
    public SafeNullable<int>[]? Array { get; set; }
    public List<SafeNullable<long>>? List { get; set; }
    public string? Text { get; set; }
}

[MemoryPackable]
public partial class NullableData
{
    public int? Int { get; set; }
    public int? IntNull { get; set; }
    public long? Long { get; set; }
    public bool? Bool { get; set; }
    public SafeNullableTest.IntEnum? Enum { get; set; }
    public double? Double { get; set; }
    public int?[]? Array { get; set; }
    public List<long?>? List { get; set; }
    public string? Text { get; set; }
}
