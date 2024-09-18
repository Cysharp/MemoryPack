using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// namespaced
namespace MemoryPack.Tests.Models
{
    // MEMPACK001 must be partial
    // [MemoryPackable]
    //public class Ng
    //{

    //}

    [MemoryPackable]
    public partial class StandardTypeZero
    {
    }

    [MemoryPackable]
    public partial class StandardTypeOne
    {
        public int One { get; set; }
    }


    [MemoryPackable]
    public partial class StandardTypeTwo
    {
        public int One { get; set; }
        public int Two { get; set; }

        public StandardTypeTwo()
        {
            // _ = new StandardTypeTwoFormatter();
        }

        // MEMPACK002 nested is not allowed
        //[MemoryPackable]
        //public partial class Nested
        //{
        //    public int One { get; set; }
        //}
    }

    [MemoryPackable]
    public partial struct StandardUnmanagedStruct
    {
        public int MyProperty { get; set; }
    }

    [MemoryPackable]
    public partial struct StandardStruct
    {
        public string MyProperty { get; set; }

        public StandardStruct()
        {
            MyProperty = default!;
        }
    }

    public partial class NestedContainer
    {
        [MemoryPackable]
        public partial class StandardTypeNested
        {
            public int One { get; set; }
        }
    }

    public partial class DoublyNestedContainer
    {
        public partial class DoublyNestedContainerInner
        {
            [MemoryPackable]
            public partial class StandardTypeDoublyNested
            {
                public int One { get; set; }
            }
        }
    }


    [MemoryPackable]
    public partial class WithArray
    {
        public StandardTypeOne[]? One { get; set; }
    }

}

// another namespace, same type name
namespace MemoryPack.Tests.Models.More
{

    [MemoryPackable]
    public partial class StandardTypeTwo
    {
        public string? One { get; set; }
        public string? Two { get; set; }

        public StandardTypeTwo()
        {
            // new StandardTypeTwoFormatter();
        }
    }
}

[MemoryPackable]
public partial class GlobalNamespaceType
{
    public int MyProperty { get; set; }

    public GlobalNamespaceType()
    {
        // _ = new GlobalNamespaceTypeFormatter();
    }
}
