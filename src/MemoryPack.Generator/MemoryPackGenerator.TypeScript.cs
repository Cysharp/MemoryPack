using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Diagnostics;
using System.Dynamic;
using System.IO.Compression;
using System.Text;
using System.Xml.Serialization;

namespace MemoryPack.Generator;

partial class MemoryPackGenerator
{
    static TypeMeta? GenerateTypeScript(TypeDeclarationSyntax syntax, Compilation compilation, string typeScriptOutputDirectoryPath, in SourceProductionContext context,
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

        if (typeMeta.GenerateType != GenerateType.Object)
        {
            context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.GenerateTypeScriptOnlyAllowsGenerateTypeObject, syntax.Identifier.GetLocation(), typeSymbol.Name));
            return null;
        }

        if (!Validate(typeMeta, syntax, context, reference))
        {
            return null;
        }

        var sb = new StringBuilder();

        sb.AppendLine("""
import { MemoryPackWriter } from "./MemoryPackWriter.js";
import { MemoryPackReader } from "./MemoryPackReader.js";
""");

        var collector = new TypeCollector();
        collector.Visit(typeMeta);

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

        // add ipmort(enum, union, memorypackable)
        foreach (var item in collector.GetEnums())
        {
            sb.AppendLine($"import {{ {item.Name} }} from \"./{item.Name}.js\"; ");
        }
        foreach (var item in collector.GetMemoryPackableTypes(reference).Where(x => !SymbolEqualityComparer.Default.Equals(x, typeSymbol)))
        {
            sb.AppendLine($"import {{ {item.Name} }} from \"./{item.Name}.js\"; ");
        }
        sb.AppendLine();

        try
        {
            typeMeta.EmitTypescript(sb, unionMap);
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
            if (!Directory.Exists(typeScriptOutputDirectoryPath))
            {
                Directory.CreateDirectory(typeScriptOutputDirectoryPath);
            }

            File.WriteAllText(Path.Combine(typeScriptOutputDirectoryPath, $"{typeMeta.TypeName}.ts"), sb.ToString(), new UTF8Encoding(false));
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

        return true;
    }
}

public partial class TypeMeta
{
    public void EmitTypescript(StringBuilder sb, IReadOnlyDictionary<ITypeSymbol, ITypeSymbol> unionMap)
    {
        if (IsUnion)
        {
            EmitTypeScriptUnion(sb);
            return;
        }

        if (!unionMap.TryGetValue(Symbol, out var union))
        {
            union = null;
        }

        var tsMembers = Members.Select(x => new TypeScriptMember(x, reference)).ToArray();
        var impl = (union != null) ? $"implements {union.Name} " : "";

        var code = $$"""
export class {{TypeName}} {{impl}}{
{{EmitTypeScriptMembers(tsMembers)}}
    constructor() {
{{EmitTypeScriptMembersInit(tsMembers)}}
    }

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

{{EmitTypeScriptSerializeBody(tsMembers)}}
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
        const [ok, count] = reader.tryReadObjectHeader();
        if (!ok) {
            return null;
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

    public void EmitTypeScriptUnion(StringBuilder sb)
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
            sb.AppendLine($"import {{ {item.Type.Name} }} from \"./{item.Type.Name}.js\"; ");
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
