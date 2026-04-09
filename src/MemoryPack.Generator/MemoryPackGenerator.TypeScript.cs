using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Diagnostics;
using System.Text;

namespace MemoryPack.Generator;

record struct FieldLayout(MemberMeta Member, int ByteOffset, int Size);
record struct UnmanagedStructLayout(FieldLayout[] Fields, int TotalSize);

public record TypeScriptGenerateOptions
{
    public string OutputDirectory { get; set; } = default!;
    public string ImportExtension { get; set; } = default!;
    public bool ConvertPropertyName { get; set; } = true;
    public bool EnableNullableTypes { get; set; } = false;
    public bool IsDesignTimeBuild { get; set; } = false;
}

partial class MemoryPackGenerator
{
    static TypeMeta? GenerateTypeScript(TypeDeclarationSyntax syntax, Compilation compilation, TypeScriptGenerateOptions typeScriptGenerateOptions, in SourceProductionContext context,
        ReferenceSymbols reference, IReadOnlyDictionary<ITypeSymbol, ITypeSymbol> unionMap)
    {
        var semanticModel = compilation.GetSemanticModel(syntax.SyntaxTree);

        var typeSymbol = semanticModel.GetDeclaredSymbol(syntax, context.CancellationToken);
        if (typeSymbol == null)
        {
            return null;
        }

        // require [MemoryPackable]
        if (!typeSymbol.ContainsAttribute(reference.MemoryPackableAttribute))
        {
            context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.GenerateTypeScriptMustBeMemoryPackable, syntax.Identifier.GetLocation(), typeSymbol.Name));
            return null;
        }

        var typeMeta = new TypeMeta(typeSymbol, reference);

        if (typeMeta.GenerateType is not (GenerateType.Object or GenerateType.Union))
        {
            context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.GenerateTypeScriptOnlyAllowsGenerateTypeObject, syntax.Identifier.GetLocation(), typeSymbol.Name));
            return null;
        }

        if (!Validate(typeMeta, syntax, context, reference))
        {
            return null;
        }

        var sb = new StringBuilder();

        sb.AppendLine($$"""
import { MemoryPackWriter } from "./MemoryPackWriter{{typeScriptGenerateOptions.ImportExtension}}";
import { MemoryPackReader } from "./MemoryPackReader{{typeScriptGenerateOptions.ImportExtension}}";
""");

        var collector = new TypeCollector();
        collector.Visit(typeMeta, true);

        // validate invalid enum
        foreach (var item in collector.GetEnums())
        {
            if (item.TypeKind == TypeKind.Enum && item is INamedTypeSymbol nts)
            {
                if (nts.EnumUnderlyingType!.SpecialType is SpecialType.System_Int64 or SpecialType.System_UInt64)
                {
                    context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.GenerateTypeScriptDoesNotAllowLongEnum, syntax.Identifier.GetLocation(), typeSymbol.Name, item.FullyQualifiedToString()));
                    return null;
                }
            }
        }

        // add import(enum, union, memorypackable)
        foreach (var item in collector.GetEnums())
        {
            sb.AppendLine($"import {{ {item.Name} }} from \"./{item.Name}{typeScriptGenerateOptions.ImportExtension}\";");
        }
        foreach (var item in collector.GetMemoryPackableTypes(reference)
            .Where(x => !SymbolEqualityComparer.Default.Equals(x, typeSymbol) && !x.IsMemoryPackableNoGenerate(reference)))
        {
            sb.AppendLine($"import {{ {item.Name} }} from \"./{item.Name}{typeScriptGenerateOptions.ImportExtension}\";");
        }
        sb.AppendLine();

        try
        {
            typeMeta.EmitTypescript(sb, unionMap, typeScriptGenerateOptions);
        }
        catch (NotSupportedTypeException ex)
        {
            context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.GenerateTypeScriptNotSupportedType, ex.MemberMeta!.GetLocation(syntax),
                typeMeta.Symbol.Name, ex.MemberMeta.Name, ex.MemberMeta.MemberType.FullyQualifiedToString()));
            return null;
        }

        // save to file
        try
        {
            if (!Directory.Exists(typeScriptGenerateOptions.OutputDirectory))
            {
                Directory.CreateDirectory(typeScriptGenerateOptions.OutputDirectory);
            }

            File.WriteAllText(Path.Combine(typeScriptGenerateOptions.OutputDirectory, $"{typeMeta.TypeName}.ts"), sb.ToString(), new UTF8Encoding(false));
        }
        catch (Exception ex)
        {
            Trace.WriteLine(ex.ToString());
        }

        return typeMeta;
    }

    static void GenerateEnums(IEnumerable<ISymbol?>? enums, string typeScriptOutputDirectoryPath)
    {
        if (enums == null) return;
        if (!Directory.Exists(typeScriptOutputDirectoryPath))
        {
            Directory.CreateDirectory(typeScriptOutputDirectoryPath);
        }

        foreach (var e in enums)
        {
            if (e is INamedTypeSymbol typeSymbol)
            {
                if (typeSymbol.TypeKind != TypeKind.Enum) continue;

                var sb = new StringBuilder();
                foreach (var member in typeSymbol.GetMembers())
                {
                    // (ok[0] as IFieldSymbol).ConstantValue
                    var fs = member as IFieldSymbol;
                    if (fs == null) continue;
                    var value = fs.HasConstantValue ? $" = {fs.ConstantValue}" : "";
                    sb.AppendLine($"    {fs.Name}{value},");
                }

                var code = $$"""
export const enum {{typeSymbol.Name}} {
{{sb.ToString()}}
}
""";

                File.WriteAllText(Path.Combine(typeScriptOutputDirectoryPath, $"{typeSymbol.Name}.ts"), code, new UTF8Encoding(false));
            }
        }
    }

    static bool Validate(TypeMeta type, TypeDeclarationSyntax syntax, in SourceProductionContext context, ReferenceSymbols reference)
    {
        var typeSymbol = type.Symbol;

        if (type.Symbol.IsGenericType)
        {
            context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.GenerateTypeScriptDoesNotAllowGenerics, syntax.Identifier.GetLocation(), typeSymbol.Name));
            return false;
        }

        foreach (var item in type.Members)
        {
            if (item.Kind == MemberKind.CustomFormatter)
            {
                context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.GenerateTypeScriptNotSupportedCustomFormatter, item.GetLocation(syntax), typeSymbol.Name));
                return false;
            }

            if (item.MemberType is INamedTypeSymbol memberNts
                && memberNts.EqualsUnconstructedGenericType(reference.KnownTypes.System_Nullable_T)
                && memberNts.TypeArguments[0] is INamedTypeSymbol innerNts
                && innerNts.TypeKind == TypeKind.Struct
                && innerNts.SpecialType == SpecialType.None
                && !SymbolEqualityComparer.Default.Equals(innerNts, reference.KnownTypes.System_Guid))
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    DiagnosticDescriptors.GenerateTypeScriptDoesNotAllowNullableStruct,
                    item.GetLocation(syntax),
                    typeSymbol.Name, item.Name, innerNts.Name));
                return false;
            }
        }

        return true;
    }
}

