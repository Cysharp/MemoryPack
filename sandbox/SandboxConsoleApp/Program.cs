#pragma warning disable CS8600

using MemoryPack;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;


Console.WriteLine("ok" );

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
public partial class ListGenerics<T> : List<T>
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


