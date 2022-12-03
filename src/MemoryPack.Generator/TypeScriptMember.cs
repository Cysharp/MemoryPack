using Microsoft.CodeAnalysis;

namespace MemoryPack.Generator;

internal class TypeScriptType
{
    public string TypeName { get; set; } = default!;
    public bool IsNullable { get; set; }
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

internal class NotSupportedTypeException : Exception
{
    public ITypeSymbol ErrorTypeSymbol { get; }
    public MemberMeta? MemberMeta { get; set; }

    public NotSupportedTypeException(ITypeSymbol typeSymbol)
    {
        this.ErrorTypeSymbol = typeSymbol;
    }
}

public class TypeScriptMember
{
    public MemberMeta Member { get; }
    public string MemberName { get; }
    public string TypeName { get; }
    public string DefaultValue { get; }
    public string WriteMethodTemplate { get; }
    public string ReadMethodTemplate { get; }

    public TypeScriptMember(MemberMeta member, ReferenceSymbols references, TypeScriptGenerateOptions options)
    {
        this.Member = member;
        this.MemberName = options.ConvertPropertyName
            ? Char.ToLowerInvariant(member.Name[0]) + member.Name.Substring(1)
            : member.Name;

        TypeScriptType tsType;
        try
        {
            tsType = ConvertToTypeScriptType(member.MemberType, references, options);
        }
        catch (NotSupportedTypeException ex)
        {
            ex.MemberMeta = member;
            throw;
        }

        this.TypeName = tsType.TypeName;
        this.DefaultValue = tsType.DefaultValue;
        this.WriteMethodTemplate = tsType.WriteMethodTemplate;
        this.ReadMethodTemplate = tsType.ReadMethodTemplate;
    }

