using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Text;

namespace MemoryPack.Generator;

partial class MemoryPackGenerator
{
    void RegisterSerializerContexts(
        IncrementalGeneratorInitializationContext context,
        IncrementalValuesProvider<TypeDeclarationSyntax> declarations)
    {
        var parseOptions = context.ParseOptionsProvider.Select(static (options, _) =>
        {
            var csharp = (CSharpParseOptions)options;
            return (csharp.LanguageVersion, csharp.PreprocessorSymbolNames.Contains("NET7_0_OR_GREATER"));
        });

        var source = declarations
            .Combine(context.CompilationProvider)
            .Combine(parseOptions)
            .WithTrackingName("MemoryPack.SerializerContext.2_Combined");

        context.RegisterSourceOutput(source, static (productionContext, value) =>
        {
            var declaration = value.Left.Item1;
            var compilation = value.Left.Item2;
            var (languageVersion, net7) = value.Right;
            var generatorContext = new GeneratorContext(productionContext, languageVersion, net7);
            GenerateSerializerContext(declaration, compilation, generatorContext);
        });
    }

    static bool IsContextOwned(
        TypeDeclarationSyntax declaration,
        Compilation compilation,
        ImmutableArray<TypeDeclarationSyntax> contextDeclarations,
        CancellationToken cancellationToken)
    {
        if (!((compilation.GetSemanticModel(declaration.SyntaxTree).GetDeclaredSymbol(declaration, cancellationToken)) is INamedTypeSymbol target))
        {
            return false;
        }

        var references = new ReferenceSymbols(compilation);
        foreach (var contextDeclaration in contextDeclarations)
        {
            var contextSymbol = compilation.GetSemanticModel(contextDeclaration.SyntaxTree).GetDeclaredSymbol(contextDeclaration, cancellationToken);
            if (contextSymbol is null)
            {
                continue;
            }

            foreach (var root in GetSerializerContextRoots(contextSymbol))
            {
                if (ContainsGraphType(root.Type, target, references, new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default)))
                {
                    return true;
                }
            }
        }

