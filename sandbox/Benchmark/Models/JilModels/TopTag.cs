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
    public partial class TopTag : IGenericEquality<TopTag>
    {
        [ProtoMember(1)]
        [Key(0), Id(0)]
        public string tag_name { get; set; }
        [ProtoMember(2)]
        [Key(1), Id(1)]
        public int? question_score { get; set; }
        [ProtoMember(3)]
        [Key(2), Id(2)]
        public int? question_count { get; set; }
        [ProtoMember(4)]
        [Key(3), Id(3)]
        public int? answer_score { get; set; }
        [ProtoMember(5)]
        [Key(4), Id(4)]
        public int? answer_count { get; set; }
        [ProtoMember(6)]
        [Key(5), Id(5)]
        public int? user_id { get; set; }

        public bool Equals(TopTag obj)
        {
            return
                this.answer_count.TrueEquals(obj.answer_count) &&
                this.answer_score.TrueEquals(obj.answer_score) &&
                this.question_count.TrueEquals(obj.question_count) &&
                this.question_score.TrueEquals(obj.question_score) &&
                this.tag_name.TrueEqualsString(obj.tag_name) &&
                this.user_id.TrueEquals(obj.user_id);
        }

        public bool EqualsDynamic(dynamic obj)
        {
            return
                this.answer_count.TrueEquals((int?)obj.answer_count) &&
                this.answer_score.TrueEquals((int?)obj.answer_score) &&
                this.question_count.TrueEquals((int?)obj.question_count) &&
                this.question_score.TrueEquals((int?)obj.question_score) &&
                this.tag_name.TrueEqualsString((string)obj.tag_name) &&
                this.user_id.TrueEquals((int?)obj.user_id);
        }
    }
}
