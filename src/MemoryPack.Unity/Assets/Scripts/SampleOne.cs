#nullable disable

using MemoryPack;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SampleOne : MonoBehaviour
{
    void Start()
    {
        var bin = MemoryPackSerializer.Serialize(new MyPerson { Age = 9999, Name = "hogemogeふがふが" });
        Debug.Log("Payload size:" + bin.Length);
        var v2 = MemoryPackSerializer.Deserialize<MyPerson>(bin);
        Debug.Log("OK Deserialzie:" + v2.Age + ":" + v2.Name);
    }
}

[MemoryPackable]
public partial class MyPerson
{
    public int Age { get; set; }
    public string Name { get; set; }
}

#nullable enable

[MemoryPackable]
public readonly partial struct SerializableAnimationCurve
{
    [MemoryPackIgnore]
    public readonly AnimationCurve AnimationCurve;

    [MemoryPackInclude]
    WrapMode preWrapMode => AnimationCurve.preWrapMode;
    [MemoryPackInclude]
    WrapMode postWrapMode => AnimationCurve.postWrapMode;
    [MemoryPackInclude]
    Keyframe[] keys => AnimationCurve.keys;

    [MemoryPackConstructor]
    SerializableAnimationCurve(WrapMode preWrapMode, WrapMode postWrapMode, Keyframe[] keys)
    {
        var curve = new AnimationCurve(keys);
        curve.preWrapMode = preWrapMode;
        curve.postWrapMode = postWrapMode;
        this.AnimationCurve = curve;
    }

    public SerializableAnimationCurve(AnimationCurve animationCurve)
    {
        this.AnimationCurve = animationCurve;
    }
}

public class AnimationCurveFormatter : MemoryPackFormatter<AnimationCurve>
{
    // Unity does not support scoped and TBufferWriter so change signature to `Serialize(ref MemoryPackWriter writer, ref AnimationCurve value)`
    public override void Serialize(ref MemoryPackWriter writer, scoped ref AnimationCurve? value)
    {
        if (value == null)
        {
            writer.WriteNullObjectHeader();
            return;
        }

        writer.WritePackable(new SerializableAnimationCurve(value));
    }

    public override void Deserialize(ref MemoryPackReader reader, scoped ref AnimationCurve? value)
    {
        if (reader.PeekIsNull())
        {
            value = null;
            return;
        }

        var wrapped = reader.ReadPackable<SerializableAnimationCurve>();
        value = wrapped.AnimationCurve;
    }
}
