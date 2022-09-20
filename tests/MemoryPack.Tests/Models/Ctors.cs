using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemoryPack.Tests.Models;

[MemoryPackable]
public partial class NoCtor
{
    public int X { get; set; }
}

[MemoryPackable]
public partial class OneCtor
{
    public int X { get; set; }


    public OneCtor()
    {

    }
}

[MemoryPackable]
public partial class OneCtor2
{
    public int X { get; }
    public int Y { get; }

    public OneCtor2(int x, int y)
    {
        this.X = x;
        this.Y = y;
    }
}

//[MemoryPackable]
//public partial class TwoCtor
//{
//    public TwoCtor()
//    {

//    }

//    public TwoCtor(int x, int y)
//    {

//    }
//}

[MemoryPackable]
public partial class ExplicitlyCtor
{
    public int X { get; }
    public int Y { get; set; }

    public ExplicitlyCtor()
    {

    }

    [MemoryPackConstructor]
    public ExplicitlyCtor(int x, int y)
    {
        this.X = x;
        this.Y = y;
    }
}



//[MemoryPackable]
//public partial class MultipleExplicitlyCtor
//{
//    [MemoryPackConstructor]
//    public MultipleExplicitlyCtor()
//    {

//    }

//    [MemoryPackConstructor]
//    public MultipleExplicitlyCtor(int x, int y)
//    {

//    }
//}


[MemoryPackable]
public partial class ParameterCheck
{
    bool prop1SetCalled;

    string mp;
    public string MyProperty1
    {
        get { return mp; }
        set
        {
            mp = value;
            prop1SetCalled = true;
        }
    }
    public string? MyProperty2;

    public ParameterCheck(string myProperty1)
    {
        this.mp = myProperty1;
    }

    public bool IsProp1SetCalled()
    {
        return prop1SetCalled;
    }
}
