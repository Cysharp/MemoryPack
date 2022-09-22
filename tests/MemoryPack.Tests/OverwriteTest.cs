using MemoryPack.Tests.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemoryPack.Tests;

public class OverwriteTest
{
    // TODO:require more test(collection direct, in property, etc...)

    // Requirement memo:
    // * doesn't call constructor so doesn't set readonly/init/required member

    // TODO: implement overwrite after optimization complete.
    [Fact]
    public void SimpleObject()
    {
        //var write = new Overwrite()
        //{
        //    MyProperty1 = 10,
        //    MyProperty2 = 20,
        //    MyProperty3 = "foo",
        //    MyProperty4 = "bar"
        //};

        //var bin = MemoryPackSerializer.Serialize(write);

        //write.MyProperty1 = 99;
        //write.MyProperty2 = 9999;
        //write.MyProperty3 = "hoahoahoa";
        //write.MyProperty4 = "kukukukuku";

        //var original = write;
        //MemoryPackSerializer.Deserialize<Overwrite>(bin, ref write);

        //Debug.Assert(write != null);
        //write.MyProperty1.Should().Be(10);
        //write.MyProperty2.Should().Be(20);
        //write.MyProperty3.Should().Be("foo");
        //write.MyProperty4.Should().Be("bar");

        //// same reference
        //original.Should().BeSameAs(write);


        var n = 10;
        var i = 0;
        var a = new int[n][];

    }

    // impl like this

    
//            if (value != null)
//            {
//                // only settable property.
//                value.MyProperty1 = __MyProperty1;
//                value.MyProperty1 = __MyProperty1;
//                value.MyProperty1 = __MyProperty1;
//                value.MyProperty1 = __MyProperty1;
//            }
//            else
//            {
//                value = new Overwrite()
//{
//    MyProperty1 = __MyProperty1,
//                    MyProperty2 = __MyProperty2,
//                    MyProperty3 = __MyProperty3,
//                    MyProperty4 = __MyProperty4
//                };
//            }
//            goto END;
}
