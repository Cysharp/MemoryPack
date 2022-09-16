using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemoryPack;

public static partial class MemoryPackFormatterProvider
{

    static MemoryPackFormatterProvider()
    {
        RegisterWellKnownTypesFormatters();

        // TODO: others?
    }
}
