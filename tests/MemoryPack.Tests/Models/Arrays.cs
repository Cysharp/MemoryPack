using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemoryPack.Tests.Models;


[MemoryPackable]
public partial class ArrayCheck
{
    public int[]? Array1 { get; set; }
    public int?[]? Array2 { get; set; }
    public string[]? Array3 { get; set; }
    public string?[]? Array4 { get; set; }
}


[MemoryPackable]
public partial class ArrayOptimizeCheck
{
    public StandardTypeTwo?[]? Array1 { get; set; }
    public List<StandardTypeTwo?>? List1 { get; set; }
}
