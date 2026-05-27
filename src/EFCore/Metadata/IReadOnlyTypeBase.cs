// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Represents a structural type in the model.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
/// </remarks>
public interface IReadOnlyTypeBase : IReadOnlyAnnotatable
{
    /// <summary>
    ///     Gets the model that this type belongs to.
    /// </summary>
    IReadOnlyModel Model { get; }

    /// <summary>
    ///     Gets this entity type or the one on which the complex property chain is declared.
    /// </summary>
    IReadOnlyEntityType ContainingEntityType
        => (IReadOnlyEntityType)this;

    /// <summary>
    ///     Gets the base type of this type. Returns <see langword="null" /> if this is not a
    ///     derived type in an inheritance hierarchy.
    /// </summary>
    IReadOnlyTypeBase? BaseType { get; }

    /// <summary>
    ///     Gets the name of this type.
    /// </summary>
    string Name { get; }

    /// <summary>
    ///     Gets the CLR class that is used to represent instances of this type.
    ///     Returns <see langword="null" /> if the type does not have a corresponding CLR class (known as a shadow type).
    /// </summary>
    /// <remarks>
    ///     Shadow types are not currently supported in a model that is used at runtime with a <see cref="DbContext" />.
    ///     Therefore, shadow types will only exist in migration model snapshots, etc.
    /// </remarks>
    [DynamicallyAccessedMembers(IEntityType.DynamicallyAccessedMemberTypes)]
    Type ClrType { get; }

    /// <summary>
    ///     Gets a value indicating whether this structural type is mapped to a <see cref="Type" /> that
    ///     other structural types are also mapped to.
    /// </summary>
    bool HasSharedClrType { get; }

    /// <summary>
    ///     Gets a value indicating whether this structural type has an indexer which is able to contain arbitrary properties
    ///     and a method that can be used to determine whether a given indexer property contains a value.
    /// </summary>
    bool IsPropertyBag { get; }

    /// <summary>
    ///     Gets a value indicating whether this structural type represents an abstract type.
    /// </summary>
    /// <returns><see langword="true" /> if the type is abstract, <see langword="false" /> otherwise.</returns>
    [DebuggerStepThrough]
    bool IsAbstract()
        => ClrType.IsAbstract;

    /// <summary>
    ///     Gets the friendly display name for this structural type.
    /// </summary>
    /// <returns>The display name.</returns>
    [DebuggerStepThrough]
    string DisplayName()
        => DisplayName(omitSharedType: false);

    /// <summary>
    ///     Gets the friendly display name for the given <see cref="IReadOnlyTypeBase" />.
    /// </summary>
    /// <param name="omitSharedType">
    ///     A value indicating whether the name of the type for shared type entity types should be omitted from the returned value.
    /// </param>
    /// <returns>The display name.</returns>
    [DebuggerStepThrough]
    string DisplayName(bool omitSharedType)
    {
        if (!HasSharedClrType)
        {
            return StripFileScopedTypePrefixes(ClrType.ShortDisplayName());
        }

        var clrTypeDisplayName = StripFileScopedTypePrefixes(ClrType.ShortDisplayName());

        var shortName = Name;
        var hashIndex = shortName.IndexOf("#", StringComparison.Ordinal);
        if (hashIndex == -1)
        {
            return Name + " (" + clrTypeDisplayName + ")";
        }

        var plusIndex = shortName.LastIndexOf("+", StringComparison.Ordinal);
        if (plusIndex != -1)
        {
            shortName = shortName[(plusIndex + 1)..];
        }
        else
        {
            var dotIndex = shortName.LastIndexOf(".", hashIndex, hashIndex + 1, StringComparison.Ordinal);
            if (dotIndex != -1)
            {
                dotIndex = shortName.LastIndexOf(".", dotIndex - 1, dotIndex, StringComparison.Ordinal);
                if (dotIndex != -1)
                {
                    shortName = shortName[(dotIndex + 1)..];
                }
            }
        }

        return shortName == Name
            ? shortName + " (" + clrTypeDisplayName + ")"
            : shortName;
    }

