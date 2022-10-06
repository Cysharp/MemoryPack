using MemoryPack;
using System.Buffers;

namespace Benchmark.Micro;

public class StaticAbstractVsFormatter
{
    IntClass value;
    ArrayBufferWriter<byte> bufferWriter;
    IMemoryPackFormatter<IntClass> formatter;

    public StaticAbstractVsFormatter()
    {
        this.value = new IntClass { Value = 999999 };
        this.bufferWriter = new ArrayBufferWriter<byte>(99999);

        var writer = new MemoryPackWriter<ArrayBufferWriter<byte>>(ref bufferWriter, MemoryPackSerializeOptions.Default);
        this.formatter = writer.GetFormatter<IntClass>();
    }

    [Benchmark]
    public void WriteValue()
    {
        bufferWriter.Clear();
        var writer = new MemoryPackWriter<ArrayBufferWriter<byte>>(ref bufferWriter, MemoryPackSerializeOptions.Default);
        
        writer.WriteValue(value); // GetFormatter<T>.Serialize(ref writer, ref value);
    }

    [Benchmark]
    public void FormatterSerialize()
    {
        bufferWriter.Clear();
        var writer = new MemoryPackWriter<ArrayBufferWriter<byte>>(ref bufferWriter, MemoryPackSerializeOptions.Default);
        formatter.Serialize(ref writer, ref value!); // IMemoryPackFormatter<T>.Serialize(ref writer, rf value)
    }

    [Benchmark(Baseline = true)]
    public void WritePackable()
    {
        bufferWriter.Clear();
        var writer = new MemoryPackWriter<ArrayBufferWriter<byte>>(ref bufferWriter, MemoryPackSerializeOptions.Default);
        writer.WritePackable(value); // T.Serialize(ref writer, ref value);
    }

    [Benchmark]
    public void Direct()
    {
        bufferWriter.Clear();
        var writer = new MemoryPackWriter<ArrayBufferWriter<byte>>(ref bufferWriter, MemoryPackSerializeOptions.Default);
        writer.WriteUnmanagedWithObjectHeader(1, value.Value);
    }















}

[MemoryPackable]
public partial class IntClass
{
    public int Value { get; set; }
}


