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
    public partial class FlagOption : IGenericEquality<FlagOption>
    {
        [ProtoMember(1)]
        [Key(0), Id(0)]
        public int? option_id { get; set; }
        [ProtoMember(2)]
        [Key(1), Id(1)]
        public bool? requires_comment { get; set; }
        [ProtoMember(3)]
        [Key(2), Id(2)]
        public bool? requires_site { get; set; }
        [ProtoMember(4)]
        [Key(3), Id(3)]
        public bool? requires_question_id { get; set; }
        [ProtoMember(5)]
        [Key(4), Id(4)]
        public string title { get; set; }
        [ProtoMember(6)]
        [Key(5), Id(5)]
        public string description { get; set; }
        [ProtoMember(7)]
        [Key(6), Id(6)]
        public List<FlagOption> sub_options { get; set; }
        [ProtoMember(8)]
        [Key(7), Id(7)]
        public bool? has_flagged { get; set; }
        [ProtoMember(9)]
        [Key(8), Id(8)]
        public int? count { get; set; }

        public bool Equals(FlagOption obj)
        {
            return
                this.count.TrueEquals(obj.count) &&
                this.description.TrueEqualsString(obj.description) &&
                this.has_flagged.TrueEquals(obj.has_flagged) &&
                this.option_id.TrueEquals(obj.option_id) &&
                this.requires_comment.TrueEquals(obj.requires_comment) &&
                this.requires_question_id.TrueEquals(obj.requires_question_id) &&
                this.requires_site.TrueEquals(obj.requires_site) &&
                this.sub_options.TrueEqualsList(obj.sub_options) &&
                this.title.TrueEqualsString(obj.title);
        }

        public bool EqualsDynamic(dynamic obj)
        {
            return
                this.count.TrueEquals((int?)obj.count) &&
                this.description.TrueEqualsString((string)obj.description) &&
                this.has_flagged.TrueEquals((bool?)obj.has_flagged) &&
                this.option_id.TrueEquals((int?)obj.option_id) &&
                this.requires_comment.TrueEquals((bool?)obj.requires_comment) &&
                this.requires_question_id.TrueEquals((bool?)obj.requires_question_id) &&
                this.requires_site.TrueEquals((bool?)obj.requires_site) &&
                this.sub_options.TrueEqualsListDynamic((IEnumerable<dynamic>)obj.sub_options) &&
                this.title.TrueEqualsString((string)obj.title);
        }
    }
}
