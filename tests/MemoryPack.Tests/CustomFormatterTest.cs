#pragma warning disable CS8602

using MemoryPack.Tests.Models;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.Arm;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;

namespace MemoryPack.Tests;

public class CustomFormatterTest
{
    [Fact]
    public void Check()
    {
        var value = new CustomFormatterCheck()
        {
            NoMarkField = "aaaa",
            Field1 = "aaaa",
            Prop1 = "bbbb",
            NoMarkProp = "bbbb",
            PropDict = new Dictionary<string, int> { { "ZooM", 999 }, { "DdddN", 10000 } },
            FieldDict = new Dictionary<string, string> { { "hOGe", "hugahuga" }, { "HagE", "nanonano" } },
        };


        var bin1 = MemoryPackSerializer.Serialize(value, MemoryPackSerializerOptions.Utf8);
        var bin2 = MemoryPackSerializer.Serialize(value, MemoryPackSerializerOptions.Utf16);

        var v1 = MemoryPackSerializer.Deserialize<CustomFormatterCheck>(bin1);
        var v2 = MemoryPackSerializer.Deserialize<CustomFormatterCheck>(bin2);

#if NET7_0_OR_GREATER
        v1.PropDict["zoom"].Should().Be(999);
        v1.PropDict["DDDDN"].Should().Be(10000);
        v1.FieldDict["HOGE"].Should().Be("hugahuga");
        v1.FieldDict["hage"].Should().Be("nanonano");
#endif

        v1.Prop1.Should().Be(value.Prop1);
        v1.Field1.Should().Be(value.Field1);
        v2.Prop1.Should().Be(value.Prop1);
        v2.Field1.Should().Be(value.Field1);
    }

#if NET7_0_OR_GREATER

    [Fact]
    public void Pool()
    {
        var forPool = new MemoryPoolModel(
            new[] { 1, 10, 100, 100, 10000 },
            Encoding.UTF8.GetBytes("あいうえおかきくけこさしすせそ"),
            new[] { "aaa", "bbb", "DDDDDDDDDDDDDD", "あいうえおか" },
            new[] { new StdData { MyProperty = 10 }, new StdData { MyProperty = 99 } }
        );

        var bin = MemoryPackSerializer.Serialize(forPool);
        var v2 = MemoryPackSerializer.Deserialize<MemoryPoolModel>(bin);

        //v2.Pool1.Length
        MemoryMarshal.TryGetArray<int>(v2.Pool1, out var seg1).Should().BeTrue();
        MemoryMarshal.TryGetArray<byte>(v2.Pool2, out var seg2).Should().BeTrue();
        MemoryMarshal.TryGetArray(v2.Pool3, out var seg3).Should().BeTrue();
        MemoryMarshal.TryGetArray(v2.Pool4, out var seg4).Should().BeTrue();

        seg1.Array.Length.Should().Be(PoolSize(forPool.Pool1.Length));
        seg2.Array.Length.Should().Be(PoolSize(forPool.Pool2.Length));
        seg3.Array.Length.Should().Be(PoolSize(forPool.Pool3.Length));
        seg4.Array.Length.Should().Be(PoolSize(forPool.Pool4.Length));

        v2.Pool1.ToArray().Should().Equal(forPool.Pool1.ToArray());
        v2.Pool2.ToArray().Should().Equal(forPool.Pool2.ToArray());
        v2.Pool3.ToArray().Should().Equal(forPool.Pool3.ToArray());

        v2.Pool4.Span[0].MyProperty.Should().Be(forPool.Pool4.Span[0].MyProperty);
        v2.Pool4.Span[1].MyProperty.Should().Be(forPool.Pool4.Span[1].MyProperty);

        v2.Dispose();
        v2.Dispose();
    }

    int PoolSize(int size)
    {
        size = BitOperations.Log2((uint)size - 1 | 15) - 3;
        return 16 << size;
    }

#endif
}
#if NET7_0_OR_GREATER

[MemoryPackable]
public partial class MemoryPoolModel : IDisposable
{
    [MemoryPoolFormatter<int>]
    public Memory<int> Pool1 { get; private set; }
    [MemoryPoolFormatter<byte>]
    public Memory<byte> Pool2 { get; private set; }
    [ReadOnlyMemoryPoolFormatter<string>]
    public ReadOnlyMemory<string> Pool3 { get; private set; }
    [ReadOnlyMemoryPoolFormatter<StdData>]
    public ReadOnlyMemory<StdData> Pool4 { get; private set; }

    bool usePool;

    public MemoryPoolModel(Memory<int> pool1, Memory<byte> pool2, ReadOnlyMemory<string> pool3, ReadOnlyMemory<StdData> pool4)
    {
        Pool1 = pool1;
        Pool2 = pool2;
        Pool3 = pool3;
        Pool4 = pool4;
    }

    [MemoryPackOnDeserialized]
    void OnDeserialized()
    {
        usePool = true;
    }

    static void Return<T>(Memory<T> memory) => Return((ReadOnlyMemory<T>)memory);

    static void Return<T>(ReadOnlyMemory<T> memory)
    {
        if (MemoryMarshal.TryGetArray(memory, out var segment) && segment.Array is { Length: > 0 })
        {
            ArrayPool<T>.Shared.Return(segment.Array, clearArray: RuntimeHelpers.IsReferenceOrContainsReferences<T>());
        }
    }

    public void Dispose()
    {
        if (!usePool) return;

        Return(Pool1); Pool1 = default;
        Return(Pool2); Pool2 = default;
        Return(Pool3); Pool3 = default;
        Return(Pool4); Pool4 = default;
    }
}

[MemoryPackable]
public partial class StdData
{
    public int MyProperty { get; set; }
}

#endif
