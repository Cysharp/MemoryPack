using System;
using System.Buffers;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MemoryPack.Tests;

public partial class DeserializeTest
{
    [Fact]
    public async Task StreamTest()
    {
        var expected = Enumerable.Range(1, 1000).ToArray();
        var bytes = MemoryPackSerializer.Serialize(expected);

        var stream = new RandomStream(bytes);

        var result = await MemoryPackSerializer.DeserializeAsync<int[]>(stream);
        result.Should().Equal(expected);

        // large size
        expected = Enumerable.Range(1, 100000).ToArray();
        bytes = MemoryPackSerializer.Serialize(expected);

        stream = new RandomStream(bytes);
        result = await MemoryPackSerializer.DeserializeAsync<int[]>(stream);
        result.Should().Equal(expected);
    }

    [Fact]
    public void GenericValueStructTest()
    {
        GenericStruct<int> value = new() { Id = 75, Value = 23 };

        RunMultiSegmentTest(value);
    }

    [Fact]
    public void LargeGenericValueStructTest()
    {
        GenericStruct<PrePaddedInt> value = new() { Id = 75, Value = new PrePaddedInt() { Value = 23 } };

        RunMultiSegmentTest(value);
    }

    [Fact]
    public void GenericReferenceStructTest()
    {
        GenericStruct<string> value = new GenericStruct<string>() { Id = 75, Value = "Hello World!" };

        RunMultiSegmentTest(value);
    }

    [Fact]
    public void LargeGenericReferenceStructTest()
    {
        GenericStruct<PrePaddedString> value = new() { Id = 75, Value = new PrePaddedString() { Value = "Hello World!" } };

        RunMultiSegmentTest(value);
    }

    private void RunMultiSegmentTest<T>(T value)
    {
        byte[] bytes = MemoryPackSerializer.Serialize(value);

        byte[] firstHalf = new byte[bytes.Length / 2];
        Array.Copy(bytes, 0, firstHalf, 0, firstHalf.Length);

        int secondHalfLength = bytes.Length / 2;
        if (bytes.Length % 2 != 0)
        {
            secondHalfLength++;
        }

        byte[] secondHalf = new byte[secondHalfLength];

        Array.Copy(bytes, firstHalf.Length, secondHalf, 0, secondHalfLength);

        ReadOnlySequence<byte> sequence = ReadOnlySequenceBuilder.Create(firstHalf, secondHalf);

        T? result = MemoryPackSerializer.Deserialize<T>(sequence);
        result.Should().Be(value);
    }

    [MemoryPackable]
    public partial struct GenericStruct<T>
    {
        public int Id;
        public T Value;

        public override string ToString()
        {
            return $"{Id}, {Value}";
        }
    }

    [StructLayout(LayoutKind.Explicit, Size = 516)]
    struct PrePaddedInt
    {
        [FieldOffset(512)]
        public int Value;
    }

    [MemoryPackable]
    private partial class PrePaddedString : IEquatable<PrePaddedString>
    {
        private PrePaddedInt _padding;
        public string Value { get; set; } = "";

        public bool Equals(PrePaddedString? other)
        {
            if (other is null)
                return false;

            return Value.Equals(other.Value);
        }

        public override bool Equals(object? obj)
        {
            if (obj is PrePaddedString other)
                return Equals(other);
            return false;
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }
    }

    class RandomStream : Stream
    {
        readonly byte[] underlyingBuffer;
        int position;

        public RandomStream(byte[] buffer)
        {
            this.underlyingBuffer = buffer;
        }

        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => false;

        public override long Length => underlyingBuffer.Length;

        public override long Position { get => position; set => throw new NotSupportedException(); }

        public override void Flush()
        {
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var rest = underlyingBuffer.Length - position;
            if (rest == 0) return 0;
            var len = (count > 100) ? Random.Shared.Next(100, count) : count;
            len = Math.Min(len, rest);
            underlyingBuffer.AsSpan(position, len).CopyTo(buffer.AsSpan(offset, count));
            position += len;
            return len;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }
    }
}
