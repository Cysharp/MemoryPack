using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Runtime.Serialization;
using System.Text;

namespace MemoryPack.Generator;

public enum CollectionKind
{
    None, Collection, Set, Dictionary
}

public enum MemberKind
{
    MemoryPackable, // IMemoryPackable<> or [MemoryPackable]
    Unmanaged,
    Nullable, // Nullable<int> is like unmanage but can not write to unmanaged constraint
    KnownType,
    String,
    Array,
    UnmanagedArray,
    Enum,

    // from attribute
    MemoryPackFormatter,
    MemoryPackUnion,

    Object, // others allow
    RefLike, // not allowed
    NonSerializable // not allowed
}

partial class TypeMeta
{
    DiagnosticDescriptor? ctorInvalid = null;
    readonly ReferenceSymbols reference;
    public INamedTypeSymbol Symbol { get; }
    public GenerateType GenerateType { get; }
    /// <summary>MinimallyQualifiedFormat(include generics T>)</summary>
    public string TypeName { get; }
    public MemberMeta[] Members { get; }
    public bool IsValueType { get; set; }
    public bool IsUnmanagedType { get; }
    public bool IsUnion { get; }
    public bool IsRecord { get; }
    public bool IsInterfaceOrAbstract { get; }
    public IMethodSymbol? Constructor { get; }
    public MethodMeta[] OnSerializing { get; }
    public MethodMeta[] OnSerialized { get; }
    public MethodMeta[] OnDeserializing { get; }
    public MethodMeta[] OnDeserialized { get; }
    public (byte Tag, INamedTypeSymbol Type)[] UnionTags { get; }
    public bool IsUseEmptyConstructor => Constructor == null || Constructor.Parameters.IsEmpty;

    public TypeMeta(INamedTypeSymbol symbol, ReferenceSymbols reference)
    {
        this.reference = reference;
        this.Symbol = symbol;

        var packableCtorArgs = symbol.GetAttribute(reference.MemoryPackableAttribute)?.ConstructorArguments;
        this.GenerateType = GenerateType.Object;
        if (packableCtorArgs == null)
        {
            this.GenerateType = GenerateType.NoGenerate;
        }
        else if (packableCtorArgs.Value.Length != 0)
        {
            var ctorValue = packableCtorArgs.Value[0];
            var generateType = ctorValue.Value ?? GenerateType.Object;
            this.GenerateType = (GenerateType)generateType;
        }

        this.TypeName = symbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
        this.Constructor = ChooseConstructor(symbol, reference);

        this.Members = symbol.GetAllMembers() // iterate includes parent type
            .Where(x => x is (IFieldSymbol or IPropertySymbol) and { IsStatic: false, IsImplicitlyDeclared: false })
            .Where(x =>
            {
                var include = x.ContainsAttribute(reference.MemoryPackIncludeAttribute);
                var ignore = x.ContainsAttribute(reference.MemoryPackIgnoreAttribute);
                if (ignore) return false;
                if (include) return true;
                return x.DeclaredAccessibility is Accessibility.Public;
            })
            .Where(x =>
            {
                if (x is IPropertySymbol p)
                {
                    // set only can't be serializable member
                    if (p.GetMethod == null && p.SetMethod != null)
                    {
                        return false;
                    }
                    if (p.IsIndexer) return false;
                }
                return true;
            })
            .Select(x => new MemberMeta(x, Constructor, reference))
            .ToArray();
        this.IsValueType = symbol.IsValueType;
        this.IsUnmanagedType = symbol.IsUnmanagedType;
        this.IsInterfaceOrAbstract = symbol.IsAbstract;
        this.IsUnion = symbol.ContainsAttribute(reference.MemoryPackUnionAttribute);
        this.IsRecord = symbol.IsRecord;
        this.OnSerializing = CollectMethod(reference.MemoryPackOnSerializingAttribute, IsValueType);
        this.OnSerialized = CollectMethod(reference.MemoryPackOnSerializedAttribute, IsValueType);
        this.OnDeserializing = CollectMethod(reference.MemoryPackOnDeserializingAttribute, IsValueType);
        this.OnDeserialized = CollectMethod(reference.MemoryPackOnDeserializedAttribute, IsValueType);
        if (IsUnion)
        {
            this.UnionTags = symbol.GetAttributes()
                .Where(x => SymbolEqualityComparer.Default.Equals(x.AttributeClass, reference.MemoryPackUnionAttribute))
                .Where(x => x.ConstructorArguments.Length == 2)
                .Select(x => ((byte)x.ConstructorArguments[0].Value!, (INamedTypeSymbol)x.ConstructorArguments[1].Value!))
                .ToArray();
        }
        else
        {
            this.UnionTags = Array.Empty<(byte, INamedTypeSymbol)>();
        }
    }

    // MemoryPack choose class/struct as same rule.
    // If has no explicit constrtucotr, use parameterless one(includes private).
    // If has a one parameterless/parameterized constructor, choose it.
    // If has multiple construcotrs, should apply [MemoryPackConstructor] attribute(no automatically choose one), otherwise generator error it.
    IMethodSymbol? ChooseConstructor(INamedTypeSymbol symbol, ReferenceSymbols reference)
    {
        var ctors = symbol.InstanceConstructors
            .Where(x => !x.IsImplicitlyDeclared) // remove empty ctor(struct always generate it), record's clone ctor
            .ToArray();

        if (ctors.Length == 0)
        {
            return null; // allows null as ok(not exists explicitly declared constructor == has implictly empty ctor)
        }

        if (ctors.Length == 1)
        {
            return ctors[0];
        }

        var ctorWithAttrs = ctors.Where(x => x.ContainsAttribute(reference.MemoryPackConstructorAttribute)).ToArray();

        if (ctorWithAttrs.Length == 0)
        {
            ctorInvalid = DiagnosticDescriptors.MultipleCtorWithoutAttribute;
            return null;
        }
        else if (ctorWithAttrs.Length == 1)
        {
            return ctorWithAttrs[0]; // ok
        }
        else
        {
            ctorInvalid = DiagnosticDescriptors.MultipleCtorAttribute;
            return null;
        }
    }

