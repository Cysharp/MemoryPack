using Benchmark.BenchmarkNetUtilities;
using BinaryPack.Models.Helpers;
using MemoryPack;
using System.Net.Http;

namespace Benchmark.Benchmarks;

[PayloadColumn]
public class Utf16VsUtf8
{
    readonly string ascii;
    readonly string japanese;
    readonly string largeAscii;

    readonly byte[] utf16Jpn;
    readonly byte[] utf8Jpn;
    readonly byte[] utf16Ascii;
    readonly byte[] utf8Ascii;
    readonly byte[] utf16LargeAscii;
    readonly byte[] utf8LargeAscii;

    public Utf16VsUtf8()
    {
        this.japanese = "あいうえおかきくけこさしすせそたちつてとなにぬねのはひふへほまみむめもやゆよらりるれろわをん";
        this.ascii = "abcedfghijklmnopqrstuvwxyz0123456789";
        this.utf16Jpn = MemoryPackSerializer.Serialize(japanese, MemoryPackSerializeOptions.Default);
        this.utf8Jpn = MemoryPackSerializer.Serialize(japanese, MemoryPackSerializeOptions.Utf8);
        this.utf16Ascii = MemoryPackSerializer.Serialize(ascii, MemoryPackSerializeOptions.Default);
        this.utf8Ascii = MemoryPackSerializer.Serialize(ascii, MemoryPackSerializeOptions.Utf8);

        this.largeAscii = RandomProvider.NextString(600);
        this.utf16LargeAscii = MemoryPackSerializer.Serialize(largeAscii, MemoryPackSerializeOptions.Default);
        this.utf8LargeAscii = MemoryPackSerializer.Serialize(largeAscii, MemoryPackSerializeOptions.Utf8);
    }

    [Benchmark]
    public byte[] SerializeUtf16Ascii()
    {
        return MemoryPackSerializer.Serialize(ascii);
    }

    [Benchmark]
    public byte[] SerializeUtf16Japanese()
    {
        return MemoryPackSerializer.Serialize(japanese);
    }

    [Benchmark]
    public byte[] SerializeUtf8Ascii()
    {
        return MemoryPackSerializer.Serialize(ascii, MemoryPackSerializeOptions.Utf8);
    }

    [Benchmark]
    public byte[] SerializeUtf8Japanese()
    {
        return MemoryPackSerializer.Serialize(japanese, MemoryPackSerializeOptions.Utf8);
    }

    [Benchmark]
    public byte[] SerializeUtf16LargeAscii()
    {
        return MemoryPackSerializer.Serialize(largeAscii, MemoryPackSerializeOptions.Default);
    }

    [Benchmark]
    public byte[] SerializeUtf8LargeAscii()
    {
        return MemoryPackSerializer.Serialize(largeAscii, MemoryPackSerializeOptions.Utf8);
    }

    [Benchmark]
    public void DeserializeUtf16Ascii()
    {
        MemoryPackSerializer.Deserialize<string>(utf16Ascii);
    }

    [Benchmark]
    public void DeserializeUtf16Japanese()
    {
        MemoryPackSerializer.Deserialize<string>(utf16Jpn);
    }

    [Benchmark]
    public void DeserializeUtf8Ascii()
    {
        MemoryPackSerializer.Deserialize<string>(utf8Ascii);
    }

    [Benchmark]
    public void DeserializeUtf8Japanese()
    {
        MemoryPackSerializer.Deserialize<string>(utf8Jpn);
    }

    [Benchmark]
    public void DeserializeUtf16LargeAscii()
    {
        MemoryPackSerializer.Deserialize<string>(utf16LargeAscii);
    }

    [Benchmark]
    public void DeserializeUtf8LargeAscii()
    {
        MemoryPackSerializer.Deserialize<string>(utf8LargeAscii);
    }
}
