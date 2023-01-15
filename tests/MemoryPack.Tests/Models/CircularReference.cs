using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemoryPack.Tests.Models;

[MemoryPackable(GenerateType.CircularReference)]
public partial class Node
{
    [MemoryPackOrder(0)]
    public Node? Parent { get; set; }
    [MemoryPackOrder(1)]
    public Node[]? Children { get; set; }
}

[MemoryPackable(GenerateType.CircularReference)]
public partial class PureNode
{
    [MemoryPackOrder(0)]
    public int Id { get; set; }
    [MemoryPackOrder(1)]
    public ulong Id2 { get; set; }
}

[MemoryPackable]
public partial class CircularHolder
{
    public List<Node>? List { get; set; }
    public List<PureNode>? ListPure { get; set; }
}


// https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/preserve-references?pivots=dotnet-7-0
[MemoryPackable(GenerateType.CircularReference)]
public partial class Employee
{
    [MemoryPackOrder(0)]
    public string? Name { get; set; }
    [MemoryPackOrder(1)]
    public Employee? Manager { get; set; }
    [MemoryPackOrder(2)]
    public List<Employee>? DirectReports { get; set; }
}



[MemoryPackable(GenerateType.CircularReference, SerializeLayout.Sequential)]
public partial class SequentialCircularReference
{
    public string? Name { get; set; }
    public SequentialCircularReference? Manager { get; set; }
    public List<SequentialCircularReference>? DirectReports { get; set; }
}
