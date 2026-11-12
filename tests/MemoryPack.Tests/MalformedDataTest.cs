using MemoryPack.Compression;
using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace MemoryPack.Tests;

// Malformed(untrusted) binary must be rejected by exception, never read/write outside of the buffer.
public partial class MalformedDataTest
{
    [Theory]
    [InlineData(new byte[] { 1, 0, 0, 0, 128 })] // #306
    [InlineData(new byte[] { 1, 0, 0, 0, 128, 0, 0, 0, 0, 97, 98, 99, 100 })] // utf8 length = int.MaxValue
    [InlineData(new byte[] { 1, 1, 0, 0, 128, 0, 0, 0, 0, 97, 98, 99, 100 })] // utf8 length = int.MaxValue - 1
    [InlineData(new byte[] { 1, 2, 0, 0, 128, 0, 0, 0, 0, 97, 98, 99, 100 })] // utf8 length = int.MaxValue - 2
    [InlineData(new byte[] { 1, 3, 0, 0, 128, 0, 0, 0, 0, 97, 98, 99, 100 })] // utf8 length = int.MaxValue - 3
    [InlineData(new byte[] { 1, 4, 0, 0, 128, 0, 0, 0, 0, 97, 98, 99, 100 })] // utf8 length = int.MaxValue - 4
    public void Utf8StringHeaderOverflow(byte[] payload)
    {
        Assert.Throws<MemoryPackSerializationException>(() => MemoryPackSerializer.Deserialize<MalformedStringData>(payload));
        Assert.Throws<MemoryPackSerializationException>(() => MemoryPackSerializer.Deserialize<MalformedStringData>(new ReadOnlySequence<byte>(payload)));
        Assert.Throws<MemoryPackSerializationException>(() => MemoryPackSerializer.Deserialize<MalformedStringData>(ReadOnlySequenceBuilder.Create(payload.AsSpan(0, 3).ToArray(), payload.AsSpan(3).ToArray())));
    }

    [Fact]
    public void VersionTolerantNegativeSkip()
    {
        // list of 2 elements.
        // 1st: declares 2 members(type knows 1), member lengths are VarInt: member[0] = 4 bytes, member[1] = -100.
        // 2nd: 1 member.
        byte[] element = { 2, 4, 156, 1, 0, 0, 0 };
        var payload = new byte[4 + element.Length + 1 + 4 + 4];
        BinaryPrimitives.WriteInt32LittleEndian(payload, 2);
        element.CopyTo(payload, 4);
        payload[4 + element.Length] = 1;
        BinaryPrimitives.WriteInt32LittleEndian(payload.AsSpan(4 + element.Length + 1), 2147483632);

        Assert.Throws<MemoryPackSerializationException>(() => MemoryPackSerializer.Deserialize<List<MalformedVersionTolerantData>>(payload));
        Assert.Throws<MemoryPackSerializationException>(() => MemoryPackSerializer.Deserialize<List<MalformedVersionTolerantData>>(new ReadOnlySequence<byte>(payload)));
        Assert.Throws<MemoryPackSerializationException>(() => MemoryPackSerializer.Deserialize<List<MalformedVersionTolerantData>>(ReadOnlySequenceBuilder.Create(payload.AsSpan(0, 6).ToArray(), payload.AsSpan(6).ToArray())));
    }

