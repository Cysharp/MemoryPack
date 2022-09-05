using MemoryPack;
using MemoryPack.Formatters;
using System.Buffers;
using System.Runtime.CompilerServices;

ConsoleAppFramework.ConsoleApp.Run<StandardRunner>(args);

public class StandardRunner : ConsoleAppBase
{
    [RootCommand]
    public void Run()
    {
        //int? v = 88;
        //v = null;
        //var bytes = MemoryPackSerializer.Serialize(v);

        var foo = new CollectionFormatter<int>();
        var bar = new CollectionFormatter<int?>();
        var v1 = new List<int> { 1, 10, 100 } as IReadOnlyCollection<int>;
        var v2 = new int?[] { 1, null, 100 } as IReadOnlyCollection<int?>;
        var v3 = new[] { 1, 10, 100 };

        {
            if (!MemoryPackFormatterProvider.IsRegistered<int[]>())
            {
                MemoryPackFormatterProvider.Register<int[]>(new UnmanagedTypeArrayFormatter<int>());
            }

            MemoryPackFormatterProvider.Register(new UnmanagedTypeArrayFormatter<int>());

            var xs = MemoryPackSerializer.Serialize(v3);




        }

        {
            var writer = new ArrayBufferWriter<byte>();
            var context = new SerializationContext<ArrayBufferWriter<byte>>(writer);
            foo.Serialize(ref context, ref v1);

            context.Flush();

            var a = writer.WrittenMemory;
        }


        //var bytes = MemoryPackSerializer.Serialize(new Version(10, 20, 30, 40));

        //foreach (var item in bytes)
        //{
        //    Console.WriteLine(item);
        //}

        //var version = MemoryPackSerializer.Deserialize<Version>(bytes);
        //Console.WriteLine(version!.ToString());
    }
}


public class Bar : IMemoryPackFormatter<Bar>
{
    [ModuleInitializer]
    internal static void RegisterFormatter()
    {
        if (!MemoryPackFormatterProvider.IsRegistered<Bar>())
        {
            MemoryPackFormatterProvider.Register(new BarFormatter());
        }
    }


    public void Deserialize(ref DeserializationContext context, ref Bar? value)
    {
        throw new NotImplementedException();
    }

    public void Serialize<TBufferWriter>(ref SerializationContext<TBufferWriter> context, ref Bar? value) where TBufferWriter : IBufferWriter<byte>
    {
        throw new NotImplementedException();
    }

    sealed class BarFormatter : IMemoryPackFormatter<Bar>
    {
        public void Deserialize(ref DeserializationContext context, ref Bar? value)
        {
            throw new NotImplementedException();
        }

        public void Serialize<TBufferWriter>(ref SerializationContext<TBufferWriter> context, ref Bar? value)
            where TBufferWriter : IBufferWriter<byte>
        {
            throw new NotImplementedException();
        }
    }
}

public class Foo<T> : IMemoryPackable<Foo<T>>
{
    public T MyProperty { get; set; } = default!;
    public T[] MyProperty2 { get; set; } = default!;

    static Foo()
    {
        if (!MemoryPackFormatterProvider.IsRegistered<Foo<T>>())
        {
            MemoryPackFormatterProvider.Register<Foo<T>>(new Formatter());
        }
        if (!MemoryPackFormatterProvider.IsRegistered<T[]>())
        {
            MemoryPackFormatterProvider.Register<T[]>(RuntimeHelpers.IsReferenceOrContainsReferences<T>() ? new ArrayFormatter<T>() : new DangerousUnmanagedTypeArrayFormatter<T>());
        }
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