    /// <summary>
    ///     Gets a short name for the given <see cref="IReadOnlyTypeBase" /> that can be used in other identifiers.
    /// </summary>
    /// <returns>The short name.</returns>
    [DebuggerStepThrough]
    string ShortName()
    {
        if (!HasSharedClrType)
        {
            var name = ClrType.ShortDisplayName();
            if (name.StartsWith("<>", StringComparison.Ordinal))
            {
                // Anonymous and closure types: <>f__AnonymousType0, <>c__DisplayClass0_0, ...
                name = name[2..];
            }
            else
            {
                // File-scoped types: Roslyn synthesizes the metadata name
                // <FileName>F<hex>__UserTypeName for `file class` / `file record` declarations.
                // Strip these sentinels wherever they appear (top-level or nested in generic args),
                // so e.g. List<<File>F1234__Inner> becomes List<Inner>.
                name = StripFileScopedTypePrefixes(name);
            }

            var lessIndex = name.IndexOf('<', StringComparison.Ordinal);
            return lessIndex == -1 ? name : name[..lessIndex];
        }

        var hashIndex = Name.LastIndexOf("#", StringComparison.Ordinal);
        if (hashIndex == -1)
        {
            var plusIndex = Name.LastIndexOf("+", StringComparison.Ordinal);
            if (plusIndex == -1)
            {
                var dotIndex = Name.LastIndexOf(".", StringComparison.Ordinal);
                return dotIndex == -1
                    ? Name
                    : Name[(dotIndex + 1)..];
            }

            return Name[(plusIndex + 1)..];
        }

        return Name[(hashIndex + 1)..];
    }

    /// <summary>
    ///     Strips Roslyn's synthesized file-scoped type prefix (<c>&lt;FileName&gt;F&lt;hex&gt;__</c>)
    ///     from a CLR display name, including occurrences nested inside generic argument lists.
    ///     For example <c>List&lt;&lt;Program&gt;F1234__Inner&gt;</c> becomes <c>List&lt;Inner&gt;</c>.
    /// </summary>
    /// <remarks>
    ///     The <c>&gt;F</c> signature distinguishes file-scoped types from other compiler-generated
    ///     types whose names begin with <c>&lt;</c> (async state machines <c>&lt;Method&gt;d__0</c>,
    ///     local function host classes <c>&lt;Method&gt;g__Local|0_0</c>, anonymous types
    ///     <c>&lt;&gt;f__AnonymousType</c>, closure display classes <c>&lt;&gt;c__DisplayClass</c>).
    ///     Roslyn's synthesized metadata pattern uses the literal <c>&lt;filename&gt;F&lt;hex&gt;__</c>
    ///     shape; the filename portion does not contain <c>&lt;</c>, so the closing <c>&gt;</c> of a
    ///     sentinel is always the next <c>&gt;</c> after the opening <c>&lt;</c> with no
    ///     intervening <c>&lt;</c>.
    /// </remarks>
    private static string StripFileScopedTypePrefixes(string name)
    {
        if (name.IndexOf('<', StringComparison.Ordinal) == -1)
        {
            return name;
        }

        StringBuilder? sb = null;
        var i = 0;
        while (i < name.Length)
        {
            if (name[i] == '<')
            {
                // Look for the immediately-following `>F<hex>__` sentinel:
                //   - the next `>` must come without any nested `<` in between (filenames have neither)
                //   - the char right after `>` must be `F`
                //   - a `__` must follow (the prefix terminator)
                var closeAngle = name.IndexOf('>', i + 1);
                if (closeAngle != -1
                    && closeAngle + 1 < name.Length
                    && name[closeAngle + 1] == 'F')
                {
                    var nestedLt = name.IndexOf('<', i + 1, closeAngle - i - 1);
                    if (nestedLt == -1)
                    {
                        var separator = name.IndexOf("__", closeAngle + 1, StringComparison.Ordinal);
                        if (separator != -1)
                        {
                            sb ??= new StringBuilder(name.Length).Append(name, 0, i);
                            i = separator + 2;
                            continue;
                        }
                    }
                }
            }

            sb?.Append(name[i]);
            i++;
        }

        return sb?.ToString() ?? name;
    }

