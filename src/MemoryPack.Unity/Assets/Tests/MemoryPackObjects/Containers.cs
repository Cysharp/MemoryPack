using MemoryPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    [MemoryPackable]
    public partial class MyContainer
    {
        public Dictionary<int, string> Dict = new();
    }

    [MemoryPackable]
    public partial class MyContainer2
    {
        public List<int> List = new();
    }

    [MemoryPackable]
    public partial class MyContainer3
    {
        public HashSet<long> Set = new();
    }
}