    TypeScriptType ConvertToTypeScriptType(ITypeSymbol symbol, ReferenceSymbols references, TypeScriptGenerateOptions options)
    {
        if (symbol.TypeKind == TypeKind.Enum)
        {
            var primitiveType = ConvertFromSymbol(symbol, references, options)!;

            // enum uses self typename(convert to const enum)
            return new TypeScriptType
            {
                TypeName = symbol.Name,
                DefaultValue = primitiveType.DefaultValue,
                IsNullable = symbol.NullableAnnotation == NullableAnnotation.Annotated || symbol.IsReferenceType,
                WriteMethodTemplate = $"writer.write{primitiveType.BinaryOperationMethod}({{0}})",
                ReadMethodTemplate = $"reader.read{primitiveType.BinaryOperationMethod}()"
            };
        }

        if (symbol.TypeKind == TypeKind.Array)
        {
            if (symbol is IArrayTypeSymbol array && array.IsSZArray)
            {
                var elemType = array.ElementType;
                if (elemType.SpecialType == SpecialType.System_Byte)
                {
                    return new TypeScriptType
                    {
                        TypeName = $"Uint8Array | null",
                        DefaultValue = "null",
                        WriteMethodTemplate = $"writer.writeUint8Array({{0}})",
                        ReadMethodTemplate = $"reader.readUint8Array()"
                    };
                }

                var innerType = ConvertToTypeScriptType(elemType, references, options);
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
                    var innerType = ConvertToTypeScriptType(collectionSymbol!.TypeArguments[0], references, options);
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
                    var innerType = ConvertToTypeScriptType(collectionSymbol!.TypeArguments[0], references, options);
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
                    var keyType = ConvertToTypeScriptType(collectionSymbol!.TypeArguments[0], references, options);
                    var valueType = ConvertToTypeScriptType(collectionSymbol!.TypeArguments[1], references, options);
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

        if (symbol.TryGetMemoryPackableType(references, out _, out _) || symbol.IsWillImplementMemoryPackUnion(references))
        {
            return new TypeScriptType
            {
                TypeName = $"{symbol.Name} | null",
                DefaultValue = "null",
                WriteMethodTemplate = $"{symbol.Name}.serializeCore(writer, {{0}})",
                ReadMethodTemplate = $"{symbol.Name}.deserializeCore(reader)"
            };
        }

        var isNullable = (symbol is INamedTypeSymbol nts && nts.EqualsUnconstructedGenericType(references.KnownTypes.System_Nullable_T));
        if (isNullable)
        {
            var primitiveType = ConvertFromSymbol(((INamedTypeSymbol)symbol).TypeArguments[0], references, options)!;

            return new TypeScriptType
            {
                TypeName = $"{primitiveType.TypeName} | null",
                DefaultValue = "null",
                WriteMethodTemplate = $"writer.writeNullable{primitiveType.BinaryOperationMethod}({{0}})",
                ReadMethodTemplate = $"reader.readNullable{primitiveType.BinaryOperationMethod}()"
            };
        }

        // others
        {
            var primitiveType = ConvertFromSymbol(symbol, references, options)!;
            return new TypeScriptType
            {
                TypeName = $"{primitiveType.TypeName}",
                DefaultValue = primitiveType.DefaultValue,
                WriteMethodTemplate = $"writer.write{primitiveType.BinaryOperationMethod}({{0}})",
                ReadMethodTemplate = $"reader.read{primitiveType.BinaryOperationMethod}()"
            };
        }
    }

    TypeScriptTypeCore ConvertFromSymbol(ITypeSymbol typeSymbol, ReferenceSymbols reference, TypeScriptGenerateOptions options)
    {
        var isNullable =
            options.EnableNullableTypes &&
            typeSymbol.IsReferenceType &&
            typeSymbol.NullableAnnotation == NullableAnnotation.Annotated;

        var fromSpecial = ConvertFromSpecialType(typeSymbol.SpecialType, isNullable, options.EnableNullableTypes);
        if (fromSpecial != null) return fromSpecial;

        // + Guid or Enum
        var namedType = typeSymbol as INamedTypeSymbol;
        if (namedType == null) throw new NotSupportedTypeException(typeSymbol);

        if (namedType.TypeKind == TypeKind.Enum)
        {
            var specialType = namedType.EnumUnderlyingType!.SpecialType;
            return ConvertFromSpecialType(specialType, isNullable, options.EnableNullableTypes)!;
        }

        if (SymbolEqualityComparer.Default.Equals(namedType, reference.KnownTypes.System_Guid))
        {
            return new TypeScriptTypeCore
            {
                TypeName = "string",
                DefaultValue = "\"00000000-0000-0000-0000-000000000000\"",
                BinaryOperationMethod = "Guid"
            };
        }

        throw new NotSupportedTypeException(typeSymbol);
    }

    TypeScriptTypeCore? ConvertFromSpecialType(
        SpecialType specialType,
        bool isNullable,
        bool allowNullableTypes)
    {
        // NOTE The function to get the TypeScript type was duplicated in order
        //      to keep the old behavior of the code generator.
        return allowNullableTypes
            ? GetNullableTypesAllowedTypeScriptType(specialType, isNullable)
            : GetNonNullableTypesAllowedTypeScriptType(specialType);
    }

    static TypeScriptTypeCore? GetNonNullableTypesAllowedTypeScriptType(SpecialType specialType)
    {
        string typeName;
        string binaryOperationMethod;
        string defaultValue;

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
                typeName = "Date";
                binaryOperationMethod = "Date";
                defaultValue = "new Date(0)";

                break;

            default:
                return null;
        }

        return new TypeScriptTypeCore
        {
            TypeName = typeName,
            DefaultValue = defaultValue,
            BinaryOperationMethod = binaryOperationMethod
        };
    }

    static TypeScriptTypeCore? GetNullableTypesAllowedTypeScriptType(SpecialType specialType, bool isNullable)
    {
        string typeName;
        string binaryOperationMethod;
        string defaultValue;

        string GetTypeName(string typeName) =>
            isNullable ? $"{typeName} | null" : typeName;

        string GetDefaultValue(string defaultValue) =>
            isNullable ? "null" : defaultValue;

        switch (specialType)
        {
            case SpecialType.System_Boolean:
                typeName = GetTypeName("boolean");
                binaryOperationMethod = "Boolean";
                defaultValue = GetDefaultValue("false");

                break;

            case SpecialType.System_String:
                typeName = GetTypeName("string");
                binaryOperationMethod = "String";
                defaultValue = GetDefaultValue(@"""""");

                break;

            case SpecialType.System_SByte:
                typeName = GetTypeName("number");
                binaryOperationMethod = "Int8";
                defaultValue = GetDefaultValue("0");

                break;

            case SpecialType.System_Byte:
                typeName = GetTypeName("number");
                binaryOperationMethod = "Uint8";
                defaultValue = GetDefaultValue("0");

                break;

            case SpecialType.System_Int16:
                typeName = GetTypeName("number");
                binaryOperationMethod = "Int16";
                defaultValue = GetDefaultValue("0");

                break;

            case SpecialType.System_UInt16:
                typeName = GetTypeName("number");
                binaryOperationMethod = "Uint16";
                defaultValue = GetDefaultValue("0");

                break;

            case SpecialType.System_Int32:
                typeName = GetTypeName("number");
                binaryOperationMethod = "Int32";
                defaultValue = GetDefaultValue("0");

                break;

            case SpecialType.System_UInt32:
                typeName = GetTypeName("number");
                binaryOperationMethod = "Uint32";
                defaultValue = GetDefaultValue("0");

                break;

            case SpecialType.System_Single:
                typeName = GetTypeName("number");
                binaryOperationMethod = "Float32";
                defaultValue = GetDefaultValue("0");

                break;

            case SpecialType.System_Double:
                typeName = GetTypeName("number");
                binaryOperationMethod = "Float64";
                defaultValue = GetDefaultValue("0");

                break;

            case SpecialType.System_Int64:
                typeName = GetTypeName("bigint");
                binaryOperationMethod = "Int64";
                defaultValue = GetDefaultValue("0n");

                break;

            case SpecialType.System_UInt64:
                typeName = GetTypeName("bigint");
                binaryOperationMethod = "Uint64";
                defaultValue = GetDefaultValue("0n");

                break;

            case SpecialType.System_DateTime:
                typeName = GetTypeName("Date");
                binaryOperationMethod = "Date";
                defaultValue = GetDefaultValue("new Date(0)");

                break;

            default:
                return null;
        }

        return new TypeScriptTypeCore
        {
            TypeName = typeName,
            DefaultValue = defaultValue,
            BinaryOperationMethod = binaryOperationMethod
        };
    }
}
