// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Represents a type in an <see cref="IConventionModel" />.
/// </summary>
/// <remarks>
///     <para>
///         This interface is used during model creation and allows the metadata to be modified.
///         Once the model is built, <see cref="IReadOnlyTypeBase" /> represents a read-only view of the same metadata.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see> for more information and examples.
///     </para>
/// </remarks>
public interface IConventionTypeBase : IReadOnlyTypeBase, IConventionAnnotatable
{
    /// <summary>
    ///     Gets the model that this type belongs to.
    /// </summary>
    new IConventionModel Model { get; }

    /// <summary>
    ///     Gets the builder that can be used to configure this type.
    /// </summary>
    /// <exception cref="InvalidOperationException">If the type has been removed from the model.</exception>
    new IConventionTypeBaseBuilder Builder { get; }

    /// <summary>
    ///     Gets this entity type or the one on which the complex property chain is declared.
    /// </summary>
    new IConventionEntityType ContainingEntityType
        => (IConventionEntityType)this;

    /// <summary>
    ///     Marks the given member name as ignored, preventing conventions from adding a matching property
    ///     or navigation to the type.
    /// </summary>
    /// <param name="memberName">The name of the member to be ignored.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The ignored member name.</returns>
    string? AddIgnored(string memberName, bool fromDataAnnotation = false);

    /// <summary>
    ///     Removes the ignored member name.
    /// </summary>
    /// <param name="memberName">The name of the member to be removed.</param>
    /// <returns>The removed ignored member name.</returns>
    string? RemoveIgnored(string memberName);

    /// <summary>
    ///     Indicates whether the given member name is ignored.
    /// </summary>
    /// <param name="memberName">The name of the member to be ignored.</param>
    /// <returns>
    ///     The configuration source if the given member name is ignored,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    ConfigurationSource? FindIgnoredConfigurationSource(string memberName);

    /// <summary>
    ///     Gets all the ignored members.
    /// </summary>
    /// <returns>The list of ignored member names.</returns>
    IEnumerable<string> GetIgnoredMembers();

    /// <summary>
    ///     Indicates whether the given member name is ignored.
    /// </summary>
    /// <param name="memberName">The name of the member that might be ignored.</param>
    /// <returns><see langword="true" /> if the given member name is ignored.</returns>
    bool IsIgnored(string memberName)
        => FindIgnoredConfigurationSource(memberName) != null;

    /// <summary>
    ///     Adds a property to this entity type.
    /// </summary>
    /// <param name="memberInfo">The corresponding member on the entity class.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The newly created property.</returns>
    [RequiresUnreferencedCode("Currently used only in tests")]
    IConventionProperty? AddProperty(MemberInfo memberInfo, bool fromDataAnnotation = false)
        => AddProperty(
            memberInfo.GetSimpleMemberName(), memberInfo.GetMemberType(),
            memberInfo, setTypeConfigurationSource: true, fromDataAnnotation);

    /// <summary>
    ///     Adds a property to this entity type.
    /// </summary>
    /// <param name="name">The name of the property to add.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The newly created property.</returns>
    IConventionProperty? AddProperty(string name, bool fromDataAnnotation = false);

    /// <summary>
    ///     Adds a property to this entity type.
    /// </summary>
    /// <param name="name">The name of the property to add.</param>
    /// <param name="propertyType">The type of value the property will hold.</param>
    /// <param name="setTypeConfigurationSource">Indicates whether the type configuration source should be set.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The newly created property.</returns>
    IConventionProperty? AddProperty(
        string name,
        [DynamicallyAccessedMembers(IProperty.DynamicallyAccessedMemberTypes)] Type propertyType,
        bool setTypeConfigurationSource = true,
        bool fromDataAnnotation = false);

