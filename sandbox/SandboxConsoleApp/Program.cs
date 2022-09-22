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

Console.WriteLine(ArrayPool<byte>.Shared.Rent(30000).Length);
Console.WriteLine(ArrayPool<byte>.Shared.Rent(60000).Length);
Console.WriteLine(ArrayPool<byte>.Shared.Rent(120000).Length);
Console.WriteLine(ArrayPool<byte>.Shared.Rent(150000).Length);
return;


[MemoryPackable]
public partial class Packable<T>
{
    public int TakoyakiX { get; set; }
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
    public int MyProperty { get; set; }
}



public class C
{
    public int Foo { get; init; }
    public required int Bar { get; init; }


}


[MemoryPackable]
public partial record MyRecord(int foo, int bar, string baz);

[MemoryPackable]
public partial struct FooA
{
    public int Foo { get; set; }
    public int Bar { get; set; }
    public int Baz { get; set; }
    public string Tako { get; set; }
}


