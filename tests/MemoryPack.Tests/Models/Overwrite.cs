using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemoryPack.Tests.Models;


[MemoryPackable]
public partial class Overwrite
{
    public int MyProperty1 { get; set; }
    public int MyProperty2 { get; set; }
    public String? MyProperty3 { get; set; }
    public string? MyProperty4 { get; set; }
}

