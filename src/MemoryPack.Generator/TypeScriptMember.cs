using Microsoft.CodeAnalysis;
using System.Reflection.PortableExecutable;
using System;

namespace MemoryPack.Generator;

internal class TypeScriptType
{
    public string TypeName { get; set; } = default!;
    public string DefaultValue { get; set; } = default!;
    public string WriteMethodTemplate { get; set; } = default!;
    public string ReadMethodTemplate { get; set; } = default!;
}

internal class TypeScriptTypeCore
{
    public string TypeName { get; set; } = default!;
    public string DefaultValue { get; set; } = default!;
    public string BinaryOperationMethod { get; set; } = default!;
}

public class TypeScriptMember
{
    public MemberMeta Member { get; }
    public string MemberName { get; }
    public string TypeName { get; }
    public string DefaultValue { get; }
    public string WriteMethodTemplate { get; }
    public string ReadMethodTemplate { get; }

    public TypeScriptMember(MemberMeta member, ReferenceSymbols references)
    {
        this.Member = member;
        this.MemberName = Char.ToLowerInvariant(member.Name[0]) + member.Name.Substring(1);

        var tsType = ConvertToTypeScriptType(member.MemberType, references);

        this.TypeName = tsType.TypeName;
        this.DefaultValue = tsType.DefaultValue;
        this.WriteMethodTemplate = tsType.WriteMethodTemplate;
        this.ReadMethodTemplate = tsType.ReadMethodTemplate;
    }

    TypeScriptType ConvertToTypeScriptType(ITypeSymbol symbol, ReferenceSymbols references)
    {
        var isNullable = (symbol is INamedTypeSymbol nts && nts.EqualsUnconstructedGenericType(references.KnownTypes.System_Nullable_T));
        if (isNullable)
        {
            var primitiveType = ConvertFromSpecialType(((INamedTypeSymbol)symbol).TypeArguments[0].SpecialType);

            return new TypeScriptType
            {
                TypeName = $"{primitiveType.TypeName} | null",
                DefaultValue = "null",
                WriteMethodTemplate = $"writer.writeNullable{primitiveType.BinaryOperationMethod}({{0}})",
                ReadMethodTemplate = $"reader.readNullable{primitiveType.BinaryOperationMethod}()"
            };
        }

        if (symbol.TypeKind == TypeKind.Enum)
        {
            var specialType = ((INamedTypeSymbol)symbol).EnumUnderlyingType!.SpecialType;
            var primitiveType = ConvertFromSpecialType(specialType);

            // enum uses self typename(convert to const enum)
            return new TypeScriptType
            {
                TypeName = symbol.Name,
                DefaultValue = primitiveType.DefaultValue,
                WriteMethodTemplate = $"writer.write{primitiveType.BinaryOperationMethod}({{0}})",
                ReadMethodTemplate = $"reader.read{primitiveType.BinaryOperationMethod}()"
            };
        }

        if (symbol.TypeKind == TypeKind.Array)
        {
            if (symbol is IArrayTypeSymbol array && array.IsSZArray)
            {
                var elemType = array.ElementType;
                var innerType = ConvertToTypeScriptType(elemType, references);
                var typeName = innerType.TypeName.Contains("null") ? $"({innerType.TypeName})" : innerType.TypeName;

                var elementWriter = string.Format(innerType.WriteMethodTemplate, "x");
                var elementReader = string.Format(innerType.ReadMethodTemplate);

                return new TypeScriptType
                {
                    TypeName = $"{typeName}[] | null",
                    DefaultValue = "null",
                    WriteMethodTemplate = $"writer.writeArray({{0}}, (writer, x) => {elementWriter})",
                    ReadMethodTemplate = $"reader.readArray(reader => {elementReader})"
                };
            }
        }

        // is collection

        var (collectionKind, collectionSymbol) = TypeMeta.ParseCollectionKind(symbol as INamedTypeSymbol, references);
        switch (collectionKind)
        {
            case CollectionKind.Collection:
                {
                    var innerType = ConvertToTypeScriptType(collectionSymbol!.TypeArguments[0], references);
                    // same as Array
                    var typeName = innerType.TypeName.Contains("null") ? $"({innerType.TypeName})" : innerType.TypeName;

                    var elementWriter = string.Format(innerType.WriteMethodTemplate, "x");
                    var elementReader = string.Format(innerType.ReadMethodTemplate);

                    return new TypeScriptType
                    {
                        TypeName = $"{typeName}[] | null",
                        DefaultValue = "null",
                        WriteMethodTemplate = $"writer.writeArray({{0}}, (writer, x) => {elementWriter})",
                        ReadMethodTemplate = $"reader.readArray(reader => {elementReader})"
                    };
                }
            case CollectionKind.Set:
                {
                    var innerType = ConvertToTypeScriptType(collectionSymbol!.TypeArguments[0], references);
                    var elementWriter = string.Format(innerType.WriteMethodTemplate, "x");
                    var elementReader = string.Format(innerType.ReadMethodTemplate);

                    return new TypeScriptType
                    {
                        TypeName = $"Set<{innerType.TypeName}> | null",
                        DefaultValue = "null",
                        WriteMethodTemplate = $"writer.writeSet({{0}}, (writer, x) => {elementWriter})",
                        ReadMethodTemplate = $"reader.readSet(reader => {elementReader})"
                    };
                }
            case CollectionKind.Dictionary:
                {
                    var keyType = ConvertToTypeScriptType(collectionSymbol!.TypeArguments[0], references);
                    var valueType = ConvertToTypeScriptType(collectionSymbol!.TypeArguments[1], references);
                    var keyWriter = string.Format(keyType.WriteMethodTemplate, "x");
                    var keyReader = string.Format(keyType.ReadMethodTemplate);
                    var valueWriter = string.Format(valueType.WriteMethodTemplate, "x");
                    var valueReader = string.Format(valueType.ReadMethodTemplate);

                    return new TypeScriptType
                    {
                        TypeName = $"Map<{keyType.TypeName}, {valueType.TypeName}> | null",
                        DefaultValue = "null",
                        WriteMethodTemplate = $"writer.writeMap({{0}}, (writer, x) => {keyWriter}, (writer, x) => {valueWriter})",
                        ReadMethodTemplate = $"reader.readMap(reader => {keyReader}, reader => {valueReader})"
                    };
                }
            default:
                break;
        }

        if (symbol.IsWillImplementIMemoryPackable(references) || symbol.IsWillImplementMemoryPackUnion(references))
        {
            return new TypeScriptType
            {
                TypeName = $"{symbol.Name} | null",
                DefaultValue = "null",
                WriteMethodTemplate = $"{symbol.Name}.serializeCore(writer, {{0}})",
                ReadMethodTemplate = $"{symbol.Name}.deserializeCore(reader)"
            };
        }

        // standard
        {
            var primitiveType = ConvertFromSpecialType(symbol.SpecialType);
            return new TypeScriptType
            {
                TypeName = $"{primitiveType.TypeName}",
                DefaultValue = primitiveType.DefaultValue,
                WriteMethodTemplate = $"writer.write{primitiveType.BinaryOperationMethod}({{0}})",
                ReadMethodTemplate = $"reader.read{primitiveType.BinaryOperationMethod}()"
            };
        }
    }

