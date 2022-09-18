using MemoryPack;
using MemoryPack.Formatters;
using MessagePack;
using Microsoft.Extensions.DependencyInjection;
using System.Buffers;
using System.IO.Pipelines;
using System.Runtime.CompilerServices;
using System.Xml.Linq;

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


        }

        {
            var writer = new ArrayBufferWriter<byte>();
            var context = new MemoryPackWriter<ArrayBufferWriter<byte>>(ref writer);
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

        //var v = mc.MyProperty;
        //vf.Deserialize(ref ctx, ref v);
    }

    // [RootCommand]
    public void Run3()
    {
        var foo = MemoryPackSerializer.Serialize<IReadOnlyCollection<Version>>(null);



    }

    //[RootCommand]
    public void Run4()
    {


    }

    [RootCommand]
    public void Run5()
    {
        var mc = new MyClass() { MyProperty = 9999, MyProperty2 = "foobarbaz" };
        var writer = new ArrayBufferWriter<byte>();
        var writer2 = new MemoryPackWriter<ArrayBufferWriter<byte>>(ref writer);

        var bytes = MemoryPackSerializer.Serialize(mc);
        var mc2 = MemoryPackSerializer.Deserialize<MyClass>(bytes);
        
    }
}

[MemoryPackable]
public partial class MyClass
{
    public int MyProperty { get; set; }
    public string MyProperty2 { get; set; } = default!;

    public static void Foo()
    {
    }
}


//[MessagePackObject]
////[GenerateSerializer]
//public partial class MyClass : IMemoryPackable<MyClass>
//{
//    [Key(0)]
//    public int X { get; set; }
//    [Key(1)]
//    public int Y { get; set; }
//    [Key(2)]
//    public int Z { get; set; }
//    [Key(3)]
//    public string? FirstName { get; set; }
//    [Key(4)]
//    public string? LastName { get; set; }

//    static MyClass()
//    {
//        if (!MemoryPackFormatterProvider.IsRegistered<MyClass>())
//        {
//            MemoryPackFormatterProvider.Register(new Formatter());
//        }
//    }

//    public static void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref MyClass? value)
//        where TBufferWriter : IBufferWriter<byte>
//    {
//        if (value == null)
//        {
//            writer.WriteNullObjectHeader();
//            return;
//        }
//        else
//        {
//            writer.WriteObjectHeader(4);
//        }

//        {
//            ref var spanRef = ref writer.GetSpanReference(sizeof(int) /* X */ + sizeof(int) /* Y */ + sizeof(int) /* Z */);
//            Unsafe.WriteUnaligned(ref spanRef, value.X);
//            Unsafe.WriteUnaligned(ref Unsafe.Add(ref spanRef, sizeof(int)), value.Y);
//            Unsafe.WriteUnaligned(ref Unsafe.Add(ref spanRef, sizeof(int) + sizeof(int)), value.Z);
//            writer.Advance(sizeof(int) + sizeof(int) + sizeof(int));
//        }
//        {
//            writer.WriteString(value.FirstName);
//        }
//        {
//            writer.WriteString(value.LastName);
//        }
//    }

//    public static void Deserialize(ref MemoryPackReader reader, scoped ref MyClass? value)
//    {
//        if (value == null)
//        {
//            value = new MyClass();
//        }

//        reader.TryReadObjectHeader(out var count);

//        {
//            ref var spanRef = ref reader.GetSpanReference(sizeof(int) + sizeof(int) + sizeof(int));
//            value.X = Unsafe.ReadUnaligned<int>(ref spanRef);
//            value.Y = Unsafe.ReadUnaligned<int>(ref Unsafe.Add(ref spanRef, sizeof(int)));
//            value.Z = Unsafe.ReadUnaligned<int>(ref Unsafe.Add(ref spanRef, sizeof(int) + sizeof(int)));
//            reader.Advance(sizeof(int) + sizeof(int) + sizeof(int));
//        }
//        {
//            value.FirstName = reader.ReadString();
//        }
//        {
//            value.LastName = reader.ReadString();
//        }
//    }