public partial class TypeMeta
{
    public void EmitTypescript(StringBuilder sb, IReadOnlyDictionary<ITypeSymbol, ITypeSymbol> unionMap, TypeScriptGenerateOptions options)
    {
        string importExt = options.ImportExtension;
        if (IsUnion)
        {
            EmitTypeScriptUnion(sb, importExt);
            return;
        }

        if (!unionMap.TryGetValue(Symbol, out var union))
        {
            union = null;
        }

        var tsMembers = Members.Select(x => new TypeScriptMember(x, reference, options)).ToArray();
        var impl = (union != null) ? $"implements {union.Name} " : "";

        if (IsUnmanagedType)
        {
            EmitTypeScriptUnmanagedStruct(sb, tsMembers, impl);
            return;
        }

        var serializeParam = IsValueType ? TypeName : $"{TypeName} | null";
        var arrayElemType = IsValueType ? TypeName : $"{TypeName} | null";
        var deserializeReturnType = IsValueType ? TypeName : $"{TypeName} | null";
        var nullHeaderCheck = IsValueType ? "" : $$"""
        if (value == null) {
            writer.writeNullObjectHeader();
            return;
        }

""";
        var nullObjectHandler = IsValueType
            ? $"throw new Error(\"Cannot deserialize null into struct {TypeName}.\");"
            : "return null;";

        var code = $$"""
export class {{TypeName}} {{impl}}{
{{EmitTypeScriptMembers(tsMembers)}}
    constructor() {
{{EmitTypeScriptMembersInit(tsMembers)}}
    }

    static serialize(value: {{serializeParam}}): Uint8Array {
        const writer = MemoryPackWriter.getSharedInstance();
        this.serializeCore(writer, value);
        return writer.toArray();
    }

    static serializeCore(writer: MemoryPackWriter, value: {{serializeParam}}): void {
{{nullHeaderCheck}}{{EmitTypeScriptSerializeBody(tsMembers)}}
    }

    static serializeArray(value: ({{arrayElemType}})[] | null): Uint8Array {
        const writer = MemoryPackWriter.getSharedInstance();
        this.serializeArrayCore(writer, value);
        return writer.toArray();
    }

    static serializeArrayCore(writer: MemoryPackWriter, value: ({{arrayElemType}})[] | null): void {
        writer.writeArray(value, (writer, x) => {{TypeName}}.serializeCore(writer, x));
    }

    static deserialize(buffer: ArrayBuffer): {{deserializeReturnType}} {
        return this.deserializeCore(new MemoryPackReader(buffer));
    }

    static deserializeCore(reader: MemoryPackReader): {{deserializeReturnType}} {
        const [ok, count] = reader.tryReadObjectHeader();
        if (!ok) {
            {{nullObjectHandler}}
        }

        const value = new {{TypeName}}();
        if (count == {{tsMembers.Length}}) {
{{EmitTypeScriptDeserializeBody(tsMembers, false)}}
        }
        else if (count > {{tsMembers.Length}}) {
            throw new Error("Current object's property count is larger than type schema, can't deserialize about versioning.");
        }
        else {
{{EmitTypeScriptDeserializeBody(tsMembers, true)}}
        }
        return value;
    }

    static deserializeArray(buffer: ArrayBuffer): ({{arrayElemType}})[] | null {
        return this.deserializeArrayCore(new MemoryPackReader(buffer));
    }

    static deserializeArrayCore(reader: MemoryPackReader): ({{arrayElemType}})[] | null {
        return reader.readArray(reader => {{TypeName}}.deserializeCore(reader));
    }
}
""";

        sb.AppendLine(code);
    }

