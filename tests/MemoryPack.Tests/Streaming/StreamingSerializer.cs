using MemoryPack.Streaming;
using System;
using System.IO;
using System.Linq;
using System.IO.Pipelines;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace MemoryPack.Tests.Streaming;

public class StreamingSerializer
{
    [Fact]
    public async Task Serialize()
    {
        var seq = Enumerable.Range(1, 10000).ToArray();

        {
            var ms = new MemoryStream();
            await MemoryPackStreamingSerializer.SerializeAsync(ms, seq.Length, seq);
            var v2 = MemoryPackSerializer.Deserialize<int[]>(ms.ToArray());

            v2.Should().Equal(seq);
        }

        {
            var pipe = new Pipe();

            await MemoryPackStreamingSerializer.SerializeAsync(pipe.Writer, seq.Length, seq);

            await pipe.Writer.CompleteAsync();

            pipe.Reader.TryRead(out var result);

            result.IsCompleted.Should().BeTrue();
            var v2 = MemoryPackSerializer.Deserialize<int[]>(result.Buffer);

            v2.Should().Equal(seq);
        }
    }

    [Fact]
    public async Task Deserialize()
    {
        var seq = Enumerable.Range(1, 10000).ToArray();
        var bin = MemoryPackSerializer.Serialize(seq);

        {
            var ms = new MemoryStream(bin);

            var list = new List<int>();
            await foreach (var item in MemoryPackStreamingSerializer.DeserializeAsync<int>(ms))
            {
                list.Add(item);
            }

            list.Should().Equal(seq);
        }




    }

}


[MemoryPackable]
public partial class SampleClassForMemoryPack
{
    public int Id { get; set; }
    public string Name { get; set; }



    public SampleClassForMemoryPack(int id, string name)
    {
        Id = id;
        Name = name;
    }

    public bool Equals(SampleClassForMemoryPack? other)
    {
        if (ReferenceEquals(null, other))
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return Id == other.Id && Name == other.Name;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj))
        {
            return false;
        }

        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (obj.GetType() != GetType())
        {
            return false;
        }

        return Equals((SampleClassForMemoryPack)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Id, Name);
    }

    public override string ToString()
    {
        return $"{Id}-{Name}";
    }
}
