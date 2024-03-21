using MemoryPack.Compression;
using System;
using System.Buffers;
using System.IO.Compression;

namespace MemoryPack.Tests;

public class BrotliTest
{
    [Fact]
    public void LargeByteArray()
    {
        var data = new SaveData();

        var bin = data.MemCmpSerialize();
        data.MemDecmpDeserialize(bin);
    }

    [Fact]
    public void EncodeEmptyCntent()
    {
        var buffer = new ArrayBufferWriter<byte>();
        using var state = MemoryPackWriterOptionalStatePool.Rent(null);
        var writer = new MemoryPackWriter<ArrayBufferWriter<byte>>(ref buffer, state);

        using var compressor = new BrotliCompressor(CompressionLevel.Fastest);
        compressor.CopyTo(ref writer);

        using var decompressor = new BrotliDecompressor();
        decompressor.Decompress(compressor.ToArray()).ToArray().Should().BeEmpty();
    }

    [Fact]
    public void EncodeEmptyFinalBlock()
    {
        using var state = MemoryPackWriterOptionalStatePool.Rent(null);

        var compressor = new BrotliCompressor(CompressionLevel.Fastest);
        var coWriter = new MemoryPackWriter<BrotliCompressor>(ref compressor, state);

        var bytes = new byte[248];
        Random.Shared.NextBytes(bytes);
        coWriter.WriteUnmanagedArray(bytes);
        coWriter.Flush();

        var buffer = new ArrayBufferWriter<byte>();
        compressor.CopyTo(buffer);

        using var readerState = MemoryPackReaderOptionalStatePool.Rent(null);
        using var decompressor = new BrotliDecompressor();
        var decompressed = decompressor.Decompress(compressor.ToArray());
        var reader = new MemoryPackReader(in decompressed, readerState);

        reader.ReadArray<byte>().Should().BeEquivalentTo(bytes);
        compressor.Dispose();
    }
}

[MemoryPackable]
public partial class SaveData
{
    public byte[] Areas = new byte[10000000];

    public SaveData()
    {
        var rnd = new Random(1000);
        for (int i = 0; i < Areas.Length; ++i)
        {
            // if (rnd.Next() % 2 != 0) continue;
            Areas[i] = (byte)(rnd.Next() % 256);
        }
    }

    public byte[] MemCmpSerialize()
    {
        using var cp = new BrotliCompressor();
        MemoryPackSerializer.Serialize(cp, this);
        return cp.ToArray();
    }

    public bool MemDecmpDeserialize(byte[] bin)
    {
        try
        {
            using var dcp = new BrotliDecompressor();
            var buffer = dcp.Decompress(bin);
            var data = MemoryPackSerializer.Deserialize<SaveData>(buffer);
            if (data is null) return false;
            Array.Copy(data.Areas, this.Areas, data.Areas.Length);
        }
        catch
        {
            return false;
        }
        return true;
    }
}
