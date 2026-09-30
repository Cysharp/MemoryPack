#if NET7_0_OR_GREATER

using MemoryPack.Formatters;
using MemoryPack.Internal;
using System.Buffers;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MemoryPack;

/// <summary>
/// Owns the formatter graph used by source-generated context serialization.
/// A context never falls back to <see cref="MemoryPackFormatterProvider"/> for
/// a formatter that is missing from its generated graph.
/// </summary>
public abstract class MemoryPackSerializerContext
{
    Dictionary<Type, IMemoryPackFormatter>? formatters;

    public MemoryPackSerializerOptions Options { get; }

    protected MemoryPackSerializerContext(MemoryPackSerializerOptions? options = null)
    {
        Options = options ?? MemoryPackSerializerOptions.Default;
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    protected void Initialize(MemoryPackSerializerContextBuilder builder)
    {
        if (formatters is not null)
        {
            throw new InvalidOperationException("The serializer context has already been initialized.");
        }

        formatters = builder.Build();
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    protected MemoryPackFormatter<T> GetGeneratedFormatter<T>()
    {
        if (formatters is null)
        {
            throw new InvalidOperationException("The serializer context has not been initialized.");
        }

        if (formatters.TryGetValue(typeof(T), out var formatter))
        {
            return (MemoryPackFormatter<T>)formatter;
        }

        MemoryPackSerializationException.ThrowMessage($"Type is not registered in this MemoryPackSerializerContext: {typeof(T).FullName}");
        return null!;
    }

    public IMemoryPackFormatter GetFormatter(Type type)
    {
        if (formatters is null)
        {
            throw new InvalidOperationException("The serializer context has not been initialized.");
        }

        if (formatters.TryGetValue(type, out var formatter))
        {
            return formatter;
        }

        MemoryPackSerializationException.ThrowMessage($"Type is not registered in this MemoryPackSerializerContext: {type.FullName}");
        return null!;
    }

    public byte[] Serialize(Type type, object? value)
    {
        var bufferWriter = new ReusableLinkedArrayBufferWriter(useFirstBuffer: true, pinned: false);
        var optionalState = MemoryPackWriterOptionalStatePool.Rent(Options);
        try
        {
            var writer = new MemoryPackWriter<ReusableLinkedArrayBufferWriter>(ref bufferWriter, bufferWriter.DangerousGetFirstBuffer(), optionalState);
            GetFormatter(type).Serialize(ref writer, ref value);
            writer.Flush();
            return bufferWriter.ToArrayAndReset();
        }
        finally
        {
            bufferWriter.Reset();
            ((IDisposable)optionalState).Dispose();
        }
    }

    public object? Deserialize(Type type, ReadOnlySpan<byte> buffer)
    {
        var optionalState = MemoryPackReaderOptionalStatePool.Rent(Options);
        var reader = new MemoryPackReader(buffer, optionalState);
        object? value = null;
        try
        {
            GetFormatter(type).Deserialize(ref reader, ref value);
            return value;
        }
        finally
        {
            reader.Dispose();
            ((IDisposable)optionalState).Dispose();
        }
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static MemoryPackFormatter<T> GetFormatter<TContext, T>(TContext context)
        where TContext : MemoryPackSerializerContext, IMemoryPackSerializerContext<TContext, T>
    {
        return TContext.GetFormatter(context);
    }
}

[EditorBrowsable(EditorBrowsableState.Never)]
public interface IMemoryPackSerializerContext<TContext, T>
    where TContext : MemoryPackSerializerContext, IMemoryPackSerializerContext<TContext, T>
{
    static abstract MemoryPackFormatter<T> GetFormatter(TContext context);

    static abstract void Serialize<TBufferWriter>(TContext context, ref MemoryPackWriter<TBufferWriter> writer, scoped ref T? value)
        where TBufferWriter : IBufferWriter<byte>;

    static abstract void Deserialize(TContext context, ref MemoryPackReader reader, scoped ref T? value);
}

[EditorBrowsable(EditorBrowsableState.Never)]
public interface IMemoryPackSerializerContextFormatterRegister<T>
{
    static abstract void RegisterFormatter(MemoryPackSerializerContextBuilder builder);
}

[EditorBrowsable(EditorBrowsableState.Never)]
public interface IMemoryPackSerializerContextFormatterFactory<T>
{
    static abstract MemoryPackFormatter<T> CreateFormatter(MemoryPackSerializerContextBuilder builder);
}

/// <summary>
/// Construction-only registry used by generated code. The completed registry is
/// owned by one serializer context instance.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class MemoryPackSerializerContextBuilder
{
    Dictionary<Type, IMemoryPackFormatter>? formatters = new();
    readonly Dictionary<string, Type> typeNames = new(StringComparer.Ordinal);

    public bool BeginFormatter<T>()
    {
        EnsureMutable();
        AddType(typeof(T));
        if (formatters!.ContainsKey(typeof(T)))
        {
            return false;
        }

        formatters.Add(typeof(T), new MemoryPackFormatterSlot<T>());
        return true;
    }

    public void CompleteFormatter<T>(MemoryPackFormatter<T> formatter)
    {
        EnsureMutable();
        if (!formatters!.TryGetValue(typeof(T), out var current))
        {
            throw new InvalidOperationException($"Formatter construction was not started for {typeof(T).FullName}.");
        }

        if (current is MemoryPackFormatterSlot<T> slot)
        {
            slot.SetFormatter(formatter);
        }

        formatters[typeof(T)] = formatter;
    }

    public void RegisterFormatter<T>(MemoryPackFormatter<T> formatter)
    {
        EnsureMutable();
        AddType(typeof(T));
        formatters!.TryAdd(typeof(T), formatter);
    }

    public void RegisterUnmanaged<T>()
        where T : unmanaged
    {
        RegisterFormatter<T>(new UnmanagedFormatter<T>());
    }

    public void RegisterDangerousUnmanaged<T>()
    {
        RegisterFormatter<T>(new DangerousUnmanagedFormatter<T>());
    }

    public void RegisterString()
    {
        RegisterFormatter<string>(StringFormatter.Default);
    }

    public void RegisterType()
    {
        RegisterFormatter<Type>(new ContextTypeFormatter(typeNames));
    }

    public void RegisterNullable<T>()
        where T : struct
    {
        if (!Contains<T?>())
        {
            RegisterFormatter<T?>(new ContextNullableFormatter<T>(GetFormatter<T>()));
        }
    }

    public void RegisterMemoryPackable<T>()
        where T : IMemoryPackSerializerContextFormatterRegister<T>
    {
        T.RegisterFormatter(this);
    }

    public void RegisterFactory<T, TFactory>()
        where TFactory : IMemoryPackSerializerContextFormatterFactory<T>
    {
        if (!BeginFormatter<T>())
        {
            return;
        }

        CompleteFormatter<T>(TFactory.CreateFormatter(this));
    }

    public MemoryPackFormatter<T> GetFormatter<T>()
    {
        EnsureMutable();
        if (formatters!.TryGetValue(typeof(T), out var formatter))
        {
            return (MemoryPackFormatter<T>)formatter;
        }

        throw new InvalidOperationException($"The generated formatter graph did not register {typeof(T).FullName}.");
    }

    public void RegisterArray<T>()
    {
        if (!Contains<T?[]>())
        {
            RegisterFormatter<T?[]>(new ContextArrayFormatter<T>(GetFormatter<T>()));
        }
    }

    public void RegisterTwoDimensionalArray<T>()
    {
        if (!Contains<T?[,]>()) RegisterFormatter<T?[,]>(new ContextTwoDimensionalArrayFormatter<T>(GetFormatter<T>()));
    }

    public void RegisterThreeDimensionalArray<T>()
    {
        if (!Contains<T?[,,]>()) RegisterFormatter<T?[,,]>(new ContextThreeDimensionalArrayFormatter<T>(GetFormatter<T>()));
    }

    public void RegisterFourDimensionalArray<T>()
    {
        if (!Contains<T?[,,,]>()) RegisterFormatter<T?[,,,]>(new ContextFourDimensionalArrayFormatter<T>(GetFormatter<T>()));
    }

    public void RegisterList<T>()
    {
        if (!Contains<List<T?>>())
        {
            RegisterFormatter<List<T?>>(new ContextListFormatter<T>(GetFormatter<T>()));
        }
    }

    public void RegisterEnumerable<TCollection, TElement>(
        Func<IEnumerable<TElement?>, TCollection> materialize,
        bool reverse = false)
        where TCollection : IEnumerable<TElement?>
    {
        if (!Contains<TCollection>())
        {
            RegisterFormatter<TCollection>(new ContextEnumerableFormatter<TCollection, TElement>(GetFormatter<TElement>(), materialize, reverse));
        }
    }

    public void RegisterMap<TMap, TKey, TValue>(Func<IEnumerable<KeyValuePair<TKey, TValue?>>, TMap> materialize)
        where TKey : notnull
        where TMap : IEnumerable<KeyValuePair<TKey, TValue?>>
    {
        if (!Contains<TMap>())
        {
            RegisterFormatter<TMap>(new ContextMapFormatter<TMap, TKey, TValue>(GetFormatter<TKey>(), GetFormatter<TValue>(), materialize));
        }
    }

    public void RegisterPriorityQueue<TElement, TPriority>()
    {
        RegisterFormatter<PriorityQueue<TElement?, TPriority?>>(new ContextPriorityQueueFormatter<TElement, TPriority>(GetFormatter<TElement>(), GetFormatter<TPriority>()));
    }

    public void RegisterGrouping<TKey, TElement>()
        where TKey : notnull
    {
        RegisterFormatter<IGrouping<TKey, TElement>>(new ContextGroupingFormatter<TKey, TElement>(GetFormatter<TKey>(), GetFormatter<TElement>()));
    }

    public void RegisterLookup<TKey, TElement>()
        where TKey : notnull
    {
        RegisterFormatter<ILookup<TKey, TElement>>(new ContextLookupFormatter<TKey, TElement>(GetFormatter<TKey>(), GetFormatter<TElement>()));
    }

    public void RegisterImmutableArray<T>()
    {
        RegisterFormatter<ImmutableArray<T?>>(new ContextImmutableArrayFormatter<T>(GetFormatter<T>()));
    }

    public void RegisterArraySegment<T>() => RegisterFormatter<ArraySegment<T?>>(new ContextArraySegmentFormatter<T>(GetFormatter<T>()));

    public void RegisterMemory<T>() => RegisterFormatter<Memory<T?>>(new ContextMemoryFormatter<T>(GetFormatter<T>()));

    public void RegisterReadOnlyMemory<T>() => RegisterFormatter<ReadOnlyMemory<T?>>(new ContextReadOnlyMemoryFormatter<T>(GetFormatter<T>()));

    public void RegisterReadOnlySequence<T>() => RegisterFormatter<ReadOnlySequence<T?>>(new ContextReadOnlySequenceFormatter<T>(GetFormatter<T>()));

    public void RegisterKeyValuePair<TKey, TValue>()
        => RegisterFormatter<KeyValuePair<TKey?, TValue?>>(new ContextKeyValuePairFormatter<TKey, TValue>(GetFormatter<TKey>(), GetFormatter<TValue>()));

    public void RegisterLazy<T>() => RegisterFormatter<Lazy<T?>>(new ContextLazyFormatter<T>(GetFormatter<T>()));

    public void RegisterDictionary<TKey, TValue>()
        where TKey : notnull
    {
        if (!Contains<Dictionary<TKey, TValue?>>())
        {
            RegisterFormatter<Dictionary<TKey, TValue?>>(new ContextDictionaryFormatter<TKey, TValue>(GetFormatter<TKey>(), GetFormatter<TValue>()));
        }
    }

    public void RegisterCollection<TCollection, TElement>()
        where TCollection : ICollection<TElement?>, new()
    {
        if (!Contains<TCollection>())
        {
            RegisterFormatter<TCollection>(new ContextGenericCollectionFormatter<TCollection, TElement>(GetFormatter<TElement>()));
        }
    }

    public void RegisterSet<TSet, TElement>()
        where TSet : ISet<TElement?>, new()
    {
        if (!Contains<TSet>())
        {
            RegisterFormatter<TSet>(new ContextGenericSetFormatter<TSet, TElement>(GetFormatter<TElement>()));
        }
    }

    public void RegisterDictionary<TDictionary, TKey, TValue>()
        where TKey : notnull
        where TDictionary : IDictionary<TKey, TValue?>, new()
    {
        if (!Contains<TDictionary>())
        {
            RegisterFormatter<TDictionary>(new ContextGenericDictionaryFormatter<TDictionary, TKey, TValue>(GetFormatter<TKey>(), GetFormatter<TValue>()));
        }
    }

    public bool Contains<T>()
    {
        EnsureMutable();
        return formatters!.ContainsKey(typeof(T));
    }

    internal Dictionary<Type, IMemoryPackFormatter> Build()
    {
        EnsureMutable();
        var result = formatters!;
        foreach (var formatter in result.Values)
        {
            if (formatter is IMemoryPackFormatterSlot slot && !slot.IsInitialized)
            {
                throw new InvalidOperationException("A recursive formatter slot was not initialized.");
            }
        }

        formatters = null;
        return result;
    }

    void EnsureMutable()
    {
        if (formatters is null)
        {
            throw new InvalidOperationException("The serializer context builder has already been consumed.");
        }
    }

    void AddType(Type type)
    {
        var name = ContextTypeFormatter.GetWireName(type);
        if (typeNames.TryGetValue(name, out var existing) && existing != type)
        {
            throw new InvalidOperationException($"The serializer context contains two types with the same wire name: {name}.");
        }

        typeNames[name] = type;
    }
}

interface IMemoryPackFormatterSlot
{
    bool IsInitialized { get; }
}

sealed class MemoryPackFormatterSlot<T> : MemoryPackFormatter<T>, IMemoryPackFormatterSlot
{
    MemoryPackFormatter<T>? formatter;

    public bool IsInitialized => formatter is not null;

    public void SetFormatter(MemoryPackFormatter<T> value)
    {
        formatter = value;
    }

    public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref T? value)
    {
        formatter!.Serialize(ref writer, ref value);
    }

    public override void Deserialize(ref MemoryPackReader reader, scoped ref T? value)
    {
        formatter!.Deserialize(ref reader, ref value);
    }
}

#endif
