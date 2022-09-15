using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MemoryPack;

public ref struct MemoryPackReader
{
    ReadOnlySequence<byte> bufferSource;
    ref byte bufferReference;
    int bufferLength;
    byte[]? rentBuffer;
    long rest; // TODO:used for malformed length check

    public MemoryPackReader(in ReadOnlySequence<byte> sequence)
    {
        this.bufferSource = sequence.IsSingleSegment ? ref Unsafe.NullRef<ReadOnlySequence<byte>>() : ref sequence;
        var span = sequence.FirstSpan;
        this.bufferReference = ref MemoryMarshal.GetReference(span);
        this.bufferLength = span.Length;
        this.rentBuffer = null;
        this.rest = sequence.Length;
    }

    public MemoryPackReader(ReadOnlySpan<byte> buffer)
    {
        this.bufferSource = ReadOnlySequence<byte>.Empty;
        this.bufferReference = ref MemoryMarshal.GetReference(buffer);
        this.bufferLength = buffer.Length;
        this.rentBuffer = null;
        this.rest = buffer.Length;
    }

    public ref byte GetSpanReference(int sizeHint)
    {
        if (sizeHint <= bufferLength)
        {
            return ref bufferReference;
        }

        if (rentBuffer != null)
        {
            ArrayPool<byte>.Shared.Return(rentBuffer);
            rentBuffer = null;
        }

        bufferSource = bufferSource.Slice(bufferLength);
        rest = bufferSource.Length;

        if (rest < sizeHint)
        {
            if (bufferSource.FirstSpan.Length < sizeHint)
            {
                bufferReference = ref MemoryMarshal.GetReference(bufferSource.FirstSpan);
                bufferLength = bufferSource.FirstSpan.Length;
                return ref bufferReference;
            }

            rentBuffer = ArrayPool<byte>.Shared.Rent(sizeHint);
            bufferSource.Slice(0, sizeHint).CopyTo(rentBuffer);
            var span = rentBuffer.AsSpan(0, sizeHint);
            bufferReference = ref MemoryMarshal.GetReference(span);
            bufferLength = span.Length;
            return ref bufferReference;
        }

        throw new Exception("TODO:message.");
    }

    public void Advance(int count)
    {
        // TODO:check length.
        bufferLength = bufferLength - count;
        bufferReference = ref Unsafe.Add(ref bufferReference, count);
    }

    public void Dispose()
    {
        if (rentBuffer != null)
        {
            ArrayPool<byte>.Shared.Return(rentBuffer);
        }
    }

    // helpers

    public bool TryReadPropertyCount(out byte count)
    {
        count = GetSpanReference(1);
        Advance(1);
        return count != MemoryPackCode.NullObject;
    }

    // TODO: should check deserialize does nott occur buffer overrun
    public bool TryReadLength(out int length)
    {
        length = Unsafe.ReadUnaligned<int>(ref GetSpanReference(4));
        Advance(4);

        // If collection-length is larger than buffer-length, it is invalid data.
        if (rest < length)
        {
            ThrowInsufficientBufferUnless();
        }

        return length != MemoryPackCode.NullLength;
    }

    public string? ReadString()
    {
        if (!TryReadUnmanagedSpan<char>(out var view, out var advanceLength))
        {
            return null;
        }

        if (view.Length == 0)
        {
            return "";
        }

        var str = new string(view);
        Advance(advanceLength);

        return str;
    }

    public T[]? ReadUnmanagedArray<T>()
        where T : unmanaged
    {
        return DangerousReadUnmanagedArray<T>();
    }

    // T: should be unamanged type
    public unsafe T[]? DangerousReadUnmanagedArray<T>()
    {
        if (!TryReadLength(out var length))
        {
            return null;
        }

        if (length == 0) return Array.Empty<T>();

        var size = length * Unsafe.SizeOf<T>();
        ref var src = ref GetSpanReference(size);
        var dest = GC.AllocateUninitializedArray<T>(length);

        Buffer.MemoryCopy(Unsafe.AsPointer(ref src), Unsafe.AsPointer(ref dest), size, size);
        Advance(size);

        return dest;
    }

    public bool TryReadUnmanagedSpan<T>(out ReadOnlySpan<T> view, out int advanceLength)
        where T : unmanaged
    {
        return DangerousTryReadUnmanagedSpan(out view, out advanceLength);
    }

    // T: should be unamanged type
    public bool DangerousTryReadUnmanagedSpan<T>(out ReadOnlySpan<T> view, out int advanceLength)
    {
        if (!TryReadLength(out var length))
        {
            view = default;
            advanceLength = 0;
            return false;
        }

        view = MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<byte, T>(ref GetSpanReference(length)), length);
        advanceLength = view.Length * Unsafe.SizeOf<T>();
        return true;
    }

    [DoesNotReturn]
    void ThrowInsufficientBufferUnless()
    {
        throw new EndOfStreamException();
    }
}
