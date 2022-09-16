using System.Diagnostics.CodeAnalysis;

namespace MemoryPack;

// Throw helpers is sometimes called from generated code so public.
public static class ThrowHelper
{
    [DoesNotReturn]
    public static void ThrowInvalidPropertyCount(byte expected, byte actual)
    {
        throw new InvalidOperationException($"Current object's property count is {expected} but binary's header maked as {actual}, can't deserialize about versioning.");
    }

    // TODO:inefficient range?
    [DoesNotReturn]
    public static void ThrowInvalidRange(int expected, int actual)
    {
        throw new InvalidOperationException($"Requires size is {expected} but buffer length is {actual}.");
    }

    [DoesNotReturn]
    public static void ThrowInvalidAdvance()
    {
        throw new InvalidOperationException($"Cannot advance past the end of the buffer.");
    }
}
