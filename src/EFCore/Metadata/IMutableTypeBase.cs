// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Represents a type in an <see cref="IMutableModel" />.
/// </summary>
/// <remarks>
///     <para>
///         This interface is used during model creation and allows the metadata to be modified.
///         Once the model is built, <see cref="IReadOnlyTypeBase" /> represents a read-only view of the same metadata.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and
///         examples.
///     </para>
/// </remarks>
public interface IMutableTypeBase : IReadOnlyTypeBase, IMutableAnnotatable
{
    /// <summary>
    ///     Gets the model that this type belongs to.
    /// </summary>
    new IMutableModel Model { get; }

    /// <summary>
    ///     Gets this entity type or the one on which the complex property chain is declared.
    /// </summary>
    new IMutableEntityType ContainingEntityType
        => (IMutableEntityType)this;

    /// <summary>
    ///     Marks the given member name as ignored, preventing conventions from adding a matching property
    ///     or navigation to the type.
    /// </summary>
    /// <param name="memberName">The name of the member to be ignored.</param>
    /// <returns>The name of the ignored member.</returns>
    string? AddIgnored(string memberName);

    /// <summary>
    ///     Removes the ignored member name.
    /// </summary>
    /// <param name="memberName">The name of the member to be removed.</param>
    /// <returns>The removed ignored member name, or <see langword="null" /> if the member name was not found.</returns>
    string? RemoveIgnored(string memberName);

    /// <summary>
    ///     Indicates whether the given member name is ignored.
    /// </summary>
    /// <param name="memberName">The name of the member that might be ignored.</param>
    /// <returns><see langword="true" /> if the given member name is ignored.</returns>
    bool IsIgnored(string memberName);

    /// <summary>
    ///     Gets all the ignored members.
    /// </summary>
    /// <returns>The list of ignored member names.</returns>
    IEnumerable<string> GetIgnoredMembers();

    /// <summary>
    ///     Adds a property to this type.
    /// </summary>
    /// <param name="memberInfo">The corresponding member on the CLR type.</param>
    /// <returns>The newly created property.</returns>
    [RequiresUnreferencedCode("Currently used only in tests")]
    IMutableProperty AddProperty(MemberInfo memberInfo)
        => AddProperty(memberInfo.GetSimpleMemberName(), memberInfo.GetMemberType(), memberInfo);

    /// <summary>
    ///     Adds a property to this type.
    /// </summary>
    /// <param name="name">The name of the property to add.</param>
    /// <returns>The newly created property.</returns>
    IMutableProperty AddProperty(string name);

    /// <summary>
    ///     Adds a property to this type.
    /// </summary>
    /// <param name="name">The name of the property to add.</param>
    /// <param name="propertyType">The type of value the property will hold.</param>
    /// <returns>The newly created property.</returns>
    IMutableProperty AddProperty(string name, [DynamicallyAccessedMembers(IProperty.DynamicallyAccessedMemberTypes)] Type propertyType);

    /// <summary>
    ///     Adds a property to this type.
    /// </summary>
    /// <param name="name">The name of the property to add.</param>
    /// <param name="propertyType">The type of value the property will hold.</param>
    /// <param name="memberInfo">
    ///     <para>
    ///         The corresponding CLR type member.
    ///     </para>
    ///     <para>
    ///         An indexer with a <see cref="string" /> parameter and <see cref="object" /> return type can be used.
    ///     </para>
    /// </param>
    /// <returns>The newly created property.</returns>
    IMutableProperty AddProperty(
        string name,
        [DynamicallyAccessedMembers(IProperty.DynamicallyAccessedMemberTypes)] Type propertyType,
        MemberInfo memberInfo);

    /// <summary>
    ///     Adds a property backed up by an indexer to this type.
    /// </summary>
    /// <param name="name">The name of the property to add.</param>
    /// <param name="propertyType">The type of value the property will hold.</param>
    /// <returns>The newly created property.</returns>
    IMutableProperty AddIndexerProperty(
        string name,
        [DynamicallyAccessedMembers(IProperty.DynamicallyAccessedMemberTypes)] Type propertyType)
    {
        var indexerPropertyInfo = FindIndexerPropertyInfo();
        if (indexerPropertyInfo == null)
        {
            throw new InvalidOperationException(
                CoreStrings.NonIndexerEntityType(name, DisplayName(), typeof(string).ShortDisplayName()));
        }

        return AddProperty(name, propertyType, indexerPropertyInfo);
    }