    /// <summary>
    ///     Adds a property to this entity type.
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
    /// <param name="setTypeConfigurationSource">Indicates whether the type configuration source should be set.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The newly created property.</returns>
    IConventionProperty? AddProperty(
        string name,
        [DynamicallyAccessedMembers(IProperty.DynamicallyAccessedMemberTypes)] Type propertyType,
        MemberInfo memberInfo,
        bool setTypeConfigurationSource = true,
        bool fromDataAnnotation = false);

    /// <summary>
    ///     Adds a property backed by and indexer to this entity type.
    /// </summary>
    /// <param name="name">The name of the property to add.</param>
    /// <param name="propertyType">The type of value the property will hold.</param>
    /// <param name="setTypeConfigurationSource">Indicates whether the type configuration source should be set.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The newly created property.</returns>
    IConventionProperty? AddIndexerProperty(
        string name,
        [DynamicallyAccessedMembers(IProperty.DynamicallyAccessedMemberTypes)] Type propertyType,
        bool setTypeConfigurationSource = true,
        bool fromDataAnnotation = false)
    {
        var indexerPropertyInfo = FindIndexerPropertyInfo();
        if (indexerPropertyInfo == null)
        {
            throw new InvalidOperationException(
                CoreStrings.NonIndexerEntityType(name, DisplayName(), typeof(string).ShortDisplayName()));
        }

        return AddProperty(name, propertyType, indexerPropertyInfo, setTypeConfigurationSource, fromDataAnnotation);
    }

    /// <summary>
    ///     Gets the property with a given name. Returns <see langword="null" /> if no property with the given name is defined.
    /// </summary>
    /// <remarks>
    ///     This API only finds scalar properties and does not find navigation, complex or service properties.
    /// </remarks>
    /// <param name="name">The name of the property.</param>
    /// <returns>The property, or <see langword="null" /> if none is found.</returns>
    new IConventionProperty? FindProperty(string name);

    /// <summary>
    ///     Gets a property on the given entity type. Returns <see langword="null" /> if no property is found.
    /// </summary>
    /// <remarks>
    ///     This API only finds scalar properties and does not find navigation, complex or service properties.
    /// </remarks>
    /// <param name="memberInfo">The property on the entity class.</param>
    /// <returns>The property, or <see langword="null" /> if none is found.</returns>
    new IConventionProperty? FindProperty(MemberInfo memberInfo)
        => (IConventionProperty?)((IReadOnlyTypeBase)this).FindProperty(memberInfo);

    /// <summary>
    ///     Finds matching properties on the given entity type. Returns <see langword="null" /> if any property is not found.
    /// </summary>
    /// <remarks>
    ///     This API only finds scalar properties and does not find navigation, complex or service properties.
    /// </remarks>
    /// <param name="propertyNames">The property names.</param>
    /// <returns>The properties, or <see langword="null" /> if any property is not found.</returns>
    new IReadOnlyList<IConventionProperty>? FindProperties(IReadOnlyList<string> propertyNames)
        => (IReadOnlyList<IConventionProperty>?)((IReadOnlyTypeBase)this).FindProperties(propertyNames);

    /// <summary>
    ///     Gets a property with the given name.
    /// </summary>
    /// <remarks>
    ///     This API only finds scalar properties and does not find navigation properties.
    /// </remarks>
    /// <param name="name">The property name.</param>
    /// <returns>The property.</returns>
    new IConventionProperty GetProperty(string name)
        => (IConventionProperty)((IReadOnlyTypeBase)this).GetProperty(name);

    /// <summary>
    ///     Finds a property declared on the type with the given name.
    ///     Does not return properties defined on a base type.
    /// </summary>
    /// <param name="name">The property name.</param>
    /// <returns>The property, or <see langword="null" /> if none is found.</returns>
    new IConventionProperty? FindDeclaredProperty(string name)
        => (IConventionProperty?)((IReadOnlyTypeBase)this).FindDeclaredProperty(name);

