using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace MemoryPack;

public interface IMemoryPackFormatterProvider
{
    IMemoryPackFormatter<T>? GetFormatter<T>();
}

public static class MemoryPackFormatterProviderExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IMemoryPackFormatter<T> GetRequiredFormatter<T>(this IMemoryPackFormatterProvider provider)
    {
        var formatter = provider.GetFormatter<T>();
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
}