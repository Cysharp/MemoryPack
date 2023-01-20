#pragma warning disable CS8602

using MemoryPack.Tests.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;

namespace MemoryPack.Tests;

public class CircularReferenceTest
{
    [Fact]
    public void MicrosoftExample()
    {
        // https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/preserve-references?pivots=dotnet-7-0

        Employee tyler = new()
        {
            Name = "Tyler Stein"
        };

        Employee adrian = new()
        {
            Name = "Adrian King"
        };

        tyler.DirectReports = new List<Employee> { adrian };
        adrian.Manager = tyler;

        var bin = MemoryPackSerializer.Serialize(tyler);
        Employee? tylerDeserialized = MemoryPackSerializer.Deserialize<Employee>(bin);

        tylerDeserialized?.DirectReports?[0].Manager.Should().BeSameAs(tylerDeserialized);
    }

    [Fact]
    public void NodeTest()
    {
        var parent = new Node();
        var a1 = new Node();
        var a2 = new Node();
        var a3 = new Node();
        a1.Parent = parent;
        a2.Parent = parent;
        a3.Parent = parent;
        parent.Children = new[] { a1, a2, a3 };

        var bin = MemoryPackSerializer.Serialize(parent);
        var value2 = MemoryPackSerializer.Deserialize<Node>(bin);

        foreach (var item in value2!.Children)
        {
            item.Parent.Should().BeSameAs(value2);
        }
    }

    [Fact]
    public void PureNodeTest()
    {
        var node = new PureNode() { Id = 10, Id2 = 1000 };

        var bin = MemoryPackSerializer.Serialize(node);
        var value2 = MemoryPackSerializer.Deserialize<PureNode>(bin);

        value2.Id.Should().Be(10);
        value2.Id2.Should().Be(1000);
    }

    [Fact]
    public void InHolder()
    {
        var holder = new CircularHolder();
        holder.List = new List<Node>();
        holder.ListPure = new List<PureNode>();

        {
            var parent = new Node();
            var a1 = new Node();
            var a2 = new Node();
            var a3 = new Node();
            a1.Parent = parent;
            a2.Parent = parent;
            a3.Parent = parent;
            parent.Children = new[] { a1, a2, a3 };

            var parent2 = new Node();
            parent2.Children = new[] { parent, a2 };

            holder.List.AddRange(new[] { parent, parent, parent2, parent, parent2 });
        }
        {
            var pure1 = new PureNode() { Id = 10, Id2 = 1000 };
            var pure2 = new PureNode() { Id = 100, Id2 = 100000 };

            holder.ListPure.Add(pure1);
            holder.ListPure.Add(pure1);
            holder.ListPure.Add(pure2);
            holder.ListPure.Add(pure2);
            holder.ListPure.Add(pure1);
        }


        var bin = MemoryPackSerializer.Serialize(holder);
        var value2 = MemoryPackSerializer.Deserialize<CircularHolder>(bin);

        {
            var parent = value2.List[0];
            var parent2 = value2.List[2];
            var a1 = parent.Children[0];
            var a2 = parent.Children[1];
            var a3 = parent.Children[2];

            parent.Should().NotBeSameAs(parent2);
            parent2.Children[0].Should().BeSameAs(parent);
            parent2.Children[1].Should().BeSameAs(a2);
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

    [Fact]
    public void Sequential()
    {
        SequentialCircularReference tyler = new()
        {
            Name = "Tyler Stein"
        };

        SequentialCircularReference adrian = new()
        {
            Name = "Adrian King"
        };

        tyler.DirectReports = new List<SequentialCircularReference> { adrian };
        adrian.Manager = tyler;

        var bin = MemoryPackSerializer.Serialize(tyler);
        SequentialCircularReference? tylerDeserialized = MemoryPackSerializer.Deserialize<SequentialCircularReference>(bin);

        tylerDeserialized?.DirectReports?[0].Manager.Should().BeSameAs(tylerDeserialized);
    }

}
