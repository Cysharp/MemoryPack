using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace MemoryPack.Generator;

// share context for SourceGenerator and IncrementalGenerator
public interface IGeneratorContext
{
    CancellationToken CancellationToken { get; }
    void ReportDiagnostic(Diagnostic diagnostic);
    void AddSource(string hintName, string source);
}
