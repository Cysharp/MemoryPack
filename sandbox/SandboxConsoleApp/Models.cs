using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SandboxConsoleApp;

public interface IMore
{
    public Version Description { get; set; }
}

public class NewBase
{
    public long Description { get; set; }
}

[MemoryPackable]
public partial struct FooUnman
{
    public float MyProperty { get; set; }
    public float MyProperty2 { get; set; }
}

[MemoryPackable]
public partial class NewProp : NewBase, IMore
{
    Version IMore.Description { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public new string? Description { get; set; }

    public NewProp()
    {

    }
}


[MemoryPackable]
public partial class NotNotOmu
{
    public Guid? GUIDNULLABLE { get; set; }
}


[MemoryPackable]
public partial class Mop
{
    public NoGen? MyProperty { get; set; }
    public LisList? MyLisList { get; set; }
    public List<Suage>? SuageMan { get; set; }
}


[MemoryPackable]
public partial class NotSample
{
    [Utf8StringFormatter]
    public string? Custom1 { get; set; }

}

[MemoryPackable(GenerateType.CircularReference)]
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

[MemoryPackable(GenerateType.CircularReference)]
public partial class Suage
{
    [MemoryPackOrder(0)]
    public int Prop1 { get; set; }
    [MemoryPackOrder(2)]
    public int Prop2 { get; set; }

    //public Suage(int prop1, int prop2)
    //{
    //    this.Prop1 = prop1;
    //    this.Prop2 = prop2;
    //}
}



[MemoryPackable(GenerateType.NoGenerate)]
public partial class NoGen
{
}

[MemoryPackable(GenerateType.Collection)]
public partial class LisList : List<int>
{

}




[MemoryPackable]
public partial class InstantiateFromServiceProvider
{
    public int MyProperty { get; private set; }

    [MemoryPackOnDeserializing]
    static void OnDeserializing(ref MemoryPackReader reader, ref InstantiateFromServiceProvider value)
    {
        if (value != null) return;
        value = reader.Options.ServiceProvider!.GetRequiredService<InstantiateFromServiceProvider>();
    }
}


