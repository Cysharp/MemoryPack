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
    public partial class SuggestedEdit : IGenericEquality<SuggestedEdit>
    {
        [ProtoMember(1)]
        [Key(0), Id(0)]
        public int? suggested_edit_id { get; set; }
        [ProtoMember(2)]
        [Key(1), Id(1)]
        public int? post_id { get; set; }
        [ProtoMember(3)]
        [Key(2), Id(2)]
        public PostType? post_type { get; set; }
        [ProtoMember(4)]
        [Key(3), Id(3)]
        public string body { get; set; }
        [ProtoMember(5)]
        [Key(4), Id(4)]
        public string title { get; set; }
        [ProtoMember(6)]
        [Key(5), Id(5)]
        public List<string> tags { get; set; }
        [ProtoMember(7)]
        [Key(6), Id(6)]
        public string comment { get; set; }
        [ProtoMember(8)]
        [Key(7), Id(7)]
        public DateTime? creation_date { get; set; }
        [ProtoMember(9)]
        [Key(8), Id(8)]
        public DateTime? approval_date { get; set; }
        [ProtoMember(10)]
        [Key(9), Id(9)]
        public DateTime? rejection_date { get; set; }
        [ProtoMember(11)]
        [Key(10), Id(10)]
        public ShallowUser proposing_user { get; set; }

        public bool Equals(SuggestedEdit obj)
        {
            return
                this.approval_date.TrueEquals(obj.approval_date) &&
                this.body.TrueEqualsString(obj.body) &&
                this.comment.TrueEqualsString(obj.comment) &&
                this.creation_date.TrueEquals(obj.creation_date) &&
                this.post_id.TrueEquals(obj.post_id) &&
                this.post_type.TrueEquals(obj.post_type) &&
                this.proposing_user.TrueEquals(obj.proposing_user) &&
                this.rejection_date.TrueEquals(obj.rejection_date) &&
                this.suggested_edit_id.TrueEquals(obj.suggested_edit_id) &&
                this.tags.TrueEqualsString(obj.tags) &&
                this.title.TrueEqualsString(obj.title);
        }

        public bool EqualsDynamic(dynamic obj)
        {
            return
                this.approval_date.TrueEquals((DateTime?)obj.approval_date) &&
                this.body.TrueEqualsString((string)obj.body) &&
                this.comment.TrueEqualsString((string)obj.comment) &&
                this.creation_date.TrueEquals((DateTime?)obj.creation_date) &&
                this.post_id.TrueEquals((int?)obj.post_id) &&
                this.post_type.TrueEquals((PostType?)obj.post_type) &&
                (this.proposing_user == null && obj.proposing_user == null || this.proposing_user.EqualsDynamic(obj.proposing_user)) &&
                this.rejection_date.TrueEquals((DateTime?)obj.rejection_date) &&
                this.suggested_edit_id.TrueEquals((int?)obj.suggested_edit_id) &&
                this.tags.TrueEqualsString((IEnumerable<string>)obj.tags) &&
                this.title.TrueEqualsString((string)obj.title);
        }
    }
}
