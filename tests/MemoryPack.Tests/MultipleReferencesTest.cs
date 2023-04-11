using MemoryPack.Tests.Models;

namespace MemoryPack.Tests;

public class MultipleReferencesTest
{
    [Fact]
    public void InHolder()
    {
        var holder = new MultipleReferencesHolder();

        {
            var parent = new RefNode();
            var a1 = new RefNode();
            var a2 = new RefNode();
            var a3 = new RefNode();

            parent.AddChild(a1);
            parent.AddChild(a2);
            parent.AddChild(a3);

            var parent2 = new RefNode();
            parent2.AddChild(parent);
            parent2.AddChild(a2);

            holder.Add(parent);
            holder.Add(parent);
            holder.Add(parent2);
            holder.Add(parent);
            holder.Add(parent2);
        }
        {
            var pure1 = new PureRefNode(id: 10, id2: 1000);
            var pure2 = new PureRefNode(id: 100, id2: 100000);

            holder.Add(pure1);
            holder.Add(pure1);
            holder.Add(pure2);
            holder.Add(pure2);
            holder.Add(pure1);
        }

        var bin = MemoryPackSerializer.Serialize(holder);
        var value2 = MemoryPackSerializer.Deserialize<MultipleReferencesHolder>(bin);
        value2.Should().NotBeNull();

        {
            var parent = value2!.List[0];
            var parent2 = value2.List[2];
            var a1 = parent.Children[0];
            var a2 = parent.Children[1];
            var a3 = parent.Children[2];

            parent.Should().NotBeSameAs(parent2);
            ReferenceEquals(parent, parent2).Should().BeFalse();
            parent2.Children[0].Should().BeSameAs(parent);
            ReferenceEquals(parent2.Children[0], parent).Should().BeTrue();
            parent2.Children[1].Should().BeSameAs(a2);
            ReferenceEquals(parent2.Children[1], a2).Should().BeTrue();
        }
        {
            var pure1 = value2.ListPure[0];
            var pure2 = value2.ListPure[2];

            pure1.Should().NotBeSameAs(pure2);
            pure1.Should().BeSameAs(value2.ListPure[1]);
            pure1.Should().BeSameAs(value2.ListPure[4]);
            pure2.Should().BeSameAs(value2.ListPure[3]);
        }
    }
}
