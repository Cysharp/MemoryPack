using Benchmark.BenchmarkNetUtilities;
using MemoryPack;
using MemoryPack.Compression;
using System.IO.Compression;

namespace Benchmark.Benchmarks;

[CategoriesColumn]
[PayloadColumn]
public class Compression<T> : SerializerTestBase<T>
{
    public Compression()
        : base()
    {

    }

    [Benchmark(Baseline = true)]
    public byte[] SerializeMemoryPack()
    {
        return MemoryPackSerializer.Serialize(value, MemoryPackSerializeOptions.Utf8);
    }

    [Benchmark]
    public byte[] SerializeMemoryPackUtf16()
    {
        return MemoryPackSerializer.Serialize(value, MemoryPackSerializeOptions.Utf16);
    }

    [Benchmark]
    public byte[] BrotliCompress()
    {
        using var compressor = new BrotliCompressor();
        MemoryPackSerializer.Serialize(compressor, value, MemoryPackSerializeOptions.Utf8);
        return compressor.ToArray();
    }

    //[Benchmark]
    //public async Task<byte[]> BrotliCompressStream()
    //{
    //    using (var ms = new MemoryStream())
    //    {
    //        using (var brotli = new BrotliStream(ms, CompressionMode.Compress))
    //        {
    //            await MemoryPackSerializer.SerializeAsync(brotli, value, MemoryPackSerializeOptions.Utf8).ConfigureAwait(false);
    //        }
    //        ms.Flush();
    //        return ms.ToArray();
    //    }
    //}

    [Benchmark]
    public byte[] BrotliCompressUtf16()
    {
        using var compressor = new BrotliCompressor();
        MemoryPackSerializer.Serialize(compressor, value, MemoryPackSerializeOptions.Utf16);
        return compressor.ToArray();
    }
}
