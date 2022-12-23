using MemoryPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemoryPack.Tests.Models
{

    [MemoryPackable]
    [Serializable]
    public partial class Person
    {
        public int Age;
        public string Name;
    }

    [MemoryPackable]
    [Serializable]
    public partial class Primitives
    {
        public short Short;
        public int Int;
        public long Long;
        public byte Byte;
        public bool Bool;
        public char Char;
        public float Float;
        public double Double;
        public string String;
    }

    [MemoryPackable]
    [Serializable]
    public partial class NestCase
    {

        public Inner A;
        public Inner B;
        public Inner C;
        public Inner D;
        public Inner E;
        public Inner F;
        public Inner G;
        public Inner H;
        public Inner I;
    }

    [MemoryPackable]
    [Serializable]
    public partial class Inner
    {
        public int Int;
        public double Double;
        public string String;
    }
}
