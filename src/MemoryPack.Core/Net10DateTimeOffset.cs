using System.Runtime.InteropServices;

namespace MemoryPack;

/// <summary>
/// <see cref="DateTimeOffset"/> compatible struct that binary layout is fixed to the layout of .NET 10 or later(int offsetMinutes, long utcTicks).
/// <para>
/// <see cref="DateTimeOffset"/> is serialized as its memory layout but it is different by runtime(.NET 9 or lower is (short offsetMinutes, long utcTicks), Unity is (long utcTicks, short offsetMinutes)).
/// If use this type instead of <see cref="DateTimeOffset"/>, the binary is same in all runtimes and it is compatible with <see cref="DateTimeOffset"/> of .NET 10 or later.
/// </para>
/// <para>
/// In deserialize, the binary serialized by <see cref="DateTimeOffset"/> of .NET 9 or lower can also be read.
/// The binary serialized by <see cref="DateTimeOffset"/> of Unity can not be read, use <see cref="UnityDateTimeOffset"/> for it.
/// </para>
/// </summary>
[StructLayout(LayoutKind.Explicit, Size = 16)]
public readonly struct Net10DateTimeOffset
{
#pragma warning disable CS0414 // The field is assigned but its value is never used
#pragma warning disable IDE0052

    // In .NET 9 or lower, DateTimeOffset is (short offsetMinutes, 6 bytes padding, long utcTicks) and padding is not initialized.
    // In .NET 10 or later, DateTimeOffset is (int offsetMinutes, 4 bytes padding, long utcTicks).
    // Write: offsetMinutes as int(sign extended) and padding as zero, it can be read from the both.
    // Read: only uses lower 16 bit of offsetMinutes(valid range of offset is -840 ~ +840), it can read the both.

    [FieldOffset(0)]
    readonly int offsetMinutes;
    [FieldOffset(4)]
    readonly int padding;
    [FieldOffset(8)]
    readonly long utcTicks;

#pragma warning restore IDE0052
#pragma warning restore CS0414

    public Net10DateTimeOffset(DateTimeOffset value)
    {
        this.offsetMinutes = (int)(value.Offset.Ticks / TimeSpan.TicksPerMinute);
        this.padding = 0;
        this.utcTicks = value.UtcTicks;
    }

    /// <summary>
    /// Convert to <see cref="DateTimeOffset"/>, throws <see cref="ArgumentOutOfRangeException"/> if the value(comes from malformed data) is out of range.
    /// </summary>
    public DateTimeOffset ToDateTimeOffset()
    {
        var offsetTicks = (short)offsetMinutes * TimeSpan.TicksPerMinute;
        return new DateTimeOffset(unchecked(utcTicks + offsetTicks), new TimeSpan(offsetTicks));
    }

    public static implicit operator Net10DateTimeOffset(DateTimeOffset value)
    {
        return new Net10DateTimeOffset(value);
    }

    public static implicit operator DateTimeOffset(Net10DateTimeOffset value)
    {
        return value.ToDateTimeOffset();
    }

    public override string ToString()
    {
        return ToDateTimeOffset().ToString();
    }
}