    public void EmitTypeScriptUnmanagedStruct(StringBuilder sb, TypeScriptMember[] tsMembers, string impl)
    {
        var layout = ComputeUnmanagedLayout(Members);

        string EmitSerializeFields()
        {
            int offset = 0;
            var builder = new StringBuilder();

            for (int i = 0; i < layout.Fields.Length; i++)
            {
                var field = layout.Fields[i];
                int padding = field.ByteOffset - offset;
                if (padding > 0)
                {
                    builder.AppendLine($"        writer.writeZeros({padding});");
                }
                builder.AppendLine($"        {string.Format(tsMembers[i].WriteMethodTemplate, "value." + tsMembers[i].MemberName)};");
                offset = field.ByteOffset + field.Size;
            }

            int trailingPadding = layout.TotalSize - offset;
            if (trailingPadding > 0)
            {
                builder.AppendLine($"        writer.writeZeros({trailingPadding});");
            }

            return builder.ToString();
        }

        string EmitDeserializeFields()
        {
            int offset = 0;
            var builder = new StringBuilder();

            for (int i = 0; i < layout.Fields.Length; i++)
            {
                var field = layout.Fields[i];
                int padding = field.ByteOffset - offset;
                if (padding > 0)
                {
                    builder.AppendLine($"        reader.skipBytes({padding});");
                }
                builder.AppendLine($"        value.{tsMembers[i].MemberName} = {tsMembers[i].ReadMethodTemplate};");
                offset = field.ByteOffset + field.Size;
            }

            int trailingPadding = layout.TotalSize - offset;
            if (trailingPadding > 0)
            {
                builder.AppendLine($"        reader.skipBytes({trailingPadding});");
            }

            return builder.ToString();
        }

        var code = $$"""
export class {{TypeName}} {{impl}}{
{{EmitTypeScriptMembers(tsMembers)}}
    constructor() {
{{EmitTypeScriptMembersInit(tsMembers)}}
    }

    static serialize(value: {{TypeName}}): Uint8Array {
        const writer = MemoryPackWriter.getSharedInstance();
        this.serializeCore(writer, value);
        return writer.toArray();
    }

    static serializeCore(writer: MemoryPackWriter, value: {{TypeName}}): void {
{{EmitSerializeFields()}}
    }

    static serializeArray(value: {{TypeName}}[] | null): Uint8Array {
        const writer = MemoryPackWriter.getSharedInstance();
        this.serializeArrayCore(writer, value);
        return writer.toArray();
    }

    static serializeArrayCore(writer: MemoryPackWriter, value: {{TypeName}}[] | null): void {
        writer.writeArray(value, (writer, x) => {{TypeName}}.serializeCore(writer, x));
    }

    static deserialize(buffer: ArrayBuffer): {{TypeName}} {
        return this.deserializeCore(new MemoryPackReader(buffer));
    }

    static deserializeCore(reader: MemoryPackReader): {{TypeName}} {
        const value = new {{TypeName}}();
{{EmitDeserializeFields()}}
        return value;
    }

    static deserializeArray(buffer: ArrayBuffer): {{TypeName}}[] | null {
        return this.deserializeArrayCore(new MemoryPackReader(buffer));
    }

    static deserializeArrayCore(reader: MemoryPackReader): {{TypeName}}[] | null {
        return reader.readArray(reader => {{TypeName}}.deserializeCore(reader));
    }
}
""";

        sb.AppendLine(code);
    }

