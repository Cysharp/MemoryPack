using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MemoryPack.Tests;

public class MemoryLayoutTest
{
#if NET7_0_OR_GREATER

    [Fact]
    public void Default()
    {
        Unsafe.SizeOf<DateTimeParamDefault>().Should().Be(32);

        var (offset1, offset2, offset3) = GetOffsets();

        offset1.Should().Be(0);
        offset2.Should().Be(16);
        offset3.Should().Be(24);
    }

    [Fact]
    public void Sequential()
    {
        Unsafe.SizeOf<DateTimeParamSequential>().Should().Be(32);

        var (offset1, offset2, offset3) = GetOffsetsSequential();

        offset1.Should().Be(0);
        offset2.Should().Be(16);
        offset3.Should().Be(24);
    }

#else

    // https://github.com/dotnet/runtime/issues/44579
    [Fact]
    public void SequentialLayoutPromotedToAutoInBeforeNet7()
    {
        var defaultOffsets = GetOffsets();
        var sequentialOffsets = GetOffsetsSequential();
        var autoOffsets = GetOffsetsAuto();

        defaultOffsets.Should().Be(sequentialOffsets);
        defaultOffsets.Should().Be(autoOffsets);
    }

#endif

    [Fact]
    public void Auto()
    {
        Unsafe.SizeOf<DateTimeParamAuto>().Should().Be(32);

        var (offset1, offset2, offset3) = GetOffsetsAuto();

        offset1.Should().Be(16);
        offset2.Should().Be(0);
        offset3.Should().Be(8);
    }

    [Fact]
    public void Explicit()
    {
        Unsafe.SizeOf<DateTimeParamExplicit>().Should().Be(25);

        var (offset1, offset2, offset3) = GetOffsetsExplicit();

        offset1.Should().Be(9);
        offset2.Should().Be(1);
        offset3.Should().Be(0);
    }

    [Fact]
    public void DateTimeOffsetLayout()
    {
        // if this tast fail, runtime auto layout was changed.
        var dto = new DateTimeOffset(2012, 11, 3, 4, 12, 8, TimeSpan.FromHours(9));
        var data = MemoryMarshal.AsBytes(MemoryMarshal.CreateSpan(ref dto, 1)).ToArray();
        data.Should().Equal(new byte[] { 28, 2, 0, 0, 0, 0, 0, 0, 0, 180, 89, 22, 69, 135, 207, 8 });
    }

    // The generated TypeScript runtime's writeDateTimeOffset/readDateTimeOffset (see
    // TypeScriptRuntime.cs) rely on this layout: 8 bytes of offsetMinutes + padding, then the UTC
    // ticks with the top 2 DateTimeKind bits masked off (8 bytes). offsetMinutes is a short on
    // .NET 8/9 and an int on .NET 10, so the padding bytes after the low 2 bytes differ by runtime
    // (zero vs sign-extension) and are intentionally not asserted here.
    [Theory]
    [MemberData(nameof(DateTimeOffsetSamples))]
    public void DateTimeOffsetWireFormatMatchesTypeScriptRuntimeAlgorithm(DateTimeOffset value)
    {
        const ulong dateTimeMask = 0b00111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111UL;

        var actual = MemoryPackSerializer.Serialize(value);
        actual.Length.Should().Be(16);

        // offsetMinutes low 16 bits and masked UTC ticks match what the TS runtime reads/writes
        BitConverter.ToInt16(actual, 0).Should().Be((short)value.Offset.TotalMinutes);
        BitConverter.ToUInt64(actual, 8).Should().Be((ulong)value.UtcTicks & dateTimeMask);

        // readDateTimeOffset: skip offsetMinutes+padding (8 bytes), read ticks & mask as UTC instant
        var utcTicks = (long)(BitConverter.ToUInt64(actual, 8) & dateTimeMask);
        new DateTime(utcTicks, DateTimeKind.Utc).Should().Be(value.UtcDateTime);

        // writeDateTimeOffset: sign-extended int32 offsetMinutes + 4 zero bytes + masked UTC ticks
        // must deserialize back to the same value on every runtime layout.
        var written = new byte[16];
        BitConverter.GetBytes((int)value.Offset.TotalMinutes).CopyTo(written, 0);
        BitConverter.GetBytes((ulong)value.UtcTicks & dateTimeMask).CopyTo(written, 8);
        var roundTripped = MemoryPackSerializer.Deserialize<DateTimeOffset>(written);
        roundTripped.Should().Be(value);
        roundTripped.Offset.Should().Be(value.Offset);
    }

