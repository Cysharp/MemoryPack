using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace MemoryPack;

public static class MemoryPackFormatterProvider
{
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
            formatter = CreateFormatter(typeof(T)) as IMemoryPackFormatter<T>;
            Check<T>.registered = true;
        }
    }

    // should avoid large loop in generic type so use nongenerics method.
    internal static IMemoryPackFormatter? CreateFormatter(Type type)
    {
        return null;
    }
}