    [Fact]
    public void VersionTolerantSkipIsStillWorking()
    {
        // unknown member(4 bytes) is skipped.
        byte[] payload = { 2, 4, 4, 10, 0, 0, 0, 99, 99, 99, 99 };
        var value = MemoryPackSerializer.Deserialize<MalformedVersionTolerantData>(payload);
        value!.A.Should().Be(10);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-4)]
    [InlineData(int.MinValue)]
    public void ReaderRejectsNegativeSize(int size)
    {
        var buffer = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
        var sequences = new[]
        {
            new ReadOnlySequence<byte>(buffer),
            ReadOnlySequenceBuilder.Create(new byte[] { 1, 2, 3 }, new byte[] { 4, 5, 6, 7, 8 }, new byte[] { 9, 10 })
        };

        foreach (var sequence in sequences)
        {
            foreach (var advanceBefore in new[] { 0, 2, 4 })
            {
                GetSpanReferenceIsFailed(sequence, advanceBefore, size).Should().BeTrue();
                AdvanceIsFailed(sequence, advanceBefore, size).Should().BeTrue();
            }
        }

        static bool GetSpanReferenceIsFailed(ReadOnlySequence<byte> sequence, int advanceBefore, int size)
        {
            using var state = MemoryPackReaderOptionalStatePool.Rent(null);
            var reader = new MemoryPackReader(sequence, state);
            try
            {
                reader.GetSpanReference(advanceBefore);
                reader.Advance(advanceBefore);
                try
                {
                    reader.GetSpanReference(size);
                }
                catch (MemoryPackSerializationException)
                {
                    return true;
                }
                return false;
            }
            finally
            {
                reader.Dispose();
            }
        }

        static bool AdvanceIsFailed(ReadOnlySequence<byte> sequence, int advanceBefore, int size)
        {
            using var state = MemoryPackReaderOptionalStatePool.Rent(null);
            var reader = new MemoryPackReader(sequence, state);
            try
            {
                reader.GetSpanReference(advanceBefore);
                reader.Advance(advanceBefore);
                try
                {
                    reader.Advance(size);
                }
                catch (MemoryPackSerializationException)
                {
                    reader.Consumed.Should().Be(advanceBefore);
                    return true;
                }
                return false;
            }
            finally
            {
                reader.Dispose();
            }
        }
    }

    [Fact]
    public void UnmanagedArrayNegativeLength()
    {
        // length = -2
        byte[] payload = { 254, 255, 255, 255, 1, 2, 3, 4, 5, 6, 7, 8 };
        Assert.ThrowsAny<Exception>(() => MemoryPackSerializer.Deserialize<int[]>(payload));
        Assert.ThrowsAny<Exception>(() => MemoryPackSerializer.Deserialize<List<int>>(payload));
        Assert.ThrowsAny<Exception>(() => MemoryPackSerializer.Deserialize<System.Numerics.BigInteger>(payload));
    }

    [Fact]
    public void MultiDimensionalArrayDimensionsMismatch()
    {
        // 2D: dimensions [65536, 32768] but only 8 bytes of data, must not allocate by dimensions.
        {
            var payload = new byte[1 + 4 + 4 + 4 + 8];
            payload[0] = 3;
            BinaryPrimitives.WriteInt32LittleEndian(payload.AsSpan(1), 65536);
            BinaryPrimitives.WriteInt32LittleEndian(payload.AsSpan(5), 32768);
            BinaryPrimitives.WriteInt32LittleEndian(payload.AsSpan(9), 8);
            Assert.Throws<MemoryPackSerializationException>(() => MemoryPackSerializer.Deserialize<byte[,]>(payload));
            Assert.Throws<MemoryPackSerializationException>(() => MemoryPackSerializer.Deserialize<string[,]>(payload));
        }
        // 3D
        {
            var payload = new byte[1 + 4 + 4 + 4 + 4 + 8];
            payload[0] = 4;
            BinaryPrimitives.WriteInt32LittleEndian(payload.AsSpan(1), 2048);
            BinaryPrimitives.WriteInt32LittleEndian(payload.AsSpan(5), 1024);
            BinaryPrimitives.WriteInt32LittleEndian(payload.AsSpan(9), 1024);
            BinaryPrimitives.WriteInt32LittleEndian(payload.AsSpan(13), 8);
            Assert.Throws<MemoryPackSerializationException>(() => MemoryPackSerializer.Deserialize<byte[,,]>(payload));
            Assert.Throws<MemoryPackSerializationException>(() => MemoryPackSerializer.Deserialize<string[,,]>(payload));
        }
        // 4D
        {
            var payload = new byte[1 + 4 + 4 + 4 + 4 + 4 + 8];
            payload[0] = 5;
            BinaryPrimitives.WriteInt32LittleEndian(payload.AsSpan(1), 256);
            BinaryPrimitives.WriteInt32LittleEndian(payload.AsSpan(5), 256);
            BinaryPrimitives.WriteInt32LittleEndian(payload.AsSpan(9), 256);
            BinaryPrimitives.WriteInt32LittleEndian(payload.AsSpan(13), 128);
            BinaryPrimitives.WriteInt32LittleEndian(payload.AsSpan(17), 8);
            Assert.Throws<MemoryPackSerializationException>(() => MemoryPackSerializer.Deserialize<byte[,,,]>(payload));
            Assert.Throws<MemoryPackSerializationException>(() => MemoryPackSerializer.Deserialize<string[,,,]>(payload));
        }
    }

    [Fact]
    public void MultiDimensionalArrayIsStillWorking()
    {
        {
            var value = new int[3, 4];
            var value2 = new string?[3, 4];
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    value[i, j] = i * 10 + j;
                    value2[i, j] = (i * 10 + j).ToString();
                }
            }
            var result = MemoryPackSerializer.Deserialize<int[,]>(MemoryPackSerializer.Serialize(value))!;
            var result2 = MemoryPackSerializer.Deserialize<string?[,]>(MemoryPackSerializer.Serialize(value2))!;
            (result.GetLength(0), result.GetLength(1)).Should().Be((3, 4));
            (result2.GetLength(0), result2.GetLength(1)).Should().Be((3, 4));
            result.Cast<int>().Should().Equal(value.Cast<int>());
            result2.Cast<string?>().Should().Equal(value2.Cast<string?>());
        }
        {
            var value = new int[2, 3, 4];
            var value2 = new int[2, 3, 4, 5];
            var n = 0;
            foreach (ref var item in System.Runtime.InteropServices.MemoryMarshal.CreateSpan(ref value[0, 0, 0], value.Length)) item = n++;
            foreach (ref var item in System.Runtime.InteropServices.MemoryMarshal.CreateSpan(ref value2[0, 0, 0, 0], value2.Length)) item = n++;
            var result = MemoryPackSerializer.Deserialize<int[,,]>(MemoryPackSerializer.Serialize(value))!;
            var result2 = MemoryPackSerializer.Deserialize<int[,,,]>(MemoryPackSerializer.Serialize(value2))!;
            (result.GetLength(0), result.GetLength(1), result.GetLength(2)).Should().Be((2, 3, 4));
            (result2.GetLength(0), result2.GetLength(1), result2.GetLength(2), result2.GetLength(3)).Should().Be((2, 3, 4, 5));
            result.Cast<int>().Should().Equal(value.Cast<int>());
            result2.Cast<int>().Should().Equal(value2.Cast<int>());
        }
        {
            // zero length dimension
            MemoryPackSerializer.Deserialize<int[,]>(MemoryPackSerializer.Serialize(new int[0, 5]))!.GetLength(1).Should().Be(5);
            MemoryPackSerializer.Deserialize<string[,,]>(MemoryPackSerializer.Serialize(new string[2, 0, 5]))!.GetLength(2).Should().Be(5);
        }
    }

    [Fact]
    public void BrotliStringLengthOverflow()
    {
        // string length 1073741825, byte length(* 2) overflows int, must not pass decompression size limit check.
        var payload = new byte[1 + 4 + 8];
        payload[0] = 1;
        BinaryPrimitives.WriteInt32LittleEndian(payload.AsSpan(1), 1073741825);
        Assert.Throws<OverflowException>(() => MemoryPackSerializer.Deserialize<MalformedBrotliStringData>(payload));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(31)]
    [InlineData(32)]
    [InlineData(33)]
    [InlineData(64)]
    [InlineData(100)]
    [InlineData(1000)]
    public void BitArrayRoundtrip(int length)
    {
        var value = new BitArray(length);
        for (int i = 0; i < length; i += 3)
        {
            value[i] = true;
        }

        var bin = MemoryPackSerializer.Serialize(value);

        // (object header, int length, (int collection length, int[]))
        bin.Length.Should().Be(1 + 4 + 4 + ((length + 31) / 32) * 4);

        var value2 = MemoryPackSerializer.Deserialize<BitArray>(bin)!;
        value2.Length.Should().Be(length);
        for (int i = 0; i < length; i++)
        {
            value2[i].Should().Be(i % 3 == 0);
        }
    }

    [Fact]
    public void BitArrayBinaryIsSameInAllRuntimes()
    {
        // BitArray(100), every third bit is true, serialized by .NET 8.
        var expected = Convert.FromHexString("02640000000400000049922449922449922449922409000000");

        var value = new BitArray(100);
        for (int i = 0; i < 100; i += 3)
        {
            value[i] = true;
        }

        MemoryPackSerializer.Serialize(value).Should().Equal(expected);

        var value2 = MemoryPackSerializer.Deserialize<BitArray>(expected)!;
        value2.Length.Should().Be(100);
        for (int i = 0; i < 100; i++)
        {
            value2[i].Should().Be(i % 3 == 0);
        }
    }
}

[MemoryPackable]
public partial class MalformedStringData
{
    public string? Text { get; set; }
}

[MemoryPackable(GenerateType.VersionTolerant)]
public partial class MalformedVersionTolerantData
{
    [MemoryPackOrder(0)]
    public int A { get; set; }
}

[MemoryPackable]
public partial class MalformedBrotliStringData
{
    [BrotliStringFormatter]
    public string? Text { get; set; }
}
