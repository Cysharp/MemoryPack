using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemoryPack.Tests;

public class VarIntTest
{
    [Fact]
    public void ReadWrite()
    {
        var buffer = new ArrayBufferWriter<byte>();

        using var state = MemoryPackWriterOptionalStatePool.Rent(null);
        var writer = new MemoryPackWriter<ArrayBufferWriter<byte>>(ref buffer, state);

        writer.WriteVarInt(1);
        writer.WriteVarInt((byte)10);
        writer.WriteVarInt(100);
        writer.WriteVarInt((short)1000);
        writer.WriteVarInt((ushort)10000);
        writer.WriteVarInt(100000);
        writer.WriteVarInt((ulong)1000000);
        writer.WriteVarInt(10000000);
        writer.WriteVarInt(100000000);
        writer.WriteVarInt(1000000000);
        writer.WriteVarInt(10000000000);
        writer.WriteVarInt(100000000000);
        writer.WriteVarInt(-1);
        writer.WriteVarInt(-10);
        writer.WriteVarInt((sbyte)-100);
        writer.WriteVarInt(-1000);
        writer.WriteVarInt(-10000);
        writer.WriteVarInt((long)-100000);
        writer.WriteVarInt(-1000000);
        writer.WriteVarInt(-10000000);
        writer.WriteVarInt(-100000000);
        writer.WriteVarInt(-1000000000);
        writer.WriteVarInt(-10000000000);
        writer.WriteVarInt(-100000000000);

        writer.Flush();

        using var state2 = MemoryPackReaderOptionalStatePool.Rent(null);
        var reader = new MemoryPackReader(buffer.WrittenSpan, state2);

        var l = new long[24];
        for (int i = 0; i < l.Length; i++)
        {
            l[i] = reader.ReadVarIntInt64();
        }

        l[0].Should().Be(1);
        l[1].Should().Be((byte)10);
        l[2].Should().Be(100);
        l[3].Should().Be((short)1000);
        l[4].Should().Be((ushort)10000);
        l[5].Should().Be(100000);
        l[6].Should().Be((long)1000000);
        l[7].Should().Be(10000000);
        l[8].Should().Be(100000000);
        l[9].Should().Be(1000000000);
        l[10].Should().Be(10000000000);
        l[11].Should().Be(100000000000);
        l[12].Should().Be(-1);
        l[13].Should().Be(-10);
        l[14].Should().Be((sbyte)-100);
        l[15].Should().Be(-1000);
        l[16].Should().Be(-10000);
        l[17].Should().Be((long)-100000);
        l[18].Should().Be(-1000000);
        l[19].Should().Be(-10000000);
        l[20].Should().Be(-100000000);
        l[21].Should().Be(-1000000000);
        l[22].Should().Be(-10000000000);
        l[23].Should().Be(-100000000000);
    }


}
