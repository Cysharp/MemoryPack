using System.Diagnostics.CodeAnalysis;

namespace MemoryPack;

// Throw helpers is sometimes called from generated code so public.
public static class ThrowHelper
{
    // TODO:use class MemoryPackSerializationException : Exception

    [DoesNotReturn]
    public static void ThrowInvalidPropertyCount(byte expected, byte actual)
    {
        throw new InvalidOperationException($"Current object's property count is {expected} but binary's header maked as {actual}, can't deserialize about versioning.");
    }

    [DoesNotReturn]
    public static void ThrowInvalidCollection()
    {
        throw new InvalidOperationException($"Current read to collection, the buffer header is not collection.");
    }

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

    [DoesNotReturn]
    public static void ThrowSequenceReachedEnd()
    {
        throw new InvalidOperationException($"Sequence reached end, reader can not provide more buffer.");
    }

    [DoesNotReturn]
    public static void ThrowWriteInvalidPropertyCount(byte propertyCount)
    {
        throw new InvalidOperationException($"Property count allows < 250 but try to write {propertyCount}.");
    }

    [DoesNotReturn]
    public static void ThrowInsufficientBufferUnless(int length)
    {
        throw new InvalidOperationException($"Length header size is larger than buffer size, length: {length}.");
    }

    [DoesNotReturn]
    public static void ThrowNotRegisteredInProvider(Type type)
    {
        throw new InvalidOperationException($"{type.FullName} is not registered in this provider.");
    }

    [DoesNotReturn]
    public static void ThrowRegisterInProviderFailed(Type type, Exception innerException)
    {
        throw new InvalidOperationException($"{type.FullName} is failed in provider at creating formatter.", innerException);
    }

    [DoesNotReturn]
    public static void ThrowNotFoundInUnionType(Type actualType, Type baseType)
    {
        throw new InvalidOperationException($"Type {actualType.FullName} is not annotated in {baseType.FullName} MessagePackUnion.");
    }

    [DoesNotReturn]
    public static void ThrowInvalidTag(byte tag, Type baseType)
    {
        throw new InvalidOperationException($"Data read tag: {tag} but not found in {baseType.FullName} MessagePackUnion annotations.");
    }

    [DoesNotReturn]
    public static void ThrowReachedDepthLimit(Type type)
    {
        throw new InvalidOperationException($"Serializing Type '{type}' reached depth limit, maybe detect circular reference.");
    }

    [DoesNotReturn]
    public static void ThrowInvalidConcurrrentCollectionOperation()
    {
        throw new InvalidOperationException($"ConcurrentCollection is Added/Removed in serializing, however serialize concurrent collection is not thread-safe.");
    }

    [DoesNotReturn]
    public static void ThrowDeserializeObjectIsNull(string target)
    {
        throw new InvalidOperationException($"Deserialized {target} is null.");
    }
}
