using System;
using System.Collections.Generic;
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
}

[MemoryPackable]
public partial class MyClass<T>
{
    public T? MyProperty { get; set; }
}



public enum ForeE
{
    A, B, C
}
