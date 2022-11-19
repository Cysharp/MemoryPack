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
    public enum QuestionTimelineAction : byte
    {
        question = 1,
        answer = 2,
        comment = 3,
        unaccepted_answer = 4,
        accepted_answer = 5,
        vote_aggregate = 6,
        revision = 7,
        post_state_changed = 8
    }

    [ProtoContract]
    [MemoryPackable, MessagePackObject, GenerateSerializer]
    public partial class QuestionTimeline : IGenericEquality<QuestionTimeline>
    {
        [ProtoMember(1)]
        [Key(0), Id(0)]
        public QuestionTimelineAction? timeline_type { get; set; }
        [ProtoMember(2)]
        [Key(1), Id(1)]
        public int? question_id { get; set; }
        [ProtoMember(3)]
        [Key(2), Id(2)]
        public int? post_id { get; set; }
        [ProtoMember(4)]
        [Key(3), Id(3)]
        public int? comment_id { get; set; }
        [ProtoMember(5)]
        [Key(4), Id(4)]
        public string revision_guid { get; set; }
        [ProtoMember(6)]
        [Key(5), Id(5)]
        public int? up_vote_count { get; set; }
        [ProtoMember(7)]
        [Key(6), Id(6)]
        public int? down_vote_count { get; set; }
        [ProtoMember(8)]
        [Key(7), Id(7)]
        public DateTime? creation_date { get; set; }
        [ProtoMember(9)]
        [Key(8), Id(8)]
        public ShallowUser user { get; set; }
        [ProtoMember(10)]
        [Key(9), Id(9)]
        public ShallowUser owner { get; set; }

        public bool Equals(QuestionTimeline obj)
        {
            return
                this.comment_id.TrueEquals(obj.comment_id) &&
                this.creation_date.TrueEquals(obj.creation_date) &&
                this.down_vote_count.TrueEquals(obj.down_vote_count) &&
                this.owner.TrueEquals(obj.owner) &&
                this.post_id.TrueEquals(obj.post_id) &&
                this.question_id.TrueEquals(obj.question_id) &&
                this.revision_guid.TrueEqualsString(obj.revision_guid) &&
                this.timeline_type.TrueEquals(obj.timeline_type) &&
                this.up_vote_count.TrueEquals(obj.up_vote_count) &&
                this.user.TrueEquals(obj.user);
        }

        public bool EqualsDynamic(dynamic obj)
        {
            return
                this.comment_id.TrueEquals((int?)obj.comment_id) &&
                this.creation_date.TrueEquals((DateTime?)obj.creation_date) &&
                this.down_vote_count.TrueEquals((int?)obj.down_vote_count) &&
                (this.owner == null && obj.owner == null || this.owner.EqualsDynamic(obj.owner)) &&
                this.post_id.TrueEquals((int?)obj.post_id) &&
                this.question_id.TrueEquals((int?)obj.question_id) &&
                this.revision_guid.TrueEqualsString((string)obj.revision_guid) &&
                this.timeline_type.TrueEquals((QuestionTimelineAction?)obj.timeline_type) &&
                this.up_vote_count.TrueEquals((int?)obj.up_vote_count) &&
                (this.user == null && obj.user == null || this.user.EqualsDynamic(obj.user));
        }
    }
}
