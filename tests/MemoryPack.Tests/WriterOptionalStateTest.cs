using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemoryPack.Tests;

public class WriterOptionalStateTest
{
    [Fact]
    public void AddReference()
    {
        var state = MemoryPackWriterOptionalStatePool.Rent(null);

        var o0 = new object();
        var o1 = new object();
        var o2 = new object();

        var (exists, id) = state.GetOrAddReference(o0);
        exists.Should().BeFalse();
        id.Should().Be(0);

        (exists, id) = state.GetOrAddReference(o1);
        exists.Should().BeFalse();
        id.Should().Be(1);

        (exists, id) = state.GetOrAddReference(o0);
        exists.Should().BeTrue();
        id.Should().Be(0);

        (exists, id) = state.GetOrAddReference(o2);
        exists.Should().BeFalse();
        id.Should().Be(2);

        (exists, id) = state.GetOrAddReference(o1);
        exists.Should().BeTrue();
        id.Should().Be(1);

        (exists, id) = state.GetOrAddReference(o2);
        exists.Should().BeTrue();
        id.Should().Be(2);

        state.Reset();
    }
}
