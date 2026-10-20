using MemoryPack.Tests.Models;
using Newtonsoft.Json;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemoryPack.Tests;

public class FixedArrayTest
{
    [Fact]
    public void Check()
    {
        var checker = new FixedArrayCheck()
        {
            ByteData = new byte[10],
            NestedData =
            [
                new FixedArrayCheck.Nested()
                {
                    Data1 = 0,
                    Data2 = "Hello World. !@#$%^&*()_+",
                    Data3 = (123456789, 987654321)
                }
            ],
            StringData = new string[]
            {
                "Hello World. !@#$%^&*()_+",
                "Hi",
                "Greetings",
                "Nice to meet you",
                "The end"
            }
        };

        var bin = MemoryPackSerializer.Serialize(checker);
        var v2 = MemoryPackSerializer.Deserialize<FixedArrayCheck>(bin);

        v2.Should().BeEquivalentTo(checker);
    }

    [Fact]
    public void Check2()
    {
        var checker = new BigFixedArrayCheck();

        checker.BigData1 = new byte[100_000];
        checker.BigData2 = new byte[1_000_000];

        Random.Shared.NextBytes(checker.BigData1);
        Random.Shared.NextBytes(checker.BigData2);

        var bin = MemoryPackSerializer.Serialize(checker);
        var v2 = MemoryPackSerializer.Deserialize<BigFixedArrayCheck>(bin);
#pragma warning disable CS8602
        v2.BigData1.Should().BeEquivalentTo(checker.BigData1);
        v2.BigData2.Should().BeEquivalentTo(checker.BigData2);
    }
}

