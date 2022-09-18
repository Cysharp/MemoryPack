using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MemoryPack;

public static partial class MemoryPackFormatterProvider
{
    [ModuleInitializer]
    internal static void Initialize()
    {
        RegisterWellKnownTypesFormatters();

        // TODO: others?
    }
}
