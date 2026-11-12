using MemoryPack;
using MemoryPack.Tests.Models;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Net10DateTimeOffset, UnityDateTimeOffset, SafeNullable, BitArray and malformed data checks in Unity(Mono, IL2CPP).
public class CompatTypesTest
{
    static readonly DateTimeOffset[] Values = new[]
    {
        new DateTimeOffset(2026, 11, 30, 16, 48, 36, TimeSpan.FromMinutes(-300)),
        new DateTimeOffset(2026, 11, 30, 16, 48, 36, TimeSpan.FromMinutes(540)),
        new DateTimeOffset(2023, 10, 25, 17, 45, 0, TimeSpan.FromHours(8)),
        new DateTimeOffset(2026, 11, 30, 16, 48, 36, TimeSpan.Zero),
        DateTimeOffset.MinValue,
        DateTimeOffset.MaxValue,
    };

    static string ToHex(byte[] bytes)
    {
        return BitConverter.ToString(bytes);
    }

    static void AreSame(DateTimeOffset expected, DateTimeOffset actual)
    {
        Assert.AreEqual(expected.UtcTicks, actual.UtcTicks);
        Assert.AreEqual(expected.Offset, actual.Offset);
    }

    static byte[] UnityLayout(DateTimeOffset value)
    {
        // (long utcTicks, short offsetMinutes, 6 bytes zero)
        var bin = new byte[16];
        BitConverter.GetBytes(value.UtcTicks).CopyTo(bin, 0);
        BitConverter.GetBytes((short)value.Offset.TotalMinutes).CopyTo(bin, 8);
        return bin;
    }

    static byte[] Net10Layout(DateTimeOffset value)
    {
        // (int offsetMinutes, 4 bytes zero, long utcTicks)
        var bin = new byte[16];
        BitConverter.GetBytes((int)value.Offset.TotalMinutes).CopyTo(bin, 0);
        BitConverter.GetBytes(value.UtcTicks).CopyTo(bin, 8);
        return bin;
    }

    // IMPORTANT: this is the assumption of UnityDateTimeOffset, layout of DateTimeOffset in this runtime is (long utcTicks, short offsetMinutes, padding).
    [Test]
    public void NativeDateTimeOffsetLayoutIsUnityLayout()
    {
        foreach (var value in Values)
        {
            var native = MemoryPackSerializer.Serialize(value);
            Debug.Log("DateTimeOffset " + value.ToString("o") + " native: " + ToHex(native) + " UnityDateTimeOffset: " + ToHex(MemoryPackSerializer.Serialize<UnityDateTimeOffset>(value)));

            Assert.AreEqual(16, native.Length);

            // padding is not initialized, clear before compare.
            Array.Clear(native, 10, 6);
            CollectionAssert.AreEqual(UnityLayout(value), native);
        }
    }

    [Test]
    public void UnityDateTimeOffsetIsCompatibleWithNativeDateTimeOffset()
    {
        foreach (var value in Values)
        {
            CollectionAssert.AreEqual(UnityLayout(value), MemoryPackSerializer.Serialize<UnityDateTimeOffset>(value));

            // DateTimeOffset -> UnityDateTimeOffset
            AreSame(value, MemoryPackSerializer.Deserialize<UnityDateTimeOffset>(MemoryPackSerializer.Serialize(value)));
            // UnityDateTimeOffset -> DateTimeOffset
            AreSame(value, MemoryPackSerializer.Deserialize<DateTimeOffset>(MemoryPackSerializer.Serialize<UnityDateTimeOffset>(value)));
        }
    }

    [Test]
    public void UnityDateTimeOffsetKnownBinary()
    {
        // https://github.com/Cysharp/MemoryPack/issues/192 serialized in Unity 2022.3.7f1
        var value = new DateTimeOffset(2023, 10, 25, 17, 45, 0, TimeSpan.FromHours(8));
        var bin = new byte[] { 0, 182, 36, 14, 63, 213, 219, 8, 224, 1, 0, 0, 0, 0, 0, 0 };

        CollectionAssert.AreEqual(bin, MemoryPackSerializer.Serialize<UnityDateTimeOffset>(value));
        AreSame(value, MemoryPackSerializer.Deserialize<UnityDateTimeOffset>(bin));
    }

