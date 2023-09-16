using System.Buffers;

namespace MemoryPack.Tests;

public class PrimitiveTest
{
    [Fact]
    public void ArrayWriterInt()
    {
        var buffer = new ArrayBufferWriter<byte>(1024);

        MemoryPackSerializer.Serialize(buffer, 123);

        buffer.WrittenCount.Should().Be(4);

        var i = MemoryPackSerializer.Deserialize<int>(buffer.WrittenSpan);
        i.Should().Be(123);
    }

    [Fact]
    public void NonGenericInt()
    {
        var bin = MemoryPackSerializer.Serialize(123);
        var i = MemoryPackSerializer.Deserialize<int>(bin);
        i.Should().Be(123);

        var j = (int)MemoryPackSerializer.Deserialize(typeof(int), bin)!;
        j.Should().Be(123);
    }
}