    /// <summary>
    ///     Gets a property on the given type. Returns <see langword="null" /> if no property is found.
    /// </summary>
    /// <remarks>
    ///     This API only finds scalar properties and does not find navigation, complex or service properties.
    /// </remarks>
    /// <param name="memberInfo">The property on the class.</param>
    /// <returns>The property, or <see langword="null" /> if none is found.</returns>
    new IMutableProperty? FindProperty(MemberInfo memberInfo)
        => (IMutableProperty?)((IReadOnlyTypeBase)this).FindProperty(memberInfo);

    /// <summary>
    ///     Gets the property with a given name. Returns <see langword="null" /> if no property with the given name is defined.
    /// </summary>
    /// <remarks>
    ///     This API only finds scalar properties and does not find navigation, complex or service properties.
    ///     a navigation property.
    /// </remarks>
    /// <param name="name">The name of the property.</param>
    /// <returns>The property, or <see langword="null" /> if none is found.</returns>
    new IMutableProperty? FindProperty(string name);

    /// <summary>
    ///     Finds matching properties on this type. Returns <see langword="null" /> if any property is not found.
    /// </summary>
    /// <remarks>
    ///     This API only finds scalar properties and does not find navigation, complex or service properties.
    /// </remarks>
    /// <param name="propertyNames">The property names.</param>
    /// <returns>The properties, or <see langword="null" /> if any property is not found.</returns>
    new IReadOnlyList<IMutableProperty>? FindProperties(IReadOnlyList<string> propertyNames)
        => (IReadOnlyList<IMutableProperty>?)((IReadOnlyTypeBase)this).FindProperties(propertyNames);

    /// <summary>
    ///     Finds a property declared on the type with the given name.
    ///     Does not return properties defined on a base type.
    /// </summary>
    /// <param name="name">The property name.</param>
    /// <returns>The property, or <see langword="null" /> if none is found.</returns>
    new IMutableProperty? FindDeclaredProperty(string name)
        => (IMutableProperty?)((IReadOnlyTypeBase)this).FindDeclaredProperty(name);

    /// <summary>
    ///     Gets a property with the given name.
    /// </summary>
    /// <remarks>
    ///     This API only finds scalar properties and does not find navigation, complex or service properties.
    /// </remarks>
    /// <param name="name">The property name.</param>
    /// <returns>The property.</returns>
    new IMutableProperty GetProperty(string name)
        => (IMutableProperty)((IReadOnlyTypeBase)this).GetProperty(name);

    /// <summary>
    ///     Gets all scalar properties declared on this type.
    /// </summary>
    /// <remarks>
    ///     This method does not return properties declared on base types.
    ///     It is useful when iterating over all types to avoid processing the same property more than once.
    ///     Use <see cref="GetProperties" /> to also return properties declared on base types.
    /// </remarks>
    /// <returns>Declared scalar properties.</returns>
    new IEnumerable<IMutableProperty> GetDeclaredProperties();

    /// <summary>
    ///     Gets all scalar properties declared on the types derived from this type.
    /// </summary>
    /// <remarks>
    ///     This method does not return properties declared on the given type itself.
    ///     Use <see cref="GetProperties" /> to return properties declared on this
    ///     and base typed types.
    /// </remarks>
    /// <returns>Derived scalar properties.</returns>
    new IEnumerable<IMutableProperty> GetDerivedProperties()
        => ((IReadOnlyTypeBase)this).GetDerivedProperties().Cast<IMutableProperty>();

    /// <summary>
    ///     Gets all scalar properties defined on this  type.
    /// </summary>
    /// <remarks>
    ///     This API only returns scalar properties and does not return navigation, complex or service properties.
    ///     properties.
    /// </remarks>
    /// <returns>The properties defined on this type.</returns>
    new IEnumerable<IMutableProperty> GetProperties();

    /// <summary>
    ///     Removes a property from this type.
    /// </summary>
    /// <param name="name">The name of the property to remove.</param>
    /// <returns>The removed property, or <see langword="null" /> if the property was not found.</returns>
    IMutableProperty? RemoveProperty(string name);

    /// <summary>
    ///     Removes a property from this type.
    /// </summary>
    /// <param name="property">The property to remove.</param>
    /// <returns>The removed property, or <see langword="null" /> if the property was not found.</returns>
    IMutableProperty? RemoveProperty(IReadOnlyProperty property);

    /// <summary>
    ///     Adds a complex property to this type.
    /// </summary>
    /// <param name="memberInfo">The corresponding member on the class.</param>
    /// <param name="complexTypeName">The name of the complex type.</param>
    /// <param name="collection">Indicates whether the property represents a collection.</param>
    /// <returns>The newly created property.</returns>
    [RequiresUnreferencedCode("Currently used only in tests")]
    IMutableComplexProperty AddComplexProperty(MemberInfo memberInfo, string? complexTypeName = null, bool collection = false)
        => AddComplexProperty(
            memberInfo.GetSimpleMemberName(), memberInfo.GetMemberType(),
            collection ? memberInfo.GetMemberType().GetSequenceType() : memberInfo.GetMemberType(), complexTypeName, collection);

