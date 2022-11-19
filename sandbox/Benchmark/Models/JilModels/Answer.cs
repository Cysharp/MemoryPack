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
    public partial class Answer : IGenericEquality<Answer>
    {
        [ProtoMember(1)]
        [Key(0), Id(0)]
        public int? question_id { get; set; }
        [ProtoMember(2)]
        [Key(1), Id(1)]
        public int? answer_id { get; set; }
        [ProtoMember(3)]
        [Key(2), Id(2)]
        public DateTime? locked_date { get; set; }
        [ProtoMember(4)]
        [Key(3), Id(3)]
        public DateTime? creation_date { get; set; }
        [ProtoMember(5)]
        [Key(4), Id(4)]
        public DateTime? last_edit_date { get; set; }
        [ProtoMember(6)]
        [Key(5), Id(5)]
        public DateTime? last_activity_date { get; set; }
        [ProtoMember(7)]
        [Key(6), Id(6)]
        public int? score { get; set; }
        [ProtoMember(8)]
        [Key(7), Id(7)]
        public DateTime? community_owned_date { get; set; }
        [ProtoMember(9)]
        [Key(8), Id(8)]
        public bool? is_accepted { get; set; }
        [ProtoMember(10)]
        [Key(9), Id(9)]
        public string body { get; set; }
        [ProtoMember(11)]
        [Key(10), Id(10)]
        public ShallowUser owner { get; set; }
        [ProtoMember(12)]
        [Key(11), Id(11)]
        public string title { get; set; }
        [ProtoMember(13)]
        [Key(12), Id(12)]
        public int? up_vote_count { get; set; }
        [ProtoMember(14)]
        [Key(13), Id(13)]
        public int? down_vote_count { get; set; }
        [ProtoMember(15)]
        [Key(14), Id(14)]
        public List<Comment> comments { get; set; }
        [ProtoMember(16)]
        [Key(15), Id(15)]
        public string link { get; set; }
        [ProtoMember(17)]
        [Key(16), Id(16)]
        public List<string> tags { get; set; }
        [ProtoMember(18)]
        [Key(17), Id(17)]
        public bool? upvoted { get; set; }
        [ProtoMember(19)]
        [Key(18), Id(18)]
        public bool? downvoted { get; set; }
        [ProtoMember(20)]
        [Key(19), Id(19)]
        public bool? accepted { get; set; }
        [ProtoMember(21)]
        [Key(20), Id(20)]
        public ShallowUser last_editor { get; set; }
        [ProtoMember(22)]
        [Key(21), Id(21)]
        public int? comment_count { get; set; }
        [ProtoMember(23)]
        [Key(22), Id(22)]
        public string body_markdown { get; set; }
        [ProtoMember(24)]
        [Key(23), Id(23)]
        public string share_link { get; set; }

        public bool Equals(Answer obj)
        {
            return
                this.accepted.TrueEquals(obj.accepted) &&
                this.answer_id.TrueEquals(obj.answer_id) &&
                this.body.TrueEqualsString(obj.body) &&
                this.body_markdown.TrueEqualsString(obj.body_markdown) &&
                this.comment_count.TrueEquals(obj.comment_count) &&
                this.comments.TrueEqualsList(obj.comments) &&
                this.community_owned_date.TrueEquals(obj.community_owned_date) &&
                this.creation_date.TrueEquals(obj.creation_date) &&
                this.down_vote_count.TrueEquals(obj.down_vote_count) &&
                this.downvoted.TrueEquals(obj.downvoted) &&
                this.is_accepted.TrueEquals(obj.is_accepted) &&
                this.last_activity_date.TrueEquals(obj.last_activity_date) &&
                this.last_edit_date.TrueEquals(obj.last_edit_date) &&
                this.last_editor.TrueEquals(obj.last_editor) &&
                this.link.TrueEqualsString(obj.link) &&
                this.locked_date.TrueEquals(obj.locked_date) &&
                this.owner.TrueEquals(obj.owner) &&
                this.question_id.TrueEquals(obj.question_id) &&
                this.score.TrueEquals(obj.score) &&
                this.share_link.TrueEqualsString(obj.share_link) &&
                this.tags.TrueEqualsString(obj.tags) &&
                this.title.TrueEqualsString(obj.title) &&
                this.up_vote_count.TrueEquals(obj.up_vote_count) &&
                this.upvoted.TrueEquals(obj.upvoted);
        }

        public bool EqualsDynamic(dynamic obj)
        {
            return
                this.accepted.TrueEquals((bool?)obj.accepted) &&
                this.answer_id.TrueEquals((int?)obj.answer_id) &&
                this.body.TrueEqualsString((string)obj.body) &&
                this.body_markdown.TrueEqualsString((string)obj.body_markdown) &&
                this.comment_count.TrueEquals((int?)obj.comment_count) &&
                this.comments.TrueEqualsListDynamic((IEnumerable<dynamic>)obj.comments) &&
                this.community_owned_date.TrueEquals((DateTime?)obj.community_owned_date) &&
                this.creation_date.TrueEquals((DateTime?)obj.creation_date) &&
                this.down_vote_count.TrueEquals((int?)obj.down_vote_count) &&
                this.downvoted.TrueEquals((bool?)obj.downvoted) &&
                this.is_accepted.TrueEquals((bool?)obj.is_accepted) &&
                this.last_activity_date.TrueEquals((DateTime?)obj.last_activity_date) &&
                this.last_edit_date.TrueEquals((DateTime?)obj.last_edit_date) &&
                (this.last_editor == null && obj.last_editor == null || this.last_editor.EqualsDynamic(obj.last_editor)) &&
                this.link.TrueEqualsString((string)obj.link) &&
                this.locked_date.TrueEquals((DateTime?)obj.locked_date) &&
                (this.owner == null && obj.owner == null || this.owner.EqualsDynamic(obj.owner)) &&
                this.question_id.TrueEquals((int?)obj.question_id) &&
                this.score.TrueEquals((int?)obj.score) &&
                this.share_link.TrueEqualsString((string)obj.share_link) &&
                this.tags.TrueEqualsString((IEnumerable<string>)obj.tags) &&
                this.title.TrueEqualsString((string)obj.title) &&
                this.up_vote_count.TrueEquals((int?)obj.up_vote_count) &&
                this.upvoted.TrueEquals((bool?)obj.upvoted);
        }
    }
}
