using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MemoryPack.Tests;

public class DeserializeTest
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