    /// <summary>
    ///     Determines whether the current type can be assigned to the specified type, i.e. is derived from or identical to it.
    /// </summary>
    /// <param name="targetType">The type to check.</param>
    /// <returns>
    ///     <see langword="true" /> if the current type is assignable to <paramref name="targetType" />,
    ///     otherwise <see langword="false" />.
    /// </returns>
    bool IsAssignableTo(IReadOnlyTypeBase targetType)
        => this == targetType || targetType.GetDerivedTypes().Contains(this);

    /// <summary>
    ///     Determines whether the current type can be assigned from the specified type, i.e. is a base type of or identical to it.
    /// </summary>
    /// <param name="derivedType">The type to check.</param>
    /// <returns>
    ///     <see langword="true" /> if the current type is assignable from <paramref name="derivedType" />,
    ///     otherwise <see langword="false" />.
    /// </returns>
    bool IsAssignableFrom(IReadOnlyTypeBase derivedType)
        => this == derivedType || GetDerivedTypes().Contains(derivedType);

    /// <summary>
    ///     Determines if this type derives from (but is not the same as) a given type.
    /// </summary>
    /// <param name="baseType">The type to check if it is a base type of this type.</param>
    /// <returns>
    ///     <see langword="true" /> if this type derives from (but is not the same as) <paramref name="baseType" />,
    ///     otherwise <see langword="false" />.
    /// </returns>
    bool IsStrictlyDerivedFrom(IReadOnlyTypeBase baseType)
        => this != Check.NotNull(baseType) && baseType.IsAssignableFrom(this);

    /// <summary>
    ///     Gets all types in the model that derive from this type.
    /// </summary>
    /// <returns>The derived types.</returns>
    IEnumerable<IReadOnlyTypeBase> GetDerivedTypes();

    /// <summary>
    ///     Returns all derived types of this type, including the type itself.
    /// </summary>
    /// <returns>Derived types.</returns>
    IEnumerable<IReadOnlyTypeBase> GetDerivedTypesInclusive()
        => new[] { this }.Concat(GetDerivedTypes());

    /// <summary>
    ///     Gets all types in the model that directly derive from this type.
    /// </summary>
    /// <returns>The derived types.</returns>
    IEnumerable<IReadOnlyTypeBase> GetDirectlyDerivedTypes();

    /// <summary>
    ///     Gets the root base type for a given entity type.
    /// </summary>
    /// <returns>
    ///     The root base type. If the given entity type is not a derived type, then the same entity type is returned.
    /// </returns>
    IReadOnlyTypeBase GetRootType()
        => BaseType?.GetRootType() ?? this;

    /// <summary>
    ///     Returns the property that will be used for storing a discriminator value.
    /// </summary>
    /// <returns>The property that will be used for storing a discriminator value.</returns>
    IReadOnlyProperty? FindDiscriminatorProperty()
    {
        var propertyName = GetDiscriminatorPropertyName();
        return propertyName == null ? null : FindProperty(propertyName);
    }

    /// <summary>
    ///     Returns the name of the property that will be used for storing a discriminator value.
    /// </summary>
    /// <returns>The name of the property that will be used for storing a discriminator value.</returns>
    string? GetDiscriminatorPropertyName();

    /// <summary>
    ///     Returns the discriminator value for this type.
    /// </summary>
    /// <returns>The discriminator value for this type.</returns>
    object? GetDiscriminatorValue()
    {
        var annotation = FindAnnotation(CoreAnnotationNames.DiscriminatorValue);
        return annotation != null
            ? annotation.Value
            : !ClrType.IsInstantiable()
            || (BaseType == null && GetDirectlyDerivedTypes().Count() == 0)
                ? null
                : (object?)GetDefaultDiscriminatorValue();
    }

    /// <summary>
    ///     Returns the default discriminator value that would be used for this type.
    /// </summary>
    /// <returns>The default discriminator value for this type.</returns>
    string GetDefaultDiscriminatorValue()
        => !HasSharedClrType ? ClrType.ShortDisplayName() : ShortName();

