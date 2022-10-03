using Benchmark.Models;
using MemoryPack;
using MessagePack;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Benchmark.Micro;

public class RawSerialize
{
    MyClass value;
    [ThreadStatic]
    static byte[]? bufferCache;

    [ThreadStatic]
    static ReusableLinkedArrayBufferWriter? staticWriter;

    public RawSerialize()
    {
        value = new MyClass { X = 100, Y = 99999999, Z = 4444, FirstName = "Hoge Huga Tako", LastName = "あいうえおかきくけこ" };
    }

    [Benchmark]
    public byte[] Raw()
    {
        var dest = bufferCache;
        if (dest == null)
        {
            dest = bufferCache = GC.AllocateUninitializedArray<byte>(60000, true);
        }

        ref var p = ref MemoryMarshal.GetArrayDataReference(dest);

        Unsafe.WriteUnaligned(ref p, value.X);
        Unsafe.WriteUnaligned(ref Unsafe.Add(ref p, 4), value.Y);
        Unsafe.WriteUnaligned(ref Unsafe.Add(ref p, 8), value.Z);


        var f = value.FirstName!;
        var len1 = f.Length * 2;
        ref readonly var p2 = ref f.GetPinnableReference();
        Unsafe.WriteUnaligned(ref Unsafe.Add(ref p, 12), f.Length);
        Unsafe.CopyBlockUnaligned(ref Unsafe.Add(ref p, 16), ref Unsafe.As<char, byte>(ref Unsafe.AsRef(p2)), (uint)len1);

        var l = value.LastName!;
        var len2 = l.Length * 2;
        ref readonly var p3 = ref l.GetPinnableReference();
        Unsafe.WriteUnaligned(ref Unsafe.Add(ref p, 16 + len1), l.Length); ;
        Unsafe.CopyBlockUnaligned(ref Unsafe.Add(ref p, 20 + len1), ref Unsafe.As<char, byte>(ref Unsafe.AsRef(p3)), (uint)len2);

        var result = GC.AllocateUninitializedArray<byte>(20 + len1 + len2);

        dest.AsSpan(0, result.Length).CopyTo(result);

        return result;
    }

    [Benchmark]
    public byte[] MessagePackSerialize()
    {
        return MessagePackSerializer.Serialize(value);
    }

    [Benchmark]
    public byte[] HandMemoryPackWriterEmpty()
    {
        var bufWriter = staticWriter;
        if (bufWriter == null)
        {
            bufWriter = staticWriter = new ReusableLinkedArrayBufferWriter(true, true);
        }

        var writer = new MemoryPackWriter<ReusableLinkedArrayBufferWriter>(ref bufWriter, bufWriter.DangerousGetFirstBuffer(), MemoryPackSerializeOptions.Default);
        try
        {
            if (value == null)
            {
                writer.WriteNullObjectHeader();
                goto END;
            }

        //writer.WriteObjectHeader(5);
        //writer.WriteUnmanaged(value.X, value.Y, value.Z);
        //writer.WriteString(value.FirstName);
        //writer.WriteString(value.LastName);



        END:
            writer.Flush();
            return bufWriter.ToArrayAndReset();
        }
        finally
        {
            bufWriter.Reset();
        }
    }

    [Benchmark]
    public byte[] HandMemoryPackWriterHeaderOnly()
    {
        var bufWriter = staticWriter;
        if (bufWriter == null)
        {
            bufWriter = staticWriter = new ReusableLinkedArrayBufferWriter(true, true);
        }

        var writer = new MemoryPackWriter<ReusableLinkedArrayBufferWriter>(ref bufWriter, bufWriter.DangerousGetFirstBuffer(), MemoryPackSerializeOptions.Default);
        try
        {
            if (value == null)
            {
                writer.WriteNullObjectHeader();
                goto END;
            }

            writer.WriteObjectHeader(5);
        //writer.WriteUnmanaged(value.X, value.Y, value.Z);
        //writer.WriteString(value.FirstName);
        //writer.WriteString(value.LastName);


        END:
            writer.Flush();
            return bufWriter.ToArrayAndReset();
        }
        finally
        {
            bufWriter.Reset();
        }
    }

