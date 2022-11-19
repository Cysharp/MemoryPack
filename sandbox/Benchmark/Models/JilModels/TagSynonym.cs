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
    public partial class TagSynonym : IGenericEquality<TagSynonym>
    {
        [ProtoMember(1)]
        [Key(0), Id(0)]
        public string from_tag { get; set; }
        [ProtoMember(2)]
        [Key(1), Id(1)]
        public string to_tag { get; set; }
        [ProtoMember(3)]
        [Key(2), Id(2)]
        public int? applied_count { get; set; }
        [ProtoMember(4)]
        [Key(3), Id(3)]
        public DateTime? last_applied_date { get; set; }
        [ProtoMember(5)]
        [Key(4), Id(4)]
        public DateTime? creation_date { get; set; }

        public bool Equals(TagSynonym obj)
        {
            return
                this.applied_count.TrueEquals(obj.applied_count) &&
                this.creation_date.TrueEquals(obj.creation_date) &&
                this.from_tag.TrueEqualsString(obj.from_tag) &&
                this.last_applied_date.TrueEquals(obj.last_applied_date) &&
                this.to_tag.TrueEqualsString(obj.to_tag);
        }

        public bool EqualsDynamic(dynamic obj)
        {
            return
                this.applied_count.TrueEquals((int?)obj.applied_count) &&
                this.creation_date.TrueEquals((DateTime?)obj.creation_date) &&
                this.from_tag.TrueEqualsString((string)obj.from_tag) &&
                this.last_applied_date.TrueEquals((DateTime?)obj.last_applied_date) &&
                this.to_tag.TrueEqualsString((string)obj.to_tag);
        }
    }
}
