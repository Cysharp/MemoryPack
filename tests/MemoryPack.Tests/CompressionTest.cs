using MemoryPack.Compression;
using MemoryPack.Tests.Models;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MemoryPack.Tests;

public class CompressionTest
{
    [Fact]
    public void CompressDecompress()
    {
        // pattern1, huge compression
        var pattern1 = Enumerable.Range(1, 1000).Select(_ => string.Concat(Enumerable.Repeat("http://", 1000)))
            .Prepend("hogehogehugahugahugahugahogehoge!")
            .ToArray();

        // pattern2, small compression
        var pattern2 = new string[] { "a", "b", "c" };

        var texts = new[] { pattern1, pattern2 };
        foreach (var text in texts)
        {
#if NET7_0_OR_GREATER
            using var brotli = new BrotliCompressor();
#else
            using var brotli = new BrotliCompressor(CompressionLevel.Fastest);
#endif

            MemoryPackSerializer.Serialize(brotli, text);

            var originalSerialized = MemoryPackSerializer.Serialize(text);

            var array1 = brotli.ToArray();

            var arrayWriter = new ArrayBufferWriter<byte>();
            brotli.CopyTo(arrayWriter);

            var array2 = arrayWriter.WrittenMemory;

            // check BrotliCompressor ToArray()/CopyTo returns same result.
            array1.AsSpan().SequenceEqual(array2.Span).Should().BeTrue();

            using var decompressor = new BrotliDecompressor();

            var decompressed = decompressor.Decompress(array1);

            var referenceDecompress = ReferenceDecompress(array1);
            var decompressedArray = decompressed.ToArray();

            // check decompress results correct
            referenceDecompress.SequenceEqual(decompressedArray).Should().BeTrue();

            originalSerialized.AsSpan().SequenceEqual(decompressed.ToArray()).Should().BeTrue();

            // deserialized check
            var more = MemoryPackSerializer.Deserialize<string[]>(decompressed);

            text.Length.Should().Be(more!.Length);
            foreach (var (first, second) in text.Zip(more))
            {
                first.AsSpan().SequenceEqual(second).Should().BeTrue();
            }
        }
    }

    [Fact]
    public void AttributeCompression()
    {

        // pattern1, huge compression
        var pattern1 = Enumerable.Range(1, 1000).Select(_ => string.Concat(Enumerable.Repeat("http://", 1000)))
            .Prepend("hogehogehugahugahugahugahogehoge!")
            .ToArray();

        // pattern2, small compression
        var pattern2 = new string[] { "a", "b", "c" };

        foreach (var pattern in new[] { pattern1, pattern2 })
        {
            var data = new CompressionAttrData()
            {
                Id1 = 14141,
                Data = Encoding.UTF8.GetBytes(string.Concat(pattern)),
                String = string.Concat(pattern),
                Id2 = 99999
            };

            var bin = MemoryPackSerializer.Serialize(data);
            var v2 = MemoryPackSerializer.Deserialize<CompressionAttrData>(bin)!;

            v2.Id1.Should().Be(data.Id1);
            v2.Id2.Should().Be(data.Id2);
            v2.Data.Should().Equal(data.Data);
            v2.String.Should().Be(data.String);
        }
    }

#if NET7_0_OR_GREATER

    [Fact]
    public void AttributeCompression2()
    {

        // pattern1, huge compression
        var pattern1 = Enumerable.Range(1, 1000).Select(_ => string.Concat(Enumerable.Repeat("http://", 1000)))
            .Prepend("hogehogehugahugahugahugahogehoge!")
            .ToArray();

        // pattern2, small compression
        var pattern2 = new string[] { "a", "b", "c" };

        foreach (var pattern in new[] { pattern1, pattern2 })
        {
            var data = new CompressionAttrData2()
            {
                Id1 = 14141,
                Data = Encoding.UTF8.GetBytes(string.Concat(pattern)),
                Two = new StandardTypeTwo { One = 9999, Two = 1111 },
                String = string.Concat(pattern),
                Id2 = 99999
            };

            var bin = MemoryPackSerializer.Serialize(data);
            var v2 = MemoryPackSerializer.Deserialize<CompressionAttrData2>(bin)!;

            v2.Id1.Should().Be(data.Id1);
            v2.Id2.Should().Be(data.Id2);
            v2.Data.Should().Equal(data.Data);
            v2.String.Should().Be(data.String);

            v2.Two.One.Should().Be(data.Two.One);
            v2.Two.Two.Should().Be(data.Two.Two);
        }
    }

#endif

    byte[] ReferenceDecompress(byte[] bytes)
    {
        using (var ms = new MemoryStream(bytes))
        using (var brotli = new BrotliStream(ms, CompressionMode.Decompress))
        {
            var dest = new MemoryStream();
            brotli.CopyTo(dest);
            return dest.ToArray();
        }
    }
}


[MemoryPackable]
public partial class CompressionAttrData
{
    public int Id1 { get; set; }

    [BrotliFormatter]
    public byte[] Data { get; set; } = default!;

    [BrotliStringFormatter]
    public string String { get; set; } = default!;

    public int Id2 { get; set; }
}

#if NET7_0_OR_GREATER

[MemoryPackable]
public partial class CompressionAttrData2
{
    public int Id1 { get; set; }

    [BrotliFormatter]
    public byte[] Data { get; set; } = default!;

    [BrotliStringFormatter]
    public string String { get; set; } = default!;

    [BrotliFormatter<StandardTypeTwo>]
    public StandardTypeTwo Two { get; set; } = default!;

    public int Id2 { get; set; }
}

#endif