    [Benchmark]
    public byte[] HandMemoryPackWriterHeaderInt3()
    {
        var bufWriter = staticWriter;
        if (bufWriter == null)
        {
            bufWriter = staticWriter = new ReusableLinkedArrayBufferWriter(true, true);
        }

        var writer = new MemoryPackWriter<ReusableLinkedArrayBufferWriter>(ref bufWriter, bufWriter.DangerousGetFirstBuffer(), MemoryPackSerializeOptions.Default);
        try
        {
            if (value == null)
            {
                writer.WriteNullObjectHeader();
                goto END;
            }

            writer.WriteObjectHeader(5);
            writer.WriteUnmanaged(value.X, value.Y, value.Z);
        //writer.WriteString(value.FirstName);
        //writer.WriteString(value.LastName);


        END:
            writer.Flush();
            return bufWriter.ToArrayAndReset();
        }
        finally
        {
            bufWriter.Reset();
        }
    }

    [Benchmark]
    public byte[] HandMemoryPackWriterHeaderInt3String1()
    {
        var bufWriter = staticWriter;
        if (bufWriter == null)
        {
            bufWriter = staticWriter = new ReusableLinkedArrayBufferWriter(true, true);
        }

        var writer = new MemoryPackWriter<ReusableLinkedArrayBufferWriter>(ref bufWriter, bufWriter.DangerousGetFirstBuffer(), MemoryPackSerializeOptions.Default);
        try
        {
            if (value == null)
            {
                writer.WriteNullObjectHeader();
                goto END;
            }

            writer.WriteObjectHeader(5);
            writer.WriteUnmanaged(value.X, value.Y, value.Z);
            writer.WriteString(value.FirstName);
        //writer.WriteString(value.LastName);


        END:
            writer.Flush();
            return bufWriter.ToArrayAndReset();
        }
        finally
        {
            bufWriter.Reset();
        }
    }

    [Benchmark]
    public byte[] HandMemoryPackFull()
    {
        var bufWriter = staticWriter;
        if (bufWriter == null)
        {
            bufWriter = staticWriter = new ReusableLinkedArrayBufferWriter(true, true);
        }

        var writer = new MemoryPackWriter<ReusableLinkedArrayBufferWriter>(ref bufWriter, bufWriter.DangerousGetFirstBuffer(), MemoryPackSerializeOptions.Default);
        try
        {
            if (value == null)
            {
                writer.WriteNullObjectHeader();
                goto END;
            }

            writer.WriteObjectHeader(5);
            writer.WriteUnmanaged(value.X, value.Y, value.Z);
            writer.WriteString(value.FirstName);
            writer.WriteString(value.LastName);


        END:
            writer.Flush();
            return bufWriter.ToArrayAndReset();
        }
        finally
        {
            bufWriter.Reset();
        }
    }

    [Benchmark]
    public byte[] MemoryPackSerialize()
    {
        return MemoryPackSerializer.Serialize(value);
    }
}


internal sealed class ReusableLinkedArrayBufferWriter : IBufferWriter<byte>
{
    const int InitialBufferSize = 262144; // 256K(32768, 65536, 131072, 262144)
    static readonly byte[] noUseFirstBufferSentinel = new byte[0];

    List<BufferSegment> buffers; // add freezed buffer.

    byte[] firstBuffer; // cache firstBuffer to avoid call ArrayPoo.Rent/Return
    int firstBufferWritten;

    BufferSegment current;
    int nextBufferSize;

    int totalWritten;

    public int TotalWritten => totalWritten;
    bool UseFirstBuffer => firstBuffer != noUseFirstBufferSentinel;

    public ReusableLinkedArrayBufferWriter(bool useFirstBuffer, bool pinned)
    {
        this.buffers = new List<BufferSegment>();
        this.firstBuffer = useFirstBuffer
            ? GC.AllocateUninitializedArray<byte>(InitialBufferSize, pinned)
            : noUseFirstBufferSentinel;
        this.firstBufferWritten = 0;
        this.current = default;
        this.nextBufferSize = InitialBufferSize;
        this.totalWritten = 0;
    }

    public byte[] DangerousGetFirstBuffer() => firstBuffer;

    public Memory<byte> GetMemory(int sizeHint = 0)
    {
        // MemoryPack don't use GetMemory.
        throw new NotSupportedException();
    }

