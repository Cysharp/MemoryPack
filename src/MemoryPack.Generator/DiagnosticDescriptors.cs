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

    public static readonly DiagnosticDescriptor NestedNotAllow = new(
        id: "MEMPACK002",
        title: "MemoryPackable object must not be nested type",
        messageFormat: "The MemoryPackable object '{0}' must be not nested type",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor AbstractMustUnion = new(
        id: "MEMPACK003",
        title: "abstract/interface type of MemoryPackable object must annotate with Union",
        messageFormat: "abstract/interface type of MemoryPackable object '{0}' must annotate with Union",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);
}