    [Test]
    public void Net10DateTimeOffsetBinary()
    {
        foreach (var value in Values)
        {
            var bin = MemoryPackSerializer.Serialize<Net10DateTimeOffset>(value);
            CollectionAssert.AreEqual(Net10Layout(value), bin);
            AreSame(value, MemoryPackSerializer.Deserialize<Net10DateTimeOffset>(bin));
        }

        // memory dump of DateTimeOffset(2026-11-30T16:48:36-05:00) in .NET 10 and .NET 9
        var expected = new DateTimeOffset(2026, 11, 30, 16, 48, 36, TimeSpan.FromMinutes(-300));
        var net10 = new byte[] { 212, 254, 255, 255, 0, 0, 0, 0, 0, 146, 181, 175, 43, 79, 223, 8 };
        var net9 = new byte[] { 212, 254, 0, 0, 0, 0, 0, 0, 0, 146, 181, 175, 43, 79, 223, 8 };
        CollectionAssert.AreEqual(net10, MemoryPackSerializer.Serialize<Net10DateTimeOffset>(expected));
        AreSame(expected, MemoryPackSerializer.Deserialize<Net10DateTimeOffset>(net10));
        AreSame(expected, MemoryPackSerializer.Deserialize<Net10DateTimeOffset>(net9));
    }

    [Test]
    public void SafeNullableIsCompatibleWithNullable()
    {
        // (hasValue = (1, 0, 0, 0), value)
        CollectionAssert.AreEqual(new byte[] { 1, 0, 0, 0, 10, 0, 0, 0 }, MemoryPackSerializer.Serialize<SafeNullable<int>>(10));
        CollectionAssert.AreEqual(new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 }, MemoryPackSerializer.Serialize<SafeNullable<int>>(null));
        CollectionAssert.AreEqual(new byte[] { 1, 0, 0, 0, 0, 0, 0, 0, 10, 0, 0, 0, 0, 0, 0, 0 }, MemoryPackSerializer.Serialize<SafeNullable<long>>(10));
        CollectionAssert.AreEqual(new byte[] { 1, 1 }, MemoryPackSerializer.Serialize<SafeNullable<bool>>(true));

        // Nullable<T> <-> SafeNullable<T>
        Assert.AreEqual(8, MemoryPackSerializer.Serialize<int?>(10).Length);
        Assert.AreEqual(16, MemoryPackSerializer.Serialize<long?>(10).Length);
        Assert.AreEqual(10, MemoryPackSerializer.Deserialize<int?>(MemoryPackSerializer.Serialize<SafeNullable<int>>(10)));
        Assert.AreEqual(null, MemoryPackSerializer.Deserialize<int?>(MemoryPackSerializer.Serialize<SafeNullable<int>>(null)));
        Assert.AreEqual(10, MemoryPackSerializer.Deserialize<SafeNullable<int>>(MemoryPackSerializer.Serialize<int?>(10)).Value);
        Assert.IsFalse(MemoryPackSerializer.Deserialize<SafeNullable<int>>(MemoryPackSerializer.Serialize<int?>(null)).HasValue);
        Assert.AreEqual(long.MinValue, MemoryPackSerializer.Deserialize<SafeNullable<long>>(MemoryPackSerializer.Serialize<long?>(long.MinValue)).Value);

        // garbage padding of Nullable<T>
        Assert.AreEqual(10, MemoryPackSerializer.Deserialize<SafeNullable<int>>(new byte[] { 1, 201, 202, 203, 10, 0, 0, 0 }).Value);
        Assert.IsFalse(MemoryPackSerializer.Deserialize<SafeNullable<int>>(new byte[] { 0, 201, 202, 203, 0, 0, 0, 0 }).HasValue);

        // members
        var nullable = new CompatNullableData { SafeInt = 10, SafeIntNull = null, SafeLong = long.MinValue, SafeBool = true, SafeEnum = CompatIntEnum.C };
        var safe = MemoryPackSerializer.Deserialize<CompatSafeNullableData>(MemoryPackSerializer.Serialize(nullable));
        Assert.AreEqual(10, safe.SafeInt.Value);
        Assert.IsFalse(safe.SafeIntNull.HasValue);
        Assert.AreEqual(long.MinValue, safe.SafeLong.Value);
        Assert.AreEqual(true, safe.SafeBool.Value);
        Assert.AreEqual(CompatIntEnum.C, safe.SafeEnum.Value);