    /// <summary>
    ///     Adds a complex property to this type.
    /// </summary>
    /// <param name="name">The name of the property to add.</param>
    /// <param name="collection">Indicates whether the property represents a collection.</param>
    /// <returns>The newly created property.</returns>
    IMutableComplexProperty AddComplexProperty(string name, bool collection = false);

    /// <summary>
    ///     Adds a complex property to this type.
    /// </summary>
    /// <param name="name">The name of the property to add.</param>
    /// <param name="propertyType">The property type.</param>
    /// <param name="complexType">The type of value the property will hold.</param>
    /// <param name="complexTypeName">The name of the complex type.</param>
    /// <param name="collection">Indicates whether the property represents a collection.</param>
    /// <returns>The newly created property.</returns>
    IMutableComplexProperty AddComplexProperty(
        string name,
        [DynamicallyAccessedMembers(IProperty.DynamicallyAccessedMemberTypes)] Type propertyType,
        [DynamicallyAccessedMembers(IProperty.DynamicallyAccessedMemberTypes)] Type complexType,
        string? complexTypeName = null,
        bool collection = false);

    /// <summary>
    ///     Adds a complex property to this type.
    /// </summary>
    /// <param name="name">The name of the property to add.</param>
    /// <param name="propertyType">The property type.</param>
    /// <param name="memberInfo">
    ///     <para>
    ///         The corresponding CLR type member.
    ///     </para>
    ///     <para>
    ///         An indexer with a <see cref="string" /> parameter and <see cref="object" /> return type can be used.
    ///     </para>
    /// </param>
    /// <param name="complexType">The type of value the property will hold.</param>
    /// <param name="complexTypeName">The name of the complex type.</param>
    /// <param name="collection">Indicates whether the property represents a collection.</param>
    /// <returns>The newly created property.</returns>
    IMutableComplexProperty AddComplexProperty(
        string name,
        [DynamicallyAccessedMembers(IProperty.DynamicallyAccessedMemberTypes)] Type propertyType,
        MemberInfo memberInfo,
        [DynamicallyAccessedMembers(IProperty.DynamicallyAccessedMemberTypes)] Type complexType,
        string? complexTypeName = null,
        bool collection = false);

    /// <summary>
    ///     Adds a complex property backed up by an indexer to this type.
    /// </summary>
    /// <param name="name">The name of the property to add.</param>
    /// <param name="propertyType">The property type.</param>
    /// <param name="complexType">The type of value the property will hold.</param>
    /// <param name="complexTypeName">The name of the complex type.</param>
    /// <param name="collection">Indicates whether the property represents a collection.</param>
    /// <returns>The newly created property.</returns>
    IMutableComplexProperty AddComplexIndexerProperty(
        string name,
        [DynamicallyAccessedMembers(IProperty.DynamicallyAccessedMemberTypes)] Type propertyType,
        [DynamicallyAccessedMembers(IProperty.DynamicallyAccessedMemberTypes)] Type complexType,
        string? complexTypeName = null,
        bool collection = false)
    {
        var indexerPropertyInfo = FindIndexerPropertyInfo();
        if (indexerPropertyInfo == null)
        {
            throw new InvalidOperationException(
                CoreStrings.NonIndexerEntityType(name, DisplayName(), typeof(string).ShortDisplayName()));
        }

        return AddComplexProperty(name, propertyType, indexerPropertyInfo, complexType, complexTypeName, collection);
    }

    /// <summary>
    ///     Gets a complex property on the given type. Returns <see langword="null" /> if no property is found.
    /// </summary>
    /// <remarks>
    ///     This API only finds complex properties and does not find navigation, scalar or service properties.
    /// </remarks>
    /// <param name="memberInfo">The member on the CLR type.</param>
    /// <returns>The property, or <see langword="null" /> if none is found.</returns>
    new IMutableComplexProperty? FindComplexProperty(MemberInfo memberInfo)
        => (IMutableComplexProperty?)((IReadOnlyEntityType)this).FindComplexProperty(memberInfo);

    /// <summary>
    ///     Gets the complex property with a given name. Returns <see langword="null" /> if no property with the given name is defined.
    /// </summary>
    /// <remarks>
    ///     This API only finds complex properties and does not find navigation, scalar or service properties.
    /// </remarks>
    /// <param name="name">The name of the property.</param>
    /// <returns>The property, or <see langword="null" /> if none is found.</returns>
    new IMutableComplexProperty? FindComplexProperty(string name);

