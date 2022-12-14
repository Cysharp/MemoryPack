using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MemoryPack.Tests;

public class ReflectionTest
{
    [Fact]
    public void InvokeExplicitInterface()
    {
        var type = typeof(ReflecCheck);

        var m = type.GetMethod("MemoryPack.IMemoryPackFormatterRegister.RegisterFormatter", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
        m.Should().NotBeNull();

#if NET7_0_OR_GREATER
        var p = type.GetProperty("global::MemoryPack.IFixedSizeMemoryPackable.Size", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
        p.Should().NotBeNull();
#endif
    }
}

[MemoryPackable]
public partial class ReflecCheck
{
    public int MyProperty1 { get; set; }
    public int MyProperty2 { get; set; }
}
