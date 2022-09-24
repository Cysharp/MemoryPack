using MemoryPack.Formatters;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;

// Array and Array-like type formatters
// T[]
// T[] where T: unmnaged
// Memory
// ReadOnlyMemory
// ArraySegment
// ReadOnlySequence

namespace MemoryPack
{
    public static partial class MemoryPackFormatterProvider
    {
        static readonly Dictionary<Type, Type> ArrayLikeFormatters = new Dictionary<Type, Type>(4)
        {
            // If T[], choose UnmanagedArrayFormatter or DangerousUnmanagedTypeArrayFormatter or ArrayFormatter
            { typeof(ArraySegment<>), typeof(ArraySegmentFormatter<>) },
            { typeof(Memory<>), typeof(MemoryFormatter<>) },
            { typeof(ReadOnlyMemory<>), typeof(ReadOnlyMemoryFormatter<>) },
            { typeof(ReadOnlySequence<>), typeof(ReadOnlySequenceFormatter<>) },
        };
    }
}

namespace MemoryPack.Formatters
{
    public sealed class UnmanagedArrayFormatter<T> : MemoryPackFormatter<T[]>
        where T : unmanaged
    {
        public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref T[]? value)
        {
            writer.WriteUnmanagedArray(value);
        }

        public override void Deserialize(ref MemoryPackReader reader, scoped ref T[]? value)
        {
            reader.ReadUnmanagedArray<T>(ref value);
        }
    }

    public sealed class DangerousUnmanagedTypeArrayFormatter<T> : MemoryPackFormatter<T[]>
    {
        public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref T[]? value)
        {
            writer.DangerousWriteUnmanagedArray(value);
        }

        public override void Deserialize(ref MemoryPackReader reader, scoped ref T[]? value)
        {
            reader.DangerousReadUnmanagedArray<T>(ref value);
        }
    }

    public sealed class ArrayFormatter<T> : MemoryPackFormatter<T?[]>
    {
        public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref T?[]? value)
        {
            writer.WriteArray(value);
        }

        public override void Deserialize(ref MemoryPackReader reader, scoped ref T?[]? value)
        {
            reader.ReadArray(ref value);
        }
    }

    public sealed class ArraySegmentFormatter<T> : MemoryPackFormatter<ArraySegment<T?>>
    {
        public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref ArraySegment<T?> value)
        {
            writer.WriteSpan(value.AsMemory().Span);
        }

        public override void Deserialize(ref MemoryPackReader reader, scoped ref ArraySegment<T?> value)
        {
            var array = reader.ReadArray<T>();
            value = (array == null) ? default : (ArraySegment<T?>)array;
        }
    }

    public sealed class MemoryFormatter<T> : MemoryPackFormatter<Memory<T?>>
    {
        public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref Memory<T?> value)
        {
            writer.WriteSpan(value.Span);
        }

        public override void Deserialize(ref MemoryPackReader reader, scoped ref Memory<T?> value)
        {
            value = reader.ReadArray<T>();
        }
    }

    public sealed class ReadOnlyMemoryFormatter<T> : MemoryPackFormatter<ReadOnlyMemory<T?>>
    {
        public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref ReadOnlyMemory<T?> value)
        {
            writer.WriteSpan(value.Span);
        }

        public override void Deserialize(ref MemoryPackReader reader, scoped ref ReadOnlyMemory<T?> value)
        {
            value = reader.ReadArray<T>();
        }
    }

    public sealed class ReadOnlySequenceFormatter<T> : MemoryPackFormatter<ReadOnlySequence<T?>>
    {
        public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref ReadOnlySequence<T?> value)
        {
            if (value.IsSingleSegment)
            {
                writer.WriteSpan(value.FirstSpan);
                return;
            }

            writer.WriteLengthHeader(checked((int)value.Length));
            foreach (var memory in value)
            {
                writer.WriteSpanWithoutLengthHeader(memory.Span);
            }
        }

        public override void Deserialize(ref MemoryPackReader reader, scoped ref ReadOnlySequence<T?> value)
        {
            var array = reader.ReadArray<T>();
            value = (array == null) ? default : new ReadOnlySequence<T?>(array);
        }
    }
}
