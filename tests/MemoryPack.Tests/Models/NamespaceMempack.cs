﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemoryPack.MemoryPack
{

    [MemoryPackable]
    public partial class MemModel1
    {
        public int MyProperty { get; set; }
    }

}

namespace MemoryPack.Tests.MemoryPack
{

    [MemoryPackable]
    public partial class MemModel2
    {
        public int MyProperty { get; set; }
    }

}


namespace MemoryPack.Tests.Models.MemoryPack
{

    [MemoryPackable]
    public partial class MemModel3
    {
        public int MyProperty { get; set; }
    }

}