//    class Formatter : MemoryPackFormatter<MyClass>
//    {
//        public override void Deserialize(ref MemoryPackReader reader, scoped ref MyClass? value)
//        {
//            MyClass.Deserialize(ref reader, ref value);
//        }

//        public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref MyClass? value)
//        {
//            writer.WritePackable(value);
//        }
//    }
//}


////[MessagePackObject]
//[MemoryPackable]
//public struct Vector3
//{
//    //  [Key(0)]
//    public float X;
//    //[Key(1)]
//    public float Y;
//    //[Key(2)]
//    public float Z;
//}

//public record TakoYaki
//{
//    public required string OK { get; init; }
//}


//public static class MyExtensions
//{
//    public static bool Hoge<T>(ref this T source)
//        where T : struct
//    {
//        return true;
//    }
//}


////[MemoryPackable]
////[MemoryPackUnion(0, typeof(UnionSample))]
////[MemoryPackUnion(1, typeof(Derived))]
////public class UnionSample : IMemoryPackable<UnionSample>
////{
////    public static void Deserialize(ref MemoryPackReader reader, scoped ref UnionSample? value)
////    {
////        throw new NotImplementedException();
////    }

////    static readonly Dictionary<Type, byte> __typeToTag = new Dictionary<Type, byte>()
////    {
////        { typeof(UnionSample), 0 },
////        { typeof(Derived), 1 },
////    };

////    public static void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref UnionSample? value) where TBufferWriter : IBufferWriter<byte>
////    {
////        if (value == null)
////        {
////            return;
////        }

////        if (!__typeToTag.TryGetValue(value.GetType(), out var tag))
////        {
////            // throws.
////        }

////        writer.WriteUnionHeader(tag);
////        switch (tag)
////        {
////            case 0:
////                {
////                    SerializeSelf(ref writer, ref value);
////                }
////                break;
////            case 1:
////                {
////                    var v = (Derived)value;
////                    writer.WriteObject(v);
////                    break;
////                }
////        }

////        throw new NotImplementedException();
////    }

////    static void SerializeSelf<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref UnionSample? value)
////        where TBufferWriter : IBufferWriter<byte>
////    {
////        // serialize self
////    }
////}

////[MemoryPackable]
////public class Derived : UnionSample
////{

////}


//[MemoryPackable]
//public partial class MySample
//{
//    public int MyProperty { get; set; }


//    [MemoryPackOnSerializing]
//    void BeforeSerialize() { }


//    [MemoryPackOnSerializing]
//    static void StaticBeforeSerialize() { }

//    [MemoryPackOnSerialized]
//    void AfterSerialize() { }

//    [MemoryPackOnSerialized]
//    static void StaticAfterSerialize() { }

//    [MemoryPackOnDeserializing]
//    static void StaticBeforeDeserialize() { }

//    [MemoryPackOnDeserializing]
//    void BeforeDeserializing() { }

//    [MemoryPackOnDeserialized]
//    void AfterDeserialize() { }

//    [MemoryPackOnDeserialized]
//    static void StaticAfterDeserialize() { }
//}


//public class Bar : MemoryPackFormatter<Bar>
//{
//    [ModuleInitializer]
//    internal static void RegisterFormatter()
//    {
//        if (!MemoryPackFormatterProvider.IsRegistered<Bar>())
//        {
//            MemoryPackFormatterProvider.Register(new BarFormatter());
//        }
//    }


//    public override void Deserialize(ref MemoryPackReader reader, scoped ref Bar? value)
//    {
//        throw new NotImplementedException();
//    }

//    public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref Bar? value)
//    {
//        throw new NotImplementedException();
//    }

//    sealed class BarFormatter : MemoryPackFormatter<Bar>
//    {
//        public override void Deserialize(ref MemoryPackReader reader, scoped ref Bar? value)
//        {
//            throw new NotImplementedException();
//        }

//        public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref Bar? value)
//        {
//            throw new NotImplementedException();
//        }
//    }
//}

//public class Foo<T> : IMemoryPackable<Foo<T>>
//{
//    public T MyProperty { get; set; } = default!;
//    public T[] MyProperty2 { get; set; } = default!;

