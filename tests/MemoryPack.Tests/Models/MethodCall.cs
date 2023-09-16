using System.Buffers;
using System.Collections.Generic;

namespace MemoryPack.Tests.Models;


[MemoryPackable]
public partial struct Hoge
{
    public string MyProperty { get; set; }
}

[MemoryPackable]
public partial class MethodCall
{
    public static List<string> Log { get; } = new List<string>();

    int mp;
    public int MyProperty
    {
        get
        {
            Log.Add("Get");
            return mp;
        }
        set
        {
            Log.Add("Set");
            mp = value;
        }
    }

    public MethodCall()
    {
        Log.Add("Constructor");
    }

    [MemoryPackOnSerializing]
    public static void OnSerializing1()
    {
        Log.Add(nameof(OnSerializing1));
    }

    // check allow private.
    [MemoryPackOnSerializing]
    void OnSerializing2()
    {
        Log.Add(nameof(OnSerializing2));
    }


    [MemoryPackOnSerialized]
    static void OnSerialized1()
    {
        Log.Add(nameof(OnSerialized1));
    }

    [MemoryPackOnSerialized]
    public void OnSerialized2()
    {
        Log.Add(nameof(OnSerialized2));
    }

    [MemoryPackOnDeserializing]
    public static void OnDeserializing1()
    {
        Log.Add(nameof(OnDeserializing1));
    }

    [MemoryPackOnDeserializing]
    public void OnDeserializing2()
    {
        Log.Add(nameof(OnDeserializing2));
    }

    [MemoryPackOnDeserialized]
    public static void OnDeserialized1()
    {
        Log.Add(nameof(OnDeserialized1));
    }

    [MemoryPackOnDeserialized]
    public void OnDeserialized2()
    {
        Log.Add(nameof(OnDeserialized2));
    }

    // allow more



    [MemoryPackOnSerializing]
    public static void OnSerializing_M1<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref MethodCall? value)
#if NET7_0_OR_GREATER
        where TBufferWriter : IBufferWriter<byte>
#else
        where TBufferWriter : class, IBufferWriter<byte>
#endif
    {
        Log.Add(nameof(OnSerializing_M1));
    }

    [MemoryPackOnSerializing]
    public void OnSerializing_M2<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref MethodCall? value)
#if NET7_0_OR_GREATER
        where TBufferWriter : IBufferWriter<byte>
#else
        where TBufferWriter : class, IBufferWriter<byte>
#endif
    {
        Log.Add(nameof(OnSerializing_M2));
    }

    [MemoryPackOnSerialized]
    public static void OnSerialized_M1<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref MethodCall? value)
#if NET7_0_OR_GREATER
        where TBufferWriter : IBufferWriter<byte>
#else
        where TBufferWriter : class, IBufferWriter<byte>
#endif
    {
        Log.Add(nameof(OnSerialized_M1));
    }


    [MemoryPackOnSerialized]
    public void OnSerialized_M2<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref MethodCall? value)
#if NET7_0_OR_GREATER
        where TBufferWriter : IBufferWriter<byte>
#else
        where TBufferWriter : class, IBufferWriter<byte>
#endif
    {
        Log.Add(nameof(OnSerialized_M2));
    }



    [MemoryPackOnDeserializing]
    public static void OnDeserializing_M1(ref MemoryPackReader reader, ref MethodCall? value)
    {
        Log.Add(nameof(OnDeserializing_M1));
    }

    [MemoryPackOnDeserializing]
    public void OnDeserializing_M2(ref MemoryPackReader reader, ref MethodCall? value)
    {
        Log.Add(nameof(OnDeserializing_M2));
    }

    [MemoryPackOnDeserialized]
    public static void OnDeserialized_M1(ref MemoryPackReader reader, ref MethodCall? value)
    {
        Log.Add(nameof(OnDeserialized_M1));
    }

    [MemoryPackOnDeserialized]
    public void OnDeserialized_M2(ref MemoryPackReader reader, ref MethodCall? value)
    {
        Log.Add(nameof(OnDeserialized_M2));
    }


    // not allow parameter exists.

    //[MemoryPackOnSerialized]
    //public void InvalidMethodThatHasParameter(int x)
    //{
    //}
}


// unmanaged type can't add attributes.
//[MemoryPackable]
//public partial struct UnmanagedStructMethod
//{
//    public int X;

//    [MemoryPackOnSerialized]
//    public void Foo()
//    {
//    }
//}
