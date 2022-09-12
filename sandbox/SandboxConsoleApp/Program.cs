using MemoryPack;
using MemoryPack.Formatters;
using System.Buffers;
using System.Runtime.CompilerServices;

ConsoleAppFramework.ConsoleApp.Run<StandardRunner>(args);

public class StandardRunner : ConsoleAppBase
{
    // [RootCommand]
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


            var i = int.Parse("100");
            i.Hoge();
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

    // [RootCommand]
    public void Run2()
    {
        var mc = new MyClass();

        var bin = MemoryPackSerializer.Serialize(new Version(1, 10, 100, 1000));

        var vf = new VersionFormatter();
        var ctx = new DeserializationContext(bin);

        var v = mc.MyProperty;
        vf.Deserialize(ref ctx, ref v);
    }

    [RootCommand]
    public void Run3()
    {
        var v3 = Enumerable.Repeat(new Vector3 { X = 10.3f, Y = 40.5f, Z = 13411.3f }, 1000).ToArray();
        var serialize2 = MemoryPackSerializer.Serialize(v3);
        var writer = new ArrayBufferWriter<byte>(serialize2.Length);
        MemoryPackSerializer.Serialize(ref writer, v3);
        writer.Clear();
        
    }
}


//[MessagePackObject]
public struct Vector3
{
  //  [Key(0)]
    public float X;
    //[Key(1)]
    public float Y;
    //[Key(2)]
    public float Z;
}


public static class MyExtensions
{
    public static bool Hoge<T>(ref this T source)
        where T : struct
    {
        return true;
    }
}

public class MyClass
{
    public Version? MyProperty { get; set; }
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