//    static Foo()
//    {
//        if (!MemoryPackFormatterProvider.IsRegistered<Foo<T>>())
//        {
//            MemoryPackFormatterProvider.Register<Foo<T>>(new Formatter());
//        }
//        if (!MemoryPackFormatterProvider.IsRegistered<T[]>())
//        {
//            // MemoryPackFormatterProvider.Register<T[]>(RuntimeHelpers.IsReferenceOrContainsReferences<T>() ? new ArrayFormatter<T>() : new DangerousUnmanagedTypeArrayFormatter<T>());
//        }
//    }

//    static void IMemoryPackable<Foo<T>>.Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref Foo<T>? value)
//    {


//        // throw new NotImplementedException();
//        // write T...
//    }

//    static void IMemoryPackable<Foo<T>>.Deserialize(ref MemoryPackReader reader, scoped ref Foo<T>? value)
//    {
//        throw new NotImplementedException();
//    }

//    sealed class Formatter : MemoryPackFormatter<Foo<T>>
//    {
//        public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref Foo<T>? value)
//        {
//            writer.WritePackable(value);
//        }

//        public override void Deserialize(ref MemoryPackReader reader, scoped ref Foo<T>? value)
//        {
//            reader.ReadPackable(ref value);
//        }
//    }
//}

//[MemoryPackable(allowPrivate: true)]
//public record TestAttrInRecord : IMemoryPackable<TestAttrInRecord>
//{

//    static void IMemoryPackable<TestAttrInRecord>.Deserialize(ref MemoryPackReader reader,scoped ref TestAttrInRecord? value)
//    {
//        throw new NotImplementedException();
//    }

//    static void IMemoryPackable<TestAttrInRecord>.Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref TestAttrInRecord? value)
//    {
//        throw new NotImplementedException();
//    }
//}

//[MemoryPackable(true)]
//public record struct TestAttrInStructRecord : IMemoryPackable<TestAttrInStructRecord>
//{
//    static void IMemoryPackable<TestAttrInStructRecord>.Deserialize(ref MemoryPackReader reader,scoped ref TestAttrInStructRecord value)
//    {
//        reader.ReadUnmanaged(out value);
//    }

//    static void IMemoryPackable<TestAttrInStructRecord>.Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref TestAttrInStructRecord value)
//    {
//        writer.WriteUnmanaged(value);
//    }
//}

//internal partial record struct TestAttrInProp
//{

//    [MemoryPackable]
//    public int MyProperty { get; set; }
//}


//partial record struct TestAttrInProp
//{
//}

//[MemoryPackable]
//public class HugaHuga
//{
//    public TestAttrInRecord? MyProperty { get; set; }
//}


//partial class MySample : IMemoryPackable<MySample>
//{
//    static MySample()
//    {
//        if (!MemoryPackFormatterProvider.IsRegistered<MySample>())
//        {
//            MemoryPackFormatterProvider.Register(new MySampleFormatter());
//        }
//    }

//    static void IMemoryPackable<MySample>.Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref MySample? value)
//    {

//        if (value == null)
//        {
//            writer.WriteNullObjectHeader();
//            return;
//        }

//        writer.WriteObjectHeader(1);
//        writer.WriteUnmanaged(value.MyProperty);

//    }

//    static void IMemoryPackable<MySample>.Deserialize(ref MemoryPackReader reader, scoped ref MySample? value)
//    {

//        if (!reader.TryReadObjectHeader(out var count))
//        {
//            value = default;
//            return;
//        }

//        if (count != 1) ThrowHelper.ThrowInvalidPropertyCount(1, count);
//        reader.ReadUnmanaged<Int32>(out var __MyProperty);
//        value = new MySample()
//        {
//            MyProperty = __MyProperty
//        };

//    }

//    sealed class MySampleFormatter : MemoryPackFormatter<MySample>
//    {
//        public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref MySample? value)
//        {
//            writer.WritePackable(value);
//        }

//        public override void Deserialize(ref MemoryPackReader reader, scoped ref MySample? value)
//        {
//            reader.ReadPackable(ref value);
//        }
//    }
//}
