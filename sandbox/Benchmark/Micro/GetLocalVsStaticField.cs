using MemoryPack;
using MemoryPack.Formatters;
using MessagePack.Formatters;
using Microsoft.Diagnostics.Tracing.Parsers.ClrPrivate;
using Orleans.Serialization.Buffers;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Benchmark.Micro;

public class GetLocalVsStaticField
{
    ArrayBufferWriter<byte> bufferWriter;

    IMemoryPackFormatter<int> interfaceFormatter;
    UnmanagedFormatter<int> directFormatter;

    public GetLocalVsStaticField()
    {
        bufferWriter = new ArrayBufferWriter<byte>();
        interfaceFormatter = UnmanagedFormatter<int>.Default;
        directFormatter = UnmanagedFormatter<int>.Default;
    }

    [Benchmark(Baseline = true)]
    public void GetFromProvider()
    {
        using var state = MemoryPackWriterOptionalStatePool.Rent(MemoryPackSerializeOptions.Default);
        var writer = new MemoryPackWriter<ArrayBufferWriter<byte>>(ref bufferWriter, state);
        for (int i = 0; i < 100; i++)
        {
            writer.GetFormatter<int>().Serialize(ref writer, ref i);
        }
        bufferWriter.Clear();
    }

    [Benchmark]
    public void GetFromLocal()
    {
        using var state = MemoryPackWriterOptionalStatePool.Rent(MemoryPackSerializeOptions.Default);
        var writer = new MemoryPackWriter<ArrayBufferWriter<byte>>(ref bufferWriter, state);
        var formatter = writer.GetFormatter<int>();
        for (int i = 0; i < 100; i++)
        {
            formatter.Serialize(ref writer, ref i);
        }
        bufferWriter.Clear();
    }

    [Benchmark]
    public void GetFromFieldInterface()
    {
        using var state = MemoryPackWriterOptionalStatePool.Rent(MemoryPackSerializeOptions.Default);
        var writer = new MemoryPackWriter<ArrayBufferWriter<byte>>(ref bufferWriter, state);
        for (int i = 0; i < 100; i++)
        {
            interfaceFormatter.Serialize(ref writer, ref i);
        }
        bufferWriter.Clear();
    }

    [Benchmark]
    public void GetFromFieldDirect()
    {
        using var state = MemoryPackWriterOptionalStatePool.Rent(MemoryPackSerializeOptions.Default);
        var writer = new MemoryPackWriter<ArrayBufferWriter<byte>>(ref bufferWriter, state);
        for (int i = 0; i < 100; i++)
        {
            directFormatter.Serialize(ref writer, ref i);
        }
        bufferWriter.Clear();
    }

    [Benchmark]
    public void WriteUnmanaged()
    {
        using var state = MemoryPackWriterOptionalStatePool.Rent(MemoryPackSerializeOptions.Default);
        var writer = new MemoryPackWriter<ArrayBufferWriter<byte>>(ref bufferWriter, state);
        for (int i = 0; i < 100; i++)
        {
            writer.WriteUnmanaged(i);
        }
        bufferWriter.Clear();
    }
}