        return false;
    }

    static void GenerateSerializerContext(TypeDeclarationSyntax declaration, Compilation compilation, IGeneratorContext context)
    {
        var semanticModel = compilation.GetSemanticModel(declaration.SyntaxTree);
        if (semanticModel.GetDeclaredSymbol(declaration, context.CancellationToken) is not INamedTypeSymbol symbol)
        {
            return;
        }

        if (!IsPartial(declaration))
        {
            context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.SerializerContextMustBePartial, declaration.Identifier.GetLocation(), symbol.Name));
            return;
        }

        if (IsNested(declaration) && !IsNestedContainingTypesPartial(declaration))
        {
            context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.NestedContainingTypesMustBePartial, declaration.Identifier.GetLocation(), symbol.Name));
            return;
        }

        if (!context.IsNet7OrGreater)
        {
            context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.SerializerContextRequiresNet7, declaration.Identifier.GetLocation(), symbol.Name));
            return;
        }

        if (symbol.TypeParameters.Length != 0)
        {
            context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.SerializerContextDoesNotAllowGenerics, declaration.Identifier.GetLocation(), symbol.Name));
            return;
        }

        var contextBase = compilation.GetTypeByMetadataName("MemoryPack.MemoryPackSerializerContext");
        if (contextBase is null ||
            symbol.TypeKind != TypeKind.Class ||
            symbol.IsAbstract ||
            !symbol.GetAllBaseTypes().Any(x => SymbolEqualityComparer.Default.Equals(x, contextBase)))
        {
            context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.SerializerContextMustDeriveFromBase, declaration.Identifier.GetLocation(), symbol.Name));
            return;
        }

        var roots = GetSerializerContextRoots(symbol)
            .GroupBy(static x => x.Type, SymbolEqualityComparer.Default)
            .Select(static group => group.Any(static x => x.FormatterType is not null)
                ? group.First(static x => x.FormatterType is not null)
                : group.First())
            .ToArray();
        if (roots.Length == 0)
        {
            return;
        }

        var references = new ReferenceSymbols(compilation);
        var registrations = new StringBuilder();
        var formatterDeclarations = new StringBuilder();
        var registered = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);
        var formatterOverrideTypes = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);
        var failed = false;
        foreach (var root in roots.Where(static x => x.FormatterType is not null))
        {
            if (root.FormatterType is INamedTypeSymbol formatterType &&
                references.IMemoryPackSerializerContextFormatterFactory is not null &&
                formatterType.AllInterfaces.Any(x => x.EqualsUnconstructedGenericType(references.IMemoryPackSerializerContextFormatterFactory)))
            {
                registrations.AppendLine($"        builder.RegisterFactory<{root.Type.FullyQualifiedToString()}, {root.FormatterType!.FullyQualifiedToString()}>();");
            }
            else
            {
                registrations.AppendLine($"        builder.RegisterFormatter<{root.Type.FullyQualifiedToString()}>(new {root.FormatterType!.FullyQualifiedToString()}());");
            }
            registered.Add(root.Type);
            formatterOverrideTypes.Add(root.Type);
        }

        foreach (var root in roots.Where(static x => x.FormatterType is null))
        {
            var supported = new HashSet<ITypeSymbol>(formatterOverrideTypes, SymbolEqualityComparer.Default);
            if (!IsGraphSupported(root.Type, references, supported) ||
                !TryEmitGraphRegistration(root.Type, references, registrations, "        ", registered, formatterDeclarations))
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    DiagnosticDescriptors.SerializerContextTypeNotSupported,
                    declaration.Identifier.GetLocation(),
                    symbol.Name,
                    root.Type.ToDisplayString()));
                failed = true;
            }
        }

        if (failed)
        {
            return;
        }

        var contextType = symbol.FullyQualifiedToString();
        var interfaces = string.Join(", ", roots.Select(x => $"global::MemoryPack.IMemoryPackSerializerContext<{contextType}, {x.Type.FullyQualifiedToString()}>").Distinct());
        var rootFormatterTypes = roots.Select(x => GetRootFormatterType(x.Type, x.FormatterType, references)).ToArray();
        var fields = roots.Select((x, index) => $"    readonly {rootFormatterTypes[index]} __rootFormatter{index};").NewLine();
        var assignments = roots.Select((x, index) => $"        __rootFormatter{index} = ({rootFormatterTypes[index]})GetGeneratedFormatter<{x.Type.FullyQualifiedToString()}>();").NewLine();
        var accessors = roots.Select((x, index) => $$"""
    static global::MemoryPack.MemoryPackFormatter<{{x.Type.FullyQualifiedToString()}}> global::MemoryPack.IMemoryPackSerializerContext<{{contextType}}, {{x.Type.FullyQualifiedToString()}}>.GetFormatter({{contextType}} context)
        => context.__rootFormatter{{index}};

    static void global::MemoryPack.IMemoryPackSerializerContext<{{contextType}}, {{x.Type.FullyQualifiedToString()}}>.Serialize<TBufferWriter>({{contextType}} context, ref global::MemoryPack.MemoryPackWriter<TBufferWriter> writer, scoped ref {{x.Type.FullyQualifiedToString()}}{{(x.Type.IsReferenceType ? "?" : "")}} value)
    {
        writer.WriteValueWithFormatter(context.__rootFormatter{{index}}, value);
    }

    static void global::MemoryPack.IMemoryPackSerializerContext<{{contextType}}, {{x.Type.FullyQualifiedToString()}}>.Deserialize({{contextType}} context, ref global::MemoryPack.MemoryPackReader reader, scoped ref {{x.Type.FullyQualifiedToString()}}{{(x.Type.IsReferenceType ? "?" : "")}} value)
    {
        reader.ReadValueWithFormatter(context.__rootFormatter{{index}}, ref value);
    }
""").NewLine();

        var builder = new StringBuilder();
        builder.AppendLine("// <auto-generated/>");
        builder.AppendLine("#nullable enable");
        builder.AppendLine("#pragma warning disable CS8619");
        builder.AppendLine("#pragma warning disable CS8620");
        builder.AppendLine("#pragma warning disable CS8631");
        builder.AppendLine();

        var ns = symbol.ContainingNamespace;
        if (!ns.IsGlobalNamespace)
        {
            builder.AppendLine($"namespace {ns.ToDisplayString()};");
            builder.AppendLine();
        }

        var containingTypes = GetContainingTypeDeclarations(symbol);
        foreach (var containingType in containingTypes)
        {
            builder.AppendLine(containingType);
            builder.AppendLine("{");
        }

        var declarationKind = symbol.IsRecord ? "record class" : "class";
        builder.AppendLine($"partial {declarationKind} {symbol.Name} : {interfaces}");
        builder.AppendLine("{");
        builder.AppendLine(fields);
        builder.AppendLine();
        builder.AppendLine($"    public static {symbol.Name} Default {{ get; }} = new {symbol.Name}();");
        builder.AppendLine();
        builder.AppendLine($"    public {symbol.Name}() : this(null)");
        builder.AppendLine("    {");
        builder.AppendLine("    }");
        builder.AppendLine();
        builder.AppendLine($"    public {symbol.Name}(global::MemoryPack.MemoryPackSerializerOptions? options) : base(options)");
        builder.AppendLine("    {");
        builder.AppendLine("        var builder = new global::MemoryPack.MemoryPackSerializerContextBuilder();");
        builder.Append(registrations);
        builder.AppendLine("        Initialize(builder);");
        builder.AppendLine(assignments);
        builder.AppendLine("    }");
        builder.AppendLine();
        builder.AppendLine(accessors);
        builder.AppendLine(formatterDeclarations.ToString());
        builder.AppendLine("}");

        for (var i = 0; i < containingTypes.Count; i++)
        {
            builder.AppendLine("}");
        }

        var hintName = symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
            .Replace("global::", "")
            .Replace('<', '_')
            .Replace('>', '_') + ".MemoryPackSerializerContext.g.cs";
        context.AddSource(hintName, builder.ToString());
    }

    internal static bool TryEmitGraphRegistration(
        ITypeSymbol type,
        ReferenceSymbols references,
        StringBuilder builder,
        string indent,
        HashSet<ITypeSymbol> registered,
        StringBuilder? formatterDeclarations = null)
    {
        if (!registered.Add(type))
        {
            return true;
        }

        var fullType = type.FullyQualifiedToString();
        if (type is ITypeParameterSymbol)
        {
            // A factory emitted into an open generic MemoryPackable type is
            // closed by the generated context. Its concrete type arguments
            // are registered by the context before this factory is invoked.
            return true;
        }

        if (type.SpecialType == SpecialType.System_String)
        {
            builder.AppendLine($"{indent}builder.RegisterString();");
            return true;
        }

        if (SymbolEqualityComparer.Default.Equals(type, references.KnownTypes.System_Type))
        {
            builder.AppendLine($"{indent}builder.RegisterType();");
            return true;
        }

        var knownLeafFormatter = GetKnownLeafFormatter(type);
        if (knownLeafFormatter is not null)
        {
            builder.AppendLine($"{indent}builder.RegisterFormatter<{fullType}>(new {knownLeafFormatter}());");
            return true;
        }

        if (type is INamedTypeSymbol nullable && nullable.EqualsUnconstructedGenericType(references.KnownTypes.System_Nullable_T))
        {
            var valueType = nullable.TypeArguments[0];
            if (valueType.IsUnmanagedType)
            {
                builder.AppendLine($"{indent}builder.RegisterDangerousUnmanaged<{fullType}>();");
                return true;
            }

            if (!TryEmitGraphRegistration(valueType, references, builder, indent, registered, formatterDeclarations))
            {
                return false;
            }
            builder.AppendLine($"{indent}builder.RegisterNullable<{valueType.FullyQualifiedToString()}>();");
            return true;
        }

        if (type.IsUnmanagedType || type.TypeKind == TypeKind.Enum)
        {
            builder.AppendLine($"{indent}builder.RegisterUnmanaged<{fullType}>();");
            return true;
        }

        if (type is IArrayTypeSymbol array)
        {
            if (!TryEmitGraphRegistration(array.ElementType, references, builder, indent, registered, formatterDeclarations))
            {
                return false;
            }
            var method = array.Rank switch
            {
                1 when array.IsSZArray => "RegisterArray",
                2 => "RegisterTwoDimensionalArray",
                3 => "RegisterThreeDimensionalArray",
                4 => "RegisterFourDimensionalArray",
                _ => null,
            };
            if (method is null)
            {
                return false;
            }
            builder.AppendLine($"{indent}builder.{method}<{array.ElementType.FullyQualifiedToString()}>();");
            return true;
        }

        if (type is INamedTypeSymbol named)
        {
            if (named.EqualsUnconstructedGenericType(references.KnownTypes.System_Collections_Generic_List_T))
            {
                var element = named.TypeArguments[0];
                if (!TryEmitGraphRegistration(element, references, builder, indent, registered, formatterDeclarations))
                {
                    return false;
                }
                builder.AppendLine($"{indent}builder.RegisterList<{element.FullyQualifiedToString()}>();");
                return true;
            }

            if (named.IsGenericType)
            {
                var genericName = named.ConstructUnboundGenericType().ToDisplayString();
                if ((genericName.StartsWith("System.Tuple<", StringComparison.Ordinal) ||
                     genericName.StartsWith("System.ValueTuple<", StringComparison.Ordinal)) &&
                    named.TypeArguments.Length <= 8)
                {
                    if (formatterDeclarations is null)
                    {
                        return false;
                    }
                    foreach (var argument in named.TypeArguments)
                    {
                        if (!TryEmitGraphRegistration(argument, references, builder, indent, registered, formatterDeclarations)) return false;
                    }

                    var formatterName = $"__MemoryPackContextTupleFormatter{registered.Count}";
                    var arguments = string.Join(", ", named.TypeArguments.Select(x => $"builder.GetFormatter<{x.FullyQualifiedToString()}>()"));
                    builder.AppendLine($"{indent}builder.RegisterFormatter<{fullType}>(new {formatterName}({arguments}));");
                    formatterDeclarations.AppendLine(EmitContextTupleFormatter(named, formatterName));
                    return true;
                }

                if (genericName == "System.Collections.Immutable.ImmutableArray<>")
                {
                    var element = named.TypeArguments[0];
                    if (!TryEmitGraphRegistration(element, references, builder, indent, registered, formatterDeclarations)) return false;
                    builder.AppendLine($"{indent}builder.RegisterImmutableArray<{element.FullyQualifiedToString()}>();");
                    return true;
                }

                if (genericName == "System.Collections.Generic.PriorityQueue<,>")
                {
                    var element = named.TypeArguments[0];
                    var priority = named.TypeArguments[1];
                    if (!TryEmitGraphRegistration(element, references, builder, indent, registered, formatterDeclarations) ||
                        !TryEmitGraphRegistration(priority, references, builder, indent, registered, formatterDeclarations)) return false;
                    builder.AppendLine($"{indent}builder.RegisterPriorityQueue<{element.FullyQualifiedToString()}, {priority.FullyQualifiedToString()}>();");
                    return true;
                }

                if (genericName is "System.Linq.IGrouping<,>" or "System.Linq.ILookup<,>")
                {
                    var key = named.TypeArguments[0];
                    var element = named.TypeArguments[1];
                    if (!TryEmitGraphRegistration(key, references, builder, indent, registered, formatterDeclarations) ||
                        !TryEmitGraphRegistration(element, references, builder, indent, registered, formatterDeclarations)) return false;
                    var method = genericName == "System.Linq.IGrouping<,>" ? "RegisterGrouping" : "RegisterLookup";
                    builder.AppendLine($"{indent}builder.{method}<{key.FullyQualifiedToString()}, {element.FullyQualifiedToString()}>();");
                    return true;
                }

                if (TryGetContextEnumerableRegistration(named, out var materialize, out var reverse))
                {
                    var element = named.TypeArguments[0];
                    if (!TryEmitGraphRegistration(element, references, builder, indent, registered, formatterDeclarations)) return false;
                    builder.AppendLine($"{indent}builder.RegisterEnumerable<{fullType}, {element.FullyQualifiedToString()}>({materialize}, reverse: {reverse.ToString().ToLowerInvariant()});");
                    return true;
                }

                if (TryGetContextMapRegistration(named, out materialize))
                {
                    var key = named.TypeArguments[0];
                    var value = named.TypeArguments[1];
                    if (!TryEmitGraphRegistration(key, references, builder, indent, registered, formatterDeclarations) ||
                        !TryEmitGraphRegistration(value, references, builder, indent, registered, formatterDeclarations)) return false;
                    builder.AppendLine($"{indent}builder.RegisterMap<{fullType}, {key.FullyQualifiedToString()}, {value.FullyQualifiedToString()}>({materialize});");
                    return true;
                }
            }

            if (named.IsGenericType)
            {
                var genericName = named.ConstructUnboundGenericType().ToDisplayString();
                var method = genericName switch
                {
                    "System.ArraySegment<>" => "RegisterArraySegment",
                    "System.Memory<>" => "RegisterMemory",
                    "System.ReadOnlyMemory<>" => "RegisterReadOnlyMemory",
                    "System.Buffers.ReadOnlySequence<>" => "RegisterReadOnlySequence",
                    "System.Lazy<>" => "RegisterLazy",
                    _ => null,
                };
                if (method is not null)
                {
                    var element = named.TypeArguments[0];
                    if (!TryEmitGraphRegistration(element, references, builder, indent, registered, formatterDeclarations)) return false;
                    builder.AppendLine($"{indent}builder.{method}<{element.FullyQualifiedToString()}>();");
                    return true;
                }

                if (genericName == "System.Collections.Generic.KeyValuePair<,>")
                {
                    var key = named.TypeArguments[0];
                    var value = named.TypeArguments[1];
                    if (!TryEmitGraphRegistration(key, references, builder, indent, registered, formatterDeclarations) ||
                        !TryEmitGraphRegistration(value, references, builder, indent, registered, formatterDeclarations)) return false;
                    builder.AppendLine($"{indent}builder.RegisterKeyValuePair<{key.FullyQualifiedToString()}, {value.FullyQualifiedToString()}>();");
                    return true;
                }
            }

            if (named.IsGenericType &&
                named.ConstructUnboundGenericType().ToDisplayString() == "System.Collections.Generic.Dictionary<,>")
            {
                var key = named.TypeArguments[0];
                var value = named.TypeArguments[1];
                if (!TryEmitGraphRegistration(key, references, builder, indent, registered, formatterDeclarations) ||
                    !TryEmitGraphRegistration(value, references, builder, indent, registered, formatterDeclarations))
                {
                    return false;
                }
                builder.AppendLine($"{indent}builder.RegisterDictionary<{key.FullyQualifiedToString()}, {value.FullyQualifiedToString()}>();");
                return true;
            }

            if (named.TryGetMemoryPackableType(references, out var generateType, out _) &&
                generateType is GenerateType.Object or GenerateType.VersionTolerant or GenerateType.CircularReference or GenerateType.Collection or GenerateType.Union)
            {
                if (!named.Locations.Any(static x => x.IsInSource) &&
                    (references.IMemoryPackSerializerContextFormatterRegister is null ||
                     !named.AllInterfaces.Any(x => x.EqualsUnconstructedGenericType(references.IMemoryPackSerializerContextFormatterRegister))))
                {
                    return false;
                }

                if (named.IsGenericType)
                {
                    foreach (var argument in named.TypeArguments)
                    {
                        if (!TryEmitGraphRegistration(argument, references, builder, indent, registered, formatterDeclarations))
                        {
                            return false;
                        }
                    }
                }

                builder.AppendLine($"{indent}builder.RegisterMemoryPackable<{fullType}>();");
                return true;
            }
        }

        return false;
    }

    static string? GetKnownLeafFormatter(ITypeSymbol type)
    {
        return type.FullyQualifiedToString() switch
        {
            "global::System.Version" => "global::MemoryPack.Formatters.VersionFormatter",
            "global::System.Uri" => "global::MemoryPack.Formatters.UriFormatter",
            "global::System.TimeZoneInfo" => "global::MemoryPack.Formatters.TimeZoneInfoFormatter",
            "global::System.Numerics.BigInteger" => "global::MemoryPack.Formatters.BigIntegerFormatter",
            "global::System.Collections.BitArray" => "global::MemoryPack.Formatters.BitArrayFormatter",
            "global::System.Text.StringBuilder" => "global::MemoryPack.Formatters.StringBuilderFormatter",
            "global::System.Globalization.CultureInfo" => "global::MemoryPack.Formatters.CultureInfoFormatter",
            _ => null,
        };
    }

    static bool TryGetContextEnumerableRegistration(INamedTypeSymbol type, out string materialize, out bool reverse)
    {
        materialize = "";
        reverse = false;
        if (!type.IsGenericType || type.TypeArguments.Length != 1)
        {
            return false;
        }

        var element = type.TypeArguments[0].FullyQualifiedToString();
        var name = type.ConstructUnboundGenericType().ToDisplayString();
        switch (name)
        {
            case "System.Collections.Generic.Stack<>":
                materialize = $"static items => new global::System.Collections.Generic.Stack<{element}>(items)";
                reverse = true;
                return true;
            case "System.Collections.Generic.Queue<>":
                materialize = $"static items => new global::System.Collections.Generic.Queue<{element}>(items)";
                return true;
            case "System.Collections.Generic.LinkedList<>":
                materialize = $"static items => new global::System.Collections.Generic.LinkedList<{element}>(items)";
                return true;
            case "System.Collections.Generic.HashSet<>":
                materialize = $"static items => new global::System.Collections.Generic.HashSet<{element}>(items)";
                return true;
            case "System.Collections.Generic.SortedSet<>":
                materialize = $"static items => new global::System.Collections.Generic.SortedSet<{element}>(items)";
                return true;
            case "System.Collections.ObjectModel.Collection<>":
                materialize = $"static items => new global::System.Collections.ObjectModel.Collection<{element}>(global::System.Linq.Enumerable.ToList(items))";
                return true;
            case "System.Collections.ObjectModel.ObservableCollection<>":
                materialize = $"static items => new global::System.Collections.ObjectModel.ObservableCollection<{element}>(items)";
                return true;
            case "System.Collections.Concurrent.ConcurrentQueue<>":
                materialize = $"static items => new global::System.Collections.Concurrent.ConcurrentQueue<{element}>(items)";
                return true;
            case "System.Collections.Concurrent.ConcurrentStack<>":
                materialize = $"static items => new global::System.Collections.Concurrent.ConcurrentStack<{element}>(items)";
                reverse = true;
                return true;
            case "System.Collections.Concurrent.ConcurrentBag<>":
                materialize = $"static items => new global::System.Collections.Concurrent.ConcurrentBag<{element}>(items)";
                return true;
            case "System.Collections.ObjectModel.ReadOnlyCollection<>":
                materialize = $"static items => new global::System.Collections.ObjectModel.ReadOnlyCollection<{element}>(global::System.Linq.Enumerable.ToList(items))";
                return true;
            case "System.Collections.ObjectModel.ReadOnlyObservableCollection<>":
                materialize = $"static items => new global::System.Collections.ObjectModel.ReadOnlyObservableCollection<{element}>(new global::System.Collections.ObjectModel.ObservableCollection<{element}>(items))";
                return true;
            case "System.Collections.Concurrent.BlockingCollection<>":
                materialize = $"static items => new global::System.Collections.Concurrent.BlockingCollection<{element}>(new global::System.Collections.Concurrent.ConcurrentQueue<{element}>(items))";
                return true;
            case "System.Collections.Generic.IEnumerable<>":
                materialize = "static items => global::System.Linq.Enumerable.ToArray(items)";
                return true;
            case "System.Collections.Generic.ICollection<>":
            case "System.Collections.Generic.IReadOnlyCollection<>":
            case "System.Collections.Generic.IList<>":
            case "System.Collections.Generic.IReadOnlyList<>":
                materialize = "static items => global::System.Linq.Enumerable.ToList(items)";
                return true;
            case "System.Collections.Generic.ISet<>":
            case "System.Collections.Generic.IReadOnlySet<>":
                materialize = "static items => global::System.Linq.Enumerable.ToHashSet(items)";
                return true;
            case "System.Collections.Immutable.ImmutableList<>":
            case "System.Collections.Immutable.IImmutableList<>":
                materialize = "static items => global::System.Collections.Immutable.ImmutableList.CreateRange(items)";
                return true;
            case "System.Collections.Immutable.ImmutableQueue<>":
            case "System.Collections.Immutable.IImmutableQueue<>":
                materialize = "static items => global::System.Collections.Immutable.ImmutableQueue.CreateRange(items)";
                return true;
            case "System.Collections.Immutable.ImmutableStack<>":
            case "System.Collections.Immutable.IImmutableStack<>":
                materialize = "static items => global::System.Collections.Immutable.ImmutableStack.CreateRange(items)";
                reverse = true;
                return true;
            case "System.Collections.Immutable.ImmutableSortedSet<>":
                materialize = "static items => global::System.Collections.Immutable.ImmutableSortedSet.CreateRange(items)";
                return true;
            case "System.Collections.Immutable.ImmutableHashSet<>":
            case "System.Collections.Immutable.IImmutableSet<>":
                materialize = "static items => global::System.Collections.Immutable.ImmutableHashSet.CreateRange(items)";
                return true;
            case "System.Collections.Frozen.FrozenSet<>":
                materialize = "static items => global::System.Collections.Frozen.FrozenSet.ToFrozenSet(items, null)";
                return true;
            default:
                return false;
        }
    }

    static bool TryGetContextMapRegistration(INamedTypeSymbol type, out string materialize)
    {
        materialize = "";
        if (!type.IsGenericType || type.TypeArguments.Length != 2)
        {
            return false;
        }

        var key = type.TypeArguments[0].FullyQualifiedToString();
        var value = type.TypeArguments[1].FullyQualifiedToString();
        var name = type.ConstructUnboundGenericType().ToDisplayString();
        var dictionary = $"global::System.Linq.Enumerable.ToDictionary(items, static item => item.Key, static item => item.Value)";
        switch (name)
        {
            case "System.Collections.Generic.SortedDictionary<,>":
                materialize = $"static items => new global::System.Collections.Generic.SortedDictionary<{key}, {value}>({dictionary})";
                return true;
            case "System.Collections.Generic.SortedList<,>":
                materialize = $"static items => new global::System.Collections.Generic.SortedList<{key}, {value}>({dictionary})";
                return true;
            case "System.Collections.Concurrent.ConcurrentDictionary<,>":
                materialize = $"static items => new global::System.Collections.Concurrent.ConcurrentDictionary<{key}, {value}>(items)";
                return true;
            case "System.Collections.ObjectModel.ReadOnlyDictionary<,>":
                materialize = $"static items => new global::System.Collections.ObjectModel.ReadOnlyDictionary<{key}, {value}>({dictionary})";
                return true;
            case "System.Collections.Generic.IDictionary<,>":
            case "System.Collections.Generic.IReadOnlyDictionary<,>":
                materialize = $"static items => {dictionary}";
                return true;
            case "System.Collections.Immutable.ImmutableDictionary<,>":
            case "System.Collections.Immutable.IImmutableDictionary<,>":
                materialize = "static items => global::System.Collections.Immutable.ImmutableDictionary.CreateRange(items)";
                return true;
            case "System.Collections.Immutable.ImmutableSortedDictionary<,>":
                materialize = "static items => global::System.Collections.Immutable.ImmutableSortedDictionary.CreateRange(items)";
                return true;
            case "System.Collections.Frozen.FrozenDictionary<,>":
                materialize = "static items => global::System.Collections.Frozen.FrozenDictionary.ToFrozenDictionary(items, null)";
                return true;
            default:
                return false;
        }
    }

    static string EmitContextTupleFormatter(INamedTypeSymbol type, string formatterName)
    {
        var fullType = type.FullyQualifiedToString();
        var isValueTuple = type.ConstructUnboundGenericType().ToDisplayString().StartsWith("System.ValueTuple<", StringComparison.Ordinal);
        var arguments = type.TypeArguments.ToArray();
        var constructorType = isValueTuple
            ? $"global::System.ValueTuple<{string.Join(", ", arguments.Select(x => x.FullyQualifiedToString()))}>"
            : fullType;
        var formatterFields = arguments.Select((x, i) => $"        readonly global::MemoryPack.MemoryPackFormatter<{x.FullyQualifiedToString()}> __formatter{i};").NewLine();
        var parameters = string.Join(", ", arguments.Select((x, i) => $"global::MemoryPack.MemoryPackFormatter<{x.FullyQualifiedToString()}> formatter{i}"));
        var assignments = arguments.Select((_, i) => $"            __formatter{i} = formatter{i};").NewLine();
        var itemNames = arguments.Select((_, i) => i == 7 ? "Rest" : $"Item{i + 1}").ToArray();
        var serializeItems = arguments.Select((_, i) => $$"""
            var __item{{i}} = value.{{itemNames[i]}};
            __formatter{{i}}.Serialize(ref writer, ref __item{{i}});
""").NewLine();
        var locals = arguments.Select((x, i) =>
            $"            {x.FullyQualifiedToString()}{(x.IsReferenceType ? "?" : "")} __item{i} = default;").NewLine();
        var deserializeItems = arguments.Select((_, i) => $"            __formatter{i}.Deserialize(ref reader, ref __item{i});").NewLine();
        var constructorArguments = string.Join(", ", arguments.Select((_, i) => $"__item{i}"));
        var serializeHeader = isValueTuple
            ? ""
            : $$"""
            if (value is null)
            {
                writer.WriteNullObjectHeader();
                return;
            }
            writer.WriteObjectHeader({{arguments.Length}});
""";
        var deserializeHeader = isValueTuple
            ? ""
            : $$"""
            if (!reader.TryReadObjectHeader(out var count))
            {
                value = null;
                return;
            }
            if (count != {{arguments.Length}})
            {
                global::MemoryPack.MemoryPackSerializationException.ThrowInvalidPropertyCount({{arguments.Length}}, count);
            }
""";
        var nullable = isValueTuple ? "" : "?";

        return $$"""
    sealed class {{formatterName}} : global::MemoryPack.MemoryPackFormatter<{{fullType}}>
    {
{{formatterFields}}
        public {{formatterName}}({{parameters}})
        {
{{assignments}}
        }

        public override void Serialize<TBufferWriter>(ref global::MemoryPack.MemoryPackWriter<TBufferWriter> writer, scoped ref {{fullType}}{{nullable}} value)
        {
{{serializeHeader}}
{{serializeItems}}
        }

        public override void Deserialize(ref global::MemoryPack.MemoryPackReader reader, scoped ref {{fullType}}{{nullable}} value)
        {
{{deserializeHeader}}
{{locals}}
{{deserializeItems}}
            value = new {{constructorType}}({{constructorArguments}});
        }
    }
""";
    }

    internal static string GetRootFormatterType(ITypeSymbol type, ITypeSymbol? formatterType, ReferenceSymbols references)
    {
        var fullType = type.FullyQualifiedToString();
        if (formatterType is INamedTypeSymbol namedFormatter &&
            namedFormatter.GetAllBaseTypes().Any(x => x.OriginalDefinition.ToDisplayString() == "MemoryPack.MemoryPackFormatter<T>"))
        {
            return formatterType.FullyQualifiedToString();
        }

        if (type.SpecialType == SpecialType.System_String) return "global::MemoryPack.Formatters.StringFormatter";
        if (SymbolEqualityComparer.Default.Equals(type, references.KnownTypes.System_Type)) return "global::MemoryPack.Formatters.ContextTypeFormatter";
        var leaf = GetKnownLeafFormatter(type);
        if (leaf is not null) return leaf;

        if (type is INamedTypeSymbol nullable && nullable.EqualsUnconstructedGenericType(references.KnownTypes.System_Nullable_T))
        {
            return nullable.TypeArguments[0].IsUnmanagedType
                ? $"global::MemoryPack.Formatters.DangerousUnmanagedFormatter<{fullType}>"
                : $"global::MemoryPack.Formatters.ContextNullableFormatter<{nullable.TypeArguments[0].FullyQualifiedToString()}>";
        }

        if (type.IsUnmanagedType || type.TypeKind == TypeKind.Enum)
        {
            return $"global::MemoryPack.Formatters.UnmanagedFormatter<{fullType}>";
        }

        if (type is IArrayTypeSymbol array)
        {
            var element = array.ElementType.FullyQualifiedToString();
            return array.Rank switch
            {
                1 when array.IsSZArray => $"global::MemoryPack.Formatters.ContextArrayFormatter<{element}>",
                2 => $"global::MemoryPack.Formatters.ContextTwoDimensionalArrayFormatter<{element}>",
                3 => $"global::MemoryPack.Formatters.ContextThreeDimensionalArrayFormatter<{element}>",
                4 => $"global::MemoryPack.Formatters.ContextFourDimensionalArrayFormatter<{element}>",
                _ => $"global::MemoryPack.MemoryPackFormatter<{fullType}>",
            };
        }

        if (type is INamedTypeSymbol named)
        {
            if (named.EqualsUnconstructedGenericType(references.KnownTypes.System_Collections_Generic_List_T))
            {
                return $"global::MemoryPack.Formatters.ContextListFormatter<{named.TypeArguments[0].FullyQualifiedToString()}>";
            }

            if (named.IsGenericType)
            {
                var genericName = named.ConstructUnboundGenericType().ToDisplayString();
                var arguments = named.TypeArguments.Select(x => x.FullyQualifiedToString()).ToArray();
                switch (genericName)
                {
                    case "System.Collections.Generic.Dictionary<,>":
                        return $"global::MemoryPack.Formatters.ContextDictionaryFormatter<{arguments[0]}, {arguments[1]}>";
                    case "System.ArraySegment<>":
                        return $"global::MemoryPack.Formatters.ContextArraySegmentFormatter<{arguments[0]}>";
                    case "System.Memory<>":
                        return $"global::MemoryPack.Formatters.ContextMemoryFormatter<{arguments[0]}>";
                    case "System.ReadOnlyMemory<>":
                        return $"global::MemoryPack.Formatters.ContextReadOnlyMemoryFormatter<{arguments[0]}>";
                    case "System.Buffers.ReadOnlySequence<>":
                        return $"global::MemoryPack.Formatters.ContextReadOnlySequenceFormatter<{arguments[0]}>";
                    case "System.Collections.Generic.KeyValuePair<,>":
                        return $"global::MemoryPack.Formatters.ContextKeyValuePairFormatter<{arguments[0]}, {arguments[1]}>";
                    case "System.Lazy<>":
                        return $"global::MemoryPack.Formatters.ContextLazyFormatter<{arguments[0]}>";
                    case "System.Collections.Immutable.ImmutableArray<>":
                        return $"global::MemoryPack.Formatters.ContextImmutableArrayFormatter<{arguments[0]}>";
                    case "System.Collections.Generic.PriorityQueue<,>":
                        return $"global::MemoryPack.Formatters.ContextPriorityQueueFormatter<{arguments[0]}, {arguments[1]}>";
                    case "System.Linq.IGrouping<,>":
                        return $"global::MemoryPack.Formatters.ContextGroupingFormatter<{arguments[0]}, {arguments[1]}>";
                    case "System.Linq.ILookup<,>":
                        return $"global::MemoryPack.Formatters.ContextLookupFormatter<{arguments[0]}, {arguments[1]}>";
                }

                if (TryGetContextEnumerableRegistration(named, out _, out _))
                {
                    return $"global::MemoryPack.Formatters.ContextEnumerableFormatter<{fullType}, {arguments[0]}>";
                }

                if (TryGetContextMapRegistration(named, out _))
                {
                    return $"global::MemoryPack.Formatters.ContextMapFormatter<{fullType}, {arguments[0]}, {arguments[1]}>";
                }
            }

            if (named.TryGetMemoryPackableType(references, out var generateType, out _))
            {
                if (generateType == GenerateType.Collection)
                {
                    var (kind, collectionSymbol) = TypeMeta.ParseCollectionKind(named, references);
                    var arguments = collectionSymbol!.TypeArguments.Select(x => x.FullyQualifiedToString()).ToArray();
                    return kind switch
                    {
                        CollectionKind.Collection => $"global::MemoryPack.Formatters.ContextGenericCollectionFormatter<{fullType}, {arguments[0]}>",
                        CollectionKind.Set => $"global::MemoryPack.Formatters.ContextGenericSetFormatter<{fullType}, {arguments[0]}>",
                        CollectionKind.Dictionary => $"global::MemoryPack.Formatters.ContextGenericDictionaryFormatter<{fullType}, {arguments[0]}, {arguments[1]}>",
                        _ => $"global::MemoryPack.MemoryPackFormatter<{fullType}>",
                    };
                }

                if (generateType is GenerateType.Object or GenerateType.VersionTolerant or GenerateType.CircularReference or GenerateType.Union)
                {
                    return $"{fullType}.__MemoryPackSerializerContextFormatter";
                }
            }
        }

        return $"global::MemoryPack.MemoryPackFormatter<{fullType}>";
    }

    static bool IsGraphSupported(ITypeSymbol type, ReferenceSymbols references, HashSet<ITypeSymbol> visited)
    {
        if (!visited.Add(type))
        {
            return true;
        }

        if (type is ITypeParameterSymbol ||
            type.SpecialType == SpecialType.System_String ||
            SymbolEqualityComparer.Default.Equals(type, references.KnownTypes.System_Type) ||
            GetKnownLeafFormatter(type) is not null ||
            type.IsUnmanagedType ||
            type.TypeKind == TypeKind.Enum)
        {
            return true;
        }

        if (type is IArrayTypeSymbol array)
        {
            return ((array.IsSZArray && array.Rank == 1) || array.Rank is 2 or 3 or 4) &&
                   IsGraphSupported(array.ElementType, references, visited);
        }

        if (type is not INamedTypeSymbol named)
        {
            return false;
        }

        if (named.EqualsUnconstructedGenericType(references.KnownTypes.System_Nullable_T))
        {
            return IsGraphSupported(named.TypeArguments[0], references, visited);
        }

        if (named.EqualsUnconstructedGenericType(references.KnownTypes.System_Collections_Generic_List_T))
        {
            return IsGraphSupported(named.TypeArguments[0], references, visited);
        }

        if (named.IsGenericType)
        {
            var genericName = named.ConstructUnboundGenericType().ToDisplayString();
            if (genericName is "System.ArraySegment<>" or "System.Memory<>" or "System.ReadOnlyMemory<>" or
                "System.Buffers.ReadOnlySequence<>" or "System.Lazy<>" or "System.Collections.Generic.KeyValuePair<,>" or
                "System.Collections.Immutable.ImmutableArray<>" or "System.Collections.Generic.PriorityQueue<,>" or
                "System.Linq.IGrouping<,>" or "System.Linq.ILookup<,>" ||
                genericName.StartsWith("System.Tuple<", StringComparison.Ordinal) ||
                genericName.StartsWith("System.ValueTuple<", StringComparison.Ordinal) ||
                TryGetContextEnumerableRegistration(named, out _, out _) ||
                TryGetContextMapRegistration(named, out _))
            {
                return named.TypeArguments.All(x => IsGraphSupported(x, references, visited));
            }
        }

        if (named.IsGenericType && named.ConstructUnboundGenericType().ToDisplayString() == "System.Collections.Generic.Dictionary<,>")
        {
            return IsGraphSupported(named.TypeArguments[0], references, visited) &&
                   IsGraphSupported(named.TypeArguments[1], references, visited);
        }

        if (!named.TryGetMemoryPackableType(references, out var generateType, out _) ||
            generateType is not (GenerateType.Object or GenerateType.VersionTolerant or GenerateType.CircularReference or GenerateType.Collection or GenerateType.Union))
        {
            return false;
        }

        if (!named.Locations.Any(static x => x.IsInSource))
        {
            return references.IMemoryPackSerializerContextFormatterRegister is not null &&
                   named.AllInterfaces.Any(x => x.EqualsUnconstructedGenericType(references.IMemoryPackSerializerContextFormatterRegister));
        }

        var meta = new TypeMeta(named, references);
        if (generateType == GenerateType.Collection)
        {
            var (_, collectionSymbol) = TypeMeta.ParseCollectionKind(named, references);
            return collectionSymbol is not null && collectionSymbol.TypeArguments.All(x => IsGraphSupported(x, references, visited));
        }

        if (generateType == GenerateType.Union)
        {
            return meta.UnionTags.All(x => IsGraphSupported(x.Type, references, visited));
        }

        return meta.Members.All(member => member.Symbol is null ||
            !member.RequiresSerializerContextFormatter ||
            IsGraphSupported(member.MemberType, references, visited));
    }

    internal static bool ContainsGraphType(ITypeSymbol type, ITypeSymbol target, ReferenceSymbols references, HashSet<ITypeSymbol> visited)
    {
        if (SymbolEqualityComparer.Default.Equals(type, target) ||
            (type is INamedTypeSymbol namedType && SymbolEqualityComparer.Default.Equals(namedType.OriginalDefinition, target)))
        {
            return true;
        }

        if (!visited.Add(type))
        {
            return false;
        }

        if (type is IArrayTypeSymbol array && ContainsGraphType(array.ElementType, target, references, visited))
        {
            return true;
        }

        if (type is not INamedTypeSymbol named)
        {
            return false;
        }

        foreach (var argument in named.TypeArguments)
        {
            if (ContainsGraphType(argument, target, references, visited))
            {
                return true;
            }
        }

        if (named.TryGetMemoryPackableType(references, out var generateType, out _) &&
            generateType is GenerateType.Object or GenerateType.VersionTolerant or GenerateType.CircularReference or GenerateType.Collection or GenerateType.Union)
        {
            var meta = new TypeMeta(named, references);
            if (generateType == GenerateType.Collection)
            {
                var (_, collectionSymbol) = TypeMeta.ParseCollectionKind(named, references);
                return collectionSymbol is not null && collectionSymbol.TypeArguments.Any(x => ContainsGraphType(x, target, references, visited));
            }

            foreach (var member in meta.Members)
            {
                if (member.Symbol is not null && ContainsGraphType(member.MemberType, target, references, visited))
                {
                    return true;
                }
            }

            foreach (var (_, unionType) in meta.UnionTags)
            {
                if (ContainsGraphType(unionType, target, references, visited))
                {
                    return true;
                }
            }
        }

        return false;
    }

    static IEnumerable<(ITypeSymbol Type, ITypeSymbol? FormatterType)> GetSerializerContextRoots(INamedTypeSymbol contextSymbol)
    {
        foreach (var attribute in contextSymbol.GetAttributes())
        {
            if (attribute.AttributeClass?.ToDisplayString() != MemoryPackSerializableAttributeFullName ||
                attribute.ConstructorArguments.Length == 0 ||
                attribute.ConstructorArguments[0].Value is not ITypeSymbol type)
            {
                continue;
            }

            ITypeSymbol? formatterType = null;
            foreach (var namedArgument in attribute.NamedArguments)
            {
                if (namedArgument.Key == "FormatterType")
                {
                    formatterType = namedArgument.Value.Value as ITypeSymbol;
                }
            }

            yield return (type, formatterType);
        }
    }

    static List<string> GetContainingTypeDeclarations(INamedTypeSymbol symbol)
    {
        var declarations = new List<string>();
        for (var containing = symbol.ContainingType; containing is not null; containing = containing.ContainingType)
        {
            var kind = containing.IsRecord
                ? "partial record class"
                : containing.TypeKind == TypeKind.Interface
                    ? "partial interface"
                    : containing.IsValueType
                        ? "partial struct"
                        : "partial class";
            var typeParameters = containing.TypeParameters.Length == 0
                ? ""
                : $"<{string.Join(", ", containing.TypeParameters.Select(x => x.Name))}>";
            declarations.Add($"{kind} {containing.Name}{typeParameters}");
        }
        declarations.Reverse();
        return declarations;
    }

}

