using MemoryPack.Compression;
using System;

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
