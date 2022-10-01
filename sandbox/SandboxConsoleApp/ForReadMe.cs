using MemoryPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Samples;


[MemoryPackable]
public partial class Sample2
{
    [MemoryPackAllowSerialize]
    public NotSerializableType? NotSerializableProperty { get; set; }
}

public class NotSerializableType
{

}



[MemoryPackable]
public partial class Person
{
    public readonly int Age;
    public readonly string Name;

    // You can use parametarized constructor
    public Person(int age, string name)
    {
        this.Age = age;
        this.Name = name;
    }
}

// also supports record primary constructor
[MemoryPackable]
public partial record Person2(int Age, string Name);

public partial class Person3
{
    public int Age { get; set; }
    public string Name { get; set; }

    public Person3()
    {
        this.Age = 0;
        this.Name = "";
    }

    // If exists multiple constructors, must use [MemoryPackConstructor]
    [MemoryPackConstructor]
    public Person3(int age, string name)
    {
        this.Age = age;
        this.Name = name;
    }
}


[MemoryPackable(GenerateType.Collection)]
public partial class MyList<T> : List<T>
{
}

[MemoryPackable(GenerateType.Collection)]
public partial class MyStringDictionary<TValue> : Dictionary<string, TValue>
{

}

// Annotate inheritance types
[MemoryPackable]
[MemoryPackUnion(0, typeof(FooClass))]
[MemoryPackUnion(1, typeof(BarClass))]
public partial interface IUnionSample
{
}

[MemoryPackable]
public partial class FooClass : IUnionSample
{
    public int XYZ { get; set; }
}

[MemoryPackable]
public partial class BarClass : IUnionSample
{
    public string? OPQ { get; set; }
}




public class Skelton : MemoryPackFormatter<Skelton>
{
    public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref Skelton? value)
    {
        if (value == null)
        {
            writer.WriteNullObjectHeader();
            return;
        }

        // use writer method.
    }

    public override void Deserialize(ref MemoryPackReader reader, scoped ref Skelton? value)
    {
        if (!reader.TryReadObjectHeader(out var count))
        {
            value = null;
            return;
        }

        // use reader method.
    }
}



[MemoryPackable]
public partial class Version1
{
    public int Prop1 { get; set; }
    public long Prop2 { get; set; }
}

// Add is OK.
[MemoryPackable]
public partial class Version2
{
    public int Prop1 { get; set; }
    public long Prop2 { get; set; }
    public int? AddedProp { get; set; }
}


[MemoryPackable(SerializeLayout.Explicit)]
public partial class SampleExplicitOrder
{
    [MemoryPackOrder(1)]
    public int Prop1 { get; set; }
    [MemoryPackOrder(0)]
    public int Prop0 { get; set; }
}