public partial class TypeMeta
{
    internal string EmitSerializerContextCollectionFormatter()
    {
        var (collectionKind, collectionSymbol) = ParseCollectionKind(Symbol, reference);
        var collectionType = Symbol.FullyQualifiedToString();
        var typeArguments = collectionSymbol!.TypeArguments;
        var registrations = new StringBuilder();
        var formatterDeclarations = new StringBuilder();
        var registered = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);
        foreach (var typeArgument in typeArguments)
        {
            if (!MemoryPackGenerator.TryEmitGraphRegistration(typeArgument, reference, registrations, "        ", registered, formatterDeclarations))
            {
                // A generated context can provide a FormatterType override for
                // a dependency that the declaring type cannot construct.
            }
        }

        var formatter = collectionKind switch
        {
            CollectionKind.Collection => $"new global::MemoryPack.Formatters.ContextGenericCollectionFormatter<{collectionType}, {typeArguments[0].FullyQualifiedToString()}>(builder.GetFormatter<{typeArguments[0].FullyQualifiedToString()}>())",
            CollectionKind.Set => $"new global::MemoryPack.Formatters.ContextGenericSetFormatter<{collectionType}, {typeArguments[0].FullyQualifiedToString()}>(builder.GetFormatter<{typeArguments[0].FullyQualifiedToString()}>())",
            CollectionKind.Dictionary => $"new global::MemoryPack.Formatters.ContextGenericDictionaryFormatter<{collectionType}, {typeArguments[0].FullyQualifiedToString()}, {typeArguments[1].FullyQualifiedToString()}>(builder.GetFormatter<{typeArguments[0].FullyQualifiedToString()}>(), builder.GetFormatter<{typeArguments[1].FullyQualifiedToString()}>())",
            _ => "throw new global::System.InvalidOperationException()",
        };

