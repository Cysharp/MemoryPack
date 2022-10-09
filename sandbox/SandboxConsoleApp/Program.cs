#pragma warning disable CS8600
#pragma warning disable CS0169

using MemoryPack;
using MemoryPack.Compression;
using MemoryPack.Formatters;
using Samples;
using System;
using System.Buffers;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using System.IO.Pipelines;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml.Linq;


var sb = new StringBuilder();

var foo = "tako";
sb.AppendLine($"hogemog{foo}e");

sb.AppendLine($$"""
hogemoge{{foo}}e
""");

Console.WriteLine("foo");
//var bin = MemoryPackSerializer.Serialize("hogehoge");
//var takotako = MemoryPackSerializer.Deserialize<string>(bin);

//Console.WriteLine(takotako);

// ---




//var arrayBufferWriter = new ArrayBufferWriter<byte>();




//var writer = new ArrayBufferWriter<byte>();
//var seq = new ReadOnlySequence<byte>(Encoding.UTF8.GetBytes("hogehogehugahugahage"));



// PipeWriter.Create(
//PipeWriter.Create().AsStream();
//Write(seq, writer);





//var compressed = writer.WrittenMemory;

//var stream = new BrotliStream(new MemoryStream(compressed.ToArray()), CompressionMode.Decompress, false);





//var dest = new byte[1024];


//var len = stream.Read(dest);


//var len2 = stream.Read(dest);

//var ok = BrotliDecoder.TryDecompress(compressed.Span, dest, out var written);


//var seq2 = new ReadOnlySequence<byte>(compressed);

//var writer2 = new ArrayBufferWriter<byte>();
//Read(seq2, writer2);




//MemoryPackSerializer.Serialize(brotli, "hogehogehugahuga", MemoryPackSerializeOptions.Default);


//var foobarbaz = brotli.ToArray();


//var dest2 = new byte[10];

//var decoder = new BrotliDecoder();
//var status = OperationStatus.DestinationTooSmall;
//while (status == OperationStatus.DestinationTooSmall)
//{
//    status = decoder.Decompress(tako, dest2, out var consumed, out var written);
//}


//Console.WriteLine(status);
//var status = BrotliDecoder.TryDecompress(tako, dest2, out var written2);
//Console.WriteLine(status + ":" + written2);

//var hogehoge = dest2.AsMemory(0, written2);


//var tako2 = MemoryPackSerializer.Deserialize<string>(hogehoge.Span);

//Console.WriteLine(foobarbaz.SequenceEqual(tako));


//BrotliCompression.






//var decoder = new BrotliDecoder();

//decoder.Decompress(


//public class DecompressReadOnlySequence
//{
//    ReadOnlySequence<byte> buffer;
//    BrotliDecoder decoder;


//    public DecompressReadOnlySequence()
//    {
//        // buffer.FirstSpan.


//        BrotliEncoder.GetMaxCompressedLength

//        //decoder.Decompress(


//    }

//}



//brotli.Dispose();

//var written = arrayBufferWriter.WrittenMemory;



//new BrotliStream(





//BrotliDecoder.TryDecompress(written, 




//// new BrotliDecoder().Decompress(


////encoder.Compress(, , , , true);




//encoder.Dispose();


[MemoryPackable]
[GenerateTypeScript]
public partial class FooBarBaz
{
    //public int[] MyPropertyArray { get; set; } = default!;
    //public int[] MyPropertyArray { get; set; } = default!;

    public byte[]? BytesProp { get; set; }
    public string? YoStarDearYomoda { get; private set; }
    public int[] MyPropertyArray { get; set; } = default!;
    public int[][] MyPropertyArray2 { get; set; } = default!;
    public int? MyProperty4 { get; set; }
    public Dictionary<int, List<int?>> Dictman { get; set; } = default!;
    public HashSet<int> SetMan { get; set; } = default!;

    public Sonota1 SonotaProp { get; set; } = default!;

    // TODO: check GUID, Date
    //public int MyProperty1 { get; set; }
    //public int? MyProperty2 { get; set; }
    //public Hoge? MyProperty3 { get; set; }
}


public enum Hoge : sbyte
{

}

[MemoryPackable]
[MemoryPackUnion(0, typeof(SampleUnion1))]
[MemoryPackUnion(1, typeof(SampleUnion2))]
[GenerateTypeScript]
public partial interface IMogeUnion
{
}

[MemoryPackable]
[GenerateTypeScript]
public partial class SampleUnion1 : IMogeUnion
{
    public int? MyProperty { get; set; }
}

[MemoryPackable]
[GenerateTypeScript]
public partial class SampleUnion2 : IMogeUnion
{
    public string? MyProperty { get; set; }

}


