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
    public partial class Tag : IGenericEquality<Tag>
    {
        [ProtoMember(1)]
        [Key(0), Id(0)]
        public string name { get; set; }
        [ProtoMember(2)]
        [Key(1), Id(1)]
        public int? count { get; set; }
        [ProtoMember(3)]
        [Key(2), Id(2)]
        public bool? is_required { get; set; }
        [ProtoMember(4)]
        [Key(3), Id(3)]
        public bool? is_moderator_only { get; set; }
        [ProtoMember(5)]
        [Key(4), Id(4)]
        public int? user_id { get; set; }
        [ProtoMember(6)]
        [Key(5), Id(5)]
        public bool? has_synonyms { get; set; }
        [ProtoMember(7)]
        [Key(6), Id(6)]
        public DateTime? last_activity_date { get; set; }
        [ProtoMember(8)]
        [Key(7), Id(7)]
        public List<string> synonyms { get; set; }

        public bool Equals(Tag obj)
        {
            return
                this.count.TrueEquals(obj.count) &&
                this.has_synonyms.TrueEquals(obj.has_synonyms) &&
                this.is_moderator_only.TrueEquals(obj.is_moderator_only) &&
                this.is_required.TrueEquals(obj.is_required) &&
                this.last_activity_date.TrueEquals(obj.last_activity_date) &&
                this.name.TrueEqualsString(obj.name) &&
                this.synonyms.TrueEqualsString(obj.synonyms) &&
                this.user_id.TrueEquals(obj.user_id);
        }

        public bool EqualsDynamic(dynamic obj)
        {
            return
                this.count.TrueEquals((int?)obj.count) &&
                this.has_synonyms.TrueEquals((bool?)obj.has_synonyms) &&
                this.is_moderator_only.TrueEquals((bool?)obj.is_moderator_only) &&
                this.is_required.TrueEquals((bool?)obj.is_required) &&
                this.last_activity_date.TrueEquals((DateTime?)obj.last_activity_date) &&
                this.name.TrueEqualsString((string)obj.name) &&
                this.synonyms.TrueEqualsString((IEnumerable<string>)obj.synonyms) &&
                this.user_id.TrueEquals((int?)obj.user_id);
        }
    }
}
