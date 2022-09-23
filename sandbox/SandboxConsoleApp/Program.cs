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

//if (!RuntimeHelpers.IsReferenceOrContainsReferences<Span<byte>>())
{
}

// var span = new Span<byte>();
// MemoryPackSerializer.Serialize(span);

return;

[MemoryPackable]
public partial class Foo
{
    public byte[] B;
    //[MemoryPackFormatter]
    //public Span<byte> SPAN => B.AsSpan();
}




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


