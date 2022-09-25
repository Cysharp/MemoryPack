using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Text;

namespace MemoryPack.Generator;

public enum CollectionKind
{
    None, Collection, Set, Dictionary
}

partial class MemberMeta
{
    static MemberKind ParseMemberKind(ISymbol? memberSymbol, ITypeSymbol memberType, ReferenceSymbols references)
    {
        if (memberType.SpecialType is SpecialType.System_Object or SpecialType.System_Array or SpecialType.System_Delegate or SpecialType.System_MulticastDelegate || memberType.TypeKind == TypeKind.Delegate)
        {
            return MemberKind.NonSerializable; // object, Array, delegate is not allowed
        }
        else if (memberType is INamedTypeSymbol nts && nts.IsRefLikeType)
        {
            return MemberKind.RefLike;
        }
        else if (memberType.IsUnmanagedType)
        {
            return MemberKind.Unmanaged;
        }
        else if (memberType.SpecialType == SpecialType.System_String)
        {
            return MemberKind.String;
        }
        else if (memberType.AllInterfaces.Any(x => x.EqualsUnconstructedGenericType(references.IMemoryPackable)))
        {
            return MemberKind.MemoryPackable;
        }
        else if (memberType.IsWillImplementIMemoryPackable(references))
        {
            return MemberKind.MemoryPackable;
        }
        else
        {
            if (memberType.TypeKind == TypeKind.Array)
            {
                if (memberType is IArrayTypeSymbol array && array.IsSZArray)
                {
                    var elemType = array.ElementType;
                    if (elemType.IsUnmanagedType)
                    {
                        return MemberKind.UnmanagedArray;
                    }
                }

                return MemberKind.Object;
            }

            if (memberType.TypeKind == TypeKind.TypeParameter) // T
            {
                return MemberKind.Object;
            }

            if (references.KnownTypes.Contains(memberType))
            {
                return MemberKind.KnownType;
            }

            var isIterable = memberType.AllInterfaces.Any(x => x.EqualsUnconstructedGenericType(references.KnownTypes.System_Collections_Generic_IEnumerable_T));
            if (isIterable)
            {
                return MemberKind.Object;
            }
            else
            {
                if (memberSymbol != null)
                {
                    if (memberSymbol.ContainsAttribute(references.MemoryPackFormatterAttribute))
                    {
                        return MemberKind.MemoryPackFormatter;
                    }
                }

                return MemberKind.NonSerializable; // maybe can't serialize, diagnostics target
            }
        }
    }
}

partial class TypeMeta
{
    (CollectionKind, INamedTypeSymbol?) ParseCollectionKind()
    {
        INamedTypeSymbol? dictionary = default;
        INamedTypeSymbol? set = default;
        INamedTypeSymbol? collection = default;
        foreach (var item in this.Symbol.AllInterfaces)
        {
            if (item.EqualsUnconstructedGenericType(reference.KnownTypes.System_Collections_Generic_IDictionary_T))
            {
                dictionary = item;
            }
            else if (item.EqualsUnconstructedGenericType(reference.KnownTypes.System_Collections_Generic_ISet_T))
            {
                set = item;
            }
            else if (item.EqualsUnconstructedGenericType(reference.KnownTypes.System_Collections_Generic_ICollection_T))
            {
                collection = item;
            }
        }

        if (dictionary != null)
        {
            return (CollectionKind.Dictionary, dictionary);
        }
        if (set != null)
        {
            return (CollectionKind.Set, set);
        }
        if (collection != null)
        {
            return (CollectionKind.Collection, collection);
        }

        return (CollectionKind.None, null);
    }

    public bool Validate(TypeDeclarationSyntax syntax, SourceProductionContext context)
    {
        if (GenerateType == GenerateType.NoGenerate) return true;
        if (GenerateType is GenerateType.Collection)
        {
            if (Symbol.IsAbstract)
            {
                context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.CollectionGenerateIsAbstract, syntax.Identifier.GetLocation(), Symbol.Name));
                return false;
            }