[MemoryPackable(GenerateType.Object)]
[GenerateTypeScript]
public partial class Sonota1
{
    // public NoSerializableObject? MyProperty { get; set; }
    public int HokuHoku { get; set; }
}

public class NoSerializableObject
{

}

[MemoryPackable(SerializeLayout.Explicit)]
public partial class Sonota2
{
    [MemoryPackOrder(1)]
    public int MyProperty1 { get; set; }
    [MemoryPackOrder(0)]
    public int MyProperty2 { get; set; }
}

[MemoryPackable(GenerateType.Object, SerializeLayout.Explicit)]
public partial class Sonota3
{
    [MemoryPackOrder(0)]
    public int MyProperty { get; set; }
}




//var person = new Person();
//var bin = MemoryPackSerializer.Serialize(person);

//// overwrite data to existing instance.
//MemoryPackSerializer.Deserialize(bin, ref person);

//internal void WriteCore(ReadOnlySpan<byte> buffer, bool isFinalBlock = false)
//{
//    if (_mode != CompressionMode.Compress)
//        throw new InvalidOperationException(SR.BrotliStream_Decompress_UnsupportedOperation);
//    EnsureNotDisposed();

//    OperationStatus lastResult = OperationStatus.DestinationTooSmall;
//    Span<byte> output = new Span<byte>(_buffer);
//    while (lastResult == OperationStatus.DestinationTooSmall)
//    {
//        int bytesConsumed;
//        int bytesWritten;
//        lastResult = _encoder.Compress(buffer, output, out bytesConsumed, out bytesWritten, isFinalBlock);
//        if (lastResult == OperationStatus.InvalidData)
//            throw new InvalidOperationException(SR.BrotliStream_Compress_InvalidData);
//        if (bytesWritten > 0)
//            _stream.Write(output.Slice(0, bytesWritten));
//        if (bytesConsumed > 0)
//            buffer = buffer.Slice(bytesConsumed);
//    }
//}


//internal static partial class BrotliUtils
//{
//    public const int WindowBits_Min = 10;
//    public const int WindowBits_Default = 22;
//    public const int WindowBits_Max = 24;
//    public const int Quality_Min = 0;
//    public const int Quality_Default = 4;
//    public const int Quality_Max = 11;
//    public const int MaxInputSize = int.MaxValue - 515; // 515 is the max compressed extra bytes

//    internal static int GetQualityFromCompressionLevel(CompressionLevel compressionLevel) =>
//        compressionLevel switch
//        {
//            CompressionLevel.NoCompression => Quality_Min,
//            CompressionLevel.Fastest => 1,
//            CompressionLevel.Optimal => Quality_Default,
//            CompressionLevel.SmallestSize => Quality_Max,
//            _ => throw new ArgumentException(SR.ArgumentOutOfRange_Enum, nameof(compressionLevel))
//        };
//}


[MemoryPackable(GenerateType.Collection)]
public partial class ListGenerics<T> : List<T>
{
}

[MemoryPackable]
public partial class Person
{
    public int Age { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
}


[MemoryPackable]
public partial record struct FooStruct(int x, int y);

[MemoryPackable]
public partial class Nu
{
    public UnionType? XXX;
}

[MemoryPackable]
[MemoryPackUnion(0, typeof(A))]
public partial interface UnionType
{

}

[MemoryPackable]
public partial class A : UnionType
{

}

[MemoryPackable]
public partial class Foo
{

    //Foo(int x)
    //{

    //}

    //public Foo()
    //{

    //}
}


[MemoryPackable]
public partial class MonoMono
{
    public FooBarFruit Yey { get; set; } = default!;
}


public enum FooBarFruit
{
    APple, orange, grape
}



#pragma warning disable CS8618
[MemoryPackable]
public partial class HogeHoge
{
    public BigInteger P1;
    public Version P2;
    public Uri P3;
    public TimeZoneInfo P4;
    public BitArray P5;
    public StringBuilder P6;
    public Type P7;
    public int[,] P8;
    public int[,,] P9;
    public int[,,,] P10;
    // ng
    // public int[,,,,] A5;

    // generics
    public KeyValuePair<int, int> P11;
    public Lazy<int> P12;
    public Nullable<int> P13;
    // collecition
    public ArraySegment<int> P14;
    public Memory<int> P15;
    public ReadOnlyMemory<int> P16;
    public ReadOnlySequence<int> P17;

