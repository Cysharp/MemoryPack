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
        public Dictionary<int, string> Dict = new Dictionary<int, string>();
    }

    [MemoryPackable]
    public partial class MyContainer2
    {
        public List<int> List = new List<int>();
    }

    [MemoryPackable]
    public partial class MyContainer3
    {
        public HashSet<long> Set = new HashSet<long>();
    }
}
