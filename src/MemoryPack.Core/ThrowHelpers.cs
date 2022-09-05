using System.Diagnostics.CodeAnalysis;

namespace MemoryPack;

public static class ThrowHelpers
{
    [DoesNotReturn]
    public static void InvalidPropertyCount(byte expected, byte actual)
    {
        throw new InvalidOperationException($"Current object's property count is {expected} but binary's header maked as {actual}, can't deserialize about versioning.");
    }
}