    /// <summary>
    ///     Gets all scalar properties declared on this type.
    /// </summary>
    /// <remarks>
    ///     This method does not return properties declared on base types.
    ///     It is useful when iterating over all entity types to avoid processing the same property more than once.
    ///     Use <see cref="GetProperties" /> to also return properties declared on base types.
    /// </remarks>
    /// <returns>Declared non-navigation properties.</returns>
    new IEnumerable<IConventionProperty> GetDeclaredProperties();

    /// <summary>
    ///     Gets all scalar properties declared on the types derived from this type.
    /// </summary>
    /// <remarks>
    ///     This method does not return properties declared on the given type itself.
    ///     Use <see cref="GetProperties" /> to return properties declared on this
    ///     and base entity typed types.
    /// </remarks>
    /// <returns>Derived non-navigation properties.</returns>
    new IEnumerable<IConventionProperty> GetDerivedProperties()
        => ((IReadOnlyTypeBase)this).GetDerivedProperties().Cast<IConventionProperty>();

    /// <summary>
    ///     Gets all scalar properties defined on this type.
    /// </summary>
    /// <remarks>
    ///     This API only returns scalar properties and does not return navigation, complex or service properties.
    /// </remarks>
    /// <returns>The properties defined on this type.</returns>
    new IEnumerable<IConventionProperty> GetProperties();

    /// <summary>
    ///     Removes a property from this type.
    /// </summary>
    /// <param name="name">The name of the property to remove.</param>
    /// <returns>The property that was removed.</returns>
    IConventionProperty? RemoveProperty(string name);

    /// <summary>
    ///     Removes a property from this type.
    /// </summary>
    /// <param name="property">The property to remove.</param>
    /// <returns>The removed property, or <see langword="null" /> if the property was not found.</returns>
    IConventionProperty? RemoveProperty(IReadOnlyProperty property);

    /// <summary>
    ///     Adds a property to this type.
    /// </summary>
    /// <param name="memberInfo">The corresponding member on the type.</param>
    /// <param name="complexTypeName">The name of the complex type.</param>
    /// <param name="collection">Indicates whether the property represents a collection.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The newly created property.</returns>
    [RequiresUnreferencedCode("Currently used only in tests")]
    IConventionComplexProperty? AddComplexProperty(
        MemberInfo memberInfo,
        string? complexTypeName = null,
        bool collection = false,
        bool fromDataAnnotation = false)
        => AddComplexProperty(
            memberInfo.GetSimpleMemberName(), memberInfo.GetMemberType(),
            memberInfo, memberInfo.GetMemberType(), complexTypeName, collection, fromDataAnnotation);

    /// <summary>
    ///     Adds a property to this type.
    /// </summary>
    /// <param name="name">The name of the property to add.</param>
    /// <param name="collection">Indicates whether the property represents a collection.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The newly created property.</returns>
    IConventionComplexProperty? AddComplexProperty(string name, bool collection = false, bool fromDataAnnotation = false);

    /// <summary>
    ///     Adds a property to this type.
    /// </summary>
    /// <param name="name">The name of the property to add.</param>
    /// <param name="propertyType">The property type.</param>
    /// <param name="complexType">The type of value the property will hold.</param>
    /// <param name="complexTypeName">The name of the complex type.</param>
    /// <param name="collection">Indicates whether the property represents a collection.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The newly created property.</returns>
    IConventionComplexProperty? AddComplexProperty(
        string name,
        [DynamicallyAccessedMembers(IProperty.DynamicallyAccessedMemberTypes)] Type propertyType,
        [DynamicallyAccessedMembers(IProperty.DynamicallyAccessedMemberTypes)] Type complexType,
        string? complexTypeName = null,
        bool collection = false,
        bool fromDataAnnotation = false);

    /// <summary>
    ///     Adds a property to this type.
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
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The newly created property.</returns>
    IConventionComplexProperty? AddComplexProperty(
        string name,
        [DynamicallyAccessedMembers(IProperty.DynamicallyAccessedMemberTypes)] Type propertyType,
        MemberInfo memberInfo,
        [DynamicallyAccessedMembers(IProperty.DynamicallyAccessedMemberTypes)] Type complexType,
        string? complexTypeName = null,
        bool collection = false,
        bool fromDataAnnotation = false);