    static UnmanagedStructLayout ComputeUnmanagedLayout(MemberMeta[] members)
    {
        var fields = new FieldLayout[members.Length];
        int offset = 0;
        int maxAlign = 1;

        for (int i = 0; i < members.Length; i++)
        {
            var (size, align) = GetUnmanagedTypeSizeAndAlign(members[i].MemberType);
            int padding = (align - (offset % align)) % align;
            offset += padding;
            fields[i] = new FieldLayout(members[i], offset, size);
            offset += size;
            if (align > maxAlign) maxAlign = align;
        }

        int trailingPad = (maxAlign - (offset % maxAlign)) % maxAlign;
        return new UnmanagedStructLayout(fields, offset + trailingPad);
    }

    static (int Size, int Align) GetUnmanagedTypeSizeAndAlign(ITypeSymbol type)
    {
        if (type is not INamedTypeSymbol nts) return (1, 1);

        if (nts.TypeKind == TypeKind.Enum)
            return GetUnmanagedTypeSizeAndAlign(nts.EnumUnderlyingType!);

        switch (nts.SpecialType)
        {
            case SpecialType.System_Boolean:
            case SpecialType.System_SByte:
            case SpecialType.System_Byte:
                return (1, 1);
            case SpecialType.System_Int16:
            case SpecialType.System_UInt16:
            case SpecialType.System_Char:
                return (2, 2);
            case SpecialType.System_Int32:
            case SpecialType.System_UInt32:
            case SpecialType.System_Single:
                return (4, 4);
            case SpecialType.System_Int64:
            case SpecialType.System_UInt64:
            case SpecialType.System_Double:
            case SpecialType.System_DateTime:
                return (8, 8);
        }

        if (nts.IsUnmanagedType && nts.TypeKind == TypeKind.Struct)
        {
            var instanceFields = nts.GetMembers()
                .OfType<IFieldSymbol>()
                .Where(f => !f.IsStatic)
                .ToArray();

            int off = 0;
            int maxA = 1;
            foreach (var field in instanceFields)
            {
                var (fSize, fAlign) = GetUnmanagedTypeSizeAndAlign(field.Type);
                int pad = (fAlign - (off % fAlign)) % fAlign;
                off += pad + fSize;
                if (fAlign > maxA) maxA = fAlign;
            }
            int trailingPad = (maxA - (off % maxA)) % maxA;
            return (off + trailingPad, maxA);
        }

        return (1, 1);
    }