    /// <summary>
    ///     Gets the property with the given name. Returns <see langword="null" /> if no property with the given name is defined.
    /// </summary>
    /// <remarks>
    ///     This API only finds scalar properties and does not find navigation, complex or service properties.
    /// </remarks>
    /// <param name="name">The name of the property.</param>
    /// <returns>The property, or <see langword="null" /> if none is found.</returns>
    IReadOnlyProperty? FindProperty(string name);

    /// <summary>
    ///     Gets a property with the given member info. Returns <see langword="null" /> if no property is found.
    /// </summary>
    /// <remarks>
    ///     This API only finds scalar properties and does not find navigation, complex or service properties.
    /// </remarks>
    /// <param name="memberInfo">The member on the class.</param>
    /// <returns>The property, or <see langword="null" /> if none is found.</returns>
    IReadOnlyProperty? FindProperty(MemberInfo memberInfo)
        => (Check.NotNull(memberInfo) as PropertyInfo)?.IsIndexerProperty() == true
            ? null
            : FindProperty(memberInfo.GetSimpleMemberName());

    /// <summary>
    ///     Finds matching properties on the given type. Returns <see langword="null" /> if any property is not found.
    /// </summary>
    /// <remarks>
    ///     This API only finds scalar properties and does not find navigation, complex or service properties.
    /// </remarks>
    /// <param name="propertyNames">The property names.</param>
    /// <returns>The properties, or <see langword="null" /> if any property is not found.</returns>
    IReadOnlyList<IReadOnlyProperty>? FindProperties(IReadOnlyList<string> propertyNames);

    /// <summary>
    ///     Finds a property declared on the type with the given name.
    ///     Does not return properties defined on a base type.
    /// </summary>
    /// <param name="name">The property name.</param>
    /// <returns>The property, or <see langword="null" /> if none is found.</returns>
    IReadOnlyProperty? FindDeclaredProperty(string name);

    /// <summary>
    ///     Gets a property with the given name.
    /// </summary>
    /// <remarks>
    ///     This API only finds scalar properties and does not find navigation, complex or service properties.
    /// </remarks>
    /// <param name="name">The property name.</param>
    /// <returns>The property.</returns>
    IReadOnlyProperty GetProperty(string name)
    {
        Check.NotEmpty(name);

        var property = FindProperty(name);
        return property == null
            ? throw new InvalidOperationException(CoreStrings.PropertyNotFound(name, DisplayName()))
            : property;
    }

    /// <summary>
    ///     Gets all scalar properties declared on this type.
    /// </summary>
    /// <remarks>
    ///     This method does not return properties declared on base types.
    ///     It is useful when iterating over all types to avoid processing the same property more than once.
    ///     Use <see cref="GetProperties" /> to also return properties declared on base types.
    /// </remarks>
    /// <returns>Declared scalar properties.</returns>
    IEnumerable<IReadOnlyProperty> GetDeclaredProperties();

    /// <summary>
    ///     Gets all scalar properties declared on the types derived from this type.
    /// </summary>
    /// <remarks>
    ///     This method does not return properties declared on the given type itself.
    ///     Use <see cref="GetProperties" /> to return properties declared on this
    ///     and base types.
    /// </remarks>
    /// <returns>Derived scalar properties.</returns>
    IEnumerable<IReadOnlyProperty> GetDerivedProperties();

    /// <summary>
    ///     Gets all scalar properties defined on this type.
    /// </summary>
    /// <remarks>
    ///     This API only returns scalar properties and does not return navigation, complex or service properties.
    /// </remarks>
    /// <returns>The properties defined on this type.</returns>
    IEnumerable<IReadOnlyProperty> GetProperties();

    /// <summary>
    ///     Gets the complex property with the given name. Returns <see langword="null" /> if no property with the given name is defined.
    /// </summary>
    /// <remarks>
    ///     This API only finds complex properties and does not find navigation, scalar or service properties.
    /// </remarks>
    /// <param name="name">The name of the property.</param>
    /// <returns>The property, or <see langword="null" /> if none is found.</returns>
    IReadOnlyComplexProperty? FindComplexProperty(string name);

