using MemoryPack.Internal;
using System.Buffers;
using System.Runtime.InteropServices;

namespace MemoryPack.Formatters;

// T is not unmanaged
public sealed class CollectionFormatter<T> : IMemoryPackFormatter<IReadOnlyCollection<T>>
{
    public void Serialize<TBufferWriter>(ref SerializationContext<TBufferWriter> context, ref IReadOnlyCollection<T>? value)
        where TBufferWriter : IBufferWriter<byte>
    {
        if (value == null)
        {
            context.WriteNullLength();
            return;
        }

        context.WriteLength(value.Count);
        foreach (var item in value)
        {
            // TODO: write item
        }
    }

    public void Deserialize(ref DeserializationContext context, ref IReadOnlyCollection<T>? value)
    {
        if (!context.TryReadLength(out var length))
        {
            value = null;
            return;
        }

        // TODO: security check
        var collection = new T[length];
        for (int i = 0; i < length; i++)
        {
            // TODO: read item
            // collection[i] = 
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
            context.WriteNullLength();
            return;
        }

        if (TryGetNonEnumeratedCount(value, out var count))
        {
            context.WriteLength(count);
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

                context.WriteLength(tempWriter.TotalWritten);
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
