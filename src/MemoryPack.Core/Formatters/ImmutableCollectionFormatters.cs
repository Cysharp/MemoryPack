using MemoryPack.Formatters;
using MemoryPack.Internal;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

// Immutable Collections formatters

namespace MemoryPack
{
    public static partial class MemoryPackFormatterProvider
    {
        static readonly Dictionary<Type, Type> ImmutableCollectionFormatters = new Dictionary<Type, Type>()
        {
            { typeof(ImmutableArray<>), typeof(ImmutableArrayFormatter<>) },
            { typeof(ImmutableList<>), typeof(ImmutableListFormatter<>) },
            { typeof(ImmutableQueue<>), typeof(ImmutableQueueFormatter<>) },
            { typeof(ImmutableStack<>), typeof(ImmutableStackFormatter<>) },
            { typeof(ImmutableDictionary<,>), typeof(ImmutableDictionaryFormatter<,>) },
            { typeof(ImmutableSortedDictionary<,>), typeof(ImmutableSortedDictionaryFormatter<,>) },
            { typeof(ImmutableSortedSet<>), typeof(ImmutableSortedSetFormatter<>) },
            { typeof(ImmutableHashSet<>), typeof(ImmutableHashSetFormatter<>) },
            { typeof(IImmutableList<>), typeof(InterfaceImmutableListFormatter<>) },
            { typeof(IImmutableQueue<>), typeof(InterfaceImmutableQueueFormatter<>) },
            { typeof(IImmutableStack<>), typeof(InterfaceImmutableStackFormatter<>) },
            { typeof(IImmutableDictionary<,>), typeof(InterfaceImmutableDictionaryFormatter<,>) },
            { typeof(IImmutableSet<>), typeof(InterfaceImmutableSetFormatter<>) },
        };
    }
}

namespace MemoryPack.Formatters
{
    public sealed class ImmutableArrayFormatter<T> : MemoryPackFormatter<ImmutableArray<T?>>
    {
        public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref ImmutableArray<T?> value)
        {
            if (value.IsDefault)
            {
                writer.WriteNullCollectionHeader();
            }
            else
            {
                writer.WriteSpan(value.AsSpan());
            }
        }

