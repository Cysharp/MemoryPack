using MemoryPack.Compression;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
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
