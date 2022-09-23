using MemoryPack.Formatters;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
namespace MemoryPack;

public static partial class MemoryPackFormatterProvider
{
    internal static void RegisterWellKnownTypesFormatters()
    {
        Register(new UnmanagedFormatter<SByte>());
        Register(new UnmanagedArrayFormatter<SByte>());
        Register(new UnmanagedFormatter<Byte>());
        Register(new UnmanagedArrayFormatter<Byte>());
        Register(new UnmanagedFormatter<Int16>());
        Register(new UnmanagedArrayFormatter<Int16>());
        Register(new UnmanagedFormatter<UInt16>());
        Register(new UnmanagedArrayFormatter<UInt16>());
        Register(new UnmanagedFormatter<Int32>());
        Register(new UnmanagedArrayFormatter<Int32>());
        Register(new UnmanagedFormatter<UInt32>());
        Register(new UnmanagedArrayFormatter<UInt32>());
        Register(new UnmanagedFormatter<Int64>());
        Register(new UnmanagedArrayFormatter<Int64>());
        Register(new UnmanagedFormatter<UInt64>());
        Register(new UnmanagedArrayFormatter<UInt64>());
        Register(new UnmanagedFormatter<Char>());
        Register(new UnmanagedArrayFormatter<Char>());
        Register(new UnmanagedFormatter<Single>());
        Register(new UnmanagedArrayFormatter<Single>());
        Register(new UnmanagedFormatter<Double>());
        Register(new UnmanagedArrayFormatter<Double>());
        Register(new UnmanagedFormatter<Decimal>());
        Register(new UnmanagedArrayFormatter<Decimal>());
        Register(new UnmanagedFormatter<Boolean>());
        Register(new UnmanagedArrayFormatter<Boolean>());
        Register(new UnmanagedFormatter<IntPtr>());
        Register(new UnmanagedArrayFormatter<IntPtr>());
        Register(new UnmanagedFormatter<UIntPtr>());
        Register(new UnmanagedArrayFormatter<UIntPtr>());
        Register(new UnmanagedFormatter<DateTime>());
        Register(new UnmanagedArrayFormatter<DateTime>());
        Register(new UnmanagedFormatter<DateTimeOffset>());
        Register(new UnmanagedArrayFormatter<DateTimeOffset>());
        Register(new UnmanagedFormatter<TimeSpan>());
        Register(new UnmanagedArrayFormatter<TimeSpan>());

        Register(new StringFormatter());
        Register(new ArrayFormatter<String>());
        Register(new VersionFormatter());
        Register(new ArrayFormatter<Version>());
        Register(new UriFormatter());
        Register(new ArrayFormatter<Uri>());
    }
}