    /// <summary>
    ///     Adds a property backed by and indexer to this type.
    /// </summary>
    /// <param name="name">The name of the property to add.</param>
    /// <param name="propertyType">The property type.</param>
    /// <param name="complexType">The type of value the property will hold.</param>
    /// <param name="complexTypeName">The name of the complex type.</param>
    /// <param name="collection">Indicates whether the property represents a collection.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The newly created property.</returns>
    IConventionComplexProperty? AddComplexIndexerProperty(
        string name,
        [DynamicallyAccessedMembers(IProperty.DynamicallyAccessedMemberTypes)] Type propertyType,
        [DynamicallyAccessedMembers(IProperty.DynamicallyAccessedMemberTypes)] Type complexType,
        string? complexTypeName = null,
        bool collection = false,
        bool fromDataAnnotation = false)
    {
        var indexerPropertyInfo = FindIndexerPropertyInfo();
        if (indexerPropertyInfo == null)
        {
            throw new InvalidOperationException(
                CoreStrings.NonIndexerEntityType(name, DisplayName(), typeof(string).ShortDisplayName()));
        }

        return AddComplexProperty(name, propertyType, indexerPropertyInfo, complexType, complexTypeName, fromDataAnnotation);
    }

    /// <summary>
    ///     Gets the complex property with a given name. Returns <see langword="null" /> if no property with the given name is defined.
    /// </summary>
    /// <remarks>
    ///     This API only finds complex properties and does not find navigation, scalar or service properties.
    /// </remarks>
    /// <param name="name">The name of the property.</param>
    /// <returns>The property, or <see langword="null" /> if none is found.</returns>
    new IConventionComplexProperty? FindComplexProperty(string name);

    /// <summary>
    ///     Gets a complex property with the given member info. Returns <see langword="null" /> if no property is found.
    /// </summary>
    /// <remarks>
    ///     This API only finds complex properties and does not find navigation, scalar or service properties.
    /// </remarks>
    /// <param name="memberInfo">The member on the entity class.</param>
    /// <returns>The property, or <see langword="null" /> if none is found.</returns>
    new IConventionComplexProperty? FindComplexProperty(MemberInfo memberInfo)
        => (IConventionComplexProperty?)((IReadOnlyEntityType)this).FindComplexProperty(memberInfo);

    /// <summary>
    ///     Finds a property declared on the type with the given name.
    ///     Does not return properties defined on a base type.
    /// </summary>
    /// <remarks>
    ///     This API only finds complex properties and does not find navigation, scalar or service properties.
    /// </remarks>
    /// <param name="name">The property name.</param>
    /// <returns>The property, or <see langword="null" /> if none is found.</returns>
    new IConventionComplexProperty? FindDeclaredComplexProperty(string name)
        => (IConventionComplexProperty?)((IReadOnlyEntityType)this).FindDeclaredComplexProperty(name);

    /// <summary>
    ///     Gets the complex properties defined on this type.
    /// </summary>
    /// <remarks>
    ///     This API only returns complex properties and does not find navigation, scalar or service properties.
    /// </remarks>
    /// <returns>The complex properties defined on this type.</returns>
    new IEnumerable<IConventionComplexProperty> GetComplexProperties();

    /// <summary>
    ///     Gets the complex properties declared on this type.
    /// </summary>
    /// <returns>Declared complex properties.</returns>
    new IEnumerable<IConventionComplexProperty> GetDeclaredComplexProperties();

    /// <summary>
    ///     Gets the complex properties declared on the types derived from this type.
    /// </summary>
    /// <remarks>
    ///     This method does not return complex properties declared on the given type itself.
    ///     Use <see cref="GetComplexProperties" /> to return complex properties declared on this
    ///     and base typed types.
    /// </remarks>
    /// <returns>Derived complex properties.</returns>
    new IEnumerable<IConventionComplexProperty> GetDerivedComplexProperties()
        => ((IReadOnlyEntityType)this).GetDerivedComplexProperties().Cast<IConventionComplexProperty>();

