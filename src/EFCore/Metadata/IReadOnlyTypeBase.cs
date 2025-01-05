// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;

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
            return ClrType.ShortDisplayName();
        }

        var shortName = Name;
        var hashIndex = shortName.IndexOf("#", StringComparison.Ordinal);
        if (hashIndex == -1)
        {
            return Name + " (" + ClrType.ShortDisplayName() + ")";
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
            ? shortName + " (" + ClrType.ShortDisplayName() + ")"
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
                name = name[2..];
            }

            var lessIndex = name.IndexOf("<", StringComparison.Ordinal);
            if (lessIndex == -1)
            {
                return name;
            }

            return name[..lessIndex];
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
    ///     Determines if this type derives from (or is the same as) a given type.
    /// </summary>
    /// <param name="derivedType">The type to check whether it derives from this type.</param>
    /// <returns>
    ///     <see langword="true" /> if <paramref name="derivedType" /> derives from (or is the same as) this type,
    ///     otherwise <see langword="false" />.
    /// </returns>
    bool IsAssignableFrom(IReadOnlyTypeBase derivedType)
        => this == derivedType;

    /// <summary>
    ///     Determines if this type derives from (but is not the same as) a given type.
    /// </summary>
    /// <param name="baseType">The type to check if it is a base type of this type.</param>
    /// <returns>
    ///     <see langword="true" /> if this type derives from (but is not the same as) <paramref name="baseType" />,
    ///     otherwise <see langword="false" />.
    /// </returns>
    bool IsStrictlyDerivedFrom(IReadOnlyTypeBase baseType)
        => this != Check.NotNull(baseType, nameof(baseType)) && baseType.IsAssignableFrom(this);

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
        => (Check.NotNull(memberInfo, nameof(memberInfo)) as PropertyInfo)?.IsIndexerProperty() == true
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
        Check.NotEmpty(name, nameof(name));

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
        => (Check.NotNull(memberInfo, nameof(memberInfo)) as PropertyInfo)?.IsIndexerProperty() == true
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
}
