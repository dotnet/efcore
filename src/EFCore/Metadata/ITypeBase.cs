// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Represents a type in the model.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
/// </remarks>
public interface ITypeBase : IReadOnlyTypeBase, IAnnotatable
{
    /// <summary>
    ///     Gets the model that this type belongs to.
    /// </summary>
    new IModel Model { get; }

    /// <summary>
    ///     Gets this entity type or the one on which the complex property chain is declared.
    /// </summary>
    new IEntityType ContainingEntityType
        => (IEntityType)this;

    /// <summary>
    ///     Gets the <see cref="InstantiationBinding" /> for the preferred constructor.
    /// </summary>
    InstantiationBinding? ConstructorBinding { get; }

    /// <summary>
    ///     Gets a property on the given type. Returns <see langword="null" /> if no property is found.
    /// </summary>
    /// <remarks>
    ///     This API only finds scalar properties and does not find navigation, complex or service properties.
    /// </remarks>
    /// <param name="memberInfo">The member on the CLR type.</param>
    /// <returns>The property, or <see langword="null" /> if none is found.</returns>
    new IProperty? FindProperty(MemberInfo memberInfo)
        => (IProperty?)((IReadOnlyTypeBase)this).FindProperty(memberInfo);

    /// <summary>
    ///     Gets the property with a given name. Returns <see langword="null" /> if no property with the given name is defined.
    /// </summary>
    /// <remarks>
    ///     This API only finds scalar properties and does not find navigation, complex or service properties.
    /// </remarks>
    /// <param name="name">The name of the property.</param>
    /// <returns>The property, or <see langword="null" /> if none is found.</returns>
    new IProperty? FindProperty(string name);

    /// <summary>
    ///     Finds matching properties on the given type. Returns <see langword="null" /> if any property is not found.
    /// </summary>
    /// <remarks>
    ///     This API only finds scalar properties and does not find navigation, complex or service properties.
    /// </remarks>
    /// <param name="propertyNames">The property names.</param>
    /// <returns>The properties, or <see langword="null" /> if any property is not found.</returns>
    new IReadOnlyList<IProperty>? FindProperties(
        IReadOnlyList<string> propertyNames)
        => (IReadOnlyList<IProperty>?)((IReadOnlyTypeBase)this).FindProperties(propertyNames);

    /// <summary>
    ///     Gets a property with the given name.
    /// </summary>
    /// <remarks>
    ///     This API only finds scalar properties and does not find navigation, complex or service properties.
    /// </remarks>
    /// <param name="name">The property name.</param>
    /// <returns>The property.</returns>
    new IProperty GetProperty(string name)
        => (IProperty)((IReadOnlyTypeBase)this).GetProperty(name);

    /// <summary>
    ///     Finds a property declared on the type with the given name.
    ///     Does not return properties defined on a base type.
    /// </summary>
    /// <param name="name">The property name.</param>
    /// <returns>The property, or <see langword="null" /> if none is found.</returns>
    new IProperty? FindDeclaredProperty(string name)
        => (IProperty?)((IReadOnlyTypeBase)this).FindDeclaredProperty(name);

    /// <summary>
    ///     Gets all non-navigation properties declared on this type.
    /// </summary>
    /// <remarks>
    ///     This method does not return properties declared on base types.
    ///     It is useful when iterating over all types to avoid processing the same property more than once.
    ///     Use <see cref="GetProperties" /> to also return properties declared on base types.
    /// </remarks>
    /// <returns>Declared non-navigation properties.</returns>
    new IEnumerable<IProperty> GetDeclaredProperties();

    /// <summary>
    ///     Gets all non-navigation properties declared on the types derived from this type.
    /// </summary>
    /// <remarks>
    ///     This method does not return properties declared on the given type itself.
    ///     Use <see cref="GetProperties" /> to return properties declared on this
    ///     and base typed types.
    /// </remarks>
    /// <returns>Derived non-navigation properties.</returns>
    new IEnumerable<IProperty> GetDerivedProperties()
        => ((IReadOnlyTypeBase)this).GetDerivedProperties().Cast<IProperty>();

    /// <summary>
    ///     Gets the properties defined on this type.
    /// </summary>
    /// <remarks>
    ///     This API only returns scalar properties and does not return navigation, complex or service properties.
    /// </remarks>
    /// <returns>The properties defined on this type.</returns>
    new IEnumerable<IProperty> GetProperties();

    /// <summary>
    ///     Gets the complex property with a given name. Returns <see langword="null" /> if no property with the given name is defined.
    /// </summary>
    /// <remarks>
    ///     This API only finds complex properties and does not find navigation, scalar or service properties.
    /// </remarks>
    /// <param name="name">The name of the property.</param>
    /// <returns>The property, or <see langword="null" /> if none is found.</returns>
    new IComplexProperty? FindComplexProperty(string name);

