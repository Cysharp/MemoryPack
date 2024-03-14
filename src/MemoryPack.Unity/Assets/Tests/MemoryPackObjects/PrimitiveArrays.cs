using MemoryPack;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.MemoryPackObjects
{
    [MemoryPackable]
    public partial class Vector3ArrayValue
    {
        public Vector3[] Array { get; set; }
    }
}