    TypeScriptTypeCore ConvertFromSpecialType(SpecialType specialType)
    {
        var defaultValue = "";
        var typeName = "";
        string binaryOperationMethod = "";
        switch (specialType)
        {
            case SpecialType.System_Boolean:
                typeName = "boolean";
                binaryOperationMethod = "Boolean";
                defaultValue = "false";
                break;
            case SpecialType.System_String:
                typeName = "string | null";
                binaryOperationMethod = "String";
                defaultValue = "null";
                break;
            case SpecialType.System_SByte:
                typeName = "number";
                binaryOperationMethod = "Int8";
                defaultValue = "0";
                break;
            case SpecialType.System_Byte:
                typeName = "number";
                binaryOperationMethod = "Uint8";
                defaultValue = "0";
                break;
            case SpecialType.System_Int16:
                typeName = "number";
                binaryOperationMethod = "Int16";
                defaultValue = "0";
                break;
            case SpecialType.System_UInt16:
                typeName = "number";
                binaryOperationMethod = "Uint16";
                defaultValue = "0";
                break;
            case SpecialType.System_Int32:
                typeName = "number";
                binaryOperationMethod = "Int32";
                defaultValue = "0";
                break;
            case SpecialType.System_UInt32:
                typeName = "number";
                binaryOperationMethod = "Uint32";
                defaultValue = "0";
                break;
            case SpecialType.System_Single:
                typeName = "number";
                binaryOperationMethod = "Float32";
                defaultValue = "0";
                break;
            case SpecialType.System_Double:
                typeName = "number";
                binaryOperationMethod = "Float64";
                defaultValue = "0";
                break;
            case SpecialType.System_Int64:
                typeName = "bigint";
                binaryOperationMethod = "Int64";
                defaultValue = "0n";
                break;
            case SpecialType.System_UInt64:
                typeName = "bigint";
                binaryOperationMethod = "Uint64";
                defaultValue = "0n";
                break;
            case SpecialType.System_DateTime:
                typeName = "Data";
                binaryOperationMethod = "Date";
                defaultValue = "new Date(0)";
                break;
            default:
                // Guid
                // 00000000-0000-0000-0000-000000000000
                // not special, use itself.
                break;
        }

        return new TypeScriptTypeCore { TypeName = typeName, DefaultValue = defaultValue, BinaryOperationMethod = binaryOperationMethod };
    }
}
