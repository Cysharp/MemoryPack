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
    public enum PostType : byte
    {
        question = 1,
        answer = 2
    }

    [ProtoContract]
    [MemoryPackable, MessagePackObject, GenerateSerializer]
    public partial class Comment : IGenericEquality<Comment>
    {
        [ProtoMember(1)]
        [Key(0), Id(0)]
        public int? comment_id { get; set; }
        [ProtoMember(2)]
        [Key(1), Id(1)]
        public int? post_id { get; set; }
        [ProtoMember(3)]
        [Key(2), Id(2)]
        public DateTime? creation_date { get; set; }
        [ProtoMember(4)]
        [Key(3), Id(3)]
        public PostType? post_type { get; set; }
        [ProtoMember(5)]
        [Key(4), Id(4)]
        public int? score { get; set; }
        [ProtoMember(6)]
        [Key(5), Id(5)]
        public bool? edited { get; set; }
        [ProtoMember(7)]
        [Key(6), Id(6)]
        public string body { get; set; }
        [ProtoMember(8)]
        [Key(7), Id(7)]
        public ShallowUser owner { get; set; }
        [ProtoMember(9)]
        [Key(8), Id(8)]
        public ShallowUser reply_to_user { get; set; }
        [ProtoMember(10)]
        [Key(9), Id(9)]
        public string link { get; set; }
        [ProtoMember(11)]
        [Key(10), Id(10)]
        public string body_markdown { get; set; }
        [ProtoMember(12)]
        [Key(11), Id(11)]
        public bool? upvoted { get; set; }

        public bool Equals(Comment obj)
        {
            return
                this.body.TrueEqualsString(obj.body) &&
                this.body_markdown.TrueEqualsString(obj.body_markdown) &&
                this.comment_id.TrueEquals(obj.comment_id) &&
                this.creation_date.TrueEquals(obj.creation_date) &&
                this.edited.TrueEquals(obj.edited) &&
                this.link.TrueEqualsString(obj.link) &&
                this.owner.TrueEquals(obj.owner) &&
                this.post_id.TrueEquals(obj.post_id) &&
                this.post_type.TrueEquals(obj.post_type) &&
                this.reply_to_user.TrueEquals(obj.reply_to_user) &&
                this.score.TrueEquals(obj.score) &&
                this.upvoted.TrueEquals(obj.upvoted);
        }

        public bool EqualsDynamic(dynamic obj)
        {
            return
                this.body.TrueEqualsString((string)obj.body) &&
                this.body_markdown.TrueEqualsString((string)obj.body_markdown) &&
                this.comment_id.TrueEquals((int?)obj.comment_id) &&
                this.creation_date.TrueEquals((DateTime?)obj.creation_date) &&
                this.edited.TrueEquals((bool?)obj.edited) &&
                this.link.TrueEqualsString((string)obj.link) &&
                (this.owner == null && obj.owner == null || this.owner.EqualsDynamic(obj.owner)) &&
                this.post_id.TrueEquals((int?)obj.post_id) &&
                this.post_type.TrueEquals((PostType?)obj.post_type) &&
                (this.reply_to_user == null && obj.reply_to_user == null || this.reply_to_user.EqualsDynamic(obj.reply_to_user)) &&
                this.score.TrueEquals((int?)obj.score) &&
                this.upvoted.TrueEquals((bool?)obj.upvoted);
        }
    }
}
