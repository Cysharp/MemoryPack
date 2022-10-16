using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

#nullable enable
namespace MemoryPack.Formatters {

#if NET7_0_OR_GREATER

public sealed class MemoryPackableFormatter<T> : MemoryPackFormatter<T>
    where T : IMemoryPackable<T>
{
    public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref T? value)
    {
        writer.WritePackable(value);
    }

    public override void Deserialize(ref MemoryPackReader reader, ref T? value)
    {
        reader.ReadPackable(ref value);
    }
}

#endif

}