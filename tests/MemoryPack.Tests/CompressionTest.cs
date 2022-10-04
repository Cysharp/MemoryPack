using MemoryPack.Compression;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemoryPack.Tests;

public class CompressionTest
{
    [Fact]
    public void Foo()
    {
        using var brotli = new BrotliCompressor();

        MemoryPackSerializer.Serialize(brotli, "http://http://http://http://http://http://http://http://http://http://http://http://http://http://http://http://http://http://http://http://http://http://http://http://http://http://http://http://http://http://http://http://http://http://http://http://http://http://http://http://http://http://http://http://http://http://http://http://http://http://http://http://http://http://http://http://http://http://http://http://http://http://http://http://http://http://http://http://http://http://http://http://http://http://http://http://http://http://http://http://http://http://http://http://http://http://");



        var arrayWriter = new FixedBufferWriter();

        var array1 = brotli.ToArray();

        brotli.CopyTo(arrayWriter);

        var array2 = brotli.ToArray();

        using var decompressor = new BrotliMemoryDecompressor();


        var two = decompressor.Decompress(array1.AsMemory());


        var hoge = MemoryPackSerializer.Deserialize<string>(two.Span);

    }
}

file class FixedBufferWriter : IBufferWriter<byte>
{
    byte[] buffer = new byte[10000];
    int written;

    public ReadOnlySpan<byte> WrittenSpan => buffer.AsSpan(0, written);

    public void Advance(int count)
    {
        written += count;
    }

    public Memory<byte> GetMemory(int sizeHint = 0)
    {
        throw new NotImplementedException();
    }

    public Span<byte> GetSpan(int sizeHint = 0)
    {
        if (sizeHint == 0) sizeHint = 1;
        return buffer.AsSpan(written, sizeHint);
    }
}