            var (kind, symbol) = ParseCollectionKind();
            if (kind == CollectionKind.None)
            {
                context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.CollectionGenerateNotImplementedInterface, syntax.Identifier.GetLocation(), Symbol.Name));
                return false;
            }

            var hasParameterlessConstructor = Symbol.InstanceConstructors
                .Where(x => x.DeclaredAccessibility == Accessibility.Public)
                .Any(x => x.Parameters.Length == 0);
            if (!hasParameterlessConstructor)
            {
                context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.CollectionGenerateNoParameterlessConstructor, syntax.Identifier.GetLocation(), Symbol.Name));
                return false;
            }

            return true;
        }

        // GenerateType.Object validation

        var noError = true;

        // ref strcut
        if (this.Symbol.IsRefLikeType)
        {
            context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.TypeIsRefStruct, syntax.Identifier.GetLocation(), Symbol.Name));
            return false;
        }

        // interface/abstract but not union
        if (IsInterfaceOrAbstract && !IsUnion)
        {
            context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.AbstractMustUnion, syntax.Identifier.GetLocation(), Symbol.Name));
            noError = false;
        }

        if (ctorInvalid != null)
        {
            context.ReportDiagnostic(Diagnostic.Create(ctorInvalid, syntax.Identifier.GetLocation(), Symbol.Name));
            noError = false;
        }

        // check ctor members
        if (this.Constructor != null)
        {
            var nameDict = new HashSet<string>(Members.Where(x => x.IsConstructorParameter).Select(x => x.Name), StringComparer.OrdinalIgnoreCase);
            var allParameterExists = this.Constructor.Parameters.All(x => nameDict.Contains(x.Name));
            if (!allParameterExists)
            {
                context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.ConstructorHasNoMatchedParameter, syntax.Identifier.GetLocation(), Symbol.Name));
                noError = false;
            }
        }

        // methods
        foreach (var item in OnSerializing.Concat(OnSerialized).Concat(OnDeserializing).Concat(OnDeserialized))
        {
            if (item.Symbol.Parameters.Length != 0)
            {
                // diagnostics location should be method identifier
                // however methodsymbol -> methodsyntax is slightly hard so use type identifier instead.
                context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.OnMethodHasParameter, syntax.Identifier.GetLocation(), Symbol.Name, item.Name));
                noError = false;
            }
            if (IsUnmanagedType)
            {
                context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.OnMethodInUnamannagedType, syntax.Identifier.GetLocation(), Symbol.Name, item.Name));
                noError = false;
            }
        }

        // Member override member can't annotate[Ignore][Include]
        if (Symbol.BaseType != null)
        {
            foreach (var item in Symbol.GetAllMembers(withoutOverride: false))
            {
                if (item.IsOverride)
                {
                    var include = item.ContainsAttribute(reference.MemoryPackIncludeAttribute);
                    var ignore = item.ContainsAttribute(reference.MemoryPackIgnoreAttribute);
                    if (include || ignore)
                    {
                        var attr = include ? "MemoryPackInclude" : "MemoryPackIgnore";
                        context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.OverrideMemberCantAddAnnotation, syntax.Identifier.GetLocation(), Symbol.Name, item.Name, attr));
                        noError = false;
                    }
                }
            }
        }

        // ALl Members
        if (Members.Length >= 250) // MemoryPackCode.Reserved1
        {
            context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.MembersCountOver250, syntax.Identifier.GetLocation(), Symbol.Name, Members.Length));
            noError = false;
        }

        // exists can't serialize member
        foreach (var item in Members)
        {
            if (item.Kind == MemberKind.NonSerializable)
            {
                if (item.MemberType.SpecialType is SpecialType.System_Object or SpecialType.System_Array or SpecialType.System_Delegate or SpecialType.System_MulticastDelegate || item.MemberType.TypeKind == TypeKind.Delegate)
                {
                    context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.MemberCantSerializeType, syntax.Identifier.GetLocation(), Symbol.Name, item.Name, item.MemberType.FullyQualifiedToString()));
                    noError = false;
                }
                else
                {
                    context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.MemberIsNotMemoryPackable, syntax.Identifier.GetLocation(), Symbol.Name, item.Name, item.MemberType.FullyQualifiedToString()));
                    noError = false;
                }
            }
            else if (item.Kind == MemberKind.RefLike)
            {
                context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.MemberIsRefStruct, syntax.Identifier.GetLocation(), Symbol.Name, item.Name, item.MemberType.FullyQualifiedToString()));
                noError = false;
            }
        }

        // Union validations
        if (IsUnion)
        {
            if (Symbol.IsSealed)
            {
                context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.SealedTypeCantBeUnion, syntax.Identifier.GetLocation(), Symbol.Name));
                noError = false;
            }

            if (!Symbol.IsAbstract)
            {
                context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.ConcreteTypeCantBeUnion, syntax.Identifier.GetLocation(), Symbol.Name));
                noError = false;
            }

            if (UnionTags.Select(x => x.Tag).HasDuplicate())
            {
                context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.UnionTagDuplicate, syntax.Identifier.GetLocation(), Symbol.Name));
                noError = false;
            }

            foreach (var item in UnionTags)
            {
                // type does not derived target symbol
                if (Symbol.TypeKind == TypeKind.Interface)
                {
                    // interface, check interfaces.
                    var check = item.Type.IsGenericType
                        ? item.Type.OriginalDefinition.AllInterfaces.Any(x => x.EqualsUnconstructedGenericType(Symbol))
                        : item.Type.AllInterfaces.Any(x => SymbolEqualityComparer.Default.Equals(x, Symbol));

                    if (!check)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.UnionMemberTypeNotImplementBaseType, syntax.Identifier.GetLocation(), Symbol.Name, item.Type.Name));
                        noError = false;
                    }
                }
                else
                {
                    // abstract type, check base.
                    var check = item.Type.IsGenericType
                        ? item.Type.OriginalDefinition.GetAllBaseTypes().Any(x => x.EqualsUnconstructedGenericType(Symbol))
                        : item.Type.GetAllBaseTypes().Any(x => SymbolEqualityComparer.Default.Equals(x, Symbol));

                    if (!check)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.UnionMemberTypeNotDerivedBaseType, syntax.Identifier.GetLocation(), Symbol.Name, item.Type.Name));
                        noError = false;
                    }
                }

                if (item.Type.IsValueType)
                {
                    context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.UnionMemberNotAllowStruct, syntax.Identifier.GetLocation(), Symbol.Name, item.Type.Name));
                    noError = false;
                }

                if (!item.Type.ContainsAttribute(reference.MemoryPackableAttribute))
                {
                    context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.UnionMemberMustBeMemoryPackable, syntax.Identifier.GetLocation(), Symbol.Name, item.Type.Name));
                    noError = false;
                }
            }
        }

        return noError;
    }
}
