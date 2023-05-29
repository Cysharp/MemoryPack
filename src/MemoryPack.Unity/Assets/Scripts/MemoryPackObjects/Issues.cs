using MemoryPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.MemoryPackObjects
{
    public struct float2 { }
    public struct quaternion { }

    [Serializable]
    [MemoryPackable]
    public partial class PlayerInput
    {
        public float2 move;
        public quaternion target;
        public List<KeyRecord> keyRecords = new();
    }

    public enum EAction : byte
    {
        Ability1,
        Ability2,
        Ability3,
        Ability4,
        Ability5,
        Ability6,
    }

    public enum EActionStatus : byte
    {
        KeyDown = 0,
        keyPressing = 1,
        KeyUp = 2,
    }

    [MemoryPackable]
    public partial struct KeyRecord
    {
        public EAction action;
        public EActionStatus status;
    }

}
