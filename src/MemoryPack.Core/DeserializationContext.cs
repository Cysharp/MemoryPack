using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MemoryPack;

public ref struct DeserializationContext
{
    readonly long totalLength;
    ReadOnlySequence<byte> bufferSource; // TODO:ref?
    ReadOnlySpan<byte> buffer; // TODO:ref byte bufferReference
    int bufferLength;
    byte[]? rentBuffer;

    public DeserializationContext(ReadOnlySequence<byte> sequence)
    {
        this.bufferSource = sequence.IsSingleSegment ? default : sequence;
        this.buffer = sequence.FirstSpan;
        this.bufferLength = buffer.Length;
        this.rentBuffer = null;
        this.totalLength = sequence.Length;
    }

    public DeserializationContext(ReadOnlySpan<byte> buffer)
    {
        this.bufferSource = default;
        this.buffer = buffer;
        this.bufferLength = buffer.Length;
        this.rentBuffer = null;
        this.totalLength = buffer.Length;
    }

    public ref byte GetSpanReference(int sizeHint)
    {
        if (sizeHint <= buffer.Length)
        {
            // TODO: return ref bufferReference
            return ref MemoryMarshal.GetReference(buffer);
        }

        if (rentBuffer != null)
        {
            ArrayPool<byte>.Shared.Return(rentBuffer);
            rentBuffer = null;
        }

        bufferSource = bufferSource.Slice(buffer.Length);

        if (bufferSource.Length < sizeHint)
        {
            if (bufferSource.FirstSpan.Length < sizeHint)
            {
                buffer = bufferSource.FirstSpan;
                bufferLength = buffer.Length;
                return ref MemoryMarshal.GetReference(buffer);
            }

            rentBuffer = ArrayPool<byte>.Shared.Rent(sizeHint);
            bufferSource.Slice(0, sizeHint).CopyTo(rentBuffer);
            buffer = rentBuffer.AsSpan(0, sizeHint);
            bufferLength = buffer.Length;
            return ref MemoryMarshal.GetReference(buffer);
        }

        throw new Exception("TODO:message.");
    }

    public void Advance(int count)
    {
        buffer = buffer.Slice(count); // TODO: ref Unsafe.Add(ref bufferReference, count)
        bufferLength = buffer.Length;
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

    public bool TryReadLength(out int length)
    {
        length = Unsafe.ReadUnaligned<int>(ref GetSpanReference(4));
        Advance(4);

        // If collection-length is larger than buffer-length, it is invalid data.
        if (totalLength < length)
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
    public T[]? DangerousReadUnmanagedArray<T>()
    {
        if (!DangerousTryReadUnmanagedSpan<T>(out var view, out var advanceLength))
        {
            return null;
        }

        var array = view.ToArray();
        Advance(advanceLength);

        return array;
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
