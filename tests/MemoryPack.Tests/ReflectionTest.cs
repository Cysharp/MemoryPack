﻿using System.Reflection;

namespace MemoryPack.Tests;

public class ReflectionTest
{
    [Fact]
    public void InvokeExplicitInterface()
    {
        var type = typeof(ReflecCheck);

#if NET7_0_OR_GREATER
        var m = type.GetMethod("MemoryPack.IMemoryPackFormatterRegister.RegisterFormatter", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
        m.Should().NotBeNull();

        var p = type.GetProperty("global::MemoryPack.IFixedSizeMemoryPackable.Size", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
        p.Should().NotBeNull();
#else
        var m = type.GetMethod("RegisterFormatter", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
        m.Should().NotBeNull();
#endif
    }
}

[MemoryPackable]
public partial class ReflecCheck
{
    public int MyProperty1 { get; set; }
    public int MyProperty2 { get; set; }
}