    /// <summary>
    ///     Finds a complex property declared on the type with the given name.
    ///     Does not return properties defined on a base type.
    /// </summary>
    /// <remarks>
    ///     This API only finds complex properties and does not find navigation, scalar or service properties.
    /// </remarks>
    /// <param name="name">The property name.</param>
    /// <returns>The property, or <see langword="null" /> if none is found.</returns>
    new IMutableComplexProperty? FindDeclaredComplexProperty(string name)
        => (IMutableComplexProperty?)((IReadOnlyEntityType)this).FindDeclaredComplexProperty(name);

    /// <summary>
    ///     Gets all complex properties declared on this type.
    /// </summary>
    /// <remarks>
    ///     This method does not return properties declared on base types.
    ///     It is useful when iterating over all types to avoid processing the same property more than once.
    ///     Use <see cref="GetComplexProperties" /> to also return properties declared on base types.
    /// </remarks>
    /// <returns>Declared complex properties.</returns>
    new IEnumerable<IMutableComplexProperty> GetDeclaredComplexProperties();

    /// <summary>
    ///     Gets all complex properties declared on the types derived from this type.
    /// </summary>
    /// <remarks>
    ///     This method does not return properties declared on the given type itself.
    ///     Use <see cref="GetComplexProperties" /> to return properties declared on this
    ///     and base typed types.
    /// </remarks>
    /// <returns>Derived complex properties.</returns>
    new IEnumerable<IMutableComplexProperty> GetDerivedComplexProperties()
        => ((IReadOnlyEntityType)this).GetDerivedComplexProperties().Cast<IMutableComplexProperty>();

    /// <summary>
    ///     Gets the properties defined on this type.
    /// </summary>
    /// <remarks>
    ///     This API only returns complex properties and does not find navigation, scalar or service properties.
    /// </remarks>
    /// <returns>The properties defined on this type.</returns>
    new IEnumerable<IMutableComplexProperty> GetComplexProperties();

    /// <summary>
    ///     Removes a property from this type.
    /// </summary>
    /// <param name="name">The name of the property to remove.</param>
    /// <returns>The removed property, or <see langword="null" /> if the property was not found.</returns>
    IMutableComplexProperty? RemoveComplexProperty(string name);

    /// <summary>
    ///     Removes a property from this type.
    /// </summary>
    /// <param name="property">The property to remove.</param>
    /// <returns>The removed property, or <see langword="null" /> if the property was not found.</returns>
    IMutableComplexProperty? RemoveComplexProperty(IReadOnlyProperty property);

    /// <summary>
    ///     Gets the members defined on this type and base types.
    /// </summary>
    /// <returns>Type members.</returns>
    new IEnumerable<IMutablePropertyBase> GetMembers();

    /// <summary>
    ///     Gets the members declared on this type.
    /// </summary>
    /// <returns>Declared members.</returns>
    new IEnumerable<IMutablePropertyBase> GetDeclaredMembers();

    /// <summary>
    ///     Gets the member with the given name. Returns <see langword="null" /> if no member with the given name is defined.
    /// </summary>
    /// <remarks>
    ///     This API only finds scalar properties and does not find navigation, complex or service properties.
    /// </remarks>
    /// <param name="name">The name of the property.</param>
    /// <returns>The property, or <see langword="null" /> if none is found.</returns>
    new IMutablePropertyBase? FindMember(string name);

    /// <summary>
    ///     Gets the members with the given name on this type, base types or derived types..
    /// </summary>
    /// <returns>Type members.</returns>
    new IEnumerable<IMutablePropertyBase> FindMembersInHierarchy(string name);

    /// <summary>
    ///     Sets the change tracking strategy to use for this type. This strategy indicates how the
    ///     context detects changes to properties for an instance of the type.
    /// </summary>
    /// <param name="changeTrackingStrategy">The strategy to use.</param>
    void SetChangeTrackingStrategy(ChangeTrackingStrategy? changeTrackingStrategy);

    /// <summary>
    ///     Sets the <see cref="PropertyAccessMode" /> to use for properties and navigations of this type.
    /// </summary>
    /// <remarks>
    ///     Note that individual properties and navigations can override this access mode. The value set here will
    ///     be used for any property or navigation for which no override has been specified.
    /// </remarks>
    /// <param name="propertyAccessMode">The <see cref="PropertyAccessMode" />, or <see langword="null" /> to clear the mode set.</param>
    void SetPropertyAccessMode(PropertyAccessMode? propertyAccessMode)
        => SetOrRemoveAnnotation(CoreAnnotationNames.PropertyAccessMode, propertyAccessMode);
}
