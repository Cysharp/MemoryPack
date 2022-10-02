using System;
using System.Buffers;

namespace MemoryPack.Tests;

public class WriterTest
{
    [Fact]
    public void BufferManagementTest()
    {
        var buffer = new SpanControlWriter();

        var writer = new MemoryPackWriter<SpanControlWriter>(ref buffer, MemoryPackSerializeOptions.Default);

        buffer.ProvideSpanLength = 5;

        writer.GetSpanReference(3);
        writer.Advance(3);

        buffer.SpanRequested.Should().Be(1);
        buffer.AdvancedLength.Should().Be(0);

        writer.GetSpanReference(2);
        writer.Advance(2);

        buffer.SpanRequested.Should().Be(1);
        buffer.AdvancedLength.Should().Be(0);

        // request more span
        writer.GetSpanReference(3);
        buffer.SpanRequested.Should().Be(2);
        buffer.AdvancedLength.Should().Be(5);

        // invalid advance
        var error = false;
        try
        {
            writer.Advance(9999);
        }
        catch (MemoryPackSerializationException)
        {
            error = true;
        }
        error.Should().BeTrue();
    }

    [Fact]
    public void WriteObjectHeaderTest()
    {
        var buffer = new ArrayBufferWriter<byte>();

        {
            var writer = new MemoryPackWriter<ArrayBufferWriter<byte>>(ref buffer, MemoryPackSerializeOptions.Default);

            writer.WriteNullObjectHeader();
            writer.Flush();

            buffer.WrittenSpan[0].Should().Be(MemoryPackCode.NullObject);
            buffer.Clear();
        }
        for (var i = 0; i < 250; i++)
        {
            var writer = new MemoryPackWriter<ArrayBufferWriter<byte>>(ref buffer, MemoryPackSerializeOptions.Default);
            writer.WriteObjectHeader((byte)i);
            writer.Flush();

            buffer.WrittenSpan[0].Should().Be((byte)i);
            buffer.Clear();
        }

        for (byte i = MemoryPackCode.Reserved1; i <= MemoryPackCode.NullObject; i++)
        {
            if (i == 0) break;
            var writer = new MemoryPackWriter<ArrayBufferWriter<byte>>(ref buffer, MemoryPackSerializeOptions.Default);
            var error = false;
            try
            {
                writer.WriteObjectHeader((byte)i);
            }
            catch (MemoryPackSerializationException)
            {
                error = true;
            }
            finally
            {
                buffer.Clear();
            }
            error.Should().BeTrue();
        }
    }

    [Fact]
    public void CheckForReusableLinkedArrayBufferWriter()
    {
        // InitialBuffer = 262144
        // 1(object-header) + (4 + (array-header + buffer-size) * 5)
        var buffer = new BufferTest
        {
            Buffer1 = new byte[262139],
            Buffer2 = new byte[262135],
            Buffer3 = new byte[1],
            Buffer4 = new byte[262139],
            Buffer5 = new byte[262139],
        };

        Array.Fill(buffer.Buffer1!, (byte)14);
        Array.Fill(buffer.Buffer2, (byte)30);
        Array.Fill(buffer.Buffer3, (byte)50);
        Array.Fill(buffer.Buffer4, (byte)100);
        Array.Fill(buffer.Buffer5, (byte)250);

        var bin = MemoryPackSerializer.Serialize(buffer);
        var v2 = MemoryPackSerializer.Deserialize<BufferTest>(bin);

        buffer.Buffer1.AsSpan().SequenceEqual(v2!.Buffer1).Should().BeTrue();
        buffer.Buffer2.AsSpan().SequenceEqual(v2!.Buffer2).Should().BeTrue();
        buffer.Buffer3.AsSpan().SequenceEqual(v2.Buffer3).Should().BeTrue();
        buffer.Buffer4.AsSpan().SequenceEqual(v2!.Buffer4).Should().BeTrue();
        buffer.Buffer5.AsSpan().SequenceEqual(v2!.Buffer5).Should().BeTrue();
    }

    public class SpanControlWriter : IBufferWriter<byte>
    {
        public int ProvideSpanLength { get; set; }
        public int AdvancedLength { get; private set; }
        public int SpanRequested { get; private set; }

        public void Advance(int count)
        {
            AdvancedLength += count;
        }

        public Memory<byte> GetMemory(int sizeHint = 0)
        {
            throw new NotImplementedException();
        }

        public Span<byte> GetSpan(int sizeHint = 0)
        {
            SpanRequested++;
            return new byte[Math.Max(sizeHint, ProvideSpanLength)];
        }
    }
}


[MemoryPackable]
public partial class BufferTest
{
    public byte[]? Buffer1;
    public byte[]? Buffer2;
    public byte[]? Buffer3;
    public byte[]? Buffer4;
    public byte[]? Buffer5;
}
