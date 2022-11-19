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
    public enum SearchExcerptItemType : byte
    {
        question = 1,
        answer = 2
    }

    [ProtoContract]
    [MemoryPackable, MessagePackObject, GenerateSerializer]
    public partial class SearchExcerpt : IGenericEquality<SearchExcerpt>
    {
        [ProtoMember(1)]
        [Key(0), Id(0)]
        public string title { get; set; }
        [ProtoMember(2)]
        [Key(1), Id(1)]
        public string excerpt { get; set; }
        [ProtoMember(3)]
        [Key(2), Id(2)]
        public DateTime? community_owned_date { get; set; }
        [ProtoMember(4)]
        [Key(3), Id(3)]
        public DateTime? locked_date { get; set; }
        [ProtoMember(5)]
        [Key(4), Id(4)]
        public DateTime? creation_date { get; set; }
        [ProtoMember(6)]
        [Key(5), Id(5)]
        public DateTime? last_activity_date { get; set; }
        [ProtoMember(7)]
        [Key(6), Id(6)]
        public ShallowUser owner { get; set; }
        [ProtoMember(8)]
        [Key(7), Id(7)]
        public ShallowUser last_activity_user { get; set; }
        [ProtoMember(9)]
        [Key(8), Id(8)]
        public int? score { get; set; }
        [ProtoMember(10)]
        [Key(9), Id(9)]
        public SearchExcerptItemType? item_type { get; set; }
        [ProtoMember(11)]
        [Key(10), Id(10)]
        public string body { get; set; }
        [ProtoMember(12)]
        [Key(11), Id(11)]
        public int? question_id { get; set; }
        [ProtoMember(13)]
        [Key(12), Id(12)]
        public bool? is_answered { get; set; }
        [ProtoMember(14)]
        [Key(13), Id(13)]
        public int? answer_count { get; set; }
        [ProtoMember(15)]
        [Key(14), Id(14)]
        public List<string> tags { get; set; }
        [ProtoMember(16)]
        [Key(15), Id(15)]
        public DateTime? closed_date { get; set; }
        [ProtoMember(17)]
        [Key(16), Id(16)]
        public int? answer_id { get; set; }
        [ProtoMember(18)]
        [Key(17), Id(17)]
        public bool? is_accepted { get; set; }

        public bool Equals(SearchExcerpt obj)
        {
            return
                this.answer_count.TrueEquals(obj.answer_count) &&
                this.answer_id.TrueEquals(obj.answer_id) &&
                this.body.TrueEqualsString(obj.body) &&
                this.closed_date.TrueEquals(obj.closed_date) &&
                this.community_owned_date.TrueEquals(obj.community_owned_date) &&
                this.creation_date.TrueEquals(obj.creation_date) &&
                this.excerpt.TrueEqualsString(obj.excerpt) &&
                this.is_accepted.TrueEquals(obj.is_accepted) &&
                this.is_answered.TrueEquals(obj.is_answered) &&
                this.item_type.TrueEquals(obj.item_type) &&
                this.last_activity_date.TrueEquals(obj.last_activity_date) &&
                this.last_activity_user.TrueEquals(obj.last_activity_user) &&
                this.locked_date.TrueEquals(obj.locked_date) &&
                this.owner.TrueEquals(obj.owner) &&
                this.question_id.TrueEquals(obj.question_id) &&
                this.score.TrueEquals(obj.score) &&
                this.tags.TrueEqualsString(obj.tags) &&
                this.title.TrueEqualsString(obj.title);
        }

        public bool EqualsDynamic(dynamic obj)
        {
            return
                this.answer_count.TrueEquals((int?)obj.answer_count) &&
                this.answer_id.TrueEquals((int?)obj.answer_id) &&
                this.body.TrueEqualsString((string)obj.body) &&
                this.closed_date.TrueEquals((DateTime?)obj.closed_date) &&
                this.community_owned_date.TrueEquals((DateTime?)obj.community_owned_date) &&
                this.creation_date.TrueEquals((DateTime?)obj.creation_date) &&
                this.excerpt.TrueEqualsString((string)obj.excerpt) &&
                this.is_accepted.TrueEquals((bool?)obj.is_accepted) &&
                this.is_answered.TrueEquals((bool?)obj.is_answered) &&
                this.item_type.TrueEquals((SearchExcerptItemType?)obj.item_type) &&
                this.last_activity_date.TrueEquals((DateTime?)obj.last_activity_date) &&
                (this.last_activity_user == null && obj.last_activity_user == null || this.last_activity_user.EqualsDynamic(obj.last_activity_user)) &&
                this.locked_date.TrueEquals((DateTime?)obj.locked_date) &&
                (this.owner == null && obj.owner == null || this.owner.EqualsDynamic(obj.owner)) &&
                this.question_id.TrueEquals((int?)obj.question_id) &&
                this.score.TrueEquals((int?)obj.score) &&
                this.tags.TrueEqualsString((IEnumerable<string>)obj.tags) &&
                this.title.TrueEqualsString((string)obj.title);
        }
    }
}
