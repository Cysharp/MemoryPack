using System.Diagnostics.CodeAnalysis;

namespace MemoryPack;

// Throw helpers is sometimes called from generated code so public.
public static class ThrowHelpers
{
    [DoesNotReturn]
    public static void InvalidPropertyCount(byte expected, byte actual)
    {
        throw new InvalidOperationException($"Current object's property count is {expected} but binary's header maked as {actual}, can't deserialize about versioning.");
    }
}
