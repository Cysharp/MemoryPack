using System.Buffers;

namespace MemoryPack;

#if NET7_0_OR_GREATER

public interface IFixedSizeMemoryPackable
{
    static abstract int Size { get; }
}

#endif

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
    static abstract void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref T? value)
        where TBufferWriter : IBufferWriter<byte>;
    static abstract void Deserialize(ref MemoryPackReader reader, scoped ref T? value);
#endif
}
