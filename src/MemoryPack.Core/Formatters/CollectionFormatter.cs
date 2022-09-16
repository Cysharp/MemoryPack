using MemoryPack.Internal;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MemoryPack.Formatters;

// T is not unmanaged
public sealed class CollectionFormatter<T> : IMemoryPackFormatter<IReadOnlyCollection<T?>>
{
    public void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref IReadOnlyCollection<T?>? value)
        where TBufferWriter : IBufferWriter<byte>
    {
        if (value == null)
        {
            writer.WriteNullLengthHeader();
            return;
        }

        if (!RuntimeHelpers.IsReferenceOrContainsReferences<T>())
        {
            if (value is T?[] array) // value is T[] ???
            {
                writer.DangerousWriteUnmanagedArray(array!); // nullable? ok?
                return;
            }
            else if (value is List<T?> list)
            {
                ReadOnlySpan<T> span = CollectionsMarshal.AsSpan(list);
                writer.DangerousWriteUnmanagedSpan<T>(span);
                return;
            }
        }

        writer.WriteLengthHeader(value.Count);
        if (value.Count != 0)
        {
            var formatter = MemoryPackFormatterProvider.GetFormatter<T>();
            foreach (var item in value)
            {
                var v = item;
                formatter.Serialize(ref writer, ref v);
            }
        }
    }

    public void Deserialize(ref MemoryPackReader reader, scoped ref IReadOnlyCollection<T?>? value)
    {
        if (!RuntimeHelpers.IsReferenceOrContainsReferences<T>())
        {
            value = reader.DangerousReadUnmanagedArray<T>();
            return;
        }





        if (!reader.TryReadLengthHeader(out var length))
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
        var formatter = MemoryPackFormatterProvider.GetFormatter<T>();// TODO:direct?
        var collection = new T?[length];
        for (int i = 0; i < length; i++)
        {
            // TODO: read item
            formatter.Deserialize(ref reader, ref collection[i]);
        }

        value = collection;
    }
}

public sealed class EnumerableFormatter<T> : IMemoryPackFormatter<IEnumerable<T>>
{
    public void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref IEnumerable<T>? value)
        where TBufferWriter : IBufferWriter<byte>
    {
        if (value == null)
        {
            writer.WriteNullLengthHeader();
            return;
        }

        if (TryGetNonEnumeratedCount(value, out var count))
        {
            writer.WriteLengthHeader(count);
            foreach (var item in value)
            {
                // TODO: write item
            }
        }
        else
        {
            var tempWriter = LinkedArrayBufferWriterPool.Rent();
            try
            {
                var tempContext = new MemoryPackWriter<LinkedArrayBufferWriter>(ref tempWriter);

                foreach (var item in value)
                {
                    // TODO: write item to tempContext
                }

                tempContext.Flush();

                writer.WriteLengthHeader(tempWriter.TotalWritten);
                tempWriter.WriteToAndReset(ref writer);
            }
            finally
            {
                tempWriter.Reset();
                LinkedArrayBufferWriterPool.Return(tempWriter);
            }
        }
    }

    public void Deserialize(ref MemoryPackReader reader, scoped ref IEnumerable<T>? value)
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

    public void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref Dictionary<TKey, TValue?>? value)
        where TBufferWriter : IBufferWriter<byte>
    {
        if (value == null)
        {
            writer.WriteNullLengthHeader();
            return;
        }

        writer.WriteLengthHeader(value.Count);

        var keyFormatter = MemoryPackFormatterProvider.GetFormatter<TKey>();
        var valueFormatter = MemoryPackFormatterProvider.GetFormatter<TValue?>();

        foreach (var item in value)
        {
            // TODO: which is best, var k = item.Key, Unsafe.AsRef....
            keyFormatter.Serialize(ref writer, ref Unsafe.AsRef(item.Key)!);
            valueFormatter.Serialize(ref writer, ref Unsafe.AsRef(item.Value));
        }
    }

    public void Deserialize(ref MemoryPackReader reader, scoped ref Dictionary<TKey, TValue?>? value)
    {
        if (!reader.TryReadLengthHeader(out var length))
        {
            value = null;
            return;
        }

        var keyFormatter = MemoryPackFormatterProvider.GetFormatter<TKey>();
        var valueFormatter = MemoryPackFormatterProvider.GetFormatter<TValue>();

        var dict = new Dictionary<TKey, TValue?>(length, equalityComparer);

        for (int i = 0; i < length; i++)
        {
            TKey? k = default;
            keyFormatter.Deserialize(ref reader, ref k);

            TValue? v = default;
            valueFormatter.Deserialize(ref reader, ref v);

            dict.Add(k!, v);
        }

        value = dict;
    }
}

