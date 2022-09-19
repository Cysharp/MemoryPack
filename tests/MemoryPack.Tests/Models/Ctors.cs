using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemoryPack.Tests.Models;

[MemoryPackable]
public partial class NoCtor
{

}

[MemoryPackable]
public partial class OneCtor
{
    public OneCtor()
    {

    }
}

[MemoryPackable]
public partial class OneCtor2
{
    public OneCtor2()
    {

    }
}

[MemoryPackable]
public partial class TwoCtor
{
    public TwoCtor()
    {

    }

    public TwoCtor(int x, int y)
    {

    }
}

[MemoryPackable]
public partial class ExplicitlyCtor
{
    public ExplicitlyCtor()
    {

    }

    [MemoryPackConstructor]
    public ExplicitlyCtor(int x, int y)
    {

    }
}
