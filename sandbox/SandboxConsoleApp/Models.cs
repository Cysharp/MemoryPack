using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SandboxConsoleApp;


[MemoryPackable]
public partial class NotSample
{
    [Utf8StringFormatter]
    public string? Custom1 { get; set; }

}


[MemoryPackable(GenerateType.VersionTolerant)]
public partial class TakoyakiY
{
    [MemoryPackOrder(1)]
    public string? Bar { get; set; }
    [MemoryPackOrder(10)]
    public int Foo { get; set; }
}

public class TakoyakiX : IMemoryPackable<TakoyakiX>
{
    public int Foo { get; set; }
    public string? Bar { get; set; }

    public static void RegisterFormatter()
    {
        MemoryPackFormatterProvider.Register(new Formatter());
    }


    public static MemoryPackWriter<ReusableLinkedArrayBufferWriter> CreateWriter(ReusableLinkedArrayBufferWriter writer, MemoryPackSerializeOptions options)
    {
        var self = writer;
        return new MemoryPackWriter<MemoryPack.Internal.ReusableLinkedArrayBufferWriter>(ref Unsafe.AsRef(self), options);
    }


    public static void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref TakoyakiX? value)
        where TBufferWriter : IBufferWriter<byte>
    {
        if (value == null)
        {
            return;
        }


        var tempBuffer = ReusableLinkedArrayBufferWriterPool.Rent();
        try
        {
            Span<int> offsets = stackalloc int[2];

            // write body
            {
                var tempWriter = new MemoryPackWriter<MemoryPack.Internal.ReusableLinkedArrayBufferWriter>(ref tempBuffer, writer.Options);


                tempWriter.WriteUnmanaged(value.Foo); offsets[0] = tempWriter.WrittenCount;
                tempWriter.WriteValue(value.Bar); offsets[1] = tempWriter.WrittenCount;



                tempWriter.Flush();
            }

            // write header
            writer.WriteObjectHeader(2);
            // write deltax
            for (int i = 0; i < 2; i++)
            {
                int delta;
                if (i == 0)
                {
                    delta = offsets[i];
                }
                else
                {
                    delta = offsets[i] - offsets[i - 1];
                }
                writer.WriteVarInt(delta);
            }

            // copy body to original writer
            tempBuffer.WriteToAndReset(ref writer);
        }
        finally
        {
            ReusableLinkedArrayBufferWriterPool.Return(tempBuffer);
        }
    }


    public static void Deserialize(ref MemoryPackReader reader, scoped ref TakoyakiX? value)
    {
        if (!reader.TryReadObjectHeader(out var count))
        {
            value = default!;
            goto END;
        }


        // get deltas
        Span<int> deltas = stackalloc int[count];
        for (int i = 0; i < count; i++)
        {
            deltas[i] = reader.ReadVarIntInt32();
        }

        int __Foo;
        string __Bar;

        var readCount = 2;
        if (count == 2)
        {
            if (value == null)
            {
                reader.ReadUnmanaged(out __Foo);
                __Bar = reader.ReadString()!;


                goto NEW;
            }
            else
            {
                __Foo = value.Foo;
                __Bar = value.Bar!;

                reader.ReadUnmanaged(out __Foo);
                __Bar = reader.ReadString()!;

                goto SET;
            }

        }
        else
        {
            if (value == null)
            {
                __Foo = default!;
                __Bar = default!;
            }
            else
            {
                __Foo = value.Foo;
                __Bar = value.Bar!;
            }




            if (count == 0) goto SKIP_READ;
            reader.ReadUnmanaged(out __Foo); if (count == 1) { readCount = 1; goto SKIP_READ; }
            // empty?
            reader.Advance(deltas[0]);

            __Bar = reader.ReadString()!; if (count == 2) { readCount = 2; goto SKIP_READ; }
            readCount = 2;

        SKIP_READ:
            if (value == null)
            {
                goto NEW;
            }
            else
            {
                goto SET;
            }
        }

    SET:
        value.Foo = __Foo;
        value.Bar = __Bar;
        goto READ_END;

    NEW:
        if (count == readCount) return;
        value = new TakoyakiX()
        {
            Foo = __Foo,
            Bar = __Bar
        };

    READ_END:
        // skip read all
        if (count == readCount) return;

        for (int i = readCount; i < count; i++)
        {
            reader.Advance(deltas[i]);
        }
    END:
        return;
    }

    class Formatter : MemoryPackFormatter<TakoyakiX>
    {
        public override void Deserialize(ref MemoryPackReader reader, scoped ref TakoyakiX? value)
        {
            TakoyakiX.Deserialize(ref reader, ref value);
        }

        public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref TakoyakiX? value)
        {
            TakoyakiX.Serialize(ref writer, ref value);
        }
    }
}
