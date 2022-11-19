using Benchmark.BenchmarkNetUtilities;
using BenchmarkDotNet.Configs;
using K4os.Compression.LZ4.Encoders;
using K4os.Compression.LZ4.Streams;
using MemoryPack;
using MemoryPack.Compression;
using System.IO.Compression;

namespace Benchmark.Benchmarks;

[CategoriesColumn]
[PayloadColumn]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public class Compression<T> : SerializerTestBase<T>
{
    MemoryStream ms;
    LZ4EncoderSettings Fast;
    LZ4EncoderSettings L10Opt;
    LZ4EncoderSettings L04HC;
    byte[] normal;
    byte[] brotliFast;
    byte[] lz4Fast;

    public Compression()
        : base()
    {
        ms = new MemoryStream();
        Fast = new LZ4EncoderSettings { CompressionLevel = K4os.Compression.LZ4.LZ4Level.L00_FAST };
        L10Opt = new LZ4EncoderSettings { CompressionLevel = K4os.Compression.LZ4.LZ4Level.L10_OPT };
        L04HC = new LZ4EncoderSettings { CompressionLevel = K4os.Compression.LZ4.LZ4Level.L04_HC };

        normal = SerializeMemoryPack();
        brotliFast = BrotliCompressQ1();
        lz4Fast = LZ4CompressStreamFast();
    }

    [Benchmark(Baseline = true), BenchmarkCategory(Categories.Serialize)]
    public byte[] SerializeMemoryPack()
    {
        return MemoryPackSerializer.Serialize(value, MemoryPackSerializerOptions.Utf8);
    }

    [Benchmark]
    public byte[] SerializeMemoryPackUtf16()
    {
        return MemoryPackSerializer.Serialize(value, MemoryPackSerializerOptions.Utf16);
    }

    [Benchmark, BenchmarkCategory(Categories.Serialize)]
    public byte[] BrotliCompressQ1()
    {
        using var compressor = new BrotliCompressor(quality: 1);
        MemoryPackSerializer.Serialize(compressor, value, MemoryPackSerializerOptions.Utf8);
        return compressor.ToArray();
    }

    [Benchmark, BenchmarkCategory(Categories.Serialize)]
    public byte[] BrotliCompressQ2()
    {
        using var compressor = new BrotliCompressor(quality: 2);
        MemoryPackSerializer.Serialize(compressor, value, MemoryPackSerializerOptions.Utf8);
        return compressor.ToArray();
    }

    [Benchmark, BenchmarkCategory(Categories.Serialize)]
    public byte[] BrotliCompressQ3()
    {
        using var compressor = new BrotliCompressor(quality: 3);
        MemoryPackSerializer.Serialize(compressor, value, MemoryPackSerializerOptions.Utf8);
        return compressor.ToArray();
    }

    [Benchmark, BenchmarkCategory(Categories.Serialize)]
    public byte[] BrotliCompressQ4()
    {
        using var compressor = new BrotliCompressor(quality: 4);
        MemoryPackSerializer.Serialize(compressor, value, MemoryPackSerializerOptions.Utf8);
        return compressor.ToArray();
    }

    [Benchmark, BenchmarkCategory(Categories.Serialize)]
    public byte[] BrotliCompressStreamFastest()
    {
        ms.Position = 0;
        using (var brotli = new BrotliStream(ms, CompressionLevel.Fastest, leaveOpen: true))
        {
            MemoryPackSerializer.SerializeAsync(brotli, value, MemoryPackSerializerOptions.Utf8).ConfigureAwait(false).GetAwaiter().GetResult();
        }
        ms.Flush();
        return ms.ToArray();
    }

    [Benchmark, BenchmarkCategory(Categories.Serialize)]
    public byte[] BrotliCompressStreamOptimial()
    {
        ms.Position = 0;
        using (var brotli = new BrotliStream(ms, CompressionLevel.Optimal, leaveOpen: true))
        {
            MemoryPackSerializer.SerializeAsync(brotli, value, MemoryPackSerializerOptions.Utf8).ConfigureAwait(false).GetAwaiter().GetResult();
        }
        ms.Flush();
        return ms.ToArray();
    }

    [Benchmark, BenchmarkCategory(Categories.Serialize)]
    public byte[] BrotliCompressStreamSmallestSize()
    {
        ms.Position = 0;
        using (var brotli = new BrotliStream(ms, CompressionLevel.SmallestSize, leaveOpen: true))
        {
            MemoryPackSerializer.SerializeAsync(brotli, value, MemoryPackSerializerOptions.Utf8).ConfigureAwait(false).GetAwaiter().GetResult();
        }
        ms.Flush();
        return ms.ToArray();
    }

    [Benchmark, BenchmarkCategory(Categories.Serialize)]
    public byte[] BrotliCompressStreamNoCompression()
    {
        ms.Position = 0;
        using (var brotli = new BrotliStream(ms, CompressionLevel.NoCompression, leaveOpen: true))
        {
            MemoryPackSerializer.SerializeAsync(brotli, value, MemoryPackSerializerOptions.Utf8).ConfigureAwait(false).GetAwaiter().GetResult();
        }
        ms.Flush();
        return ms.ToArray();
    }

    [Benchmark, BenchmarkCategory(Categories.Serialize)]
    public byte[] LZ4CompressStreamFast()
    {


        ms.Position = 0;
        using (var lz4 = LZ4Stream.Encode(ms, Fast, leaveOpen: true))
        {
            MemoryPackSerializer.SerializeAsync(lz4, value, MemoryPackSerializerOptions.Utf8).ConfigureAwait(false).GetAwaiter().GetResult();
        }
        ms.Flush();
        return ms.ToArray();
    }

    [Benchmark, BenchmarkCategory(Categories.Serialize)]
    public byte[] LZ4CompressStreamHc04()
    {


        ms.Position = 0;
        using (var lz4 = LZ4Stream.Encode(ms, L04HC, leaveOpen: true))
        {
            MemoryPackSerializer.SerializeAsync(lz4, value, MemoryPackSerializerOptions.Utf8).ConfigureAwait(false).GetAwaiter().GetResult();
        }
        ms.Flush();
        return ms.ToArray();
    }

    [Benchmark, BenchmarkCategory(Categories.Serialize)]
    public byte[] LZ4CompressStreamL10Opt()
    {
        ms.Position = 0;
        using (var lz4 = LZ4Stream.Encode(ms, L10Opt, leaveOpen: true))
        {
            MemoryPackSerializer.SerializeAsync(lz4, value, MemoryPackSerializerOptions.Utf8).ConfigureAwait(false).GetAwaiter().GetResult();
        }
        ms.Flush();
        return ms.ToArray();
    }

    //Decompress

    [Benchmark(Baseline = true), BenchmarkCategory(Categories.Deserialize)]
    public T? DeserializeMemoryPack()
    {
        return MemoryPackSerializer.Deserialize<T>(normal);
    }

    [Benchmark(), BenchmarkCategory(Categories.Deserialize)]
    public T? DecompressBrotli()
    {
        using var decompressor = new BrotliDecompressor();
        return MemoryPackSerializer.Deserialize<T>(decompressor.Decompress(brotliFast));
    }

    [Benchmark(), BenchmarkCategory(Categories.Deserialize)]
    public T? DecompressBrotliStream()
    {
        using (var ms2 = new MemoryStream(brotliFast))
        using (var brotli = new BrotliStream(ms2, CompressionMode.Decompress))
        {
            return MemoryPackSerializer.DeserializeAsync<T>(brotli).GetAwaiter().GetResult();
        }
    }

    [Benchmark(), BenchmarkCategory(Categories.Deserialize)]
    public T? LZ4Decompress()
    {
        using (var ms2 = new MemoryStream(lz4Fast))
        using (var lz4 = LZ4Stream.Decode(ms2, leaveOpen: true))
        {
            return MemoryPackSerializer.DeserializeAsync<T>(lz4).GetAwaiter().GetResult();
        }
    }

    // GZip



    //[Benchmark]
    //public byte[] GZipCompressStreamFastest()
    //{
    //    ms.Position = 0;
    //    using (var GZip = new GZipStream(ms, CompressionLevel.Fastest, leaveOpen: true))
    //    {
    //        MemoryPackSerializer.SerializeAsync(GZip, value, MemoryPackSerializeOptions.Utf8).ConfigureAwait(false).GetAwaiter().GetResult();
    //    }
    //    ms.Flush();
    //    return ms.ToArray();
    //}

    //[Benchmark]
    //public byte[] GZipCompressStreamOptimial()
    //{
    //    ms.Position = 0;
    //    using (var GZip = new GZipStream(ms, CompressionLevel.Optimal, leaveOpen: true))
    //    {
    //        MemoryPackSerializer.SerializeAsync(GZip, value, MemoryPackSerializeOptions.Utf8).ConfigureAwait(false).GetAwaiter().GetResult();
    //    }
    //    ms.Flush();
    //    return ms.ToArray();
    //}

    //[Benchmark]
    //public byte[] GZipCompressStreamSmallestSize()
    //{
    //    ms.Position = 0;
    //    using (var GZip = new GZipStream(ms, CompressionLevel.SmallestSize, leaveOpen: true))
    //    {
    //        MemoryPackSerializer.SerializeAsync(GZip, value, MemoryPackSerializeOptions.Utf8).ConfigureAwait(false).GetAwaiter().GetResult();
    //    }
    //    ms.Flush();
    //    return ms.ToArray();
    //}

    //[Benchmark]
    //public byte[] GZipCompressStreamNoCompression()
    //{
    //    ms.Position = 0;
    //    using (var GZip = new GZipStream(ms, CompressionLevel.NoCompression, leaveOpen: true))
    //    {
    //        MemoryPackSerializer.SerializeAsync(GZip, value, MemoryPackSerializeOptions.Utf8).ConfigureAwait(false).GetAwaiter().GetResult();
    //    }
    //    ms.Flush();
    //    return ms.ToArray();
    //}

}
