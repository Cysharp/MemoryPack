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
    public enum InboxItemType
    {
        comment = 1,
        chat_message = 2,
        new_answer = 3,
        careers_message = 4,
        careers_invitations = 5,
        meta_question = 6,
        post_notice = 7,
        moderator_message = 8
    }

    [ProtoContract]
    [MemoryPackable, MessagePackObject, GenerateSerializer]
    public partial class InboxItem : IGenericEquality<InboxItem>
    {
        [ProtoMember(1)]
        [Key(0), Id(0)]
        public InboxItemType? item_type { get; set; }
        [ProtoMember(2)]
        [Key(1), Id(1)]
        public int? question_id { get; set; }
        [ProtoMember(3)]
        [Key(2), Id(2)]
        public int? answer_id { get; set; }
        [ProtoMember(4)]
        [Key(3), Id(3)]
        public int? comment_id { get; set; }
        [ProtoMember(5)]
        [Key(4), Id(4)]
        public string title { get; set; }
        [ProtoMember(6)]
        [Key(5), Id(5)]
        public DateTime? creation_date { get; set; }
        [ProtoMember(7)]
        [Key(6), Id(6)]
        public bool? is_unread { get; set; }
        [ProtoMember(8)]
        [Key(7), Id(7)]
        public Site site { get; set; }
        [ProtoMember(9)]
        [Key(8), Id(8)]
        public string body { get; set; }
        [ProtoMember(10)]
        [Key(9), Id(9)]
        public string link { get; set; }

        public bool Equals(InboxItem obj)
        {
            return
                this.answer_id.TrueEquals(obj.answer_id) &&
                this.body.TrueEqualsString(obj.body) &&
                this.comment_id.TrueEquals(obj.comment_id) &&
                this.creation_date.TrueEquals(obj.creation_date) &&
                this.is_unread.TrueEquals(obj.is_unread) &&
                this.item_type.TrueEquals(obj.item_type) &&
                this.link.TrueEqualsString(obj.link) &&
                this.question_id.TrueEquals(obj.question_id) &&
                this.site.TrueEquals(obj.site) &&
                this.title.TrueEqualsString(obj.title);
        }

        public bool EqualsDynamic(dynamic obj)
        {
            return
                this.answer_id.TrueEquals((int?)obj.answer_id) &&
                this.body.TrueEqualsString((string)obj.body) &&
                this.comment_id.TrueEquals((int?)obj.comment_id) &&
                this.creation_date.TrueEquals((DateTime?)obj.creation_date) &&
                this.is_unread.TrueEquals((bool?)obj.is_unread) &&
                this.item_type.TrueEquals((InboxItemType?)obj.item_type) &&
                this.link.TrueEqualsString((string)obj.link) &&
                this.question_id.TrueEquals((int?)obj.question_id) &&
                (this.site == null && obj.site == null || this.site.EqualsDynamic(obj.site)) &&
                this.title.TrueEqualsString((string)obj.title);
        }
    }
}
