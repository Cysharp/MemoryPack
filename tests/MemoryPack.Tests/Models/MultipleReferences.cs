using System.Collections.Generic;

namespace MemoryPack.Tests.Models;

[MemoryPackable(GenerateType.MultipleReferences)]
public partial class RefNode
{
    [MemoryPackInclude]
    private readonly List<RefNode> children;

    public RefNode() : this(new List<RefNode>())
    {
    }

    [MemoryPackConstructor]
    private RefNode(List<RefNode> children)
    {
        this.children = children;
    }

    [MemoryPackIgnore]
    public IReadOnlyList<RefNode> Children => children;

    public void AddChild(RefNode child)
    {
        children.Add(child);
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
    [MemoryPackInclude]
    private readonly List<RefNode> list;

    [MemoryPackInclude]
    private readonly List<PureRefNode> listPure;

    public MultipleReferencesHolder() : this(new List<RefNode>(), new List<PureRefNode>())
    {
    }

    [MemoryPackConstructor]
    private MultipleReferencesHolder(List<RefNode> list, List<PureRefNode> listPure)
    {
        this.list = list;
        this.listPure = listPure;
    }

    [MemoryPackIgnore]
    public IReadOnlyList<RefNode> List => list;

    public void Add(RefNode refNode)
    {
        list.Add(refNode);
    }

    [MemoryPackIgnore]
    public IReadOnlyList<PureRefNode> ListPure => listPure;

    public void Add(PureRefNode refNode)
    {
        listPure.Add(refNode);
    }
}
