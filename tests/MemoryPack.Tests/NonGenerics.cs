using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemoryPack.Tests;

public class NonGenerics
{
    [Fact]
    public void StandardBin()
    {
        var mc = new[] { new MyClass<ForeE> { MyProperty = ForeE.C } };
        var bin = MemoryPackSerializer.Serialize(mc.GetType(), mc);


        var v2 = MemoryPackSerializer.Deserialize(mc.GetType(), bin) as MyClass<ForeE>[];
        var v3 = MemoryPackSerializer.Deserialize<MyClass<ForeE>[]>(bin);

        v2![0].MyProperty.Should().Be(ForeE.C);
        v3![0].MyProperty.Should().Be(ForeE.C);
    }

    // https://github.com/Cysharp/MemoryPack/issues/98
    [Fact]
    public async Task StreamCheck()
    {
        using var ms = new MemoryStream();

        // Generic version works
        await MemoryPackSerializer.SerializeAsync(ms, new Item());

        // Non generic version throws MemoryPackSerializationException: System.Object is not registered in this provider.
        await MemoryPackSerializer.SerializeAsync(typeof(Item), ms, new Item());

     
    }
}

[MemoryPackable]
public partial class MyClass<T>
{
    public T? MyProperty { get; set; }
}

[MemoryPackable]
public partial record Item
{
    public string Value { get; init; } = Guid.NewGuid().ToString();
}

public enum ForeE
{
    A, B, C
}