        return $$"""
partial class {{TypeName}} : global::MemoryPack.IMemoryPackSerializerContextFormatterRegister<{{collectionType}}>
{
    static void global::MemoryPack.IMemoryPackSerializerContextFormatterRegister<{{collectionType}}>.RegisterFormatter(global::MemoryPack.MemoryPackSerializerContextBuilder builder)
    {
        if (!builder.BeginFormatter<{{collectionType}}>())
        {
            return;
        }
{{registrations}}        builder.CompleteFormatter<{{collectionType}}>({{formatter}});
    }
{{formatterDeclarations}}
}
""";
    }

    internal string EmitSerializerContextUnionFormatter()
    {
        var classKind = IsRecord ? "record" : Symbol.TypeKind == TypeKind.Interface ? "interface" : "class";
        var unionType = ToUnionTagTypeFullyQualifiedToString(Symbol);
        var registrations = new StringBuilder();
        var formatterDeclarations = new StringBuilder();
        var registered = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);
        foreach (var (_, implementation) in UnionTags)
        {
            if (!MemoryPackGenerator.TryEmitGraphRegistration(implementation, reference, registrations, "        ", registered, formatterDeclarations))
            {
                // The context graph validates or overrides this dependency.
            }
        }

        var fields = UnionTags.Select((x, index) =>
            $"        readonly global::MemoryPack.MemoryPackFormatter<{ToUnionTagTypeFullyQualifiedToString(x.Type)}> __formatter{index};").NewLine();
        var parameters = string.Join(", ", UnionTags.Select((x, index) =>
            $"global::MemoryPack.MemoryPackFormatter<{ToUnionTagTypeFullyQualifiedToString(x.Type)}> formatter{index}"));
        var arguments = string.Join(", ", UnionTags.Select(x =>
            $"builder.GetFormatter<{ToUnionTagTypeFullyQualifiedToString(x.Type)}>()"));
        var assignments = UnionTags.Select((_, index) => $"            __formatter{index} = formatter{index};").NewLine();
        var serializeCases = UnionTags.Select((x, index) => $$"""
                case {{x.Tag}}:
                {
                    var item = ({{ToUnionTagTypeFullyQualifiedToString(x.Type)}}?)value;
                    writer.WriteValueWithFormatter(__formatter{{index}}, item);
                    break;
                }
""").NewLine();
        var deserializeCases = UnionTags.Select((x, index) => $$"""
                case {{x.Tag}}:
                {
                    {{ToUnionTagTypeFullyQualifiedToString(x.Type)}}? item = value as {{ToUnionTagTypeFullyQualifiedToString(x.Type)}};
                    reader.ReadValueWithFormatter(__formatter{{index}}, ref item);
                    value = item;
                    break;
                }
""").NewLine();

        return $$"""
partial {{classKind}} {{TypeName}} : global::MemoryPack.IMemoryPackSerializerContextFormatterRegister<{{unionType}}>
{
    static void global::MemoryPack.IMemoryPackSerializerContextFormatterRegister<{{unionType}}>.RegisterFormatter(global::MemoryPack.MemoryPackSerializerContextBuilder builder)
    {
        if (!builder.BeginFormatter<{{unionType}}>())
        {
            return;
        }
{{registrations}}        builder.CompleteFormatter<{{unionType}}>(new __MemoryPackSerializerContextFormatter({{arguments}}));
    }

    [global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
    public sealed class __MemoryPackSerializerContextFormatter : global::MemoryPack.MemoryPackFormatter<{{unionType}}>
    {
{{EmitUnionTypeToTagField()}}
{{fields}}
        public __MemoryPackSerializerContextFormatter({{parameters}})
        {
{{assignments}}
        }

        public override void Serialize<TBufferWriter>(ref global::MemoryPack.MemoryPackWriter<TBufferWriter> writer, scoped ref {{unionType}}? value)
        {
{{OnSerializing.Select(x => "            " + x.Emit()).NewLine()}}
            if (value is null)
            {
                writer.WriteNullUnionHeader();
                goto END;
            }

            if (!__typeToTag.TryGetValue(value.GetType(), out var tag))
            {
                global::MemoryPack.MemoryPackSerializationException.ThrowNotFoundInUnionType(value.GetType(), typeof({{unionType}}));
            }

            writer.WriteUnionHeader(tag);
            switch (tag)
            {
{{serializeCases}}                default:
                    break;
            }
        END:
{{OnSerialized.Select(x => "            " + x.Emit()).NewLine()}}
            return;
        }

        public override void Deserialize(ref global::MemoryPack.MemoryPackReader reader, scoped ref {{unionType}}? value)
        {
{{OnDeserializing.Select(x => "            " + x.Emit()).NewLine()}}
            if (!reader.TryReadUnionHeader(out var tag))
            {
                value = default;
                goto END;
            }

            switch (tag)
            {
{{deserializeCases}}                default:
                    global::MemoryPack.MemoryPackSerializationException.ThrowInvalidTag(tag, typeof({{unionType}}));
                    break;
            }
        END:
{{OnDeserialized.Select(x => "            " + x.Emit()).NewLine()}}
            return;
        }
    }
{{formatterDeclarations}}
}
""";
    }

    internal string EmitSerializerContextFormatter(IGeneratorContext context)
    {
        var classKind = (IsRecord, IsValueType) switch
        {
            (true, true) => "record struct",
            (true, false) => "record",
            (false, true) => "struct",
            _ => "class",
        };
        var type = Symbol.FullyQualifiedToString();
        var nullable = IsValueType ? "" : "?";
        var dependencies = Members.Where(static x => x.Symbol is not null && x.RequiresSerializerContextFormatter).ToArray();
        var dependencyFormatterTypes = dependencies.Select(x =>
        {
            var isRecursiveEdge = MemoryPackGenerator.ContainsGraphType(
                x.MemberType,
                Symbol,
                reference,
                new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default));
            return isRecursiveEdge
                ? $"global::MemoryPack.MemoryPackFormatter<{x.MemberType.FullyQualifiedToString()}>"
                : MemoryPackGenerator.GetRootFormatterType(x.MemberType, null, reference);
        }).ToArray();

        var registrations = new StringBuilder();
        var formatterDeclarations = new StringBuilder();
        var registered = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);
        foreach (var member in dependencies)
        {
            if (!MemoryPackGenerator.TryEmitGraphRegistration(member.MemberType, reference, registrations, "        ", registered, formatterDeclarations))
            {
                // The context graph validates or overrides this dependency.
            }
        }

        var parameters = string.Join(", ", dependencies.Select((x, index) => $"{dependencyFormatterTypes[index]} {ToCamelCase(x.Name)}Formatter"));
        var arguments = string.Join(", ", dependencies.Select((x, index) => $"({dependencyFormatterTypes[index]})builder.GetFormatter<{x.MemberType.FullyQualifiedToString()}>()"));
        var fields = dependencies.Select((x, index) => $"        readonly {dependencyFormatterTypes[index]} __{x.Name}ContextFormatter;").NewLine();
        var fieldAssignments = dependencies.Select(x => $"            __{x.Name}ContextFormatter = {ToCamelCase(x.Name)}Formatter;").NewLine();

        var originalMembers = Members;
        if (GenerateType is GenerateType.VersionTolerant or GenerateType.CircularReference && Members.Length != 0)
        {
            var maxOrder = Members.Max(x => x.Order);
            var padded = new MemberMeta[maxOrder + 1];
            for (var i = 0; i <= maxOrder; i++)
            {
                padded[i] = Members.FirstOrDefault(x => x.Order == i) ?? MemberMeta.CreateEmpty(i);
            }
            Members = padded;
        }

        var serializeBody = IsUnmanagedType ? "        writer.WriteUnmanaged(value);" : EmitSerializeBody(context.IsForUnity, useContextFormatters: true);
        var deserializeBody = IsUnmanagedType ? "        reader.ReadUnmanaged(out value);" : EmitDeserializeBody(useContextFormatters: true);
        Members = originalMembers;

        return $$"""
partial {{classKind}} {{TypeName}} : global::MemoryPack.IMemoryPackSerializerContextFormatterRegister<{{type}}>
{
    static void global::MemoryPack.IMemoryPackSerializerContextFormatterRegister<{{type}}>.RegisterFormatter(global::MemoryPack.MemoryPackSerializerContextBuilder builder)
    {
        if (!builder.BeginFormatter<{{type}}>())
        {
            return;
        }
{{registrations}}        builder.CompleteFormatter<{{type}}>(new __MemoryPackSerializerContextFormatter({{arguments}}));
    }

    [global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
    public sealed class __MemoryPackSerializerContextFormatter : global::MemoryPack.MemoryPackFormatter<{{type}}>
    {
{{fields}}
        public __MemoryPackSerializerContextFormatter({{parameters}})
        {
{{fieldAssignments}}
        }

        public override void Serialize<TBufferWriter>(ref global::MemoryPack.MemoryPackWriter<TBufferWriter> writer, scoped ref {{type}}{{nullable}} value)
        {
{{OnSerializing.Select(x => "            " + x.Emit()).NewLine()}}
{{serializeBody}}
        END:
{{OnSerialized.Select(x => "            " + x.Emit()).NewLine()}}
            return;
        }

        public override void Deserialize(ref global::MemoryPack.MemoryPackReader reader, scoped ref {{type}}{{nullable}} value)
        {
{{OnDeserializing.Select(x => "            " + x.Emit()).NewLine()}}
{{deserializeBody}}
        END:
{{OnDeserialized.Select(x => "            " + x.Emit()).NewLine()}}
            return;
        }
    }
{{formatterDeclarations}}
}
""";
    }

    static string ToCamelCase(string value)
        => value.Length == 0 ? value : char.ToLowerInvariant(value[0]) + value.Substring(1);
}
