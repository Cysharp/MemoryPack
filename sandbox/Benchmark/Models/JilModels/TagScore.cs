#nullable disable
using MemoryPack;
using MessagePack;
using Orleans;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Benchmark.Models
{
    [ProtoContract]
    [MemoryPackable, MessagePackObject, GenerateSerializer]
    public partial class TagScore : IGenericEquality<TagScore>
    {
        [ProtoMember(1)]
        [Key(0), Id(0)]
        public ShallowUser user { get; set; }
        [ProtoMember(2)]
        [Key(1), Id(1)]
        public int? score { get; set; }
        [ProtoMember(3)]
        [Key(2), Id(2)]
        public int? post_count { get; set; }

        public bool Equals(TagScore obj)
        {
            return
                this.post_count.TrueEquals(obj.post_count) &&
                this.score.TrueEquals(obj.score) &&
                this.user.TrueEquals(obj.user);
        }

        public bool EqualsDynamic(dynamic obj)
        {
            return
                this.post_count.TrueEquals((int?)obj.post_count) &&
                this.score.TrueEquals((int?)obj.score) &&
                (this.user == null && obj.user == null || this.user.EqualsDynamic(obj.user));
        }
    }
}