    public static TheoryData<DateTimeOffset> DateTimeOffsetSamples => new()
    {
        new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero),
        new DateTimeOffset(2024, 1, 1, 12, 30, 0, TimeSpan.FromHours(5.5)),
        new DateTimeOffset(2024, 6, 15, 9, 0, 0, TimeSpan.FromHours(-8)),
        DateTimeOffset.UnixEpoch,
        DateTimeOffset.MinValue,
        DateTimeOffset.MaxValue,
    };

    // can not use Marshal.OffsetOf because has AutoLayout field

    static unsafe (int, int, int) GetOffsets()
    {
        var value = default(DateTimeParamDefault);
        var basePointer = Unsafe.AsPointer(ref value);

        var dtPointer = Unsafe.AsPointer(ref value.DateTime);
        var timestampPointer = Unsafe.AsPointer(ref value.Timestamp);
        var isItInSecondsPointer = Unsafe.AsPointer(ref value.IsItInSeconds);

        var offset1 = (int)((nuint)dtPointer - (nuint)basePointer);
        var offset2 = (int)((nuint)timestampPointer - (nuint)basePointer);
        var offset3 = (int)((nuint)isItInSecondsPointer - (nuint)basePointer);
        return (offset1, offset2, offset3);
    }

    static unsafe (int, int, int) GetOffsetsSequential()
    {
        var value = default(DateTimeParamSequential);
        var basePointer = Unsafe.AsPointer(ref value);

        var dtPointer = Unsafe.AsPointer(ref value.DateTime);
        var timestampPointer = Unsafe.AsPointer(ref value.Timestamp);
        var isItInSecondsPointer = Unsafe.AsPointer(ref value.IsItInSeconds);

        var offset1 = (int)((nuint)dtPointer - (nuint)basePointer);
        var offset2 = (int)((nuint)timestampPointer - (nuint)basePointer);
        var offset3 = (int)((nuint)isItInSecondsPointer - (nuint)basePointer);
        return (offset1, offset2, offset3);
    }

    static unsafe (int, int, int) GetOffsetsAuto()
    {
        var value = default(DateTimeParamAuto);
        var basePointer = Unsafe.AsPointer(ref value);

        var dtPointer = Unsafe.AsPointer(ref value.DateTime);
        var timestampPointer = Unsafe.AsPointer(ref value.Timestamp);
        var isItInSecondsPointer = Unsafe.AsPointer(ref value.IsItInSeconds);

        var offset1 = (int)((nuint)dtPointer - (nuint)basePointer);
        var offset2 = (int)((nuint)timestampPointer - (nuint)basePointer);
        var offset3 = (int)((nuint)isItInSecondsPointer - (nuint)basePointer);
        return (offset1, offset2, offset3);
    }

    static unsafe (int, int, int) GetOffsetsExplicit()
    {
        var value = default(DateTimeParamExplicit);
        var basePointer = Unsafe.AsPointer(ref value);

        var dtPointer = Unsafe.AsPointer(ref value.DateTime);
        var timestampPointer = Unsafe.AsPointer(ref value.Timestamp);
        var isItInSecondsPointer = Unsafe.AsPointer(ref value.IsItInSeconds);

        var offset1 = (int)((nuint)dtPointer - (nuint)basePointer);
        var offset2 = (int)((nuint)timestampPointer - (nuint)basePointer);
        var offset3 = (int)((nuint)isItInSecondsPointer - (nuint)basePointer);
        return (offset1, offset2, offset3);
    }
}

public struct DateTimeParamDefault
{
    public DateTimeOffset DateTime; // short offset(2+padding) + dateTime/ulong(8) = 16
    public long Timestamp;  // 8
    public bool IsItInSeconds; // 1(+padding7) = 8
}

[StructLayout(LayoutKind.Sequential)]
public struct DateTimeParamSequential
{
    public DateTimeOffset DateTime; // short offset(2+padding) + dateTime/ulong(8) = 16
    public long Timestamp;  // 8
    public bool IsItInSeconds; // 1(+padding7) = 8
}

[StructLayout(LayoutKind.Auto)]
public struct DateTimeParamAuto
{
    public DateTimeOffset DateTime; // short offset(2+padding) + dateTime/ulong(8) = 16
    public long Timestamp;  // 8
    public bool IsItInSeconds; // 1(+padding7) = 8
}

[StructLayout(LayoutKind.Explicit, Size = 25)]
public struct DateTimeParamExplicit
{
    [FieldOffset(9)]
    public DateTimeOffset DateTime;
    [FieldOffset(1)]
    public long Timestamp;  // 8
    [FieldOffset(0)]
    public bool IsItInSeconds; // 1
}
