using MemoryPack.Formatters;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace MemoryPack;

public static class MemoryPackFormatterProvider
{
    public static bool IsRegistered<T>() => Check<T>.registered;

    public static void Register<T>(IMemoryPackFormatter<T> formatter)
    {
        Check<T>.registered = true; // avoid to call Cache() constructor called.
        Cache<T>.formatter = formatter;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IMemoryPackFormatter<T>? GetFormatter<T>()
    {
        return Cache<T>.formatter;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IMemoryPackFormatter<T> GetRequiredFormatter<T>()
    {
        var formatter = Cache<T>.formatter;
        if (formatter == null)
        {
            ThrowInvalidOperationException(typeof(T));
        }

        return formatter;
    }

    [DoesNotReturn]
    static void ThrowInvalidOperationException(Type type)
    {
        throw new InvalidOperationException($"{type.FullName} is not registered in this provider.");
    }

    static class Check<T>
    {
        public static bool registered;
    }

    static class Cache<T>
    {
        public static IMemoryPackFormatter<T>? formatter;

        static Cache()
        {
            if (Check<T>.registered) return;

            var type = typeof(T);
            var typeIsReferenceOrContainsReferences = RuntimeHelpers.IsReferenceOrContainsReferences<T>();
            formatter = CreateFormatter(type, typeIsReferenceOrContainsReferences) as IMemoryPackFormatter<T>;

            Check<T>.registered = true;
        }
    }

    internal static IMemoryPackFormatter? CreateFormatter(Type type, bool typeIsReferenceOrContainsReferences)
    {
        Type? instanceType = null;
        if (type.IsArray)
        {
            if (typeIsReferenceOrContainsReferences)
            {
                instanceType = typeof(UnmanagedTypeArrayFormatter<>).MakeGenericType(type.GetElementType()!);
            }


            // RuntimeHelpers.IsReferenceOrContainsReferences()




        }

        return (instanceType != null)
             ? Activator.CreateInstance(instanceType) as IMemoryPackFormatter
            : null;
    }
}
