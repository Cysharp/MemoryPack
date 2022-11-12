using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemoryPack.Tests;


public class VersionTest
{
    [Fact]
    public void V()
    {
        {
            var v = new Version();
            var bin = MemoryPackSerializer.Serialize(v);
            var v2 = MemoryPackSerializer.Deserialize<Version>(bin);
            v2.Should().Be(v);
        }
        {
            var v = new Version(10, 20);
            var bin = MemoryPackSerializer.Serialize(v);
            var v2 = MemoryPackSerializer.Deserialize<Version>(bin);
            v2.Should().Be(v);
        }
        {
            var v = new Version(10, 20, 30);
            var bin = MemoryPackSerializer.Serialize(v);
            var v2 = MemoryPackSerializer.Deserialize<Version>(bin);
            v2.Should().Be(v);
        }
        {
            var v = new Version(10, 20, 30, 40);
            var bin = MemoryPackSerializer.Serialize(v);
            var v2 = MemoryPackSerializer.Deserialize<Version>(bin);
            v2.Should().Be(v);
        }
    }
}
