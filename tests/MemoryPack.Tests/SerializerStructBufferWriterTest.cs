using System;
using System.Buffers;
using MemoryPack.Tests.Models;

namespace MemoryPack.Tests;

public class SerializerStructBufferWriterTest
{
    [Fact]
    public void Serialize_ShouldSupportStructAsBufferWriter_WhenValueIsNotReferenceAndNotContainsReferences()
    {
        var writer = new TestBufferWriter();
        MemoryPackSerializer.Serialize(writer, 16);
        Assert.Equal(4, writer.WrittenSize);
    }

    [Fact]
    public void Serialize_ShouldSupportStructAsBufferWriter_WhenValueIsUnmanagedSZArray()
    {
        var writer = new TestBufferWriter();
        MemoryPackSerializer.Serialize(writer, new UnmanagedStruct[] { new() { X = 1, Y = 2, Z = 3 } });
        Assert.Equal(16, writer.WrittenSize);
    }

    [Fact]
    public void Serialize_ShouldSupportStructAsBufferWriter_WhenFormatterRequired()
    {
        var writer = new TestBufferWriter();
        MemoryPackSerializer.Serialize(writer, new TestData(1));
        Assert.Equal(5, writer.WrittenSize);
    }
}

[MemoryPackable]
public partial record TestData(int A);

public struct TestBufferWriter : IBufferWriter<byte>
{
    public int WrittenSize = 0;

    public TestBufferWriter()
    {
    }

    public void Advance(int count)
    {
        WrittenSize += count;
    }

    public Memory<byte> GetMemory(int sizeHint = 0)
    {
        throw new InvalidOperationException();
    }

    public Span<byte> GetSpan(int sizeHint = 0)
    {
        return new byte[sizeHint];
    }
}
