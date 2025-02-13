#pragma warning disable CS8602

using MemoryPack.Tests.Models;
using System.Collections.Generic;

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

    [Fact]
    public void RequiredProperties()
    {
        CircularReferenceWithRequiredProperties manager = new()
        {
            FirstName = "Tyler",
            LastName = "Stein",
            Manager = null,
            DirectReports = []
        };

        CircularReferenceWithRequiredProperties emp1 = new()
        {
            FirstName = "Adrian",
            LastName = "King",
            Manager = manager,
            DirectReports = []
        };
        CircularReferenceWithRequiredProperties emp2 = new()
        {
            FirstName = "Ben",
            LastName = "Aston",
            Manager = manager,
            DirectReports = []
        };
        CircularReferenceWithRequiredProperties emp3 = new()
        {
            FirstName = "Emily",
            LastName = "Ottoline",
            Manager = emp2,
            DirectReports = []
        };
        CircularReferenceWithRequiredProperties emp4 = new()
        {
            FirstName = "Jaymes",
            LastName = "Jaiden",
            Manager = emp2,
            DirectReports = []
        };
        manager.DirectReports = [emp1, emp2];
        emp2.DirectReports = [emp3, emp4];


        var bin = MemoryPackSerializer.Serialize(manager);

        CircularReferenceWithRequiredProperties? deserialized = MemoryPackSerializer.Deserialize<CircularReferenceWithRequiredProperties>(bin);

        deserialized.Should().NotBeNull();
        deserialized!.FirstName.Should().Be("Tyler");
        deserialized.LastName.Should().Be("Stein");
        deserialized.Manager.Should().BeNull();
        deserialized.DirectReports.Should().HaveCount(2);

        var dEmp1 = deserialized.DirectReports[0];
        dEmp1.FirstName.Should().Be("Adrian");
        dEmp1.LastName.Should().Be("King");
        dEmp1.Manager.Should().BeSameAs(deserialized);
        dEmp1.DirectReports.Should().BeEmpty();

        var dEmp2 = deserialized.DirectReports[1];
        dEmp2.FirstName.Should().Be("Ben");
        dEmp2.LastName.Should().Be("Aston");
        dEmp2.Manager.Should().BeSameAs(deserialized);
        dEmp2.DirectReports.Should().HaveCount(2);

        var dEmp3 = dEmp2.DirectReports[0];
        dEmp3.FirstName.Should().Be("Emily");
        dEmp3.LastName.Should().Be("Ottoline");
        dEmp3.Manager.Should().BeSameAs(dEmp2);
        dEmp3.DirectReports.Should().BeEmpty();

        var dEmp4 = dEmp2.DirectReports[1];
        dEmp4.FirstName.Should().Be("Jaymes");
        dEmp4.LastName.Should().Be("Jaiden");
        dEmp4.Manager.Should().BeSameAs(dEmp2);
        dEmp4.DirectReports.Should().BeEmpty();
    }
}
