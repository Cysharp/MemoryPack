using MemoryPack.Formatters;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
namespace MemoryPack;

public static partial class MemoryPackFormatterProvider
{
    internal static void RegisterWellKnownTypesFormatters()
    {
        Register(new UnmanagedTypeFormatter<SByte>());
        Register(new UnmanagedTypeArrayFormatter<SByte>());
        Register(new UnmanagedTypeFormatter<Byte>());
        Register(new UnmanagedTypeArrayFormatter<Byte>());
        Register(new UnmanagedTypeFormatter<Int16>());
        Register(new UnmanagedTypeArrayFormatter<Int16>());
        Register(new UnmanagedTypeFormatter<UInt16>());
        Register(new UnmanagedTypeArrayFormatter<UInt16>());
        Register(new UnmanagedTypeFormatter<Int32>());
        Register(new UnmanagedTypeArrayFormatter<Int32>());
        Register(new UnmanagedTypeFormatter<UInt32>());
        Register(new UnmanagedTypeArrayFormatter<UInt32>());
        Register(new UnmanagedTypeFormatter<Int64>());
        Register(new UnmanagedTypeArrayFormatter<Int64>());
        Register(new UnmanagedTypeFormatter<UInt64>());
        Register(new UnmanagedTypeArrayFormatter<UInt64>());
        Register(new UnmanagedTypeFormatter<Char>());
        Register(new UnmanagedTypeArrayFormatter<Char>());
        Register(new UnmanagedTypeFormatter<Single>());
        Register(new UnmanagedTypeArrayFormatter<Single>());
        Register(new UnmanagedTypeFormatter<Double>());
        Register(new UnmanagedTypeArrayFormatter<Double>());
        Register(new UnmanagedTypeFormatter<Decimal>());
        Register(new UnmanagedTypeArrayFormatter<Decimal>());
        Register(new UnmanagedTypeFormatter<Boolean>());
        Register(new UnmanagedTypeArrayFormatter<Boolean>());
        Register(new UnmanagedTypeFormatter<IntPtr>());
        Register(new UnmanagedTypeArrayFormatter<IntPtr>());
        Register(new UnmanagedTypeFormatter<UIntPtr>());
        Register(new UnmanagedTypeArrayFormatter<UIntPtr>());
        Register(new UnmanagedTypeFormatter<DateTime>());
        Register(new UnmanagedTypeArrayFormatter<DateTime>());
        Register(new UnmanagedTypeFormatter<DateTimeOffset>());
        Register(new UnmanagedTypeArrayFormatter<DateTimeOffset>());
        Register(new UnmanagedTypeFormatter<TimeSpan>());
        Register(new UnmanagedTypeArrayFormatter<TimeSpan>());

        Register(new VersionFormatter());
        Register(new ArrayFormatter<Version>());
        Register(new UriFormatter());
        Register(new ArrayFormatter<Uri>());
    }
}