    public Span<byte> GetSpan(int sizeHint = 0)
    {
        if (current.IsNull)
        {
            // use firstBuffer
            var free = firstBuffer.Length - firstBufferWritten;
            if (free != 0 && sizeHint <= free)
            {
                return firstBuffer.AsSpan(firstBufferWritten);
            }
        }
        else
        {
            var buffer = current.FreeBuffer;
            if (buffer.Length > sizeHint)
            {
                return buffer;
            }
        }

        BufferSegment next;
        if (sizeHint <= nextBufferSize)
        {
            next = new BufferSegment(nextBufferSize);
            nextBufferSize *= 2;
        }
        else
        {
            next = new BufferSegment(sizeHint);
        }

        if (current.WrittenCount != 0)
        {
            buffers.Add(current);
        }
        current = next;
        return next.FreeBuffer;
    }

    public void Advance(int count)
    {
        if (current.IsNull)
        {
            firstBufferWritten += count;
        }
        else
        {
            current.Advance(count);
        }
        totalWritten += count;
    }

    public byte[] ToArrayAndReset()
    {
        if (totalWritten == 0) return Array.Empty<byte>();

        var result = GC.AllocateUninitializedArray<byte>(totalWritten);
        var dest = result.AsSpan();

        if (UseFirstBuffer)
        {
            firstBuffer.AsSpan(0, firstBufferWritten).CopyTo(dest);
            dest = dest.Slice(firstBufferWritten);
        }

        if (buffers.Count > 0)
        {
            foreach (var item in CollectionsMarshal.AsSpan(buffers))
            {
                item.WrittenBuffer.CopyTo(dest);
                dest = dest.Slice(item.WrittenCount);
                item.Clear(); // reset buffer-segment in this loop to avoid iterate twice for Reset
            }
        }

        if (!current.IsNull)
        {
            current.WrittenBuffer.CopyTo(dest);
            current.Clear();
        }

        ResetCore();
        return result;
    }

    public void WriteToAndReset<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer)
        where TBufferWriter : IBufferWriter<byte>
    {
        if (totalWritten == 0) return;

        if (UseFirstBuffer)
        {
            ref var spanRef = ref writer.GetSpanReference(firstBufferWritten);
            firstBuffer.AsSpan(0, firstBufferWritten).CopyTo(MemoryMarshal.CreateSpan(ref spanRef, firstBufferWritten));
            writer.Advance(firstBufferWritten);
        }

        if (buffers.Count > 0)
        {
            foreach (var item in CollectionsMarshal.AsSpan(buffers))
            {
                ref var spanRef = ref writer.GetSpanReference(item.WrittenCount);
                item.WrittenBuffer.CopyTo(MemoryMarshal.CreateSpan(ref spanRef, item.WrittenCount));
                writer.Advance(item.WrittenCount);
                item.Clear(); // reset
            }
        }

        if (!current.IsNull)
        {
            ref var spanRef = ref writer.GetSpanReference(current.WrittenCount);
            current.WrittenBuffer.CopyTo(MemoryMarshal.CreateSpan(ref spanRef, current.WrittenCount));
            writer.Advance(current.WrittenCount);
            current.Clear();
        }

        writer.Flush();

        ResetCore();
    }

    public async ValueTask WriteToAndResetAsync(Stream stream, CancellationToken cancellationToken)
    {
        if (totalWritten == 0) return;

        if (UseFirstBuffer)
        {
            await stream.WriteAsync(firstBuffer.AsMemory(0, firstBufferWritten), cancellationToken).ConfigureAwait(false);
        }

        if (buffers.Count > 0)
        {
            foreach (var item in buffers)
            {
                await stream.WriteAsync(item.WrittenMemory, cancellationToken).ConfigureAwait(false);
                item.Clear(); // reset
            }
        }

        if (!current.IsNull)
        {
            await stream.WriteAsync(current.WrittenMemory, cancellationToken).ConfigureAwait(false);
            current.Clear();
        }

        ResetCore();
    }

    // reset without list's BufferSegment element
    void ResetCore()
    {
        firstBufferWritten = 0;
        buffers.Clear();
        totalWritten = 0;
        current = default;
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

    public bool IsNull => buffer == null;

    public int WrittenCount => written;
    public Span<byte> WrittenBuffer => buffer.AsSpan(0, written);
    public Memory<byte> WrittenMemory => buffer.AsMemory(0, written);
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
        if (buffer != null)
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
        buffer = null!;
        written = 0;
    }
}
