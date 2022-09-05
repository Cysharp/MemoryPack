using MemoryPack;
using MemoryPack.Formatters;
using System.Buffers;

ConsoleAppFramework.ConsoleApp.Run<StandardRunner>(args);

public class StandardRunner : ConsoleAppBase
{
    [RootCommand]
    public void Run()
    {
        //int? v = 88;
        //v = null;
        //var bytes = MemoryPackSerializer.Serialize(v);

        var bytes = MemoryPackSerializer.Serialize(new Version(10, 20, 30, 40));

        foreach (var item in bytes)
        {
            Console.WriteLine(item);
        }

        var version = MemoryPackSerializer.Deserialize<Version>(bytes);
        Console.WriteLine(version!.ToString());
    }
}




public class Foo<T> : IMemoryPackable<Foo<T>>
{
    public T MyProperty { get; set; } = default!;

    static Foo()
    {
        MemoryPackFormatterProvider.Register(new Formatter());
    }

    static void IMemoryPackable<Foo<T>>.Serialize<TBufferWriter>(ref SerializationContext<TBufferWriter> context, ref Foo<T>? value)
    {
        // throw new NotImplementedException();
        // write T...
    }

    static void IMemoryPackable<Foo<T>>.Deserialize(ref DeserializationContext context, ref Foo<T>? value)
    {
        throw new NotImplementedException();
    }

    sealed class Formatter : IMemoryPackFormatter<Foo<T>>
    {
        public void Serialize<TBufferWriter>(ref SerializationContext<TBufferWriter> context, ref Foo<T>? value) where TBufferWriter : IBufferWriter<byte>
        {
            context.WritePackable(ref value);
        }

        public void Deserialize(ref DeserializationContext context, ref Foo<T>? value)
        {
            throw new NotImplementedException();
        }
    }
}
