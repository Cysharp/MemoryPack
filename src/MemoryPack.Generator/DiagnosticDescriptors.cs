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
        title: "MemoryPackable object can't be nested type",
        messageFormat: "The MemoryPackable object '{0}' can't be nested type",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor AbstractMustBeUnion = new(
        id: "MEMPACK003",
        title: "abstract/interface type of MemoryPackable object must be annotated with Union",
        messageFormat: "abstract/interface type of MemoryPackable object '{0}' must be annotated with Union",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor MultipleCtorWithoutAttribute = new(
        id: "MEMPACK004",
        title: "Require [MemoryPackConstructor] when multiple constructors exist",
        messageFormat: "The MemoryPackable object '{0}' must be annotated with [MemoryPackConstructor] when multiple constructors exist",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor MultipleCtorAttribute = new(
        id: "MEMPACK005",
        title: "[MemoryPackConstructor] exists in multiple constructors",
        messageFormat: "Multiple [MemoryPackConstructor] exist in '{0}' while only single ctor is allowed",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor ConstructorHasNoMatchedParameter = new(
        id: "MEMPACK006",
        title: "MemoryPackObject's constructor has no matching parameter",
        messageFormat: "The MemoryPackable object '{0}' constructor's parameter '{1}' must match a serialized member name (case-insensitive)",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor OnMethodHasParameter = new(
        id: "MEMPACK007",
        title: "MemoryPackObject's On*** methods must not have any parameters",
        messageFormat: "The MemoryPackable object '{0}''s '{1}' method must not have any parameters",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor OnMethodInUnmanagedType = new(
        id: "MEMPACK008",
        title: "MemoryPackObject's On*** methods can't be annotated in unmanaged struct",
        messageFormat: "The MemoryPackable object '{0}' is unmanaged struct that can't be annotated On***Attribute however '{1}' method annotated",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor OverrideMemberCantAddAnnotation = new(
        id: "MEMPACK009",
        title: "Override member can't be annotated with Ignore/Include attribute",
        messageFormat: "The MemoryPackable object '{0}' override member '{1}' can't be annotate with {2} attribute",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor SealedTypeCantBeUnion = new(
        id: "MEMPACK010",
        title: "Sealed type can't be union",
        messageFormat: "The MemoryPackable object '{0}' is sealed type so can't be Union",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);


    public static readonly DiagnosticDescriptor ConcreteTypeCantBeUnion = new(
        id: "MEMPACK011",
        title: "Concrete type can't be union",
        messageFormat: "The MemoryPackable object '{0}' can't be Union. Only abstract or interface can be Union",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);


    public static readonly DiagnosticDescriptor UnionTagDuplicate = new(
        id: "MEMPACK012",
        title: "Union tag is duplicate",
        messageFormat: "The MemoryPackable object '{0}' union tag value is duplicate",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);


    public static readonly DiagnosticDescriptor UnionMemberTypeNotImplementBaseType = new(
        id: "MEMPACK013",
        title: "Union member does not implement union interface",
        messageFormat: "The MemoryPackable object '{0}' union member '{1}' does not implement union interface",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);


    public static readonly DiagnosticDescriptor UnionMemberTypeNotDerivedBaseType = new(
        id: "MEMPACK014",
        title: "Union member is not derived from base union type",
        messageFormat: "The MemoryPackable object '{0}' union member '{1}' is not derived from base union type",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor UnionMemberNotAllowStruct = new(
        id: "MEMPACK015",
        title: "Union member can't be a struct",
        messageFormat: "The MemoryPackable object '{0}''s union member '{1}' is a struct while it is not allowed",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor UnionMemberMustBeMemoryPackable = new(
        id: "MEMPACK016",
        title: "Union member has to be MemoryPackable",
        messageFormat: "The MemoryPackable object '{0}' union member '{1}' has to be MemoryPackable",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor MembersCountOver250 = new(
        id: "MEMPACK017",
        title: "Max member count limit exceeded",
        messageFormat: "The MemoryPackable object '{0}''s member count is '{1}', while the max limit is 249",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor MemberCantSerializeType = new(
        id: "MEMPACK018",
        title: "Member's type can't be serialized",
        messageFormat: "The MemoryPackable object '{0}' contains member '{1}' of type '{2}' that can't be serialized",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor MemberIsNotMemoryPackable = new(
        id: "MEMPACK019",
        title: "Member is not a MemoryPackable object",
        messageFormat: "The MemoryPackable object '{0}' contains member '{1}' of type '{2}' that is not MemoryPackable. Annotate [MemoryPackable] to '{2}' or annotate `[MemoryPackAllowSerialize]` to suppress if it's an external type that can be serialized",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor TypeIsRefStruct = new(
        id: "MEMPACK020",
        title: "Type is a ref struct",
        messageFormat: "The MemoryPackable object '{0}' is a ref struct that can't be serialized",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor MemberIsRefStruct = new(
        id: "MEMPACK021",
        title: "Member is a ref struct",
        messageFormat: "The MemoryPackable object '{0}' contains member '{1}' of type '{2}' which is a ref struct that can't be serialized",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor CollectionGenerateIsAbstract = new(
        id: "MEMPACK022",
        title: "Collection type can't be abstract/interface",
        messageFormat: "The MemoryPackable object '{0}' is a GenerateType.Collection but also an abstract/interface. Only concrete type is allowed",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor CollectionGenerateNotImplementedInterface = new(
        id: "MEMPACK023",
        title: "Collection type must implement collection interface",
        messageFormat: "The MemoryPackable object '{0}' is a GenerateType.Collection but does not implement collection interface (ICollection<T>/ISet<T>/IDictionary<TKey,TValue>)",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor CollectionGenerateNoParameterlessConstructor = new(
        id: "MEMPACK024",
        title: "Collection type must require parameterless constructor",
        messageFormat: "The MemoryPackable object '{0}' is a GenerateType.Collection but does not have parameterless constructor",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor AllMembersMustAnnotateOrder = new(
        id: "MEMPACK025",
        title: "All members must annotate MemoryPackOrder while annotated with SerializeLayout.Explicit",
        messageFormat: "The MemoryPackable object '{0}''s member '{1}' does not annotate MemoryPackOrder",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor AllMembersMustBeConsecutiveNumber = new(
        id: "MEMPACK026",
        title: "All MemoryPackOrder members must be consecutive numbers starting from zero",
        messageFormat: "The MemoryPackable object '{0}''s member '{1}' is not consecutive number from zero",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor GenerateTypeScriptMustBeMemoryPackable = new(
        id: "MEMPACK027",
        title: "GenerateTypeScript must be MemoryPackable",
        messageFormat: "Type '{0}' is annotated with GenerateTypeScript but not annotated as MemoryPackable",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor GenerateTypeScriptOnlyAllowsGenerateTypeObject = new(
        id: "MEMPACK028",
        title: "GenerateTypeScript must be MemoryPackable of type GenerateType.Object",
        messageFormat: "When Type '{0}' is annotated with GenerateTypeScript, GenerateType of MemoryPackable must be GenerateType.Object",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor GenerateTypeScriptDoesNotAllowGenerics = new(
        id: "MEMPACK029",
        title: "GenerateTypeScript type does not allow having generic type parameters",
        messageFormat: "Type '{0}' is annotated with GenerateTypeScript which does not allow having generic type parameters",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor GenerateTypeScriptDoesNotAllowLongEnum = new(
        id: "MEMPACK030",
        title: "GenerateTypeScript type does not allow 64bit enum",
        messageFormat: "GenerateTypeScript type '{0}' does not support 64bit(long/ulong) enum type '{1}'. 64bit enum is not supported in TypeScript generation",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor GenerateTypeScriptNotSupportedType = new(
        id: "MEMPACK031",
        title: "Type is not supported with GenerateTypeScript",
        messageFormat: "GenerateTypeScript type '{0}' member '{1}' of type '{2}' is not supported type in TypeScript generation",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor GenerateTypeScriptNotSupportedCustomFormatter = new(
        id: "MEMPACK032",
        title: "Type with CustomFormatter is not supported with GenerateTypeScript",
        messageFormat: "GenerateTypeScript type '{0}' member '{1}' is annotated with [MemoryPackCustomFormatter] which is not supported in TypeScript generation",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor CircularReferenceOnlyAllowsParameterlessConstructor = new(
        id: "MEMPACK033",
        title: "CircularReference type MemoryPack object require parameterless constructor",
        messageFormat: "The MemoryPackable object '{0}' is GenerateType.CircularReference type but doesn't contain any parameterless constructor",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor UnmanagedStructWithLayoutAutoField = new(
        id: "MEMPACK034",
        title: "Unmanaged struct before .NET 7 must annotate LayoutKind.Auto or Explicit",
        messageFormat: "The unmanaged struct '{0}' contains field('{1}') with LayoutKind.Auto. Before .NET 7, if a struct contains field with LayoutKind.Auto then struct layout is automatically promoted to LayoutKind.Auto. But from .NET 7, default layout is set to be Sequential without annotation, breaking binary compatibility when runtime is upgraded. For safety, you must annotate [StructLayout(LayoutKind.Auto)] or LayoutKind.Explicit to type.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor UnmanagedStructMemoryPackCtor = new(
        id: "MEMPACK035",
        title: "Unmanaged struct does not allow [MemoryPackConstructor]",
        messageFormat: "The unmanaged struct '{0}' can't be annotated with [MemoryPackConstructor] because no constructors are called",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor InheritTypeCanNotIncludeParentPrivateMember = new(
        id: "MEMPACK036",
        title: "Inherited type can not include the parent type's private member",
        messageFormat: "Type '{0}' can not include the parent type's private member '{1}'",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor ReadOnlyFieldMustBeConstructorMember = new(
        id: "MEMPACK037",
        title: "Readonly field must be included as constructor member",
        messageFormat: "Type '{0}' readonly field '{1}' must be included as constructor member",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor DuplicateOrderDoesNotAllow = new(
        id: "MEMPACK038",
        title: "Each member's order must be unique",
        messageFormat: "The MemoryPackable object '{0}' member '{1}''s order overlaps with '{2}'. Each member's order must be unique",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);
}