    /// <summary>
    ///     Gets a complex property with the given member info. Returns <see langword="null" /> if no property is found.
    /// </summary>
    /// <remarks>
    ///     This API only finds complex properties and does not find navigation, scalar or service properties.
    /// </remarks>
    /// <param name="memberInfo">The member on the entity class.</param>
    /// <returns>The property, or <see langword="null" /> if none is found.</returns>
    new IComplexProperty? FindComplexProperty(MemberInfo memberInfo)
        => (IComplexProperty?)((IReadOnlyTypeBase)this).FindComplexProperty(memberInfo);

    /// <summary>
    ///     Finds a property declared on the type with the given name.
    ///     Does not return properties defined on a base type.
    /// </summary>
    /// <remarks>
    ///     This API only finds complex properties and does not find navigation, scalar or service properties.
    /// </remarks>
    /// <param name="name">The property name.</param>
    /// <returns>The property, or <see langword="null" /> if none is found.</returns>
    new IComplexProperty? FindDeclaredComplexProperty(string name)
        => (IComplexProperty?)((IReadOnlyTypeBase)this).FindDeclaredComplexProperty(name);

    /// <summary>
    ///     Gets the complex properties defined on this entity type.
    /// </summary>
    /// <remarks>
    ///     This API only returns complex properties and does not find navigation, scalar or service properties.
    /// </remarks>
    /// <returns>The complex properties defined on this entity type.</returns>
    new IEnumerable<IComplexProperty> GetComplexProperties();

    /// <summary>
    ///     Gets the complex properties declared on this entity type.
    /// </summary>
    /// <returns>Declared complex properties.</returns>
    new IEnumerable<IComplexProperty> GetDeclaredComplexProperties();

    /// <summary>
    ///     Gets the complex properties declared on the types derived from this entity type.
    /// </summary>
    /// <remarks>
    ///     This method does not return complex properties declared on the given entity type itself.
    ///     Use <see cref="GetComplexProperties" /> to return complex properties declared on this
    ///     and base entity typed types.
    /// </remarks>
    /// <returns>Derived complex properties.</returns>
    new IEnumerable<IComplexProperty> GetDerivedComplexProperties()
        => ((IReadOnlyTypeBase)this).GetDerivedComplexProperties().Cast<IComplexProperty>();

    /// <summary>
    ///     Gets the members defined on this type and base types.
    /// </summary>
    /// <returns>Type members.</returns>
    new IEnumerable<IPropertyBase> GetMembers();

    /// <summary>
    ///     Gets the members declared on this type.
    /// </summary>
    /// <returns>Declared members.</returns>
    new IEnumerable<IPropertyBase> GetDeclaredMembers();

    /// <summary>
    ///     Gets the member with the given name. Returns <see langword="null" /> if no member with the given name is defined.
    /// </summary>
    /// <remarks>
    ///     This API only finds scalar properties and does not find navigation, complex or service properties.
    /// </remarks>
    /// <param name="name">The name of the property.</param>
    /// <returns>The property, or <see langword="null" /> if none is found.</returns>
    new IPropertyBase? FindMember(string name);

    /// <summary>
    ///     Gets the members with the given name on this type, base types or derived types..
    /// </summary>
    /// <returns>Type members.</returns>
    new IEnumerable<IPropertyBase> FindMembersInHierarchy(string name);

    /// <summary>
    ///     Returns all members that may need a snapshot value when change tracking.
    /// </summary>
    /// <returns>The members.</returns>
    IEnumerable<IPropertyBase> GetSnapshottableMembers();

    /// <summary>
    ///     Gets all properties declared on the base types and types derived from this entity type.
    /// </summary>
    /// <returns>The properties.</returns>
    IEnumerable<IProperty> GetPropertiesInHierarchy()
        => throw new NotSupportedException();

    /// <summary>
    ///     Returns all properties that implement <see cref="IProperty" />, including those on complex types.
    /// </summary>
    /// <returns>The properties.</returns>
    IEnumerable<IProperty> GetFlattenedProperties();

    /// <summary>
    ///     Returns all properties that implement <see cref="IComplexProperty" />, including those on complex types.
    /// </summary>
    /// <returns>The properties.</returns>
    IEnumerable<IComplexProperty> GetFlattenedComplexProperties();

    /// <summary>
    ///     Returns all declared properties that implement <see cref="IProperty" />, including those on complex types.
    /// </summary>
    /// <returns>The properties.</returns>
    IEnumerable<IProperty> GetFlattenedDeclaredProperties();

    /// <summary>
    ///     Gets all properties declared on the base types and types derived from this entity type, including those on complex types.
    /// </summary>
    /// <returns>The properties.</returns>
    IEnumerable<IProperty> GetFlattenedPropertiesInHierarchy()
        => throw new NotSupportedException();
}