    /// <summary>
    ///     Gets a complex property with the given member info. Returns <see langword="null" /> if no property is found.
    /// </summary>
    /// <remarks>
    ///     This API only finds complex properties and does not find navigation, scalar or service properties.
    /// </remarks>
    /// <param name="memberInfo">The member on the class.</param>
    /// <returns>The property, or <see langword="null" /> if none is found.</returns>
    IReadOnlyComplexProperty? FindComplexProperty(MemberInfo memberInfo)
        => (Check.NotNull(memberInfo) as PropertyInfo)?.IsIndexerProperty() == true
            ? null
            : FindComplexProperty(memberInfo.GetSimpleMemberName());

    /// <summary>
    ///     Finds a property declared on the type with the given name.
    ///     Does not return properties defined on a base type.
    /// </summary>
    /// <param name="name">The property name.</param>
    /// <returns>The property, or <see langword="null" /> if none is found.</returns>
    IReadOnlyComplexProperty? FindDeclaredComplexProperty(string name);

    /// <summary>
    ///     Gets the complex properties defined on this type and base types.
    /// </summary>
    /// <remarks>
    ///     This API only returns complex properties and does not find navigation, scalar or service properties.
    /// </remarks>
    /// <returns>The complex properties defined on this type.</returns>
    IEnumerable<IReadOnlyComplexProperty> GetComplexProperties();

    /// <summary>
    ///     Gets the complex properties declared on this type.
    /// </summary>
    /// <returns>Declared complex properties.</returns>
    IEnumerable<IReadOnlyComplexProperty> GetDeclaredComplexProperties();

    /// <summary>
    ///     Gets the complex properties declared on the types derived from this type.
    /// </summary>
    /// <remarks>
    ///     This method does not return complex properties declared on the given type itself.
    ///     Use <see cref="GetComplexProperties" /> to return complex properties declared on this
    ///     and base typed types.
    /// </remarks>
    /// <returns>Derived complex properties.</returns>
    IEnumerable<IReadOnlyComplexProperty> GetDerivedComplexProperties();

    /// <summary>
    ///     Gets the members defined on this type and base types.
    /// </summary>
    /// <returns>Type members.</returns>
    IEnumerable<IReadOnlyPropertyBase> GetMembers();

    /// <summary>
    ///     Gets the members declared on this type.
    /// </summary>
    /// <returns>Declared members.</returns>
    IEnumerable<IReadOnlyPropertyBase> GetDeclaredMembers();

    /// <summary>
    ///     Gets the member with the given name. Returns <see langword="null" /> if no member with the given name is defined.
    /// </summary>
    /// <remarks>
    ///     This API only finds scalar properties and does not find navigation, complex or service properties.
    /// </remarks>
    /// <param name="name">The name of the property.</param>
    /// <returns>The property, or <see langword="null" /> if none is found.</returns>
    IReadOnlyPropertyBase? FindMember(string name);

    /// <summary>
    ///     Gets the members with the given name on this type, base types or derived types.
    /// </summary>
    /// <returns>Type members.</returns>
    IEnumerable<IReadOnlyPropertyBase> FindMembersInHierarchy(string name);

    /// <summary>
    ///     Gets the change tracking strategy being used for this type. This strategy indicates how the
    ///     context detects changes to properties for an instance of the type.
    /// </summary>
    /// <returns>The change tracking strategy.</returns>
    ChangeTrackingStrategy GetChangeTrackingStrategy();

    /// <summary>
    ///     Gets the <see cref="PropertyAccessMode" /> being used for properties and navigations of this type.
    /// </summary>
    /// <remarks>
    ///     Note that individual properties and navigations can override this access mode. The value returned here will
    ///     be used for any property or navigation for which no override has been specified.
    /// </remarks>
    /// <returns>The access mode being used.</returns>
    PropertyAccessMode GetPropertyAccessMode();

    /// <summary>
    ///     Returns the <see cref="PropertyInfo" /> for the indexer on the associated CLR type if one exists.
    /// </summary>
    /// <returns>The <see cref="PropertyInfo" /> for the indexer on the associated CLR type if one exists.</returns>
    PropertyInfo? FindIndexerPropertyInfo();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    Func<MaterializationContext, object> GetOrCreateMaterializer(IStructuralTypeMaterializerSource source);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    Func<MaterializationContext, object> GetOrCreateEmptyMaterializer(IStructuralTypeMaterializerSource source);
}
