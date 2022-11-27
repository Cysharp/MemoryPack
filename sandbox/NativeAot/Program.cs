using System.Buffers;

// require this unused line for reproduce error?
var bufferWriter = new ArrayBufferWriter<byte>();

var mc = new MemPackObject();

var formatter = new MemoryPackableFormatter2<MemPackObject>();
formatter.Serialize<ArrayBufferWriter<byte>>(ref mc);


public interface IMemoryPackable2<T>
{
    static abstract void Serialize<TBufferWriter>(scoped ref T? value)
        where TBufferWriter : IBufferWriter<byte>;
}

public interface IMemoryPackFormatter2<T>
{
    void Serialize<TBufferWriter>(scoped ref T? value)
        where TBufferWriter : IBufferWriter<byte>;
}

public abstract class MemoryPackFormatter2<T> : IMemoryPackFormatter2<T>
{
    public abstract void Serialize<TBufferWriter>(scoped ref T? value)
        where TBufferWriter : IBufferWriter<byte>;
}

public sealed class MemoryPackableFormatter2<T> : MemoryPackFormatter2<T>
    where T : IMemoryPackable2<T>
{
    public override void Serialize<TBufferWriter>(scoped ref T? value)
    {
        Console.WriteLine("Before");
        T.Serialize<TBufferWriter>(ref value);
        Console.WriteLine("After");
    }
}

public class MemPackObject : IMemoryPackable2<MemPackObject>
{
    public static void Serialize<TBufferWriter>(scoped ref MemPackObject? value)
          where TBufferWriter : IBufferWriter<byte>
    {
        Console.WriteLine("OK");
    }
}
