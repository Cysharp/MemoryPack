using MemoryPack.Tests.Models;
using System.Diagnostics;
using System.Collections.Generic;

namespace MemoryPack.Tests;

public class OverwriteTest
{
    [Fact]
    public void Objects()
    {
        {
            var write = new Overwrite()
            {
                MyProperty1 = 10,
                MyProperty2 = 20,
                MyProperty3 = "foo",
                MyProperty4 = "bar"
            };

            var bin = MemoryPackSerializer.Serialize(write);

            write.MyProperty1 = 99;
            write.MyProperty2 = 9999;
            write.MyProperty3 = "hoahoahoa";
            write.MyProperty4 = "kukukukuku";

            var original = write;
            MemoryPackSerializer.Deserialize<Overwrite>(bin, ref write);

            Debug.Assert(write != null);
            write.MyProperty1.Should().Be(10);
            write.MyProperty2.Should().Be(20);
            write.MyProperty3.Should().Be("foo");
            write.MyProperty4.Should().Be("bar");

            // same reference
            original.Should().BeSameAs(write);
        }
        {
            var write = new Overwrite2()
            {
                MyProperty1 = 10,
                MyProperty2 = 20,
                MyProperty3 = "foo",
                MyProperty4 = "bar"
            };

            var bin = MemoryPackSerializer.Serialize(write);

            write.MyProperty1 = 99;
            write.MyProperty2 = 9999;
            write.MyProperty3 = "hoahoahoa";
            write.MyProperty4 = "kukukukuku";

            MemoryPackSerializer.Deserialize<Overwrite2>(bin, ref write);

            write.MyProperty1.Should().Be(10);
            write.MyProperty2.Should().Be(20);
            write.MyProperty3.Should().Be("foo");
            write.MyProperty4.Should().Be("bar");
        }
        {
            var write = new Overwrite3(10, 20)
            {
                MyProperty3 = "foo",
                MyProperty4 = "bar"
            };

            var bin = MemoryPackSerializer.Serialize(write);

            write.MyProperty1 = 99;
            write.MyProperty2 = 9999;
            write.MyProperty3 = "hoahoahoa";
            write.MyProperty4 = "kukukukuku";

            var original = write;
            MemoryPackSerializer.Deserialize<Overwrite3>(bin, ref write);

            Debug.Assert(write != null);
            write.MyProperty1.Should().Be(10);
            write.MyProperty2.Should().Be(20);
            write.MyProperty3.Should().Be("foo");
            write.MyProperty4.Should().Be("bar");

            // not same reference
            original.Should().NotBeSameAs(write);
        }
        {
            var write = new Overwrite4()
            {
                MyProperty1 = 4444,
                MyProperty2 = new Overwrite()
                {
                    MyProperty1 = 10,
                    MyProperty2 = 20,
                    MyProperty3 = "foo",
                    MyProperty4 = "bar"
                },
                MyProperty3 = new List<int> { 1, 5, 9 }
            };

            var bin = MemoryPackSerializer.Serialize(write);

            write.MyProperty1 = 5555;
            write.MyProperty2.MyProperty1 = 99;
            write.MyProperty2.MyProperty2 = 9999;
            write.MyProperty2.MyProperty3 = "hoahoahoa";
            write.MyProperty2.MyProperty4 = "kukukukuku";
            write.MyProperty3.Add(99999);

            var original = write;
            MemoryPackSerializer.Deserialize<Overwrite4>(bin, ref write);

            Debug.Assert(write != null);
            Debug.Assert(write.MyProperty2 != null);
            Debug.Assert(write.MyProperty3 != null);

            write.MyProperty1.Should().Be(4444);
            write.MyProperty2.MyProperty1.Should().Be(10);
            write.MyProperty2.MyProperty2.Should().Be(20);
            write.MyProperty2.MyProperty3.Should().Be("foo");
            write.MyProperty2.MyProperty4.Should().Be("bar");
            write.MyProperty3.Should().Equal(1, 5, 9);

            original.Should().BeSameAs(write);
            original.MyProperty2.Should().BeSameAs(write.MyProperty2);
            original.MyProperty3.Should().BeSameAs(write.MyProperty3);
        }
    }
}