    public void EmitTypeScriptUnion(StringBuilder sb, string importExt)
    {
        string EmitUnionSerialize()
        {
            var sb = new StringBuilder();
            foreach (var item in UnionTags)
            {
                sb.AppendLine($$"""
        else if (value instanceof {{item.Type.Name}}) {
            writer.writeUnionHeader({{item.Tag}});
            {{item.Type.Name}}.serializeCore(writer, value);
            return;
        }
""");
            }
            return sb.ToString();
        }

        string EmitUnionDeserialize()
        {
            var sb = new StringBuilder();
            foreach (var item in UnionTags)
            {
                sb.AppendLine($$"""
            case {{item.Tag}}:
                return {{item.Type.Name}}.deserializeCore(reader);
""");
            }
            return sb.ToString();
        }

        foreach (var item in UnionTags)
        {
            sb.AppendLine($"import {{ {item.Type.Name} }} from \"./{item.Type.Name}{importExt}\"; ");
        }
        sb.AppendLine();

        var code = $$"""
export abstract class {{TypeName}} {
    static serialize(value: {{TypeName}} | null): Uint8Array {
        const writer = MemoryPackWriter.getSharedInstance();
        this.serializeCore(writer, value);
        return writer.toArray();
    }

    static serializeCore(writer: MemoryPackWriter, value: {{TypeName}} | null): void {
        if (value == null) {
            writer.writeNullObjectHeader();
            return;
        }
{{EmitUnionSerialize()}}
        else {
            throw new Error("Concrete type is not in MemoryPackUnion");
        }
    }

    static serializeArray(value: {{TypeName}}[] | null): Uint8Array {
        const writer = MemoryPackWriter.getSharedInstance();
        this.serializeArrayCore(writer, value);
        return writer.toArray();
    }

    static serializeArrayCore(writer: MemoryPackWriter, value: {{TypeName}}[] | null): void {
        writer.writeArray(value, (writer, x) => {{TypeName}}.serializeCore(writer, x));
    }

    static deserialize(buffer: ArrayBuffer): {{TypeName}} | null {
        return this.deserializeCore(new MemoryPackReader(buffer));
    }

    static deserializeCore(reader: MemoryPackReader): {{TypeName}} | null {
        const [ok, tag] = reader.tryReadUnionHeader();
        if (!ok) {
            return null;
        }

        switch (tag) {
{{EmitUnionDeserialize()}}
            default:
                throw new Error("Tag is not found in this MemoryPackUnion");
        }
    }

    static deserializeArray(buffer: ArrayBuffer): ({{TypeName}} | null)[] | null {
        return this.deserializeArrayCore(new MemoryPackReader(buffer));
    }

    static deserializeArrayCore(reader: MemoryPackReader): ({{TypeName}} | null)[] | null {
        return reader.readArray(reader => {{TypeName}}.deserializeCore(reader));
    }
}
""";
        sb.AppendLine(code);
    }

    public string EmitTypeScriptMembers(TypeScriptMember[] members)
    {
        var sb = new StringBuilder();

        foreach (var item in members)
        {
            sb.AppendLine($"    {item.MemberName}: {item.TypeName};");
        }

        return sb.ToString();
    }

    public string EmitTypeScriptMembersInit(TypeScriptMember[] members)
    {
        var sb = new StringBuilder();

        foreach (var item in members)
        {
            sb.AppendLine($"        this.{item.MemberName} = {item.DefaultValue};");
        }

        return sb.ToString();
    }

    public string EmitTypeScriptSerializeBody(TypeScriptMember[] members)
    {
        var sb = new StringBuilder();

        sb.AppendLine($"        writer.writeObjectHeader({members.Length});");
        foreach (var item in members)
        {
            sb.AppendLine($"        {string.Format(item.WriteMethodTemplate, "value." + item.MemberName)};");
        }

        return sb.ToString();
    }

    public string EmitTypeScriptDeserializeBody(TypeScriptMember[] members, bool emitSkip)
    {
        var sb = new StringBuilder();

        if (!emitSkip)
        {
            foreach (var item in members)
            {
                sb.AppendLine($"            value.{item.MemberName} = {item.ReadMethodTemplate};");
            }
        }
        else
        {
            sb.AppendLine("            if (count == 0) return value;");
            for (int i = 0; i < members.Length; i++)
            {
                var item = members[i];
                sb.AppendLine($"            value.{item.MemberName} = {item.ReadMethodTemplate}; if (count == {i + 1}) return value;");
            }
        }


        return sb.ToString();
    }
}
