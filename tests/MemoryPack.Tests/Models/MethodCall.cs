using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MemoryPack.Tests.Models;


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
