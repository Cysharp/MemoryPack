using MemoryPack.Formatters;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace MemoryPack;

// This provider is fallback if can't resolve in compile time(like generics).
// Therefore, unlike MessagePack for C#, it is static and not extensible.

public static partial class MemoryPackFormatterProvider
{
    public static bool IsRegistered<T>() => Check<T>.registered;

    public static void Register<T>(IMemoryPackFormatter<T> formatter)
    {
        Check<T>.registered = true; // avoid to call Cache() constructor called.
        Cache<T>.formatter = formatter;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IMemoryPackFormatter<T> GetFormatter<T>()
    {
        return Cache<T>.formatter;
    }

    static class Check<T>
    {
        public static bool registered;
    }

    static class Cache<T>
    {
        public static IMemoryPackFormatter<T> formatter = default!;

        static Cache()
        {
            if (Check<T>.registered) return;

            var type = typeof(T);
            var typeIsReferenceOrContainsReferences = RuntimeHelpers.IsReferenceOrContainsReferences<T>();
            var f = CreateFormatter(type, typeIsReferenceOrContainsReferences) as IMemoryPackFormatter<T>;

            formatter = f ?? new ErrorMemoryPackFormatter<T>();

            Check<T>.registered = true;
        }
    }

    internal static object? CreateFormatter(Type type, bool typeIsReferenceOrContainsReferences)
    {
        Type? instanceType = null;
        if (type.IsArray)
        {
            if (typeIsReferenceOrContainsReferences)
            {
                instanceType = typeof(UnmanagedTypeArrayFormatter<>).MakeGenericType(type.GetElementType()!);
            }
        }

        return (instanceType != null)
             ? Activator.CreateInstance(instanceType)
            : null;
    }
}

internal sealed class ErrorMemoryPackFormatter<T> : IMemoryPackFormatter<T>
{
    public void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref T? value)
        where TBufferWriter : IBufferWriter<byte>
    {
        throw new InvalidOperationException($"{typeof(T).FullName} is not registered in this provider.");
    }

    public void Deserialize(ref MemoryPackReader reader, scoped ref T? value)
    {
        throw new InvalidOperationException($"{typeof(T).FullName} is not registered in this provider.");
    }
}
