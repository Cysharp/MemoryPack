using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace MemoryPack.Tests;

public class UnityShimsTest
{
    [Fact]
    public void Keyframe()
    {
        Assert.Equal(28, Unsafe.SizeOf<Keyframe>());

        var keyframe = new Keyframe(1.23f, 4.56f);

        var raw = MemoryPackSerializer.Serialize(keyframe);
        Assert.Equal("A4-70-9D-3F-85-EB-91-40-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00", BitConverter.ToString(raw));

        var other = MemoryPackSerializer.Deserialize<Keyframe>(raw);
        Assert.Equal(1.23f, other.time, 0.001f);
        Assert.Equal(4.56f, other.value, 0.001f);
    }

    [Fact]
    public void AnimationCurve()
    {
        var curve = new AnimationCurve
        {
            preWrapMode = WrapMode.Loop,
            postWrapMode = WrapMode.ClampForever,
            keys = new Keyframe[3]
            {
                new Keyframe(1.23f, 4.56f),
                new Keyframe(1.23f, 4.56f, 7.8f, 0.9f),
                new Keyframe(1.23f, 4.56f, 7.8f, 0.9f, 1f, 3.5f),
            },
        };

        var raw = MemoryPackSerializer.Serialize(curve);
        Assert.Equal("03-02-00-00-00-08-00-00-00-03-00-00-00-A4-70-9D-3F-85-EB-91-40-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-A4-70-9D-3F-85-EB-91-40-9A-99-F9-40-66-66-66-3F-00-00-00-00-00-00-00-00-00-00-00-00-A4-70-9D-3F-85-EB-91-40-9A-99-F9-40-66-66-66-3F-03-00-00-00-00-00-80-3F-00-00-60-40", BitConverter.ToString(raw));

        var curve1 = MemoryPackSerializer.Deserialize<AnimationCurve>(raw);
        Assert.NotNull(curve1);
        Assert.Equal(WrapMode.Loop, curve1.preWrapMode);
        Assert.Equal(WrapMode.ClampForever, curve1.postWrapMode);

        Assert.Equal(3, curve1.keys.Length);
        Assert.Equal(4.56f, curve1.keys[curve1!.keys.Length - 1].value, 0.001);
    }
}
