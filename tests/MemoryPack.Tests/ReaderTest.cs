using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MemoryPack.Tests;

public class ReaderTest
{
    [Fact]
    public void ReadFromSequence()
    {
        var seq = ReadOnlySequenceBuilder.Create(
            new byte[] { 1, 2, 3 },
            new byte[] { 4, 5, 6, 7, 8 },
            new byte[] { 9, 10 });

        var reader = new MemoryPackReader(seq);


        ref var spanRef = ref reader.GetSpanReference(2);
        MemoryMarshal.CreateReadOnlySpan(ref spanRef, 2).ToArray().Should().Equal(1, 2);
        reader.Advance(2);

        spanRef = ref reader.GetSpanReference(4);
        MemoryMarshal.CreateReadOnlySpan(ref spanRef, 4).ToArray().Should().Equal(3, 4, 5, 6);
        reader.Advance(4);

        spanRef = ref reader.GetSpanReference(2);
        MemoryMarshal.CreateReadOnlySpan(ref spanRef, 2).ToArray().Should().Equal(7, 8);
        reader.Advance(2);

        spanRef = ref reader.GetSpanReference(2);
        MemoryMarshal.CreateReadOnlySpan(ref spanRef, 2).ToArray().Should().Equal(9, 10);
        reader.Advance(2); // end

        bool error = false;
        try
        {
            reader.GetSpanReference(1);
        }
        catch (MemoryPackSerializationException)
        {
            error = true;
        }
        error.Should().BeTrue();
    }

    [Fact]
    public void ReadFromSpan()
    {
        var reader = new MemoryPackReader(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 });

        ref var spanRef = ref reader.GetSpanReference(2);
        MemoryMarshal.CreateReadOnlySpan(ref spanRef, 2).ToArray().Should().Equal(1, 2);
        reader.Advance(2);

        spanRef = ref reader.GetSpanReference(4);
        MemoryMarshal.CreateReadOnlySpan(ref spanRef, 4).ToArray().Should().Equal(3, 4, 5, 6);
        reader.Advance(4);

        spanRef = ref reader.GetSpanReference(2);
        MemoryMarshal.CreateReadOnlySpan(ref spanRef, 2).ToArray().Should().Equal(7, 8);
        reader.Advance(2);

        spanRef = ref reader.GetSpanReference(2);
        MemoryMarshal.CreateReadOnlySpan(ref spanRef, 2).ToArray().Should().Equal(9, 10);
        reader.Advance(2); // end

        bool error = false;
        try
        {
            reader.GetSpanReference(1);
        }
        catch (MemoryPackSerializationException)
        {
            error = true;
        }
        error.Should().BeTrue();
    }

    [Fact]
    public void OverAdvance()
    {
        var reader = new MemoryPackReader(new byte[] { 1, 2, 3 });
        reader.Advance(2); // ok

        bool error = false;
        try
        {
            reader.Advance(10); // ng
        }
        catch (MemoryPackSerializationException)
        {
            error = true;
        }
        error.Should().BeTrue();
    }

    [Fact]
    public void ValidateInvalidLengthTest()
    {
        // int[] but invalid length
        // 2(len), 99, 9999
        var bytes = MemoryPackSerializer.Serialize(new[] { 99, 9999 });

        var reader = new MemoryPackReader(bytes);

        reader.TryReadCollectionHeader(out var len).Should().BeTrue();
        len.Should().Be(2);

        reader.ReadUnmanaged(out int v1, out int v2);
        v1.Should().Be(99);
        v2.Should().Be(9999);

        // inject invalid length
        BinaryPrimitives.WriteInt32LittleEndian(bytes, 1000); // 1000(len), 99, 9999
        reader = new MemoryPackReader(bytes);

        try
        {
            reader.TryReadCollectionHeader(out var len2);
            Assert.Fail("should throw");
        }
        catch (MemoryPackSerializationException) { }

        // just
        bytes = MemoryPackSerializer.Serialize(new byte[] { 99 });
        reader = new MemoryPackReader(bytes);
        reader.TryReadCollectionHeader(out var len3);
        len3.Should().Be(1);

        BinaryPrimitives.WriteInt32LittleEndian(bytes, 2);
        reader = new MemoryPackReader(bytes);

        try
        {
            reader.TryReadCollectionHeader(out var len4);
            Assert.Fail("should throw");
        }
        catch (MemoryPackSerializationException) { }
    }

}
