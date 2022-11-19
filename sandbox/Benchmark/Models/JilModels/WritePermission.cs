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
    public partial class WritePermission : IGenericEquality<WritePermission>
    {
        [ProtoMember(1)]
        [Key(0), Id(0)]
        public int? user_id { get; set; }
        [ProtoMember(2)]
        [Key(1), Id(1)]
        public string object_type { get; set; }
        [ProtoMember(3)]
        [Key(2), Id(2)]
        public bool? can_add { get; set; }
        [ProtoMember(4)]
        [Key(3), Id(3)]
        public bool? can_edit { get; set; }
        [ProtoMember(5)]
        [Key(4), Id(4)]
        public bool? can_delete { get; set; }
        [ProtoMember(6)]
        [Key(5), Id(5)]
        public int? max_daily_actions { get; set; }
        [ProtoMember(7)]
        [Key(6), Id(6)]
        public int? min_seconds_between_actions { get; set; }

        public bool Equals(WritePermission obj)
        {
            return
                this.can_add.TrueEquals(obj.can_add) &&
                this.can_delete.TrueEquals(obj.can_delete) &&
                this.can_edit.TrueEquals(obj.can_edit) &&
                this.max_daily_actions.TrueEquals(obj.max_daily_actions) &&
                this.min_seconds_between_actions.TrueEquals(obj.min_seconds_between_actions) &&
                this.object_type.TrueEqualsString(obj.object_type) &&
                this.user_id.TrueEquals(obj.user_id);
        }

        public bool EqualsDynamic(dynamic obj)
        {
            return
                this.can_add.TrueEquals((bool?)obj.can_add) &&
                this.can_delete.TrueEquals((bool?)obj.can_delete) &&
                this.can_edit.TrueEquals((bool?)obj.can_edit) &&
                this.max_daily_actions.TrueEquals((int?)obj.max_daily_actions) &&
                this.min_seconds_between_actions.TrueEquals((int?)obj.min_seconds_between_actions) &&
                this.object_type.TrueEqualsString((string)obj.object_type) &&
                this.user_id.TrueEquals((int?)obj.user_id);
        }
    }
}
