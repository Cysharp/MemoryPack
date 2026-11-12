using System.Runtime.InteropServices;

namespace MemoryPack;

/// <summary>
/// <see cref="DateTimeOffset"/> compatible struct that binary layout is fixed to the layout of Unity(Mono, IL2CPP)(long utcTicks, short offsetMinutes).
/// <para>
/// <see cref="DateTimeOffset"/> is serialized as its memory layout but it is different by runtime(.NET 9 or lower is (short offsetMinutes, long utcTicks), .NET 10 or later is (int offsetMinutes, long utcTicks)).
/// If use this type instead of <see cref="DateTimeOffset"/>, the binary is same in all runtimes and it is compatible with the binary serialized by <see cref="DateTimeOffset"/> of Unity.
/// </para>
/// <para>
/// If there is no need to read the binary that was serialized in Unity, use <see cref="Net10DateTimeOffset"/>.
/// </para>
/// </summary>
[StructLayout(LayoutKind.Explicit, Size = 16)]
public readonly struct UnityDateTimeOffset
{
#pragma warning disable CS0414 // The field is assigned but its value is never used
#pragma warning disable IDE0052

    // In Unity, layout of DateTimeOffset is not reordered so it is same as the declared order, (long utcTicks, short offsetMinutes, 6 bytes padding) and padding is not initialized.
    // Write: padding as zero.

    [FieldOffset(0)]
    readonly long utcTicks;
    [FieldOffset(8)]
    readonly short offsetMinutes;
    [FieldOffset(10)]
    readonly short padding1;
    [FieldOffset(12)]
    readonly int padding2;

#pragma warning restore IDE0052
#pragma warning restore CS0414

    public UnityDateTimeOffset(DateTimeOffset value)
    {
        this.utcTicks = value.UtcTicks;
        this.offsetMinutes = (short)(value.Offset.Ticks / TimeSpan.TicksPerMinute);
        this.padding1 = 0;
        this.padding2 = 0;
    }

    /// <summary>
    /// Convert to <see cref="DateTimeOffset"/>, throws <see cref="ArgumentOutOfRangeException"/> if the value(comes from malformed data) is out of range.
    /// </summary>
    public DateTimeOffset ToDateTimeOffset()
    {
        var offsetTicks = offsetMinutes * TimeSpan.TicksPerMinute;
        return new DateTimeOffset(unchecked(utcTicks + offsetTicks), new TimeSpan(offsetTicks));
    }

    public static implicit operator UnityDateTimeOffset(DateTimeOffset value)
    {
        return new UnityDateTimeOffset(value);
    }

    public static implicit operator DateTimeOffset(UnityDateTimeOffset value)
    {
        return value.ToDateTimeOffset();
    }

    public override string ToString()
    {
        return ToDateTimeOffset().ToString();
    }
}
