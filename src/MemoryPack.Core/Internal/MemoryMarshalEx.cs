#if !NET7_0_OR_GREATER

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MemoryPack.Internal;

internal static class MemoryMarshalEx
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref T GetArrayDataReference<T>(T[] array)
    {
        return ref MemoryMarshal.GetReference(array.AsSpan());
    }

    // GC
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T[] AllocateUninitializedArray<T>(int length, bool pinned = false)
    {
        return new T[length];
    }
}

#endif