        var nullable2 = MemoryPackSerializer.Deserialize<CompatNullableData>(MemoryPackSerializer.Serialize(safe));
        Assert.AreEqual(10, nullable2.SafeInt);
        Assert.AreEqual(null, nullable2.SafeIntNull);
        Assert.AreEqual(long.MinValue, nullable2.SafeLong);
        Assert.AreEqual(true, nullable2.SafeBool);
        Assert.AreEqual(CompatIntEnum.C, nullable2.SafeEnum);
    }

    [Test]
    public void Members()
    {
        var bits = new BitArray(100);
        for (int i = 0; i < 100; i += 3)
        {
            bits[i] = true;
        }

        var value = new CompatTypesData
        {
            Net10 = Values[0],
            Unity = Values[1],
            Net10Nullable = Values[2],
            UnityArray = new UnityDateTimeOffset[] { Values[0], Values[1], Values[2] },
            Net10List = new List<Net10DateTimeOffset> { Values[0], Values[1], Values[2] },
            SafeInt = 10,
            SafeIntNull = null,
            SafeLong = long.MinValue,
            SafeBool = true,
            SafeEnum = CompatIntEnum.C,
            SafeArray = new SafeNullable<int>[] { 1, null, 3 },
            SafeList = new List<SafeNullable<long>> { null, 2, 3 },
            Bits = bits,
            Text = "foo",
        };

        var bin = MemoryPackSerializer.Serialize(value);
        Debug.Log("CompatTypesData: " + ToHex(bin));
        var value2 = MemoryPackSerializer.Deserialize<CompatTypesData>(bin);

        AreSame(Values[0], value2.Net10);
        AreSame(Values[1], value2.Unity);
        AreSame(Values[2], value2.Net10Nullable.Value);
        Assert.AreEqual(3, value2.UnityArray.Length);
        Assert.AreEqual(3, value2.Net10List.Count);
        for (int i = 0; i < 3; i++)
        {
            AreSame(Values[i], value2.UnityArray[i]);
            AreSame(Values[i], value2.Net10List[i]);
        }
        Assert.AreEqual(10, value2.SafeInt.Value);
        Assert.IsFalse(value2.SafeIntNull.HasValue);
        Assert.AreEqual(long.MinValue, value2.SafeLong.Value);
        Assert.AreEqual(true, value2.SafeBool.Value);
        Assert.AreEqual(CompatIntEnum.C, value2.SafeEnum.Value);
        Assert.AreEqual(1, value2.SafeArray[0].Value);
        Assert.IsFalse(value2.SafeArray[1].HasValue);
        Assert.AreEqual(3, value2.SafeArray[2].Value);
        Assert.IsFalse(value2.SafeList[0].HasValue);
        Assert.AreEqual(2, value2.SafeList[1].Value);
        Assert.AreEqual(3, value2.SafeList[2].Value);
        Assert.AreEqual(100, value2.Bits.Length);
        for (int i = 0; i < 100; i++)
        {
            Assert.AreEqual(i % 3 == 0, value2.Bits[i]);
        }
        Assert.AreEqual("foo", value2.Text);

        // same binary in every serialize
        for (int i = 0; i < 10; i++)
        {
            CollectionAssert.AreEqual(bin, MemoryPackSerializer.Serialize(value));
        }
    }

    // https://github.com/Cysharp/MemoryPack/issues/399
    [Test]
    public void BitArrayBinaryIsSameAsDotNet()
    {
        // BitArray(100), every third bit is true, serialized by .NET 8.
        var expected = new byte[] { 2, 100, 0, 0, 0, 4, 0, 0, 0, 73, 146, 36, 73, 146, 36, 73, 146, 36, 73, 146, 36, 9, 0, 0, 0 };

        var value = new BitArray(100);
        for (int i = 0; i < 100; i += 3)
        {
            value[i] = true;
        }

        var bin = MemoryPackSerializer.Serialize(value);
        Debug.Log("BitArray: " + ToHex(bin));
        CollectionAssert.AreEqual(expected, bin);

        var value2 = MemoryPackSerializer.Deserialize<BitArray>(expected);
        Assert.AreEqual(100, value2.Length);
        for (int i = 0; i < 100; i++)
        {
            Assert.AreEqual(i % 3 == 0, value2[i]);
        }

        foreach (var length in new[] { 0, 1, 31, 32, 33, 64, 1000 })
        {
            var bitArray = new BitArray(length);
            for (int i = 0; i < length; i += 3)
            {
                bitArray[i] = true;
            }
            var bitArray2 = MemoryPackSerializer.Deserialize<BitArray>(MemoryPackSerializer.Serialize(bitArray));
            Assert.AreEqual(length, bitArray2.Length);
            for (int i = 0; i < length; i++)
            {
                Assert.AreEqual(i % 3 == 0, bitArray2[i]);
            }
        }
    }

    [Test]
    public void MalformedDataIsRejected()
    {
        // utf8 string header overflow, https://github.com/Cysharp/MemoryPack/issues/306
        Assert.Throws<MemoryPackSerializationException>(() => MemoryPackSerializer.Deserialize<CompatStringData>(new byte[] { 1, 0, 0, 0, 128 }));
        Assert.Throws<MemoryPackSerializationException>(() => MemoryPackSerializer.Deserialize<CompatStringData>(new byte[] { 1, 0, 0, 0, 128, 0, 0, 0, 0, 97, 98, 99, 100 }));
        Assert.Throws<MemoryPackSerializationException>(() => MemoryPackSerializer.Deserialize<CompatStringData>(new byte[] { 1, 3, 0, 0, 128, 0, 0, 0, 0, 97, 98, 99, 100 }));

        // negative skip of version tolerant, list of 2 elements, 1st element has unknown member that length is -100.
        var payload = new byte[] { 2, 0, 0, 0, 2, 4, 156, 1, 0, 0, 0, 1, 240, 255, 255, 127, 0, 0, 0, 0 };
        Assert.Throws<MemoryPackSerializationException>(() => MemoryPackSerializer.Deserialize<List<CompatVersionTolerantData>>(payload));

        // version tolerant skip is still working
        var tolerant = MemoryPackSerializer.Deserialize<CompatVersionTolerantData>(new byte[] { 2, 4, 4, 10, 0, 0, 0, 99, 99, 99, 99 });
        Assert.AreEqual(10, tolerant.A);

        // negative length of unmanaged array
        Assert.Throws<MemoryPackSerializationException>(() => MemoryPackSerializer.Deserialize<int[]>(new byte[] { 254, 255, 255, 255, 1, 2, 3, 4, 5, 6, 7, 8 }));

        // multi dimensional array, dimensions [65536, 32768] but collection length is 8
        var multi = new byte[] { 3, 0, 0, 1, 0, 0, 128, 0, 0, 8, 0, 0, 0, 1, 2, 3, 4, 5, 6, 7, 8 };
        Assert.Throws<MemoryPackSerializationException>(() => MemoryPackSerializer.Deserialize<byte[,]>(multi));

        // reader rejects negative size
        Assert.IsTrue(ReaderRejectsNegative(-1));
        Assert.IsTrue(ReaderRejectsNegative(-4));
        Assert.IsTrue(ReaderRejectsNegative(int.MinValue));
    }

    static bool ReaderRejectsNegative(int size)
    {
        var state = MemoryPackReaderOptionalStatePool.Rent(null);
        var reader = new MemoryPackReader(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, state);
        try
        {
            reader.Advance(2);

            var getSpanReferenceFailed = false;
            try
            {
                reader.GetSpanReference(size);
            }
            catch (MemoryPackSerializationException)
            {
                getSpanReferenceFailed = true;
            }

            var advanceFailed = false;
            try
            {
                reader.Advance(size);
            }
            catch (MemoryPackSerializationException)
            {
                advanceFailed = true;
            }

            return getSpanReferenceFailed && advanceFailed && reader.Consumed == 2;
        }
        finally
        {
            reader.Dispose();
            ((IDisposable)state).Dispose();
        }
    }
}
