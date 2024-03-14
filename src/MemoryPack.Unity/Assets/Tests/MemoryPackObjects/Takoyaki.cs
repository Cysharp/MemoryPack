#nullable enable
using MemoryPack;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Models
{
    [MemoryPackable]
    public partial class Takoyaki
    {
        public int MyProperty { get; set; }
    }
}
