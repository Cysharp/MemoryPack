using System;
using System.Collections.Generic;
using System.Linq;

namespace MemoryPack.Tests.Models;

[MemoryPackable(GenerateType.MultipleReferences)]
public partial class RefNode
{
    public RefNode() : this(Array.Empty<RefNode>())
    {
    }

    [MemoryPackConstructor]
    private RefNode(IReadOnlyList<RefNode> children)
    {
        Children = children;
    }

    public IReadOnlyList<RefNode> Children { get; private set; }

    public void AddChild(RefNode child)
    {
        var list = Children?.ToList() ?? new List<RefNode>();
        list.Add(child);
        Children = list.ToArray();
    }
}

[MemoryPackable(GenerateType.MultipleReferences)]
public partial class PureRefNode
{
    public PureRefNode(int id, ulong id2)
    {
        Id = id;
        Id2 = id2;
    }

    public int Id { get; }

    public ulong Id2 { get; }
}

[MemoryPackable]
public partial class MultipleReferencesHolder
{
    public MultipleReferencesHolder() : this(new List<RefNode>(), new List<PureRefNode>())
    {
    }

    [MemoryPackConstructor]
    private MultipleReferencesHolder(IReadOnlyList<RefNode> list, IReadOnlyList<PureRefNode> listPure)
    {
        List = list;
        ListPure = listPure;
    }

    public IReadOnlyList<RefNode> List { get; private set; }

    public void Add(RefNode refNode)
    {
        var list = List?.ToList() ?? new List<RefNode>();
        list.Add(refNode);
        List = list.ToArray();
    }

    public IReadOnlyList<PureRefNode> ListPure { get; private set; }

    public void Add(PureRefNode refNode)
    {
        var list = ListPure?.ToList() ?? new List<PureRefNode>();
        list.Add(refNode);
        ListPure = list.ToArray();
    }
}
