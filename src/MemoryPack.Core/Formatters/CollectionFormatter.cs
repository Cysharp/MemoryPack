using MemoryPack.Internal;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MemoryPack.Formatters;

// T is not unmanaged
public sealed class CollectionFormatter<T> : IMemoryPackFormatter<IReadOnlyCollection<T?>>
{
    public void Serialize<TBufferWriter>(ref SerializationContext<TBufferWriter> context, ref IReadOnlyCollection<T?>? value)
        where TBufferWriter : IBufferWriter<byte>
    {
        if (value == null)
        {
            context.WriteNullLengthHeader();
            return;
        }

        if (!RuntimeHelpers.IsReferenceOrContainsReferences<T>())
        {
            if (value is T?[] array) // value is T[] ???
            {
                context.DangerousWriteUnmanagedArray(ref array!); // nullable? ok?
                return;
            }
            else if (value is List<T?> list)
            {
                ReadOnlySpan<T> span = CollectionsMarshal.AsSpan(list);
                context.DangerousWriteUnmanagedSpan<T>(ref span);
                return;
            }
        }

        context.WriteLengthHeader(value.Count);
        if (value.Count != 0)
        {
            var formatter = MemoryPackFormatterProvider.GetRequiredFormatter<T>();
            foreach (var item in value)
            {
                var v = item;
                formatter.Serialize(ref context, ref v);
            }
        }
    }

    public void Deserialize(ref DeserializationContext context, ref IReadOnlyCollection<T?>? value)
    {
        if (!RuntimeHelpers.IsReferenceOrContainsReferences<T>())
        {
            value = context.DangerousReadUnmanagedArray<T>();
            return;
        }





        if (!context.TryReadLength(out var length))
        {
            value = null;
            return;
        }

        if (length == 0)
        {
            value = Array.Empty<T>();
            return;
        }

        // context.TryReadUnmanagedSpan<

        // TODO: security check
        var formatter = MemoryPackFormatterProvider.GetRequiredFormatter<T>();// TODO:direct?
        var collection = new T?[length];
        for (int i = 0; i < length; i++)
        {
            // TODO: read item
            formatter.Deserialize(ref context, ref collection[i]);
        }

        value = collection;
    }
}

public sealed class EnumerableFormatter<T> : IMemoryPackFormatter<IEnumerable<T>>
{
    public void Serialize<TBufferWriter>(ref SerializationContext<TBufferWriter> context, ref IEnumerable<T>? value)
        where TBufferWriter : IBufferWriter<byte>
    {
        if (value == null)
        {
            context.WriteNullLengthHeader();
            return;
        }

        if (TryGetNonEnumeratedCount(value, out var count))
        {
            context.WriteLengthHeader(count);
            foreach (var item in value)
            {
                // TODO: write item
            }
        }
        else
        {
            var tempWriter = SequentialBufferWriterPool.Rent();
            try
            {
                var tempContext = new SerializationContext<SequentialBufferWriter>(tempWriter);

                foreach (var item in value)
                {
                    // TODO: write item to tempContext
                }

                tempContext.Flush();

                context.WriteLengthHeader(tempWriter.TotalWritten);
                tempWriter.WriteToAndReset(ref context);
            }
            finally
            {
                tempWriter.Reset();
                SequentialBufferWriterPool.Return(tempWriter);
            }
        }
    }

    public void Deserialize(ref DeserializationContext context, ref IEnumerable<T>? value)
    {
        // TODO:...
        throw new NotImplementedException();
    }

    static bool TryGetNonEnumeratedCount(IEnumerable<T> value, out int count)
    {
        // TryGetNonEnumeratedCount is not check IReadOnlyCollection<T> so add check manually.
        // https://github.com/dotnet/runtime/issues/54764

        if (value.TryGetNonEnumeratedCount(out count))
        {
            return true;
        }

        if (value is IReadOnlyCollection<T> readOnlyCollection)
        {
            count = readOnlyCollection.Count;
            return true;
        }

        return false;
    }
}

public class DictionaryFormatter<TKey, TValue> : IMemoryPackFormatter<Dictionary<TKey, TValue?>>
    where TKey : notnull
{
    IEqualityComparer<TKey>? equalityComparer;

    public DictionaryFormatter()
    {

    }

    public DictionaryFormatter(IEqualityComparer<TKey> equalityComparer)
    {
        this.equalityComparer = equalityComparer;
    }

    public void Serialize<TBufferWriter>(ref SerializationContext<TBufferWriter> context, ref Dictionary<TKey, TValue?>? value)
        where TBufferWriter : IBufferWriter<byte>
    {
        if (value == null)
        {
            context.WriteNullLengthHeader();
            return;
        }

        context.WriteLengthHeader(value.Count);

        var keyFormatter = MemoryPackFormatterProvider.GetRequiredFormatter<TKey>();
        var valueFormatter = MemoryPackFormatterProvider.GetRequiredFormatter<TValue?>();

        foreach (var item in value)
        {
            // TODO: which is best, var k = item.Key, Unsafe.AsRef....
            keyFormatter.Serialize(ref context, ref Unsafe.AsRef(item.Key)!);
            valueFormatter.Serialize(ref context, ref Unsafe.AsRef(item.Value));
        }
    }

    public void Deserialize(ref DeserializationContext context, ref Dictionary<TKey, TValue?>? value)
    {
        if (!context.TryReadLength(out var length))
        {
            value = null;
            return;
        }

        var keyFormatter = MemoryPackFormatterProvider.GetRequiredFormatter<TKey>();
        var valueFormatter = MemoryPackFormatterProvider.GetRequiredFormatter<TValue>();

        var dict = new Dictionary<TKey, TValue?>(length, equalityComparer);

        for (int i = 0; i < length; i++)
        {
            TKey? k = default;
            keyFormatter.Deserialize(ref context, ref k);

            TValue? v = default;
            valueFormatter.Deserialize(ref context, ref v);

            dict.Add(k!, v);
        }

        value = dict;
    }
}

public class ArrayFormatter<T> : IMemoryPackFormatter<T[]>
{
    public void Deserialize(ref DeserializationContext context, ref T[]? value)
    {
        throw new NotImplementedException();
    }

    public void Serialize<TBufferWriter>(ref SerializationContext<TBufferWriter> context, ref T[]? value) where TBufferWriter : IBufferWriter<byte>
    {
        throw new NotImplementedException();
    }
}

public class UnmanagedTypeArrayFormatter<T> : IMemoryPackFormatter<T[]>
    where T : unmanaged
{
    public void Serialize<TBufferWriter>(ref SerializationContext<TBufferWriter> context, ref T[]? value)
        where TBufferWriter : IBufferWriter<byte>
    {
        context.WriteUnmanagedArray(ref value);
    }

    public void Deserialize(ref DeserializationContext context, ref T[]? value)
    {
        value = context.ReadUnmanagedArray<T>();
    }
}

public class DangerousUnmanagedTypeArrayFormatter<T> : IMemoryPackFormatter<T[]>
{
    public void Serialize<TBufferWriter>(ref SerializationContext<TBufferWriter> context, ref T[]? value)
        where TBufferWriter : IBufferWriter<byte>
    {
        context.DangerousWriteUnmanagedArray(ref value);
    }

    public void Deserialize(ref DeserializationContext context, ref T[]? value)
    {
        value = context.DangerousReadUnmanagedArray<T>();
    }
}
