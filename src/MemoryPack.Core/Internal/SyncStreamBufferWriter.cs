using System.Buffers;

namespace MemoryPack.Internal;

// Flush Stream when Advance called.
internal struct SyncStreamBufferWriter : IBufferWriter<byte>, IDisposable
{
    // 64K buffer

    readonly Stream underlyingStream;
    //byte[] buffer;


    public SyncStreamBufferWriter(Stream underlyingStream)
    {
        this.underlyingStream = underlyingStream;
        // this.buffer = ArrayPool<byte>.Shared.Return(
      //  this.buffer = default!;
    }

    public void Advance(int count)
    {
        // throw new NotImplementedException();

        //underlyingStream.Write(

    }

    public Span<byte> GetSpan(int sizeHint = 0)
    {
        throw new NotImplementedException();
    }

    public Memory<byte> GetMemory(int sizeHint = 0)
    {
        throw new NotSupportedException();
    }

    public void Dispose()
    {

    }
}