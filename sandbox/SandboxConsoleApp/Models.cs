using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SandboxConsoleApp;


[MemoryPackable]
public partial class NotSample
{
    [Utf8StringFormatter]
    public string? Custom1 { get; set; }

}

[MemoryPackable(GenerateType.VersionTolerant)]
public partial class Node
{
    [MemoryPackOrder(0)]
    public Node? Parent { get; set; }
    [MemoryPackOrder(1)]
    public Node[]? Children { get; set; }
}


[MemoryPackable(GenerateType.VersionTolerant)]
public partial class TakoyakiY
{
    [MemoryPackOrder(1)]
    public string? Bar { get; set; }
    [MemoryPackOrder(10)]
    public int Foo { get; set; }
}
