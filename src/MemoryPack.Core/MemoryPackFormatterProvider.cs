using MemoryPack.Formatters;
using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace MemoryPack;

// This service provider is not extension point, for wellknown types
// and fallback if can't resolve in compile time(like generics).
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
                    m!.Invoke(null, null); // Cache<T>.formatter will set from method
                    return;
                }

                var typeIsReferenceOrContainsReferences = RuntimeHelpers.IsReferenceOrContainsReferences<T>();
                var f = CreateGenericFormatter(type, typeIsReferenceOrContainsReferences) as MemoryPackFormatter<T>;

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

    internal static object? CreateGenericFormatter(Type type, bool typeIsReferenceOrContainsReferences)
    {
        Type? formatterType = null;

        if (type.IsArray)
        {
            if (type.IsSZArray)
            {
                if (!typeIsReferenceOrContainsReferences)
                {
                    formatterType = typeof(UnmanagedArrayFormatter<>).MakeGenericType(type.GetElementType()!);
                    goto CREATE;
                }
                else
                {
                    formatterType = typeof(ArrayFormatter<>).MakeGenericType(type.GetElementType()!);
                    goto CREATE;
                }
            }
            else
            {
                var rank = type.GetArrayRank();
                switch (rank)
                {
                    case 2:
                        formatterType = typeof(TwoDimentionalArrayFormatter<>).MakeGenericType(type.GetElementType()!);
                        goto CREATE;
                    case 3:
                        formatterType = typeof(ThreeDimentionalArrayFormatter<>).MakeGenericType(type.GetElementType()!);
                        goto CREATE;
                    case 4:
                        formatterType = typeof(FourDimentionalArrayFormatter<>).MakeGenericType(type.GetElementType()!);
                        goto CREATE;
                    default:
                        return null; // not supported
                }
            }
        }

        if (type.IsEnum || !typeIsReferenceOrContainsReferences)
        {
            formatterType = typeof(UnmanagedFormatter<>).MakeGenericType(type);
            goto CREATE;
        }

        formatterType = TryCreateGenericFormatterType(type, TupleFormatterTypes.TupleFormatters);
        if (formatterType != null) goto CREATE;

        formatterType = TryCreateGenericFormatterType(type, ArrayLikeFormatters);
        if (formatterType != null) goto CREATE;

        formatterType = TryCreateGenericFormatterType(type, CollectionFormatters);
        if (formatterType != null) goto CREATE;

        formatterType = TryCreateGenericFormatterType(type, ImmutableCollectionFormatters);
        if (formatterType != null) goto CREATE;

        formatterType = TryCreateGenericFormatterType(type, InterfaceCollectionFormatters);
        if (formatterType != null) goto CREATE;

        // Can't resolve formatter, return null(will create ErrorMemoryPackFormatter<T>).
        return null;

    CREATE:
        return Activator.CreateInstance(formatterType);
    }

    static Type? TryCreateGenericFormatterType(Type type, Dictionary<Type, Type> knownTypes)
    {
        if (type.IsGenericType)
        {
            var genericDefinition = type.GetGenericTypeDefinition();

            if (knownTypes.TryGetValue(genericDefinition, out var formatterType))
            {
                return formatterType.MakeGenericType(type.GetGenericArguments());
            }
        }

        return null;
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
