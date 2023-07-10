#nullable enable

using System;
using MemoryPack.Internal;
using UnityEngine;

namespace MemoryPack
{
#if UNITY_EDITOR
    [Preserve]
    internal sealed class KeyframeFormatter : MemoryPackFormatter<Keyframe>
    {
        [Preserve]
        public override void Serialize(ref MemoryPackWriter writer, ref Keyframe value)
        {
            writer.WriteUnmanaged(value.time, value.value, value.inTangent, value.outTangent, value.weightedMode, value.inWeight, value.outWeight);
        }

        [Preserve]
        public override void Deserialize(ref MemoryPackReader reader, ref Keyframe value)
        {
            reader.ReadUnmanaged(out float __time, out float __value, out float __inTangent, out float __outTangent, out WeightedMode __weightedMode, out float __inWeight, out float __outWeight);
            value.time = __time;
            value.value = __value;
            value.inTangent = __inTangent;
            value.outTangent = __outTangent;
            value.weightedMode = __weightedMode;
            value.inWeight = __inWeight;
            value.outWeight = __outWeight;
        }
    }
#endif

    [Preserve]
    internal sealed class AnimationCurveFormatter : MemoryPackFormatter<AnimationCurve>
    {
        [Preserve]
        public override void Serialize(ref MemoryPackWriter writer, ref AnimationCurve? value)
        {
            if (value == null)
            {
                writer.WriteNullObjectHeader();
                return;
            }

            writer.WriteUnmanagedWithObjectHeader(3, value.@preWrapMode, value.@postWrapMode);
#if UNITY_EDITOR
            if (value.@keys == null)
            {
                writer.WriteNullCollectionHeader();
                return;
            }

            var formatter = writer.GetFormatter<Keyframe>();
            writer.WriteCollectionHeader(value.@keys.Length);
            for (int i = 0; i < value.@keys.Length; i++)
            {
                formatter.Serialize(ref writer, ref value.@keys[i]);
            }
#else
            writer.WriteUnmanagedArray(value.@keys);
#endif
        }

        [Preserve]
        public override void Deserialize(ref MemoryPackReader reader, ref AnimationCurve? value)
        {
            if (!reader.TryReadObjectHeader(out var count))
            {
                value = null;
                return;
            }

            if (count != 3) MemoryPackSerializationException.ThrowInvalidPropertyCount(3, count);

            reader.ReadUnmanaged(out WrapMode preWrapMode, out WrapMode postWrapMode);
#if UNITY_EDITOR
            Keyframe[]? keys;
            if (!reader.TryReadCollectionHeader(out var length))
            {
                keys = null;
            }
            else if (length == 0)
            {
                keys = Array.Empty<Keyframe>();
            }
            else
            {
                keys = new Keyframe[length];
                var formatter = reader.GetFormatter<Keyframe>();
                for (int i = 0; i < length; i++)
                {
                    formatter.Deserialize(ref reader, ref keys[i]);
                }
            }
#else
            var keys = reader.ReadUnmanagedArray<global::UnityEngine.Keyframe>();
#endif

            if (value == null)
            {
                value = new AnimationCurve();
            }

            value.preWrapMode = preWrapMode;
            value.postWrapMode = postWrapMode;
            value.keys = keys;
        }
    }

    [Preserve]
    internal sealed class GradientFormatter : MemoryPackFormatter<Gradient>
    {
        [Preserve]
        public override void Serialize(ref MemoryPackWriter writer, ref Gradient? value)
        {
            if (value == null)
            {
                writer.WriteNullObjectHeader();
                return;
            }

            writer.WriteObjectHeader(3);
            writer.WriteUnmanagedArray(value.@colorKeys);
            writer.WriteUnmanagedArray(value.@alphaKeys);
            writer.WriteUnmanaged(value.@mode);
        }

        [Preserve]
        public override void Deserialize(ref MemoryPackReader reader, ref Gradient? value)
        {
            if (!reader.TryReadObjectHeader(out var count))
            {
                value = null;
                return;
            }

            if (count != 3) MemoryPackSerializationException.ThrowInvalidPropertyCount(3, count);

            var colorKeys = reader.ReadUnmanagedArray<global::UnityEngine.GradientColorKey>();
            var alphaKeys = reader.ReadUnmanagedArray<global::UnityEngine.GradientAlphaKey>();
            reader.ReadUnmanaged(out GradientMode mode);

            if (value == null)
            {
                value = new Gradient();
            }

            value.colorKeys = colorKeys;
            value.alphaKeys = alphaKeys;
            value.mode = mode;
        }
    }

    [Preserve]
    internal sealed class RectOffsetFormatter : MemoryPackFormatter<RectOffset>
    {
        [Preserve]
        public override void Serialize(ref MemoryPackWriter writer, ref RectOffset? value)
        {
            if (value == null)
            {
                writer.WriteNullObjectHeader();
                return;
            }

            writer.WriteUnmanagedWithObjectHeader(4, value.@left, value.@right, value.@top, value.@bottom);
        }

        [Preserve]
        public override void Deserialize(ref MemoryPackReader reader, ref RectOffset? value)
        {
            if (!reader.TryReadObjectHeader(out var count))
            {
                value = null;
                return;
            }

            if (count != 4) MemoryPackSerializationException.ThrowInvalidPropertyCount(4, count);

            reader.ReadUnmanaged(out int left, out int right, out int top, out int bottom);

            if (value == null)
            {
                value = new RectOffset(left, right, top, bottom);
            }
            else
            {
                value.left = left;
                value.right = right;
                value.top = top;
                value.bottom = bottom;
            }
        }
    }
}
