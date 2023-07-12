// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>
///     <para>
///         Provides a simple API surface for configuring an <see cref="IConventionEntityType" /> from conventions.
///     </para>
///     <para>
///         This interface is typically used by database providers (and other extensions). It is generally
///         not used in application code.
///     </para>
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see> for more information and examples.
/// </remarks>
public interface IConventionEntityTypeBuilder : IConventionTypeBaseBuilder
{
    /// <summary>
    ///     Gets the entity type being configured.
    /// </summary>
    new IConventionEntityType Metadata { get; }

    /// <summary>
    ///     Sets the annotation stored under the given name. Overwrites the existing annotation if an
    ///     annotation with the specified name already exists with same or lower <see cref="ConfigurationSource" />.
    /// </summary>
    /// <param name="name">The name of the annotation to be set.</param>
    /// <param name="value">The value to be stored in the annotation.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     An <see cref="IConventionEntityTypeBuilder" /> to continue configuration if the annotation was set, <see langword="null" /> otherwise.
    /// </returns>
    new IConventionEntityTypeBuilder? HasAnnotation(string name, object? value, bool fromDataAnnotation = false);

    /// <summary>
    ///     Sets the annotation stored under the given name. Overwrites the existing annotation if an
    ///     annotation with the specified name already exists with same or lower <see cref="ConfigurationSource" />.
    ///     Removes the annotation if <see langword="null" /> value is specified.
    /// </summary>
    /// <param name="name">The name of the annotation to be set.</param>
    /// <param name="value">The value to be stored in the annotation. <see langword="null" /> to remove the annotations.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     An <see cref="IConventionEntityTypeBuilder" /> to continue configuration if the annotation was set or removed,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    new IConventionEntityTypeBuilder? HasNonNullAnnotation(
        string name,
        object? value,
        bool fromDataAnnotation = false);

    /// <summary>
    ///     Removes the annotation with the given name from this object.
    /// </summary>
    /// <param name="name">The name of the annotation to remove.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     An <see cref="IConventionEntityTypeBuilder" /> to continue configuration if the annotation was set, <see langword="null" /> otherwise.
    /// </returns>
    new IConventionEntityTypeBuilder? HasNoAnnotation(string name, bool fromDataAnnotation = false);

    /// <summary>
    ///     Sets the base type of this entity type in an inheritance hierarchy.
    /// </summary>
    /// <param name="baseEntityType">The base entity type or <see langword="null" /> to indicate no base type.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     The same builder instance if the base type was configured,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    IConventionEntityTypeBuilder? HasBaseType(
        IConventionEntityType? baseEntityType,
        bool fromDataAnnotation = false);

    /// <summary>
    ///     Returns a value indicating whether the given type can be set as the base type of this entity type.
    /// </summary>
    /// <param name="baseEntityType">The base entity type or <see langword="null" /> to indicate no base type.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the given type can be set as the base type of this entity type.</returns>
    bool CanSetBaseType(IConventionEntityType? baseEntityType, bool fromDataAnnotation = false);

    /// <summary>
    ///     Removes properties in the given list if they are not part of any metadata object.
    /// </summary>
    /// <param name="properties">The properties to remove.</param>
    new IConventionEntityTypeBuilder RemoveUnusedImplicitProperties(IReadOnlyList<IConventionProperty> properties);

    /// <summary>
    ///     Removes a property from this entity type.
    /// </summary>
    /// <param name="property">The property to be removed.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     The same builder instance if the property was removed,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    new IConventionEntityTypeBuilder? HasNoProperty(IConventionProperty property, bool fromDataAnnotation = false);

    /// <summary>
    ///     Removes a complex property from this entity type.
    /// </summary>
    /// <param name="complexProperty">The complex property to be removed.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     The same builder instance if the complex property was removed,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    new IConventionEntityTypeBuilder? HasNoComplexProperty(IConventionComplexProperty complexProperty, bool fromDataAnnotation = false);

    /// <summary>
    ///     Returns an object that can be used to configure the service property with the given member info.
    ///     If no matching property exists, then a new property will be added.
    /// </summary>
    /// <param name="memberInfo">The <see cref="PropertyInfo" /> or <see cref="FieldInfo" /> of the property.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     An object that can be used to configure the property if it exists on the entity type,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    IConventionServicePropertyBuilder? ServiceProperty(
        MemberInfo memberInfo,
        bool fromDataAnnotation = false);

    /// <summary>
    ///     Returns an object that can be used to configure the service property with the given member info.
    ///     If no matching property exists, then a new property will be added.
    /// </summary>
    /// <param name="serviceType">The type of the service.</param>
    /// <param name="memberInfo">The <see cref="PropertyInfo" /> or <see cref="FieldInfo" /> of the property.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     An object that can be used to configure the property if it exists on the entity type,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    IConventionServicePropertyBuilder? ServiceProperty(
        Type serviceType,
        MemberInfo memberInfo,
        bool fromDataAnnotation = false);

    /// <summary>
    ///     Returns a value indicating whether the given service property can be added to this entity type.
    /// </summary>
    /// <param name="memberInfo">The <see cref="PropertyInfo" /> or <see cref="FieldInfo" /> of the property.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the service property can be added.</returns>
    bool CanHaveServiceProperty(MemberInfo memberInfo, bool fromDataAnnotation = false);

    /// <summary>
    ///     Removes a service property from this entity type.
    /// </summary>
    /// <param name="serviceProperty">The service property to be removed.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     The same builder instance if the service property was removed,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    IConventionEntityTypeBuilder? HasNoServiceProperty(IConventionServiceProperty serviceProperty, bool fromDataAnnotation = false);

    /// <summary>
    ///     Returns a value indicating whether the service property can be removed from this entity type.
    /// </summary>
    /// <param name="serviceProperty">The service property to be removed.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the service property can be removed from this entity type.</returns>
    bool CanRemoveServiceProperty(IConventionServiceProperty serviceProperty, bool fromDataAnnotation = false);

    /// <summary>
    ///     Excludes the given property from the entity type and prevents conventions from adding a matching property
    ///     or navigation to the type.
    /// </summary>
    /// <param name="memberName">The name of the member to be removed.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     The same builder instance so that additional configuration calls can be chained
    ///     if the given member was ignored, <see langword="null" /> otherwise.
    /// </returns>
    new IConventionEntityTypeBuilder? Ignore(string memberName, bool fromDataAnnotation = false);

    /// <summary>
    ///     Sets the properties that make up the primary key for this entity type.
    /// </summary>
    /// <param name="properties">The properties that make up the primary key.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>An object that can be used to configure the primary key.</returns>
    /// <returns>
    ///     An object that can be used to configure the primary key if it was set on the entity type,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    IConventionKeyBuilder? PrimaryKey(IReadOnlyList<IConventionProperty>? properties, bool fromDataAnnotation = false);

    /// <summary>
    ///     Sets the properties that make up the primary key for this entity type.
    /// </summary>
    /// <param name="propertyNames">The names of the properties that make up the primary key.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>An object that can be used to configure the primary key.</returns>
    /// <returns>
    ///     An object that can be used to configure the primary key if it was set on the entity type,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    IConventionKeyBuilder? PrimaryKey(IReadOnlyList<string>? propertyNames, bool fromDataAnnotation = false);

    /// <summary>
    ///     Returns a value indicating whether the given properties can be set as the primary key for this entity type.
    /// </summary>
    /// <param name="propertyNames">The names of the properties that make up the index.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the given properties can be set as the primary key.</returns>
    bool CanSetPrimaryKey(IReadOnlyList<string> propertyNames, bool fromDataAnnotation = false);

    /// <summary>
    ///     Returns a value indicating whether the given properties can be set as the primary key for this entity type.
    /// </summary>
    /// <param name="properties">The properties that make up the primary key.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the given properties can be set as the primary key.</returns>
    bool CanSetPrimaryKey(IReadOnlyList<IConventionProperty>? properties, bool fromDataAnnotation = false);

    /// <summary>
    ///     Creates an alternate key in the model for this entity type if one does not already exist over the specified
    ///     properties.
    /// </summary>
    /// <param name="properties">The properties that make up the key.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     An object that can be used to configure the key if it exists on the entity type,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    IConventionKeyBuilder? HasKey(IReadOnlyList<IConventionProperty> properties, bool fromDataAnnotation = false);

    /// <summary>
    ///     Removes a primary or alternate key from this entity type.
    /// </summary>
    /// <param name="properties">The properties that make up the key.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The key that was removed.</returns>
    IConventionEntityTypeBuilder? HasNoKey(IReadOnlyList<IConventionProperty> properties, bool fromDataAnnotation = false);

    /// <summary>
    ///     Removes a primary or alternate key from this entity type.
    /// </summary>
    /// <param name="key">The key to be removed.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     The same builder instance if the key was removed,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    IConventionEntityTypeBuilder? HasNoKey(IConventionKey key, bool fromDataAnnotation = false);

    /// <summary>
    ///     Returns a value indicating whether the key can be removed from this entity type.
    /// </summary>
    /// <param name="key">The key to be removed.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the key can be removed from this entity type.</returns>
    bool CanRemoveKey(IConventionKey key, bool fromDataAnnotation = false);

    /// <summary>
    ///     Configures the entity type to have no keys. It will only be usable for queries.
    /// </summary>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     The same builder instance if the entity type was configured as keyless,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    IConventionEntityTypeBuilder? HasNoKey(bool fromDataAnnotation = false);

    /// <summary>
    ///     Returns a value indicating whether the entity type can be marked as keyless.
    /// </summary>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the entity type can be marked as keyless.</returns>
    bool CanRemoveKey(bool fromDataAnnotation = false);

    /// <summary>
    ///     Configures an index on the specified property names.
    ///     If there is an existing index on the given list of property names,
    ///     then the existing index will be returned for configuration.
    /// </summary>
    /// <param name="propertyNames">The names of the properties that make up the index.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     An object that can be used to configure the index if it exists on the entity type,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    IConventionIndexBuilder? HasIndex(
        IReadOnlyList<string> propertyNames,
        bool fromDataAnnotation = false);

    /// <summary>
    ///     Configures an index on the specified property names.
    ///     If there is an existing index on the given list of property names,
    ///     then the existing index will be returned for configuration.
    /// </summary>
    /// <param name="propertyNames">The names of the properties that make up the index.</param>
    /// <param name="name">The name of the index.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     An object that can be used to configure the index if it exists on the entity type,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    IConventionIndexBuilder? HasIndex(
        IReadOnlyList<string> propertyNames,
        string name,
        bool fromDataAnnotation = false);

    /// <summary>
    ///     Configures an index on the specified properties.
    ///     If there is an existing index on the given list of properties,
    ///     then the existing index will be returned for configuration.
    /// </summary>
    /// <param name="properties">The properties that make up the index.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     An object that can be used to configure the index if it exists on the entity type,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    IConventionIndexBuilder? HasIndex(
        IReadOnlyList<IConventionProperty> properties,
        bool fromDataAnnotation = false);

    /// <summary>
    ///     Configures an index on the specified properties, with the specified name.
    ///     If there is an existing index on the given set of properties and with the given name,
    ///     then the existing index will be returned for configuration.
    /// </summary>
    /// <param name="properties">The properties that make up the index.</param>
    /// <param name="name">The name of the index.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     An object that can be used to configure the index if it exists on the entity type,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    IConventionIndexBuilder? HasIndex(
        IReadOnlyList<IConventionProperty> properties,
        string name,
        bool fromDataAnnotation = false);

    /// <summary>
    ///     Returns a value indicating whether and index on the given properties can be added to this entity type.
    /// </summary>
    /// <param name="propertyNames">The names of the properties that make up the index.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the index can be added.</returns>
    bool CanHaveIndex(IReadOnlyList<string> propertyNames, bool fromDataAnnotation = false);

    /// <summary>
    ///     Removes an index from this entity type.
    /// </summary>
    /// <param name="properties">The properties that make up the index.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     The same builder instance if the index was removed or didn't exist,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    IConventionEntityTypeBuilder? HasNoIndex(IReadOnlyList<IConventionProperty> properties, bool fromDataAnnotation = false);

    /// <summary>
    ///     Removes an index from this entity type.
    /// </summary>
    /// <param name="index">The index to remove.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     The same builder instance if the index was removed,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    IConventionEntityTypeBuilder? HasNoIndex(IConventionIndex index, bool fromDataAnnotation = false);

    /// <summary>
    ///     Returns a value indicating whether the index can be removed from this entity type.
    /// </summary>
    /// <param name="index">The index to remove.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the entity type can be marked as keyless.</returns>
    bool CanRemoveIndex(IConventionIndex index, bool fromDataAnnotation = false);

    /// <summary>
    ///     Configures a relationship between this and the target entity type.
    /// </summary>
    /// <param name="targetEntityType">The entity type that this relationship targets.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>An object that can be used to configure the relationship.</returns>
    IConventionForeignKeyBuilder? HasRelationship(
        IConventionEntityType targetEntityType,
        bool fromDataAnnotation = false);

    /// <summary>
    ///     Configures a relationship between this and the target entity type with the target as the principal end.
    /// </summary>
    /// <param name="principalEntityType">The entity type that this relationship targets.</param>
    /// <param name="dependentProperties">The properties on this type that make up the foreign key.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     An object that can be used to configure the relationship if it exists on the entity type,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    IConventionForeignKeyBuilder? HasRelationship(
        IConventionEntityType principalEntityType,
        IReadOnlyList<IConventionProperty> dependentProperties,
        bool fromDataAnnotation = false);

    /// <summary>
    ///     Configures a relationship between this and the target entity type with the target as the principal end.
    /// </summary>
    /// <param name="principalEntityType">The entity type that this relationship targets.</param>
    /// <param name="principalKey">The referenced key.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     An object that can be used to configure the relationship if it exists on the entity type,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    IConventionForeignKeyBuilder? HasRelationship(
        IConventionEntityType principalEntityType,
        IConventionKey principalKey,
        bool fromDataAnnotation = false);

    /// <summary>
    ///     Configures a relationship between this and the target entity type with the target as the principal end.
    /// </summary>
    /// <param name="principalEntityType">The entity type that this relationship targets.</param>
    /// <param name="dependentProperties">The properties on this type that make up the foreign key</param>
    /// <param name="principalKey">The referenced key.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     An object that can be used to configure the relationship if it exists on the entity type,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    IConventionForeignKeyBuilder? HasRelationship(
        IConventionEntityType principalEntityType,
        IReadOnlyList<IConventionProperty> dependentProperties,
        IConventionKey principalKey,
        bool fromDataAnnotation = false);

    /// <summary>
    ///     Configures a relationship between this and the target entity type.
    /// </summary>
    /// <param name="targetEntityType">The entity type that this relationship targets.</param>
    /// <param name="navigationName">
    ///     The name of the navigation property on this entity type that is part of the relationship.
    /// </param>
    /// <param name="setTargetAsPrincipal">A value indicating whether the target entity type should be configured as the principal end.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     An object that can be used to configure the relationship if it exists on the entity type,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    IConventionForeignKeyBuilder? HasRelationship(
        IConventionEntityType targetEntityType,
        string navigationName,
        bool setTargetAsPrincipal = false,
        bool fromDataAnnotation = false);

    /// <summary>
    ///     Configures a relationship between this and the target entity type.
    /// </summary>
    /// <param name="targetEntityType">The entity type that this relationship targets.</param>
    /// <param name="navigation">The navigation property on this entity type that is part of the relationship.</param>
    /// <param name="setTargetAsPrincipal">A value indicating whether the target entity type should be configured as the principal end.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     An object that can be used to configure the relationship if it exists on the entity type,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    IConventionForeignKeyBuilder? HasRelationship(
        IConventionEntityType targetEntityType,
        MemberInfo navigation,
        bool setTargetAsPrincipal = false,
        bool fromDataAnnotation = false);

    /// <summary>
    ///     Configures a relationship between this and the target entity type.
    /// </summary>
    /// <param name="targetEntityType">The entity type that this relationship targets.</param>
    /// <param name="navigationName">The name of the navigation property on this entity type that is part of the relationship.</param>
    /// <param name="inverseNavigationName">
    ///     The name of the navigation property on the target entity type that is part of the relationship. If <see langword="null" />
    ///     is specified, the relationship will be configured without a navigation property on the target end.
    /// </param>
    /// <param name="setTargetAsPrincipal">A value indicating whether the target entity type should be configured as the principal end.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     An object that can be used to configure the relationship if it exists on the entity type,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    IConventionForeignKeyBuilder? HasRelationship(
        IConventionEntityType targetEntityType,
        string navigationName,
        string? inverseNavigationName,
        bool setTargetAsPrincipal = false,
        bool fromDataAnnotation = false);

    /// <summary>
    ///     Configures a relationship between this and the target entity type.
    /// </summary>
    /// <param name="targetEntityType">The entity type that this relationship targets.</param>
    /// <param name="navigation">The navigation property on this entity type that is part of the relationship.</param>
    /// <param name="inverseNavigation">
    ///     The navigation property on the target entity type that is part of the relationship. If <see langword="null" />
    ///     is specified, the relationship will be configured without a navigation property on the target end.
    /// </param>
    /// <param name="setTargetAsPrincipal">A value indicating whether the target entity type should be configured as the principal end.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     An object that can be used to configure the relationship if it exists on the entity type,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    IConventionForeignKeyBuilder? HasRelationship(
        IConventionEntityType targetEntityType,
        MemberInfo navigation,
        MemberInfo? inverseNavigation,
        bool setTargetAsPrincipal = false,
        bool fromDataAnnotation = false);

    /// <summary>
    ///     Configures a relationship where the target entity is owned by (or part of) this entity.
    /// </summary>
    /// <param name="targetEntityType">The type that this relationship targets.</param>
    /// <param name="navigationName">The name of the navigation property on this entity type that is part of the relationship.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>An object that can be used to configure the relationship.</returns>
    IConventionForeignKeyBuilder? HasOwnership(
        Type targetEntityType,
        string navigationName,
        bool fromDataAnnotation = false);

    /// <summary>
    ///     Configures a relationship where the target entity is owned by (or part of) this entity.
    /// </summary>
    /// <param name="targetEntityType">The entity type that this relationship targets.</param>
    /// <param name="navigationName">The name of the navigation property on this entity type that is part of the relationship.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>An object that can be used to configure the relationship.</returns>
    IConventionForeignKeyBuilder? HasOwnership(
        IConventionEntityType targetEntityType,
        string navigationName,
        bool fromDataAnnotation = false);

    /// <summary>
    ///     Configures a relationship where the target entity is owned by (or part of) this entity.
    /// </summary>
    /// <param name="targetEntityType">The type that this relationship targets.</param>
    /// <param name="navigation">The navigation property on this entity type that is part of the relationship.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     An object that can be used to configure the relationship if it exists on the entity type,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    IConventionForeignKeyBuilder? HasOwnership(
        Type targetEntityType,
        MemberInfo navigation,
        bool fromDataAnnotation = false);

    /// <summary>
    ///     Configures a relationship where the target entity is owned by (or part of) this entity.
    /// </summary>
    /// <param name="targetEntityType">The entity type that this relationship targets.</param>
    /// <param name="navigation">The navigation property on this entity type that is part of the relationship.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     An object that can be used to configure the relationship if it exists on the entity type,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    IConventionForeignKeyBuilder? HasOwnership(
        IConventionEntityType targetEntityType,
        MemberInfo navigation,
        bool fromDataAnnotation = false);

    /// <summary>
    ///     Configures a relationship where the target entity is owned by (or part of) this entity.
    /// </summary>
    /// <param name="targetEntityType">The type that this relationship targets.</param>
    /// <param name="navigationName">The name of the navigation property on this entity type that is part of the relationship.</param>
    /// <param name="inverseNavigationName">
    ///     The name of the navigation property on the target entity type that is part of the relationship. If <see langword="null" />
    ///     is specified, the relationship will be configured without a navigation property on the target end.
    /// </param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     An object that can be used to configure the relationship if it exists on the entity type,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    IConventionForeignKeyBuilder? HasOwnership(
        Type targetEntityType,
        string navigationName,
        string? inverseNavigationName,
        bool fromDataAnnotation = false);

    /// <summary>
    ///     Configures a relationship where the target entity is owned by (or part of) this entity.
    /// </summary>
    /// <param name="targetEntityType">The entity type that this relationship targets.</param>
    /// <param name="navigationName">The name of the navigation property on this entity type that is part of the relationship.</param>
    /// <param name="inverseNavigationName">
    ///     The name of the navigation property on the target entity type that is part of the relationship. If <see langword="null" />
    ///     is specified, the relationship will be configured without a navigation property on the target end.
    /// </param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     An object that can be used to configure the relationship if it exists on the entity type,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    IConventionForeignKeyBuilder? HasOwnership(
        IConventionEntityType targetEntityType,
        string navigationName,
        string? inverseNavigationName,
        bool fromDataAnnotation = false);

    /// <summary>
    ///     Configures a relationship where the target entity is owned by (or part of) this entity.
    /// </summary>
    /// <param name="targetEntityType">The type that this relationship targets.</param>
    /// <param name="navigation">The navigation property on this entity type that is part of the relationship.</param>
    /// <param name="inverseNavigation">
    ///     The navigation property on the target entity type that is part of the relationship. If <see langword="null" />
    ///     is specified, the relationship will be configured without a navigation property on the target end.
    /// </param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     An object that can be used to configure the relationship if it exists on the entity type,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    IConventionForeignKeyBuilder? HasOwnership(
        Type targetEntityType,
        MemberInfo navigation,
        MemberInfo? inverseNavigation,
        bool fromDataAnnotation = false);

    /// <summary>
    ///     Configures a relationship where the target entity is owned by (or part of) this entity.
    /// </summary>
    /// <param name="targetEntityType">The entity type that this relationship targets.</param>
    /// <param name="navigation">The navigation property on this entity type that is part of the relationship.</param>
    /// <param name="inverseNavigation">
    ///     The navigation property on the target entity type that is part of the relationship. If <see langword="null" />
    ///     is specified, the relationship will be configured without a navigation property on the target end.
    /// </param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     An object that can be used to configure the relationship if it exists on the entity type,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    IConventionForeignKeyBuilder? HasOwnership(
        IConventionEntityType targetEntityType,
        MemberInfo navigation,
        MemberInfo? inverseNavigation,
        bool fromDataAnnotation = false);

    /// <summary>
    ///     Removes a relationship from this entity type.
    /// </summary>
    /// <param name="properties">The properties that the foreign key is defined on.</param>
    /// <param name="principalKey">The primary or alternate key that is referenced.</param>
    /// <param name="principalEntityType">
    ///     The entity type that the relationship targets. This may be different from the type that <paramref name="principalKey" />
    ///     is defined on when the relationship targets a derived type in an inheritance hierarchy (since the key is defined on the
    ///     base type of the hierarchy).
    /// </param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     The same builder instance if the relationship was removed or didn't exist,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    IConventionEntityTypeBuilder? HasNoRelationship(
        IReadOnlyList<IConventionProperty> properties,
        IConventionKey principalKey,
        IConventionEntityType principalEntityType,
        bool fromDataAnnotation = false);

    /// <summary>
    ///     Removes a foreign key from this entity type.
    /// </summary>
    /// <param name="foreignKey">The foreign key to be removed.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     The same builder instance if the foreign key was removed,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    IConventionEntityTypeBuilder? HasNoRelationship(IConventionForeignKey foreignKey, bool fromDataAnnotation = false);

    /// <summary>
    ///     Returns a value indicating whether the foreign key can be removed from this entity type.
    /// </summary>
    /// <param name="foreignKey">The foreign key to be removed.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the foreign key can be removed from this entity type.</returns>
    bool CanRemoveRelationship(IConventionForeignKey foreignKey, bool fromDataAnnotation = false);

    /// <summary>
    ///     Returns a value indicating whether the given navigation can be added to this entity type.
    /// </summary>
    /// <param name="navigationName">The name of the navigation.</param>
    /// <param name="type">The type of the navigation target.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the skip navigation can be added.</returns>
    bool CanHaveNavigation(string navigationName, Type? type, bool fromDataAnnotation = false);

    /// <summary>
    ///     Returns a value indicating whether the given navigation can be added to this entity type.
    /// </summary>
    /// <param name="navigation">The navigation member.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the configuration can be applied.</returns>
    bool CanHaveNavigation(MemberInfo navigation, bool fromDataAnnotation = false)
        => CanHaveNavigation(navigation.Name, navigation.GetMemberType(), fromDataAnnotation);

    /// <summary>
    ///     Removes a navigation from this entity type.
    /// </summary>
    /// <param name="navigation">The navigation to be removed.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     The same builder instance if the navigation was removed,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    IConventionEntityTypeBuilder? HasNoNavigation(IConventionNavigation navigation, bool fromDataAnnotation = false);

    /// <summary>
    ///     Returns a value indicating whether the navigation can be removed from this entity type.
    /// </summary>
    /// <param name="navigation">The navigation to be removed.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the navigation can be removed from this entity type.</returns>
    bool CanRemoveNavigation(IConventionNavigation navigation, bool fromDataAnnotation = false);

    /// <summary>
    ///     Returns a value indicating whether the given skip navigation can be added to this entity type.
    /// </summary>
    /// <param name="skipNavigationName">The name of the skip navigation.</param>
    /// <param name="type">The type of the navigation target.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the skip navigation can be added.</returns>
    bool CanHaveSkipNavigation(string skipNavigationName, Type? type, bool fromDataAnnotation = false);

    /// <summary>
    ///     Returns a value indicating whether the given skip navigation can be added to this entity type.
    /// </summary>
    /// <param name="navigation">The navigation member.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the skip navigation can be added.</returns>
    bool CanHaveSkipNavigation(MemberInfo navigation, bool fromDataAnnotation = false)
        => CanHaveSkipNavigation(navigation.Name, navigation.GetMemberType(), fromDataAnnotation);

    /// <summary>
    ///     Configures a skip navigation and the inverse between this and the target entity type.
    /// </summary>
    /// <param name="targetEntityType">The entity type that this relationship targets.</param>
    /// <param name="navigation">The navigation property on this entity type that is part of the relationship.</param>
    /// <param name="inverseNavigation">
    ///     The navigation property on the target entity type that is part of the relationship. If <see langword="null" />
    ///     is specified, the relationship will be configured without a navigation property on the target end.
    /// </param>
    /// <param name="collections">Whether both of the navigation properties are collections or aren't collections.</param>
    /// <param name="onDependent">
    ///     Whether both of the navigation property are defined on the dependent side of the underlying foreign keys.
    /// </param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     An object that can be used to configure the relationship if it exists on the entity type,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    IConventionSkipNavigationBuilder? HasSkipNavigation(
        MemberInfo navigation,
        IConventionEntityType targetEntityType,
        MemberInfo inverseNavigation,
        bool? collections = null,
        bool? onDependent = null,
        bool fromDataAnnotation = false);

    /// <summary>
    ///     Configures a skip navigation between this and the target entity type.
    /// </summary>
    /// <param name="navigation">The navigation property.</param>
    /// <param name="targetEntityType">The entity type that the navigation targets.</param>
    /// <param name="collection">Whether the navigation property is a collection property.</param>
    /// <param name="onDependent">
    ///     Whether the navigation property is defined on the dependent side of the underlying foreign key.
    /// </param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     An object that can be used to configure the relationship if it exists on the entity type,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    IConventionSkipNavigationBuilder? HasSkipNavigation(
        MemberInfo navigation,
        IConventionEntityType targetEntityType,
        bool? collection = null,
        bool? onDependent = null,
        bool fromDataAnnotation = false);

    /// <summary>
    ///     Configures a skip navigation between this and the target entity type.
    /// </summary>
    /// <param name="navigationName">The navigation property name.</param>
    /// <param name="targetEntityType">The entity type that the navigation targets.</param>
    /// <param name="navigationType">The navigation type.</param>
    /// <param name="collection">Whether the navigation property is a collection property.</param>
    /// <param name="onDependent">
    ///     Whether the navigation property is defined on the dependent side of the underlying foreign key.
    /// </param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     An object that can be used to configure the relationship if it exists on the entity type,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    IConventionSkipNavigationBuilder? HasSkipNavigation(
        string navigationName,
        IConventionEntityType targetEntityType,
        Type? navigationType = null,
        bool? collection = null,
        bool? onDependent = null,
        bool fromDataAnnotation = false);

    /// <summary>
    ///     Removes a skip navigation from this entity type.
    /// </summary>
    /// <param name="skipNavigation">The skip navigation to be removed.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     The same builder instance if the skip navigation was removed,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    IConventionEntityTypeBuilder? HasNoSkipNavigation(IConventionSkipNavigation skipNavigation, bool fromDataAnnotation = false);

    /// <summary>
    ///     Returns a value indicating whether the skip navigation can be removed from this entity type.
    /// </summary>
    /// <param name="skipNavigation">The skip navigation to be removed.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the skip navigation can be removed from this entity type.</returns>
    bool CanRemoveSkipNavigation(IConventionSkipNavigation skipNavigation, bool fromDataAnnotation = false);

    /// <summary>
    ///     Configures a database trigger when targeting a relational database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-triggers">Database triggers</see> for more information and examples.
    /// </remarks>
    /// <param name="modelName">The name of the trigger.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The same builder instance if the check constraint was configured, <see langword="null" /> otherwise.</returns>
    IConventionTriggerBuilder? HasTrigger(
        string modelName,
        bool fromDataAnnotation = false);

    /// <summary>
    ///     Returns a value indicating whether the trigger can be configured.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-triggers">Database triggers</see> for more information and examples.
    /// </remarks>
    /// <param name="modelName">The name of the trigger.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the configuration can be applied.</returns>
    bool CanHaveTrigger(
        string modelName,
        bool fromDataAnnotation = false);

    /// <summary>
    ///     Specifies a LINQ predicate expression that will automatically be applied to any queries targeting
    ///     this entity type.
    /// </summary>
    /// <param name="filter">The LINQ predicate expression.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     The same builder instance if the query filter was set,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    IConventionEntityTypeBuilder? HasQueryFilter(LambdaExpression? filter, bool fromDataAnnotation = false);

    /// <summary>
    ///     Returns a value indicating whether the given query filter can be set from the current configuration source.
    /// </summary>
    /// <param name="filter">The LINQ predicate expression.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the given query filter can be set.</returns>
    bool CanSetQueryFilter(LambdaExpression? filter, bool fromDataAnnotation = false);

    /// <summary>
    ///     Configures a query used to provide data for a keyless entity type.
    /// </summary>
    /// <param name="query">The query that will provide the underlying data for the keyless entity type.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     The same builder instance if the query was set, <see langword="null" /> otherwise.
    /// </returns>
    [Obsolete("Use InMemoryEntityTypeBuilderExtensions.ToInMemoryQuery")]
    IConventionEntityTypeBuilder? HasDefiningQuery(LambdaExpression? query, bool fromDataAnnotation = false);

    /// <summary>
    ///     Returns a value indicating whether the given defining query can be set from the current configuration source.
    /// </summary>
    /// <param name="query">The query that will provide the underlying data for the keyless entity type.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the given defining query can be set.</returns>
    [Obsolete("Use InMemoryEntityTypeBuilderExtensions.CanSetInMemoryQuery")]
    bool CanSetDefiningQuery(LambdaExpression? query, bool fromDataAnnotation = false);

    /// <summary>
    ///     Configures the <see cref="ChangeTrackingStrategy" /> to be used for this entity type.
    ///     This strategy indicates how the context detects changes to properties for an instance of the entity type.
    /// </summary>
    /// <param name="changeTrackingStrategy">
    ///     The change tracking strategy to be used.
    ///     <see langword="null" /> to reset to default.
    /// </param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     The same builder instance if the <see cref="ChangeTrackingStrategy" /> was set,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    new IConventionEntityTypeBuilder? HasChangeTrackingStrategy(
        ChangeTrackingStrategy? changeTrackingStrategy,
        bool fromDataAnnotation = false);

    /// <summary>
    ///     Sets the <see cref="PropertyAccessMode" /> to use for all properties of this entity type.
    /// </summary>
    /// <param name="propertyAccessMode">
    ///     The <see cref="PropertyAccessMode" /> to use for properties of this entity type.
    ///     <see langword="null" /> to reset to default.
    /// </param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    /// <returns>
    ///     The same builder instance if the <see cref="PropertyAccessMode" /> was set,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    new IConventionEntityTypeBuilder? UsePropertyAccessMode(
        PropertyAccessMode? propertyAccessMode,
        bool fromDataAnnotation = false);

    /// <summary>
    ///     Configures the discriminator property used to identify which entity type each row in a table represents
    ///     when an inheritance hierarchy is mapped to a single table in a relational database.
    /// </summary>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>A builder that allows the discriminator property to be configured.</returns>
    IConventionDiscriminatorBuilder? HasDiscriminator(bool fromDataAnnotation = false);

    /// <summary>
    ///     Configures the discriminator property used to identify which entity type each row in a table represents
    ///     when an inheritance hierarchy is mapped to a single table in a relational database.
    /// </summary>
    /// <param name="type">The type of values stored in the discriminator property.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>A builder that allows the discriminator property to be configured.</returns>
    IConventionDiscriminatorBuilder? HasDiscriminator(Type type, bool fromDataAnnotation = false);

    /// <summary>
    ///     Configures the discriminator property used to identify which entity type each row in a table represents
    ///     when an inheritance hierarchy is mapped to a single table in a relational database.
    /// </summary>
    /// <param name="name">The name of the discriminator property.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>A builder that allows the discriminator property to be configured.</returns>
    IConventionDiscriminatorBuilder? HasDiscriminator(string name, bool fromDataAnnotation = false);

    /// <summary>
    ///     Configures the discriminator property used to identify which entity type each row in a table represents
    ///     when an inheritance hierarchy is mapped to a single table in a relational database.
    /// </summary>
    /// <param name="name">The name of the discriminator property.</param>
    /// <param name="type">The type of values stored in the discriminator property.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>A builder that allows the discriminator property to be configured.</returns>
    IConventionDiscriminatorBuilder? HasDiscriminator(string name, Type type, bool fromDataAnnotation = false);

    /// <summary>
    ///     Configures the discriminator property used to identify which entity type each row in a table represents
    ///     when an inheritance hierarchy is mapped to a single table in a relational database.
    /// </summary>
    /// <param name="memberInfo">The property mapped to the discriminator property.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>A builder that allows the discriminator property to be configured.</returns>
    IConventionDiscriminatorBuilder? HasDiscriminator(MemberInfo memberInfo, bool fromDataAnnotation = false);

    /// <summary>
    ///     Removes the discriminator property from this entity type.
    ///     This method is usually called when the entity type is no longer mapped to the same table as any other type in
    ///     the hierarchy or when this entity type is no longer the root type.
    /// </summary>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     The same builder instance if the discriminator was configured,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    IConventionEntityTypeBuilder? HasNoDiscriminator(bool fromDataAnnotation = false);

    /// <summary>
    ///     Returns a value indicating whether the discriminator property can be configured.
    /// </summary>
    /// <param name="name">The name of the discriminator property.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the configuration can be applied.</returns>
    bool CanSetDiscriminator(string name, bool fromDataAnnotation = false);

    /// <summary>
    ///     Returns a value indicating whether the discriminator property can be configured.
    /// </summary>
    /// <param name="type">The type of values stored in the discriminator property.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the configuration can be applied.</returns>
    bool CanSetDiscriminator(Type type, bool fromDataAnnotation = false);

    /// <summary>
    ///     Returns a value indicating whether the discriminator property can be configured.
    /// </summary>
    /// <param name="type">The type of values stored in the discriminator property.</param>
    /// <param name="name">The name of the discriminator property.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the configuration can be applied.</returns>
    bool CanSetDiscriminator(string name, Type type, bool fromDataAnnotation = false);

    /// <summary>
    ///     Returns a value indicating whether the discriminator property can be configured.
    /// </summary>
    /// <param name="memberInfo">The property mapped to the discriminator property.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the configuration can be applied.</returns>
    bool CanSetDiscriminator(MemberInfo memberInfo, bool fromDataAnnotation = false);

    /// <summary>
    ///     Returns a value indicating whether the discriminator property can be removed.
    /// </summary>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the discriminator property can be removed.</returns>
    bool CanRemoveDiscriminator(bool fromDataAnnotation = false);

    /// <summary>
    ///     Gets or creates a builder for the target of a potential navigation.
    /// </summary>
    /// <param name="targetClrType">The CLR type of the target.</param>
    /// <param name="navigationInfo">The navigation property.</param>
    /// <param name="createIfMissing">Whether the entity type should be created if currently not in the model.</param>
    /// <param name="targetShouldBeOwned">Whether the target should be owned. <see langword="null" /> if it can be either.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The entity type builder or <see langword="null" /> if not found and can't be created.</returns>
    IConventionEntityTypeBuilder? GetTargetEntityTypeBuilder(
        Type targetClrType,
        MemberInfo navigationInfo,
        bool createIfMissing = true,
        bool? targetShouldBeOwned = null,
        bool fromDataAnnotation = false);
}
