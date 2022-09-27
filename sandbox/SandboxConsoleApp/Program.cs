#pragma warning disable CS8600

using MemoryPack;
using MemoryPack.Formatters;
using System;
using System.Buffers;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;





Console.WriteLine("ok");

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
