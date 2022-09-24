using MemoryPack.Formatters;
using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace MemoryPack;

// This provider is not extension point, fallback if can't resolve in compile time(like generics).
// Therefore, unlike MessagePack for C#, it is static and not extensible.

public static partial class MemoryPackFormatterProvider
{
    // for nongenerics methods
    static readonly ConcurrentDictionary<Type, IMemoryPackFormatter> formatters = new ConcurrentDictionary<Type, IMemoryPackFormatter>();

    public static bool IsRegistered<T>() => Check<T>.registered;

    public static void Register<T>(MemoryPackFormatter<T> formatter)
    {
        Check<T>.registered = true; // avoid to call Cache() constructor called.
        formatters[typeof(T)] = formatter;
        Cache<T>.formatter = formatter;
    }

    public static void Register<T>()
        where T : IMemoryPackFormatterRegister
    {
        T.RegisterFormatter();
    }

    // TODO: RegisterGenericFactory

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static MemoryPackFormatter<T> GetFormatter<T>()
    {
        return Cache<T>.formatter;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IMemoryPackFormatter GetFormatter(Type type)
    {
        if (formatters.TryGetValue(type, out var formatter))
        {
            return formatter;
        }

        ThrowHelper.ThrowNotRegisteredInProvider(type);
        return default;
    }

    static class Check<T>
    {
        public static bool registered;
    }

    static class Cache<T>
    {
        public static MemoryPackFormatter<T> formatter = default!;

        static Cache()
        {
            if (Check<T>.registered) return;

            try
            {
                var type = typeof(T);
                if (type.IsAssignableTo(typeof(IMemoryPackFormatterRegister)))
                {
                    // currently C# can not call like `if (T is IMemoryPackFormatterRegister) T.RegisterFormatter()`, so use reflection instead.
                    var m = type.GetMethod("MemoryPack.IMemoryPackFormatterRegister.RegisterFormatter", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                    m!.Invoke(null, null);
                    return;
                }

                var typeIsReferenceOrContainsReferences = RuntimeHelpers.IsReferenceOrContainsReferences<T>();
                var f = CreateFormatter(type, typeIsReferenceOrContainsReferences) as MemoryPackFormatter<T>;

                formatter = f ?? new ErrorMemoryPackFormatter<T>();
            }
            catch (Exception ex)
            {
                formatter = new ErrorMemoryPackFormatter<T>(ex);
            }

            formatters[typeof(T)] = formatter;
            Check<T>.registered = true;
        }
    }

    internal static object? CreateFormatter(Type type, bool typeIsReferenceOrContainsReferences)
    {
        // TODO: collections
        // TODO: error as error-formatter

        Type? instanceType = null;
        if (type.IsArray)
        {
            if (typeIsReferenceOrContainsReferences)
            {
                instanceType = typeof(UnmanagedArrayFormatter<>).MakeGenericType(type.GetElementType()!);
            }
        }

        instanceType = TryCreateTupleFormatterType(type);
        if (instanceType != null) goto CREATE;

        instanceType = TryCreateValueTupleFormatterType(type);
        if (instanceType != null) goto CREATE;

        return null;

    CREATE:
        return Activator.CreateInstance(instanceType);
    }
}

internal sealed class ErrorMemoryPackFormatter<T> : MemoryPackFormatter<T>
{
    readonly Exception? exception;

    public ErrorMemoryPackFormatter()
    {
        this.exception = null;
    }

    public ErrorMemoryPackFormatter(Exception exception)
    {
        this.exception = exception;
    }

    public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref T? value)
    {
        if (exception == null)
        {
            ThrowHelper.ThrowNotRegisteredInProvider(typeof(T));
        }
        else
        {
            ThrowHelper.ThrowRegisterInProviderFailed(typeof(T), exception);
        }
    }

    public override void Deserialize(ref MemoryPackReader reader, scoped ref T? value)
    {
        if (exception == null)
        {
            ThrowHelper.ThrowNotRegisteredInProvider(typeof(T));
        }
        else
        {
            ThrowHelper.ThrowRegisterInProviderFailed(typeof(T), exception);
        }
    }
}
