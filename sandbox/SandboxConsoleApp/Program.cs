#pragma warning disable CS8600

using MemoryPack;
using MemoryPack.Formatters;
using MessagePack;
using Microsoft.Extensions.DependencyInjection;
using SandboxConsoleApp;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Runtime.CompilerServices;
using System.Security;
using System.Text.RegularExpressions;
using System.Xml.Linq;

//ConsoleAppFramework.ConsoleApp.Run<StandardRunner>(args);
//ConsoleAppFramework.ConsoleApp.Run<SystemTextJsonChecker>(args);

// MemoryPackable check

//var writer = new MemoryPackWriter<ArrayBufferWriter<byte>>();
//writer.WriteUnmanagedArray
//var reader = new MemoryPackReader();

var p = new Packable<int>();
if (p is IMemoryPackable<int>)
{
    Console.WriteLine("OK");
}
else
{
    Console.WriteLine("NG");
}

// [MemoryPackable]
public partial class Packable<T>
{
    [MemoryPackIgnore]
    public object? ObjectObject { get; set; }
    [MemoryPackIgnore]
    public Array? StandardArray { get; set; }
    public int[]? Array { get; set; }
    public int[,]? MoreArray { get; set; }
    public List<int>? List { get; set; }
    public Version? Version { get; set; }

    public T? TTTTT { get; set; }

    [MemoryPackFormatter]
    public Nazo? MyProperty { get; set; }

    [MemoryPackFormatter]
    public Nazo2? MyProperty2 { get; set; }
}

public class Nazo
{

}
public class Nazo2
{

}

public class Tadano
{

}




partial class Packable<T> : IMemoryPackable<Packable<T>>
{
    static Packable()
    {
        MemoryPackFormatterProvider.Register<Packable<T>>();
    }

    static void IMemoryPackFormatterRegister.RegisterFormatter()
    {
        if (!MemoryPackFormatterProvider.IsRegistered<Packable<T>>())
        {
            MemoryPackFormatterProvider.Register(new MemoryPack.Formatters.MemoryPackableFormatter<Packable<T>>());
        }
    }

    static void IMemoryPackable<Packable<T>>.Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref Packable<T>? value)
    {

        if (value == null)
        {
            writer.WriteNullObjectHeader();
            goto END;
        }

        writer.WriteObjectHeader(7);
        writer.WriteUnmanagedArray(value.Array);
        writer.WriteObject(value.MoreArray);
        writer.WriteObject(value.List);
        writer.WriteObject(value.Version);
        writer.WriteObject(value.TTTTT);
        writer.WriteObject(value.MyProperty);
        writer.WriteObject(value.MyProperty2);
    END:

        return;
    }

    static void IMemoryPackable<Packable<T>>.Deserialize(ref MemoryPackReader reader, scoped ref Packable<T>? value)
    {

        if (!reader.TryReadObjectHeader(out var count))
        {
            value = default!;
            goto END;
        }

        if (count == 7)
        {
            var __Array = reader.ReadUnmanagedArray<int>();
            var __MoreArray = reader.ReadObject<int[,]>();
            var __List = reader.ReadObject<global::System.Collections.Generic.List<int>>();
            var __Version = reader.ReadObject<global::System.Version>();
            var __TTTTT = reader.ReadObject<T>();
            var __MyProperty = reader.ReadObject<global::Nazo>();
            var __MyProperty2 = reader.ReadObject<global::Nazo2>();
            value = new Packable<T>()
            {
                Array = __Array,
                MoreArray = __MoreArray,
                List = __List,
                Version = __Version,
                TTTTT = __TTTTT,
                MyProperty = __MyProperty,
                MyProperty2 = __MyProperty2
            };
            goto END;
        }
        else if (count > 7)
        {
            ThrowHelper.ThrowInvalidPropertyCount(7, count);
        }
        else
        {
            int[] __Array;
            int[,] __MoreArray = default!;
            global::System.Collections.Generic.List<int> __List = default!;
            global::System.Version __Version = default!;
            T __TTTTT = default!;
            global::Nazo __MyProperty = default!;
            global::Nazo2 __MyProperty2 = default!;

            if (count == 0) goto NEW;
            __Array = reader.ReadUnmanagedArray<int>(); if (count == 1) goto NEW;
            __MoreArray = reader.ReadObject<int[,]>(); if (count == 2) goto NEW;
            __List = reader.ReadObject<global::System.Collections.Generic.List<int>>(); if (count == 3) goto NEW;
            __Version = reader.ReadObject<global::System.Version>(); if (count == 4) goto NEW;
            __TTTTT = reader.ReadObject<T>(); if (count == 5) goto NEW;
            __MyProperty = reader.ReadObject<global::Nazo>(); if (count == 6) goto NEW;
            __MyProperty2 = reader.ReadObject<global::Nazo2>(); if (count == 7) goto NEW;

            NEW:
            value = new Packable<T>()
            {
                Array = __Array,
                MoreArray = __MoreArray,
                List = __List,
                Version = __Version,
                TTTTT = __TTTTT,
                MyProperty = __MyProperty,
                MyProperty2 = __MyProperty2
            };
            goto END;
        }
    END:

        return;
    }
}
