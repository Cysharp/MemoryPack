using MemoryPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SandboxNet6
{
    [MemoryPackable]
    public partial class NamespaceTest
    {
        public int MyProperty { get; set; }
    }

}