public class ArrayFormatter<T> : IMemoryPackFormatter<T?[]>
{
    public void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref T?[]? value)
        where TBufferWriter : IBufferWriter<byte>
    {
        if (value == null)
        {
            writer.WriteNullLengthHeader();
            return;
        }

        var formatter = MemoryPackFormatterProvider.GetFormatter<T>();

        writer.WriteLengthHeader(value.Length);
        for (int i = 0; i < value.Length; i++)
        {
            formatter.Serialize(ref writer, ref value[i]);
        }
    }

    public void Deserialize(ref MemoryPackReader reader, scoped ref T?[]? value)
    {
        if (!reader.TryReadLengthHeader(out var length))
        {
            value = null;
            return;
        }

        var formatter = MemoryPackFormatterProvider.GetFormatter<T>();

        value = new T[length];
        for (int i = 0; i < length; i++)
        {
            formatter.Deserialize(ref reader, ref value[i]);
        }
    }
}

public sealed class ListFormatter<T> : IMemoryPackFormatter<List<T?>>
{
    public void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref List<T?>? value)
        where TBufferWriter : IBufferWriter<byte>
    {
        if (value == null)
        {
            writer.WriteNullLengthHeader();
            return;
        }

        var formatter = MemoryPackFormatterProvider.GetFormatter<T>();

        var span = CollectionsMarshal.AsSpan(value);
        writer.WriteLengthHeader(span.Length);
        for (int i = 0; i < span.Length; i++)
        {
            formatter.Serialize(ref writer, ref span[i]);
        }
    }

    public void Deserialize(ref MemoryPackReader reader, scoped ref List<T?>? value)
    {
        if (!reader.TryReadLengthHeader(out var length))
        {
            value = null;
            return;
        }

        if (value == null)
        {
            value = new List<T?>(length);
        }
        else
        {
            value.Clear();
        }

        var formatter = MemoryPackFormatterProvider.GetFormatter<T>();

        for (int i = 0; i < length; i++)
        {
            T? v = default;
            formatter.Deserialize(ref reader, ref v);
            value.Add(v);
        }
    }
}

public sealed class StackFormatter<T> : IMemoryPackFormatter<Stack<T?>>
{
    public void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref Stack<T?>? value)
        where TBufferWriter : IBufferWriter<byte>
    {
        if (value == null)
        {
            writer.WriteNullLengthHeader();
            return;
        }


        // TODO:reverse order?
        var formatter = MemoryPackFormatterProvider.GetFormatter<T>();

        writer.WriteLengthHeader(value.Count);
        foreach (var item in value)
        {
            var v = item;
            formatter.Serialize(ref writer, ref v);
        }
    }

    public void Deserialize(ref MemoryPackReader reader, scoped ref Stack<T?>? value)
    {
        throw new NotImplementedException();
    }
}


public class UnmanagedTypeArrayFormatter<T> : IMemoryPackFormatter<T[]>
    where T : unmanaged
{
    public void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref T[]? value)
        where TBufferWriter : IBufferWriter<byte>
    {
        writer.WriteUnmanagedArray(value);
    }

    public void Deserialize(ref MemoryPackReader reader, scoped ref T[]? value)
    {
        // TODO:use ref and override.
        value = reader.ReadUnmanagedArray<T>();
    }
}

public class DangerousUnmanagedTypeArrayFormatter<T> : IMemoryPackFormatter<T[]>
{
    public void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref T[]? value)
        where TBufferWriter : IBufferWriter<byte>
    {
        writer.DangerousWriteUnmanagedArray(value);
    }

    public void Deserialize(ref MemoryPackReader reader, scoped ref T[]? value)
    {
        value = reader.DangerousReadUnmanagedArray<T>();
    }
}
