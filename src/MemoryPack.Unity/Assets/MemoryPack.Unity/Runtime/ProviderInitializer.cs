#nullable enable

using MemoryPack.Formatters;
using UnityEngine;

namespace MemoryPack
{
    public static partial class MemoryPackUnityFormatterProviderInitializer
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        public static void RegisterInitialFormatters()
        {
            // struct
            UnityRegister<UnityEngine.Vector2>();
            UnityRegister<UnityEngine.Vector3>();
            UnityRegister<UnityEngine.Vector4>();
            UnityRegister<UnityEngine.Quaternion>();
            UnityRegister<UnityEngine.Color>();
            UnityRegister<UnityEngine.Bounds>();
            UnityRegister<UnityEngine.Rect>();
            UnityRegister<UnityEngine.Keyframe>();
            MemoryPackFormatterProvider.Register(new UnmanagedFormatter<UnityEngine.WrapMode>());
            UnityRegister<UnityEngine.Matrix4x4>();
            UnityRegister<UnityEngine.GradientColorKey>();
            UnityRegister<UnityEngine.GradientAlphaKey>();
            MemoryPackFormatterProvider.Register(new UnmanagedFormatter<UnityEngine.GradientMode>());
            UnityRegister<UnityEngine.Color32>();
            UnityRegister<UnityEngine.LayerMask>();
            UnityRegister<UnityEngine.Vector2Int>();
            UnityRegister<UnityEngine.Vector3Int>();
            UnityRegister<UnityEngine.RangeInt>();
            UnityRegister<UnityEngine.RectInt>();
            UnityRegister<UnityEngine.BoundsInt>();

            // class
            if (!MemoryPackFormatterProvider.IsRegistered<AnimationCurve>())
            {
                MemoryPackFormatterProvider.Register(new AnimationCurveFormatter());
                MemoryPackFormatterProvider.Register(new ArrayFormatter<AnimationCurve>());
                MemoryPackFormatterProvider.Register(new ListFormatter<AnimationCurve>());
            }
            if (!MemoryPackFormatterProvider.IsRegistered<Gradient>())
            {
                MemoryPackFormatterProvider.Register(new GradientFormatter());
                MemoryPackFormatterProvider.Register(new ArrayFormatter<Gradient>());
                MemoryPackFormatterProvider.Register(new ListFormatter<Gradient>());
            }
            if (!MemoryPackFormatterProvider.IsRegistered<RectOffset>())
            {
                MemoryPackFormatterProvider.Register(new RectOffsetFormatter());
                MemoryPackFormatterProvider.Register(new ArrayFormatter<RectOffset>());
                MemoryPackFormatterProvider.Register(new ListFormatter<RectOffset>());
            }
        }

        static void UnityRegister<T>()
            where T : unmanaged
        {
            MemoryPackFormatterProvider.Register(new UnmanagedFormatter<T>());
            MemoryPackFormatterProvider.Register(new UnmanagedArrayFormatter<T>());
            MemoryPackFormatterProvider.Register(new ListFormatter<T>());
            MemoryPackFormatterProvider.Register(new NullableFormatter<T>());
        }
    }
}