    MethodMeta[] CollectMethod(INamedTypeSymbol attribute, bool isValueType)
    {
        return Symbol.GetMembers()
            .OfType<IMethodSymbol>()
            .Where(x => x.ContainsAttribute(attribute))
            .Select(x => new MethodMeta(x, isValueType))
            .ToArray();
    }

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

partial class MemberMeta
{
    public ISymbol Symbol { get; }
    public string Name { get; }
    public ITypeSymbol MemberType { get; }
    public bool IsField { get; }
    public bool IsProperty { get; }
    public bool IsRef { get; }
    public bool IsSettable { get; }
    public bool IsAssignable { get; }
    public bool IsConstructorParameter { get; }
    public MemberKind Kind { get; }

    public MemberMeta(ISymbol symbol, IMethodSymbol? constructor, ReferenceSymbols references)
    {
        this.Symbol = symbol;
        this.Name = symbol.Name;

        if (constructor != null)
        {
            this.IsConstructorParameter = constructor.Parameters.Any(x => x.Name.Equals(Name, StringComparison.OrdinalIgnoreCase));
        }
        else
        {
            this.IsConstructorParameter = false;
        }

        if (symbol is IFieldSymbol f)
        {
            IsProperty = false;
            IsField = true;
            IsSettable = !f.IsReadOnly; // readonly field can not set.
            IsAssignable = IsSettable && !f.IsRequired;
            IsRef = f.RefKind == RefKind.Ref || f.RefKind == RefKind.RefReadOnly;
            MemberType = f.Type;

        }
        else if (symbol is IPropertySymbol p)
        {
            IsProperty = true;
            IsField = false;
            IsSettable = !p.IsReadOnly;
            IsAssignable = IsSettable && !p.IsRequired && (p.SetMethod != null && !p.SetMethod.IsInitOnly);
            IsRef = p.RefKind == RefKind.Ref || p.RefKind == RefKind.RefReadOnly;
            MemberType = p.Type;
        }
        else
        {
            throw new Exception("member is not field or property.");
        }

        Kind = ParseMemberKind(symbol, MemberType, references);
    }

    static MemberKind ParseMemberKind(ISymbol? memberSymbol, ITypeSymbol memberType, ReferenceSymbols references)
    {
        if (memberType.SpecialType is SpecialType.System_Object or SpecialType.System_Array or SpecialType.System_Delegate or SpecialType.System_MulticastDelegate || memberType.TypeKind == TypeKind.Delegate)
        {
            return MemberKind.NonSerializable; // object, Array, delegate is not allowed
        }
        else if (memberType.TypeKind == TypeKind.Enum)
        {
            return MemberKind.Enum;
        }
        else if (memberType.IsUnmanagedType)
        {
            if (memberType is INamedTypeSymbol unmanagedNts)
            {
                if (unmanagedNts.IsRefLikeType)
                {
                    return MemberKind.RefLike;
                }
                if (unmanagedNts.EqualsUnconstructedGenericType(references.KnownTypes.System_Nullable_T))
                {
                    // unamanged nullable<T> can not pass to where T:unmanaged constraint
                    return MemberKind.Nullable;
                }
            }

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
        else if (memberType.IsWillImplementMemoryPackUnion(references))
        {
            return MemberKind.MemoryPackUnion;
        }
        else if (memberType.TypeKind == TypeKind.Array)
        {
            if (memberType is IArrayTypeSymbol array)
            {
                if (array.IsSZArray)
                {
                    var elemType = array.ElementType;
                    if (elemType.IsUnmanagedType)
                    {
                        return MemberKind.UnmanagedArray;
                    }
                    else
                    {
                        return MemberKind.Array;
                    }
                }
                else
                {
                    // allows 2, 3, 4
                    if (array.Rank <= 4)
                    {
                        return MemberKind.Object;
                    }
                }
            }

            return MemberKind.NonSerializable;
        }
        else if (memberType.TypeKind == TypeKind.TypeParameter) // T
        {
            return MemberKind.Object;
        }
        else
        {
            // or non unmanaged type
            if (memberType is INamedTypeSymbol nts)
            {
                if (nts.IsRefLikeType)
                {
                    return MemberKind.RefLike;
                }
                if (nts.EqualsUnconstructedGenericType(references.KnownTypes.System_Nullable_T))
                {
                    return MemberKind.Nullable;
                }
            }

            if (references.KnownTypes.Contains(memberType))
            {
                return MemberKind.KnownType;
            }

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

public partial class MethodMeta
{
    public IMethodSymbol Symbol { get; }
    public string Name { get; }
    public bool IsStatic { get; }
    public bool IsValueType { get; }

    public MethodMeta(IMethodSymbol symbol, bool isValueType)
    {
        this.Symbol = symbol;
        this.Name = symbol.Name;
        this.IsStatic = symbol.IsStatic;
        this.IsValueType = isValueType;
    }
}
