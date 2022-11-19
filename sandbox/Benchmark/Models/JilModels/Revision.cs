#nullable disable
using MessagePack;
using MemoryPack;
using Orleans;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Benchmark.Models
{
    public enum RevisionType : byte
    {
        single_user = 1,
        vote_based = 2
    }

    [ProtoContract]
    [MemoryPackable, MessagePackObject, GenerateSerializer]
    public partial class Revision : IGenericEquality<Revision>
    {
        [ProtoMember(1)]
        [Key(0), Id(0)]
        public string revision_guid { get; set; }
        [ProtoMember(2)]
        [Key(1), Id(1)]
        public int? revision_number { get; set; }
        [ProtoMember(3)]
        [Key(2), Id(2)]
        public RevisionType? revision_type { get; set; }
        [ProtoMember(4)]
        [Key(3), Id(3)]
        public PostType? post_type { get; set; }
        [ProtoMember(5)]
        [Key(4), Id(4)]
        public int? post_id { get; set; }
        [ProtoMember(6)]
        [Key(5), Id(5)]
        public string comment { get; set; }
        [ProtoMember(7)]
        [Key(6), Id(6)]
        public DateTime? creation_date { get; set; }
        [ProtoMember(8)]
        [Key(7), Id(7)]
        public bool? is_rollback { get; set; }
        [ProtoMember(9)]
        [Key(8), Id(8)]
        public string last_body { get; set; }
        [ProtoMember(10)]
        [Key(9), Id(9)]
        public string last_title { get; set; }
        [ProtoMember(11)]
        [Key(10), Id(10)]
        public List<string> last_tags { get; set; }
        [ProtoMember(12)]
        [Key(11), Id(11)]
        public string body { get; set; }
        [ProtoMember(13)]
        [Key(12), Id(12)]
        public string title { get; set; }
        [ProtoMember(14)]
        [Key(13), Id(13)]
        public List<string> tags { get; set; }
        [ProtoMember(15)]
        [Key(14), Id(14)]
        public bool? set_community_wiki { get; set; }
        [ProtoMember(16)]
        [Key(15), Id(15)]
        public ShallowUser user { get; set; }

        public bool Equals(Revision obj)
        {
            return
                this.body.TrueEqualsString(obj.body) &&
                this.comment.TrueEqualsString(obj.comment) &&
                this.creation_date.TrueEquals(obj.creation_date) &&
                this.is_rollback.TrueEquals(obj.is_rollback) &&
                this.last_body.TrueEqualsString(obj.last_body) &&
                this.last_tags.TrueEqualsString(obj.last_tags) &&
                this.last_title.TrueEqualsString(obj.last_title) &&
                this.post_id.TrueEquals(obj.post_id) &&
                this.post_type.TrueEquals(obj.post_type) &&
                this.revision_guid.TrueEqualsString(obj.revision_guid) &&
                this.revision_number.TrueEquals(obj.revision_number) &&
                this.revision_type.TrueEquals(obj.revision_type) &&
                this.set_community_wiki.TrueEquals(obj.set_community_wiki) &&
                this.tags.TrueEqualsString(obj.tags) &&
                this.title.TrueEqualsString(obj.title) &&
                this.user.TrueEquals(obj.user);
        }

        public bool EqualsDynamic(dynamic obj)
        {
            return
                this.body.TrueEqualsString((string)obj.body) &&
                this.comment.TrueEqualsString((string)obj.comment) &&
                this.creation_date.TrueEquals((DateTime?)obj.creation_date) &&
                this.is_rollback.TrueEquals((bool?)obj.is_rollback) &&
                this.last_body.TrueEqualsString((string)obj.last_body) &&
                this.last_tags.TrueEqualsString((IEnumerable<string>)obj.last_tags) &&
                this.last_title.TrueEqualsString((string)obj.last_title) &&
                this.post_id.TrueEquals((int?)obj.post_id) &&
                this.post_type.TrueEquals((PostType?)obj.post_type) &&
                this.revision_guid.TrueEqualsString((string)obj.revision_guid) &&
                this.revision_number.TrueEquals((int?)obj.revision_number) &&
                this.revision_type.TrueEquals((RevisionType?)obj.revision_type) &&
                this.set_community_wiki.TrueEquals((bool?)obj.set_community_wiki) &&
                this.tags.TrueEqualsString((IEnumerable<string>)obj.tags) &&
                this.title.TrueEqualsString((string)obj.title) &&
                (this.user == null && obj.user == null || this.user.EqualsDynamic(obj.user));
        }
    }
}
