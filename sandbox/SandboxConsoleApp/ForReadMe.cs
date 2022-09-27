using MemoryPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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


