using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

#nullable enable
using System.Buffers;

namespace MemoryPack {

public interface IMemoryPackFormatterRegister
{
#if NET7_0_OR_GREATER
    static abstract void RegisterFormatter();
#endif
}

public interface IMemoryPackable<T> : IMemoryPackFormatterRegister
{
    // note: serialize parameter should be `ref readonly` but current lang spec can not.
    // see proposal https://github.com/dotnet/csharplang/issues/6010

#if NET7_0_OR_GREATER
    static abstract void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref T? value)
        where TBufferWriter : IBufferWriter<byte>;
    static abstract void Deserialize(ref MemoryPackReader reader, ref T? value);
#endif
}

}