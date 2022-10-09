using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Diagnostics;
using System.Dynamic;
using System.Text;
using System.Xml.Serialization;

namespace MemoryPack.Generator;

partial class MemoryPackGenerator
{
    static void GenerateTypeScript(TypeDeclarationSyntax syntax, Compilation compilation, string typeScriptOutputDirectoryPath, in SourceProductionContext context)
    {
        var semanticModel = compilation.GetSemanticModel(syntax.SyntaxTree);

        var typeSymbol = semanticModel.GetDeclaredSymbol(syntax, context.CancellationToken);
        if (typeSymbol == null)
        {
            return;
        }

        // TODO: check has MemoryPackable attribute




        var reference = new ReferenceSymbols(compilation);
        var typeMeta = new TypeMeta(typeSymbol, reference);

        // TODO: not GenerateType.Object, error.

        // TODO: Validate FOr TypeScript

        var sb = new StringBuilder();

        sb.AppendLine("""
import { MemoryPackWriter } from "./MemoryPackWriter.js";
import { MemoryPackReader } from "./MemoryPackReader.js";

""");

        typeMeta.EmitTypescript(sb);

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
    }
}

public partial class TypeMeta
{
    public void EmitTypescript(StringBuilder sb)
    {
        if (IsUnion)
        {
            // writer.WriteLine(EmitUnionTemplate());
            return;
        }

        var tsMembers = Members.Select(x => new TypeScriptMember(x, reference)).ToArray();

        var code = $$"""
export class {{TypeName}} {
{{EmitTypeScriptMembers(tsMembers)}}
    public constructor() {
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

    static deserialize(buffer: ArrayBuffer): {{TypeName}} | null {
        return this.deserializeCore(new MemoryPackReader(buffer));
    }

    static deserializeCore(reader: MemoryPackReader): {{TypeName}} | null {
        const [ok, memberCount] = reader.tryReadObjectHeader();
        if (!ok) {
            return null;
        }

{{EmitTypeScriptDeserializeBody(tsMembers)}}
    }
}
""";

        sb.AppendLine(code);
    }

    public string EmitTypeScriptMembers(TypeScriptMember[] members)
    {
        var sb = new StringBuilder();

        // age: number | null
        foreach (var item in members)
        {
            sb.AppendLine($"    {item.MemberName}: {item.TypeName}");
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

    public string EmitTypeScriptDeserializeBody(TypeScriptMember[] members)
    {
        var sb = new StringBuilder();

        // TODO: handle memberCount
        sb.AppendLine($"        var value = new {TypeName}();");
        foreach (var item in members)
        {
            sb.AppendLine($"        value.{item.MemberName} = {item.ReadMethodTemplate};");
        }
        sb.AppendLine("        return value;");

        return sb.ToString();
    }

}