    /// <summary>
    ///     Removes a property from this type.
    /// </summary>
    /// <param name="name">The name of the property to remove.</param>
    /// <returns>The property that was removed.</returns>
    IConventionComplexProperty? RemoveComplexProperty(string name);

    /// <summary>
    ///     Removes a property from this type.
    /// </summary>
    /// <param name="property">The property to remove.</param>
    /// <returns>The removed property, or <see langword="null" /> if the property was not found.</returns>
    IConventionComplexProperty? RemoveComplexProperty(IConventionComplexProperty property);

    /// <summary>
    ///     Gets the members defined on this type and base types.
    /// </summary>
    /// <returns>Type members.</returns>
    new IEnumerable<IConventionPropertyBase> GetMembers();

    /// <summary>
    ///     Gets the members declared on this type.
    /// </summary>
    /// <returns>Declared members.</returns>
    new IEnumerable<IConventionPropertyBase> GetDeclaredMembers();

    /// <summary>
    ///     Gets the member with the given name. Returns <see langword="null" /> if no member with the given name is defined.
    /// </summary>
    /// <remarks>
    ///     This API only finds scalar properties and does not find navigation, complex or service properties.
    /// </remarks>
    /// <param name="name">The name of the property.</param>
    /// <returns>The property, or <see langword="null" /> if none is found.</returns>
    new IConventionPropertyBase? FindMember(string name);

    /// <summary>
    ///     Gets the members with the given name on this type, base types or derived types..
    /// </summary>
    /// <returns>Type members.</returns>
    new IEnumerable<IConventionPropertyBase> FindMembersInHierarchy(string name);

    /// <summary>
    ///     Sets the change tracking strategy to use for this type. This strategy indicates how the
    ///     context detects changes to properties for an instance of the type.
    /// </summary>
    /// <param name="changeTrackingStrategy">The strategy to use.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The configured value.</returns>
    ChangeTrackingStrategy? SetChangeTrackingStrategy(ChangeTrackingStrategy? changeTrackingStrategy, bool fromDataAnnotation = false);

    /// <summary>
    ///     Returns the configuration source for <see cref="IReadOnlyTypeBase.GetChangeTrackingStrategy" />.
    /// </summary>
    /// <returns>The configuration source for <see cref="IReadOnlyTypeBase.GetChangeTrackingStrategy" />.</returns>
    ConfigurationSource? GetChangeTrackingStrategyConfigurationSource();

    /// <summary>
    ///     Sets the <see cref="PropertyAccessMode" /> to use for properties of this type.
    /// </summary>
    /// <remarks>
    ///     Note that individual properties and navigations can override this access mode. The value set here will
    ///     be used for any property or navigation for which no override has been specified.
    /// </remarks>
    /// <param name="propertyAccessMode">The <see cref="PropertyAccessMode" />, or <see langword="null" /> to clear the mode set.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The configured value.</returns>
    PropertyAccessMode? SetPropertyAccessMode(PropertyAccessMode? propertyAccessMode, bool fromDataAnnotation = false)
        => (PropertyAccessMode?)SetOrRemoveAnnotation(CoreAnnotationNames.PropertyAccessMode, propertyAccessMode, fromDataAnnotation)
            ?.Value;

    /// <summary>
    ///     Returns the configuration source for <see cref="IReadOnlyTypeBase.GetPropertyAccessMode" />.
    /// </summary>
    /// <returns>The configuration source for <see cref="IReadOnlyTypeBase.GetPropertyAccessMode" />.</returns>
    ConfigurationSource? GetPropertyAccessModeConfigurationSource()
        => FindAnnotation(CoreAnnotationNames.PropertyAccessMode)?.GetConfigurationSource();
}
