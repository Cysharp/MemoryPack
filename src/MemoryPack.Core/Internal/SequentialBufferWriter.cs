using System.Buffers;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;

namespace MemoryPack.Internal;

internal static class SequentialBufferWriterPool
{
    static readonly ConcurrentQueue<SequentialBufferWriter> queue = new ConcurrentQueue<SequentialBufferWriter>();

    public static SequentialBufferWriter Rent()
    {
        if (queue.TryDequeue(out var writer))
        {
            return writer;
        }
        return new SequentialBufferWriter();
    }

    public static void Return(SequentialBufferWriter writer)
    {
        writer.Reset();
        queue.Enqueue(writer);
    }
}

// This class has large buffer so should cache [ThreadStatic] or Pool.
internal sealed class SequentialBufferWriter : IBufferWriter<byte>
{
    const int InitialBufferSize = 65536; // 64K

    List<BufferSegment> buffers;

    byte[] firstBuffer; // cache firstBuffer to avoid call ArrayPoo.Rent/Return
    int firstBufferWritten;

    BufferSegment? current;
    int nextBufferSize;

    int totalWritten;

    public int TotalWritten => totalWritten;

    public SequentialBufferWriter()
    {
        this.buffers = new List<BufferSegment>();

        this.firstBuffer = new byte[InitialBufferSize];
        this.firstBufferWritten = 0;
        this.current = null;
        this.nextBufferSize = InitialBufferSize;
        this.totalWritten = 0;
    }

    public Memory<byte> GetMemory(int sizeHint = 0)
    {
        // MemoryPack don't use GetMemory.
        throw new NotSupportedException();
    }

    public Span<byte> GetSpan(int sizeHint = 0)
    {
        if (current == null)
        {
            // use firstBuffer
            if (firstBuffer.Length - firstBufferWritten < sizeHint)
            {
                return firstBuffer.AsSpan(firstBufferWritten);
            }
        }
        else
        {
            var buffer = current.Value.FreeBuffer;
            if (buffer.Length < sizeHint)
            {
                return buffer;
            }
        }

        BufferSegment next;
        if (nextBufferSize < sizeHint)
        {
            next = new BufferSegment(nextBufferSize);
            nextBufferSize *= 2;
        }
        else
        {
            next = new BufferSegment(sizeHint);
        }

        buffers.Add(next);
        current = next;
        return next.FreeBuffer;
    }

    public void Advance(int count)
    {
        if (current == null)
        {
            firstBufferWritten += count;
        }
        else
        {
            current.Value.Advance(count);
        }
        totalWritten += count;
    }

    public byte[] ToArrayAndReset()
    {
        if (totalWritten == 0) return Array.Empty<byte>();

        var result = new byte[totalWritten];
        var dest = result.AsSpan();

        firstBuffer.AsSpan(0, firstBufferWritten).CopyTo(dest);
        dest = dest.Slice(firstBufferWritten);

        foreach (var item in CollectionsMarshal.AsSpan(buffers))
        {
            item.WrittenBuffer.CopyTo(dest);
            dest = dest.Slice(item.WrittenCount);
            item.Clear(); // reset buffer-segment in this loop to avoid iterate twice for Reset
        }

        ResetCore();
        return result;
    }

    public unsafe void WriteToAndReset<TBufferWriter>(ref SerializationContext<TBufferWriter> context)
        where TBufferWriter : IBufferWriter<byte>
    {
        if (totalWritten == 0) return;

        {
            ref var spanRef = ref context.GetSpanReference(firstBufferWritten);
            firstBuffer.AsSpan(0, firstBufferWritten).CopyTo(MemoryMarshal.CreateSpan(ref spanRef, firstBufferWritten));
            context.Advance(firstBufferWritten);
        }

        foreach (var item in CollectionsMarshal.AsSpan(buffers))
        {
            ref var spanRef = ref context.GetSpanReference(item.WrittenCount);
            item.WrittenBuffer.CopyTo(MemoryMarshal.CreateSpan(ref spanRef, item.WrittenCount));
            context.Advance(item.WrittenCount);
            item.Clear(); // reset
        }

        ResetCore();
    }

    // reset without list's BufferSegment element
    void ResetCore()
    {
        firstBufferWritten = 0;
        buffers.Clear();
        totalWritten = 0;
        current = null;
        nextBufferSize = InitialBufferSize;
    }

    public void Reset()
    {
        if (totalWritten == 0) return;
        foreach (var item in CollectionsMarshal.AsSpan(buffers))
        {
            item.Clear();
        }
        ResetCore();
    }
}

internal struct BufferSegment
{
    byte[] buffer;
    int written;

    public int WrittenCount => written;
    public Span<byte> WrittenBuffer => buffer.AsSpan(0, written);
    public Span<byte> FreeBuffer => buffer.AsSpan(written);

    public BufferSegment(int size)
    {
        buffer = ArrayPool<byte>.Shared.Rent(size);
        written = 0;
    }

    public void Advance(int count)
    {
        written += count;
    }

    public void Clear()
    {
        ArrayPool<byte>.Shared.Return(buffer);
        buffer = null!;
        written = 0;
    }
}