using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace MemoryPack.Generator;

internal static class DiagnosticDescriptors
{
    const string Category = "GenerateMemoryPack";

    public static readonly DiagnosticDescriptor MustBePartial = new(
        id: "MEMPACK001",
        title: "MemoryPackable object must be partial",
        messageFormat: "The MemoryPackable object '{0}' must be partial",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);
}
