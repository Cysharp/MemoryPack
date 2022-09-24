using MemoryPack.Formatters;
using System.Collections.Immutable;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace MemoryPack;

public static partial class MemoryPackFormatterProvider
{
    [ModuleInitializer]
    internal static void Initialize()
    {
        RegisterWellKnownTypesFormatters();
    }
}
