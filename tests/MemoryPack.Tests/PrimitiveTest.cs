using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
}