    public List<int> P18;
    public Stack<int> P19;
    public Queue<int> P20;
    public LinkedList<int> P21;
    public HashSet<int> P22;
    public PriorityQueue<int, int> P23;
    public ObservableCollection<int> P24;
    public Collection<int> P25;
    public ConcurrentQueue<int> P26;
    public ConcurrentStack<int> P27;
    public ConcurrentBag<int> P28;
    public Dictionary<int, int> P29;
    public SortedDictionary<int, int> P30;
    public SortedList<int, int> P31;
    public ConcurrentDictionary<int, int> P32;
    public ReadOnlyCollection<int> P33;
    public ReadOnlyObservableCollection<int> P34;
    public BlockingCollection<int> P35;

    public ImmutableArray<int> P36;
    public ImmutableList<int> P37;
    public ImmutableQueue<int> P38;
    public ImmutableStack<int> P39;
    public ImmutableDictionary<int, int> P40;
    // public ImmutableSortedDictionary<int, int> P41;
    public ImmutableSortedSet<int> P42;
    public ImmutableHashSet<int> P43;
    public IImmutableList<int> P44;
    public IImmutableQueue<int> P45;
    public IImmutableStack<int> P46;
    public IImmutableDictionary<int, int> P47;
    public IImmutableSet<int> P48;
    public IEnumerable<int> P49;
    public ICollection<int> P50;
    public IReadOnlyCollection<int> P51;
    public IList<int> P52;
    public IReadOnlyList<int> P53;
    public IDictionary<int, int> P54;
    public IReadOnlyDictionary<int, int> P55;
    public ILookup<int, int> P56;
    public IGrouping<int, int> P57;
    public ISet<int> P58;
    public IReadOnlySet<int> P59;

    // tuples
    public Tuple<int, string, int> T3;
    public ValueTuple<int, string, int> VT3;
    // more
    public Nullable<MyStruct> N1;
    public KeyValuePair<string, string> N2;
}


[MemoryPackable]
public partial struct MyStruct
{
    public string? V;
}


[MemoryPackable(GenerateType.Collection)]
public partial class ListInt : List<int>
{

}

[MemoryPackable(GenerateType.Collection)]
public partial class SetInt : HashSet<int>
{
}

[MemoryPackable(GenerateType.Collection)]
public partial class DictionaryIntInt : Dictionary<int, int>
{
}



[MemoryPackable(GenerateType.Collection)]
public partial class SetGenerics<T> : HashSet<T>
{
}

[MemoryPackable(GenerateType.Collection)]
public partial class DictionaryGenerics<TK, TV> : Dictionary<TK, TV>
    where TK : notnull
{
}


//public class MyCollection<T> : List<T>, IMemoryPackFormatterRegister
//{
//    static MyCollection()
//    {
//        if (!MemoryPackFormatterProvider.IsRegistered<MyCollection<T>>())
//        {
//            MemoryPackFormatterProvider.Register<MyCollection<T>>();
//        }
//    }

//    static void IMemoryPackFormatterRegister.RegisterFormatter()
//    {
//        MemoryPackFormatterProvider.RegisterCollection<MyCollection<T?>, T>();
//    }
//}


//[MemoryPackable]
//public partial class Packable<T>
//{
//    public int TakoyakiX { get; set; }
//    [MemoryPackIgnore]
//    public object? ObjectObject { get; set; }
//    [MemoryPackIgnore]
//    public Array? StandardArray { get; set; }
//    public int[]? Array { get; set; }
//    public int[,]? MoreArray { get; set; }
//    public List<int>? List { get; set; }
//    public Version? Version { get; set; }

//    public T? TTTTT { get; set; }

//    [MemoryPackFormatter]
//    public Nazo? MyProperty { get; set; }

//    [MemoryPackFormatter]
//    public Nazo2? MyProperty2 { get; set; }
//}

//public class Nazo
//{

//}
//public class Nazo2
//{

//}

//public class Tadano
//{
//    public int MyProperty { get; set; }
//}



//public class C
//{
//    public int Foo { get; init; }
//    public required int Bar { get; init; }



[MemoryPackable]
public partial class Sample
{
    // these types are serialized by default
    public int PublicField;
    public readonly int PublicReadOnlyField;
    public int PublicProperty { get; set; }
    public int PrivateSetPublicProperty { get; private set; }
    public int ReadOnlyPublicProperty { get; }
    public int InitProperty { get; init; }
    public required int RequiredInitProperty { get; init; }

    // these types are not serialized by default
    int privateProperty { get; set; }
    int privateField;
    readonly int privateReadOnlyField;

    // use [MemoryPackIgnore] to remove target of public member
    [MemoryPackIgnore]
    public int PublicProperty2 => PublicProperty + PublicField;

    // use [MemoryPackInclude] to promote private member to serialization target
    [MemoryPackInclude]
    int privateField2;
    [MemoryPackInclude]
    int privateProperty2 { get; set; }
}
