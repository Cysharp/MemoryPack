using System.Buffers;

namespace MemoryPack;

public interface IMemoryPackFormatterRegister
{
    static abstract void RegisterFormatter();
}

public interface IMemoryPackable<T> : IMemoryPackFormatterRegister
{
    // note: serialize parameter should be `ref readonly` but current lang spec can not.
    // see proposal https://github.com/dotnet/csharplang/issues/6010

    static abstract void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref T? value)
        where TBufferWriter : IBufferWriter<byte>;
    static abstract void Deserialize(ref MemoryPackReader reader, scoped ref T? value);
}
