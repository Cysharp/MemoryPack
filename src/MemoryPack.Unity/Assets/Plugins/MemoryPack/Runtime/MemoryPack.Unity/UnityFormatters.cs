#nullable enable

using System;
using System.Runtime.CompilerServices;
using MemoryPack.Internal;
using UnityEngine;

namespace MemoryPack
{
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
            }
            else if (value.@keys.Length == 0)
            {
                writer.WriteCollectionHeader(0);
            }
            else
            {
                var unit = Unsafe.SizeOf<UnityEngine.Keyframe>() - 4;
                var srcLength = unit * value.keys.Length;
                var allocSize = srcLength + 4;

                ref var dest = ref writer.GetSpanReference(allocSize);

                Unsafe.WriteUnaligned(ref dest, value.keys.Length);
                for (int i = 0; i < value.keys.Length; i++)
                {
                    Unsafe.WriteUnaligned(ref Unsafe.Add(ref dest, 4 + unit * i), value.keys[i].time);
                    Unsafe.WriteUnaligned(ref Unsafe.Add(ref dest, 4 + unit * i + Unsafe.SizeOf<float>()), value.keys[i].value);
                    Unsafe.WriteUnaligned(ref Unsafe.Add(ref dest, 4 + unit * i + Unsafe.SizeOf<float>() + Unsafe.SizeOf<float>()), value.keys[i].inTangent);
                    Unsafe.WriteUnaligned(ref Unsafe.Add(ref dest, 4 + unit * i + Unsafe.SizeOf<float>() + Unsafe.SizeOf<float>() + Unsafe.SizeOf<float>()), value.keys[i].outTangent);
                    Unsafe.WriteUnaligned(ref Unsafe.Add(ref dest, 4 + unit * i + Unsafe.SizeOf<float>() + Unsafe.SizeOf<float>() + Unsafe.SizeOf<float>() + Unsafe.SizeOf<float>()), value.keys[i].weightedMode);
                    Unsafe.WriteUnaligned(ref Unsafe.Add(ref dest, 4 + unit * i + Unsafe.SizeOf<float>() + Unsafe.SizeOf<float>() + Unsafe.SizeOf<float>() + Unsafe.SizeOf<float>() + Unsafe.SizeOf<WeightedMode>()), value.keys[i].inWeight);
                    Unsafe.WriteUnaligned(ref Unsafe.Add(ref dest, 4 + unit * i + Unsafe.SizeOf<float>() + Unsafe.SizeOf<float>() + Unsafe.SizeOf<float>() + Unsafe.SizeOf<float>() + Unsafe.SizeOf<WeightedMode>() + Unsafe.SizeOf<float>()), value.keys[i].outWeight);
                }

                writer.Advance(allocSize);
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

            if (value == null)
            {
                value = new AnimationCurve();
            }

            reader.ReadUnmanaged(out WrapMode preWrapMode, out WrapMode postWrapMode);
            value.preWrapMode = preWrapMode;
            value.postWrapMode = postWrapMode;

#if UNITY_EDITOR
            if (!reader.TryReadCollectionHeader(out var length))
            {
                value.keys = null;
            }
            else if (length == 0)
            {
                value.keys = Array.Empty<UnityEngine.Keyframe>();
            }
            else
            {
                var unit = Unsafe.SizeOf<UnityEngine.Keyframe>() - 4;
                var byteCount = length * unit;
                ref var src = ref reader.GetSpanReference(byteCount);

                var arr = new Keyframe[length];
                for (int i = 0; i < length; i++)
                {
                    arr[i].time = Unsafe.ReadUnaligned<float>(ref Unsafe.Add(ref src, unit * i));
                    arr[i].value = Unsafe.ReadUnaligned<float>(ref Unsafe.Add(ref src, unit * i + Unsafe.SizeOf<float>()));
                    arr[i].inTangent = Unsafe.ReadUnaligned<float>(ref Unsafe.Add(ref src, unit * i + Unsafe.SizeOf<float>() + Unsafe.SizeOf<float>()));
                    arr[i].outTangent = Unsafe.ReadUnaligned<float>(ref Unsafe.Add(ref src, unit * i + Unsafe.SizeOf<float>() + Unsafe.SizeOf<float>() + Unsafe.SizeOf<float>()));
                    arr[i].weightedMode = Unsafe.ReadUnaligned<WeightedMode>(ref Unsafe.Add(ref src, unit * i + Unsafe.SizeOf<float>() + Unsafe.SizeOf<float>() + Unsafe.SizeOf<float>() + Unsafe.SizeOf<float>()));
                    arr[i].inWeight = Unsafe.ReadUnaligned<float>(ref Unsafe.Add(ref src, unit * i + Unsafe.SizeOf<float>() + Unsafe.SizeOf<float>() + Unsafe.SizeOf<float>() + Unsafe.SizeOf<float>() + Unsafe.SizeOf<WeightedMode>()));
                    arr[i].outWeight = Unsafe.ReadUnaligned<float>(ref Unsafe.Add(ref src, unit * i + Unsafe.SizeOf<float>() + Unsafe.SizeOf<float>() + Unsafe.SizeOf<float>() + Unsafe.SizeOf<float>() + Unsafe.SizeOf<WeightedMode>() + Unsafe.SizeOf<float>()));
                }
                value.keys = arr;

                reader.Advance(byteCount);
            }
#else
            value.keys = reader.ReadUnmanagedArray<global::UnityEngine.Keyframe>();
#endif
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