        public override void Deserialize(ref MemoryPackReader reader, scoped ref ImmutableArray<T?> value)
        {
            var array = reader.ReadArray<T?>();
            if (array == null)
            {
                value = default;
                return;
            }

            if (array.Length == 0)
            {
                value = ImmutableArray<T?>.Empty;
                return;
            }

            // create Empty and replace inner T[] field(avoid defensive copy of Create)
            value = ImmutableArray.Create<T?>();
            ref var view = ref Unsafe.As<ImmutableArray<T?>, ImmutableArrayView<T?>>(ref value);
            view.array = array;
        }
    }

    internal struct ImmutableArrayView<T>
    {
        public T[]? array;
    }

    public sealed class ImmutableListFormatter<T> : MemoryPackFormatter<ImmutableList<T?>>
    {
        public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref ImmutableList<T?>? value)
        {
            if (value == null)
            {
                writer.WriteNullCollectionHeader();
                return;
            }

            var formatter = writer.GetFormatter<T?>();
            writer.WriteCollectionHeader(value.Count);
            foreach (var item in value)
            {
                var v = item;
                formatter.Serialize(ref writer, ref v);
            }
        }

        public override void Deserialize(ref MemoryPackReader reader, scoped ref ImmutableList<T?>? value)
        {
            if (!reader.TryReadCollectionHeader(out var length))
            {
                value = null;
                return;
            }

            if (length == 0)
            {
                value = ImmutableList<T?>.Empty;
                return;
            }

            if (length == 1)
            {
                var item = reader.ReadValue<T>();
                value = ImmutableList.Create(item);
                return;
            }

            var formatter = reader.GetFormatter<T?>();

            var builder = ImmutableList.CreateBuilder<T?>();
            for (int i = 0; i < length; i++)
            {
                T? item = default;
                formatter.Deserialize(ref reader, ref item);
                builder.Add(item);
            }

            value = builder.ToImmutable();
        }
    }

    public sealed class ImmutableQueueFormatter<T> : MemoryPackFormatter<ImmutableQueue<T?>>
    {
        public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref ImmutableQueue<T?>? value)
        {
            if (value == null)
            {
                writer.WriteNullCollectionHeader();
                return;
            }

            // ImmutableQueue<T> has no Count, so use similar serialization of IEnumerable<T>

            var tempBuffer = ReusableLinkedArrayBufferWriterPool.Rent();
            try
            {
                var tempWriter = new MemoryPackWriter<ReusableLinkedArrayBufferWriter>(ref tempBuffer, writer.Options);

                var count = 0;
                var formatter = writer.GetFormatter<T?>();
                foreach (var item in value)
                {
                    count++;
                    var v = item;
                    formatter.Serialize(ref tempWriter, ref v);
                }

                tempWriter.Flush();

                // write to parameter writer.
                writer.WriteCollectionHeader(count);
                tempBuffer.WriteToAndReset(ref writer);
            }
            finally
            {
                ReusableLinkedArrayBufferWriterPool.Return(tempBuffer);
            }
        }

        public override void Deserialize(ref MemoryPackReader reader, scoped ref ImmutableQueue<T?>? value)
        {
            if (!reader.TryReadCollectionHeader(out var length))
            {
                value = null;
                return;
            }

            if (length == 0)
            {
                value = ImmutableQueue<T?>.Empty;
                return;
            }

            if (length == 1)
            {
                var item = reader.ReadValue<T>();
                value = ImmutableQueue.Create(item);
                return;
            }

            // ImmutableQueue<T> has no builder

            var rentArray = ArrayPool<T?>.Shared.Rent(length);
            try
            {
                var formatter = reader.GetFormatter<T?>();
                for (int i = 0; i < length; i++)
                {
                    formatter.Deserialize(ref reader, ref rentArray[i]);
                }

                if (rentArray.Length == length)
                {
                    // we can use T[] ctor
                    value = ImmutableQueue.Create(rentArray);
                    return;
                }
                else
                {
                    // IEnumerable<T> method
                    value = ImmutableQueue.CreateRange((new ArraySegment<T?>(rentArray, 0, length)).AsEnumerable());
                }
            }
            finally
            {
                ArrayPool<T?>.Shared.Return(rentArray, clearArray: RuntimeHelpers.IsReferenceOrContainsReferences<T>());
            }
        }
    }

    public sealed class ImmutableStackFormatter<T> : MemoryPackFormatter<ImmutableStack<T?>>
    {
        public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref ImmutableStack<T?>? value)
        {
            if (value == null)
            {
                writer.WriteNullCollectionHeader();
                return;
            }

            // ImmutableStack<T> has no Count, so use similar serialization of IEnumerable<T>

            var tempBuffer = ReusableLinkedArrayBufferWriterPool.Rent();
            try
            {
                var tempWriter = new MemoryPackWriter<ReusableLinkedArrayBufferWriter>(ref tempBuffer, writer.Options);

                var count = 0;
                var formatter = writer.GetFormatter<T?>();
                
                foreach (var item in value.AsEnumerable().Reverse()) // serialize reverse order
                {
                    count++;
                    var v = item;
                    formatter.Serialize(ref tempWriter, ref v);
                }

                tempWriter.Flush();

                // write to parameter writer.
                writer.WriteCollectionHeader(count);
                tempBuffer.WriteToAndReset(ref writer);
            }
            finally
            {
                ReusableLinkedArrayBufferWriterPool.Return(tempBuffer);
            }
        }

        public override void Deserialize(ref MemoryPackReader reader, scoped ref ImmutableStack<T?>? value)
        {
            if (!reader.TryReadCollectionHeader(out var length))
            {
                value = null;
                return;
            }

            if (length == 0)
            {
                value = ImmutableStack<T?>.Empty;
                return;
            }

            if (length == 1)
            {
                var item = reader.ReadValue<T>();
                value = ImmutableStack.Create(item);
                return;
            }

            // ImmutableStack<T> has no builder

            var rentArray = ArrayPool<T?>.Shared.Rent(length);
            try
            {
                var formatter = reader.GetFormatter<T?>();
                for (int i = 0; i < length; i++)
                {
                    formatter.Deserialize(ref reader, ref rentArray[i]);
                }

                if (rentArray.Length == length)
                {
                    // we can use T[] ctor
                    value = ImmutableStack.Create(rentArray);
                    return;
                }
                else
                {
                    // IEnumerable<T> method
                    value = ImmutableStack.CreateRange((new ArraySegment<T?>(rentArray, 0, length)).AsEnumerable());
                }
            }
            finally
            {
                ArrayPool<T?>.Shared.Return(rentArray, clearArray: RuntimeHelpers.IsReferenceOrContainsReferences<T>());
            }
        }
    }

    public sealed class ImmutableDictionaryFormatter<TKey, TValue> : MemoryPackFormatter<ImmutableDictionary<TKey, TValue?>?>
        where TKey : notnull
    {
        static ImmutableDictionaryFormatter()
        {
            if (!MemoryPackFormatterProvider.IsRegistered<KeyValuePair<TKey, TValue?>>())
            {
                MemoryPackFormatterProvider.Register(new KeyValuePairFormatter<TKey, TValue?>());
            }
        }

        public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref ImmutableDictionary<TKey, TValue?>? value)
        {
            if (value == null)
            {
                writer.WriteNullCollectionHeader();
                return;
            }

            var formatter = writer.GetFormatter<KeyValuePair<TKey, TValue?>>();
            writer.WriteCollectionHeader(value.Count);
            foreach (var item in value)
            {
                var v = item;
                formatter.Serialize(ref writer, ref v);
            }
        }

        public override void Deserialize(ref MemoryPackReader reader, scoped ref ImmutableDictionary<TKey, TValue?>? value)
        {
            if (!reader.TryReadCollectionHeader(out var length))
            {
                value = null;
                return;
            }

            if (length == 0)
            {
                value = ImmutableDictionary<TKey, TValue?>.Empty;
                return;
            }

            var formatter = reader.GetFormatter<KeyValuePair<TKey, TValue?>>();

            var builder = ImmutableDictionary.CreateBuilder<TKey, TValue?>();
            for (int i = 0; i < length; i++)
            {
                KeyValuePair<TKey, TValue?> v = default;
                formatter.Deserialize(ref reader, ref v);
                builder.Add(v.Key, v.Value);
            }

            value = builder.ToImmutable();
        }
    }

    public sealed class ImmutableHashSetFormatter<T> : MemoryPackFormatter<ImmutableHashSet<T?>>
    {
        public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref ImmutableHashSet<T?>? value)
        {
            if (value == null)
            {
                writer.WriteNullCollectionHeader();
                return;
            }

            var formatter = writer.GetFormatter<T?>();
            writer.WriteCollectionHeader(value.Count);
            foreach (var item in value)
            {
                var v = item;
                formatter.Serialize(ref writer, ref v);
            }
        }

        public override void Deserialize(ref MemoryPackReader reader, scoped ref ImmutableHashSet<T?>? value)
        {
            if (!reader.TryReadCollectionHeader(out var length))
            {
                value = null;
                return;
            }

            if (length == 0)
            {
                value = ImmutableHashSet<T?>.Empty;
                return;
            }

            if (length == 1)
            {
                var item = reader.ReadValue<T>();
                value = ImmutableHashSet.Create(item);
                return;
            }

            var formatter = reader.GetFormatter<T?>();

            var builder = ImmutableHashSet.CreateBuilder<T?>();
            for (int i = 0; i < length; i++)
            {
                T? item = default;
                formatter.Deserialize(ref reader, ref item);
                builder.Add(item);
            }

            value = builder.ToImmutable();
        }
    }

    public sealed class ImmutableSortedDictionaryFormatter<TKey, TValue> : MemoryPackFormatter<ImmutableSortedDictionary<TKey, TValue?>?>
        where TKey : notnull
    {
        static ImmutableSortedDictionaryFormatter()
        {
            if (!MemoryPackFormatterProvider.IsRegistered<KeyValuePair<TKey, TValue?>>())
            {
                MemoryPackFormatterProvider.Register(new KeyValuePairFormatter<TKey, TValue?>());
            }
        }

        public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref ImmutableSortedDictionary<TKey, TValue?>? value)
        {
            if (value == null)
            {
                writer.WriteNullCollectionHeader();
                return;
            }

            var formatter = writer.GetFormatter<KeyValuePair<TKey, TValue?>>();
            writer.WriteCollectionHeader(value.Count);
            foreach (var item in value)
            {
                var v = item;
                formatter.Serialize(ref writer, ref v);
            }
        }

        public override void Deserialize(ref MemoryPackReader reader, scoped ref ImmutableSortedDictionary<TKey, TValue?>? value)
        {
            if (!reader.TryReadCollectionHeader(out var length))
            {
                value = null;
                return;
            }

            if (length == 0)
            {
                value = ImmutableSortedDictionary<TKey, TValue?>.Empty;
                return;
            }

            var formatter = reader.GetFormatter<KeyValuePair<TKey, TValue?>>();

            var builder = ImmutableSortedDictionary.CreateBuilder<TKey, TValue?>();
            for (int i = 0; i < length; i++)
            {
                KeyValuePair<TKey, TValue?> v = default;
                formatter.Deserialize(ref reader, ref v);
                builder.Add(v.Key, v.Value);
            }

            value = builder.ToImmutable();
        }
    }

    public sealed class ImmutableSortedSetFormatter<T> : MemoryPackFormatter<ImmutableSortedSet<T?>>
    {
        public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref ImmutableSortedSet<T?>? value)
        {
            if (value == null)
            {
                writer.WriteNullCollectionHeader();
                return;
            }

            var formatter = writer.GetFormatter<T?>();
            writer.WriteCollectionHeader(value.Count);
            foreach (var item in value)
            {
                var v = item;
                formatter.Serialize(ref writer, ref v);
            }
        }

        public override void Deserialize(ref MemoryPackReader reader, scoped ref ImmutableSortedSet<T?>? value)
        {
            if (!reader.TryReadCollectionHeader(out var length))
            {
                value = null;
                return;
            }

            if (length == 0)
            {
                value = ImmutableSortedSet<T?>.Empty;
                return;
            }

            if (length == 1)
            {
                var item = reader.ReadValue<T>();
                value = ImmutableSortedSet.Create(item);
                return;
            }

            var formatter = reader.GetFormatter<T?>();

            var builder = ImmutableSortedSet.CreateBuilder<T?>();
            for (int i = 0; i < length; i++)
            {
                T? item = default;
                formatter.Deserialize(ref reader, ref item);
                builder.Add(item);
            }

            value = builder.ToImmutable();
        }
    }

    public sealed class InterfaceImmutableListFormatter<T> : MemoryPackFormatter<IImmutableList<T?>>
    {
        public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref IImmutableList<T?>? value)
        {
            if (value == null)
            {
                writer.WriteNullCollectionHeader();
                return;
            }

            var formatter = writer.GetFormatter<T?>();
            writer.WriteCollectionHeader(value.Count);
            foreach (var item in value)
            {
                var v = item;
                formatter.Serialize(ref writer, ref v);
            }
        }

        public override void Deserialize(ref MemoryPackReader reader, scoped ref IImmutableList<T?>? value)
        {
            if (!reader.TryReadCollectionHeader(out var length))
            {
                value = null;
                return;
            }

            if (length == 0)
            {
                value = ImmutableList<T?>.Empty;
                return;
            }

            if (length == 1)
            {
                var item = reader.ReadValue<T>();
                value = ImmutableList.Create(item);
                return;
            }

            var formatter = reader.GetFormatter<T?>();

            var builder = ImmutableList.CreateBuilder<T?>();
            for (int i = 0; i < length; i++)
            {
                T? item = default;
                formatter.Deserialize(ref reader, ref item);
                builder.Add(item);
            }

            value = builder.ToImmutable();
        }
    }

    public sealed class InterfaceImmutableQueueFormatter<T> : MemoryPackFormatter<IImmutableQueue<T?>>
    {
        public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref IImmutableQueue<T?>? value)
        {
            if (value == null)
            {
                writer.WriteNullCollectionHeader();
                return;
            }

            // ImmutableQueue<T> has no Count, so use similar serialization of IEnumerable<T>

            var tempBuffer = ReusableLinkedArrayBufferWriterPool.Rent();
            try
            {
                var tempWriter = new MemoryPackWriter<ReusableLinkedArrayBufferWriter>(ref tempBuffer, writer.Options);

                var count = 0;
                var formatter = writer.GetFormatter<T?>();
                foreach (var item in value)
                {
                    count++;
                    var v = item;
                    formatter.Serialize(ref tempWriter, ref v);
                }

                tempWriter.Flush();

                // write to parameter writer.
                writer.WriteCollectionHeader(count);
                tempBuffer.WriteToAndReset(ref writer);
            }
            finally
            {
                ReusableLinkedArrayBufferWriterPool.Return(tempBuffer);
            }
        }

        public override void Deserialize(ref MemoryPackReader reader, scoped ref IImmutableQueue<T?>? value)
        {
            if (!reader.TryReadCollectionHeader(out var length))
            {
                value = null;
                return;
            }

            if (length == 0)
            {
                value = ImmutableQueue<T?>.Empty;
                return;
            }

            if (length == 1)
            {
                var item = reader.ReadValue<T>();
                value = ImmutableQueue.Create(item);
                return;
            }

            // ImmutableQueue<T> has no builder

            var rentArray = ArrayPool<T?>.Shared.Rent(length);
            try
            {
                var formatter = reader.GetFormatter<T?>();
                for (int i = 0; i < length; i++)
                {
                    formatter.Deserialize(ref reader, ref rentArray[i]);
                }

                if (rentArray.Length == length)
                {
                    // we can use T[] ctor
                    value = ImmutableQueue.Create(rentArray);
                    return;
                }
                else
                {
                    // IEnumerable<T> method
                    value = ImmutableQueue.CreateRange((new ArraySegment<T?>(rentArray, 0, length)).AsEnumerable());
                }
            }
            finally
            {
                ArrayPool<T?>.Shared.Return(rentArray, clearArray: RuntimeHelpers.IsReferenceOrContainsReferences<T>());
            }
        }
    }

    public sealed class InterfaceImmutableStackFormatter<T> : MemoryPackFormatter<IImmutableStack<T?>>
    {
        public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref IImmutableStack<T?>? value)
        {
            if (value == null)
            {
                writer.WriteNullCollectionHeader();
                return;
            }

            // ImmutableStack<T> has no Count, so use similar serialization of IEnumerable<T>

            var tempBuffer = ReusableLinkedArrayBufferWriterPool.Rent();
            try
            {
                var tempWriter = new MemoryPackWriter<ReusableLinkedArrayBufferWriter>(ref tempBuffer, writer.Options);

                var count = 0;
                var formatter = writer.GetFormatter<T?>();
                foreach (var item in value.AsEnumerable().Reverse()) // serialize reverse order
                {
                    count++;
                    var v = item;
                    formatter.Serialize(ref tempWriter, ref v);
                }

                tempWriter.Flush();

                // write to parameter writer.
                writer.WriteCollectionHeader(count);
                tempBuffer.WriteToAndReset(ref writer);
            }
            finally
            {
                ReusableLinkedArrayBufferWriterPool.Return(tempBuffer);
            }
        }

        public override void Deserialize(ref MemoryPackReader reader, scoped ref IImmutableStack<T?>? value)
        {
            if (!reader.TryReadCollectionHeader(out var length))
            {
                value = null;
                return;
            }

            if (length == 0)
            {
                value = ImmutableStack<T?>.Empty;
                return;
            }

            if (length == 1)
            {
                var item = reader.ReadValue<T>();
                value = ImmutableStack.Create(item);
                return;
            }

            // ImmutableStack<T> has no builder

            var rentArray = ArrayPool<T?>.Shared.Rent(length);
            try
            {
                var formatter = reader.GetFormatter<T?>();
                for (int i = 0; i < length; i++)
                {
                    formatter.Deserialize(ref reader, ref rentArray[i]);
                }

                if (rentArray.Length == length)
                {
                    // we can use T[] ctor
                    value = ImmutableStack.Create(rentArray);
                    return;
                }
                else
                {
                    // IEnumerable<T> method
                    value = ImmutableStack.CreateRange((new ArraySegment<T?>(rentArray, 0, length)).AsEnumerable());
                }
            }
            finally
            {
                ArrayPool<T?>.Shared.Return(rentArray, clearArray: RuntimeHelpers.IsReferenceOrContainsReferences<T>());
            }
        }
    }

    public sealed class InterfaceImmutableDictionaryFormatter<TKey, TValue> : MemoryPackFormatter<IImmutableDictionary<TKey, TValue?>?>
        where TKey : notnull
    {
        static InterfaceImmutableDictionaryFormatter()
        {
            if (!MemoryPackFormatterProvider.IsRegistered<KeyValuePair<TKey, TValue?>>())
            {
                MemoryPackFormatterProvider.Register(new KeyValuePairFormatter<TKey, TValue?>());
            }
        }

        public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref IImmutableDictionary<TKey, TValue?>? value)
        {
            if (value == null)
            {
                writer.WriteNullCollectionHeader();
                return;
            }

            var formatter = writer.GetFormatter<KeyValuePair<TKey, TValue?>>();
            writer.WriteCollectionHeader(value.Count);
            foreach (var item in value)
            {
                var v = item;
                formatter.Serialize(ref writer, ref v);
            }
        }

        public override void Deserialize(ref MemoryPackReader reader, scoped ref IImmutableDictionary<TKey, TValue?>? value)
        {
            if (!reader.TryReadCollectionHeader(out var length))
            {
                value = null;
                return;
            }

            if (length == 0)
            {
                value = ImmutableDictionary<TKey, TValue?>.Empty;
                return;
            }

            var formatter = reader.GetFormatter<KeyValuePair<TKey, TValue?>>();

            var builder = ImmutableDictionary.CreateBuilder<TKey, TValue?>();
            for (int i = 0; i < length; i++)
            {
                KeyValuePair<TKey, TValue?> v = default;
                formatter.Deserialize(ref reader, ref v);
                builder.Add(v.Key, v.Value);
            }

            value = builder.ToImmutable();
        }
    }

    public sealed class InterfaceImmutableSetFormatter<T> : MemoryPackFormatter<IImmutableSet<T?>>
    {
        public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref IImmutableSet<T?>? value)
        {
            if (value == null)
            {
                writer.WriteNullCollectionHeader();
                return;
            }

            var formatter = writer.GetFormatter<T?>();
            writer.WriteCollectionHeader(value.Count);
            foreach (var item in value)
            {
                var v = item;
                formatter.Serialize(ref writer, ref v);
            }
        }

        public override void Deserialize(ref MemoryPackReader reader, scoped ref IImmutableSet<T?>? value)
        {
            if (!reader.TryReadCollectionHeader(out var length))
            {
                value = null;
                return;
            }

            if (length == 0)
            {
                value = ImmutableHashSet<T?>.Empty;
                return;
            }

            if (length == 1)
            {
                var item = reader.ReadValue<T>();
                value = ImmutableHashSet.Create(item);
                return;
            }

            var formatter = reader.GetFormatter<T?>();

            var builder = ImmutableHashSet.CreateBuilder<T?>();
            for (int i = 0; i < length; i++)
            {
                T? item = default;
                formatter.Deserialize(ref reader, ref item);
                builder.Add(item);
            }

            value = builder.ToImmutable();
        }
    }
}
