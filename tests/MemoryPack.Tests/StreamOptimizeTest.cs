using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemoryPack.Tests;

public class StreamOptimizeTest
{
    [Fact]
    public async Task MemoryStream()
    {
        var ms = new MemoryStream();
        await MemoryPackSerializer.SerializeAsync(ms, new[] { 1, 2, 3 });
        var offset = ms.Position;
        await MemoryPackSerializer.SerializeAsync(ms, new[] { 10, 20, 30 });
        await MemoryPackSerializer.SerializeAsync(ms, new[] { 40, 50, 60 });

        ms.Position = offset;

        var data1 = await MemoryPackSerializer.DeserializeAsync<int[]>(ms);
        var data2 = await MemoryPackSerializer.DeserializeAsync<int[]>(ms);

        data1.Should().Equal(10, 20, 30);
        data2.Should().Equal(40, 50, 60);
    }

    [Fact]
    public async Task MemoryStreamNoGenerics()
    {
        var ms = new MemoryStream();
        await MemoryPackSerializer.SerializeAsync(ms, new[] { 1, 2, 3 });
        var offset = ms.Position;
        await MemoryPackSerializer.SerializeAsync(ms, new[] { 10, 20, 30 });
        await MemoryPackSerializer.SerializeAsync(ms, new[] { 40, 50, 60 });

        ms.Position = offset;

        var data1 = (int[]?)await MemoryPackSerializer.DeserializeAsync(typeof(int[]), ms);
        var data2 = (int[]?)await MemoryPackSerializer.DeserializeAsync(typeof(int[]), ms);

        data1.Should().Equal(10, 20, 30);
        data2.Should().Equal(40, 50, 60);
    }
}
