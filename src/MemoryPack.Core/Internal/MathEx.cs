using System.Runtime.CompilerServices;

namespace MemoryPack.Internal;

internal static class MathEx
{
    const int ArrayMexLength = 0x7FFFFFC7;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int NewArrayCapacity(int size)
    {
        var newSize = unchecked(size * 2);
        if ((uint)newSize > ArrayMexLength)
        {
            newSize = ArrayMexLength;
        }
        return newSize;
    }
}
