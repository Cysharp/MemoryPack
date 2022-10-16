using MemoryPack.Formatters;

namespace MemoryPack
{
    public static partial class MemoryPackFormatterProvider
    {
        static void UnityRegister<T>()
            where T : unmanaged
        {
            Register(new UnmanagedFormatter<T>());
            Register(new UnmanagedArrayFormatter<T>());
            Register(new ListFormatter<T>());
            Register(new NullableFormatter<T>());
        }

        static partial void RegisterInitialFormatters()
        {
            UnityRegister<UnityEngine.Vector2>();
            UnityRegister<UnityEngine.Vector3>();
            UnityRegister<UnityEngine.Vector4>();
            UnityRegister<UnityEngine.Vector2Int>();
            UnityRegister<UnityEngine.Vector3Int>();
            UnityRegister<UnityEngine.Bounds>();
            UnityRegister<UnityEngine.BoundsInt>();
            UnityRegister<UnityEngine.RangeInt>();
            UnityRegister<UnityEngine.Rect>();
            UnityRegister<UnityEngine.RectInt>();
        }
    }
}
