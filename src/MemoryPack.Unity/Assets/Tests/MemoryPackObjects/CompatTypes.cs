#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;

namespace MemoryPack.Tests.Models
{
    public enum CompatIntEnum
    {
        A, B, C
    }

    [MemoryPackable]
    public partial class CompatTypesData
    {
        public Net10DateTimeOffset Net10;
        public UnityDateTimeOffset Unity;
        public Net10DateTimeOffset? Net10Nullable;
        public UnityDateTimeOffset[]? UnityArray;
        public List<Net10DateTimeOffset>? Net10List;
        public SafeNullable<int> SafeInt;
        public SafeNullable<int> SafeIntNull;
        public SafeNullable<long> SafeLong;
        public SafeNullable<bool> SafeBool;
        public SafeNullable<CompatIntEnum> SafeEnum;
        public SafeNullable<int>[]? SafeArray;
        public List<SafeNullable<long>>? SafeList;
        public BitArray? Bits;
        public string? Text;
    }

    // same members as CompatTypesData but uses Nullable<T>
    [MemoryPackable]
    public partial class CompatNullableData
    {
        public int? SafeInt;
        public int? SafeIntNull;
        public long? SafeLong;
        public bool? SafeBool;
        public CompatIntEnum? SafeEnum;
    }

    [MemoryPackable]
    public partial class CompatSafeNullableData
    {
        public SafeNullable<int> SafeInt;
        public SafeNullable<int> SafeIntNull;
        public SafeNullable<long> SafeLong;
        public SafeNullable<bool> SafeBool;
        public SafeNullable<CompatIntEnum> SafeEnum;
    }

    [MemoryPackable(GenerateType.VersionTolerant)]
    public partial class CompatVersionTolerantData
    {
        [MemoryPackOrder(0)]
        public int A;
    }

    [MemoryPackable]
    public partial class CompatStringData
    {
        public string? Text;
    }
}
