// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     <para>
///         Represents an entity type in an <see cref="IConventionModel" />.
///     </para>
///     <para>
///         This interface is used during model creation and allows the metadata to be modified.
///         Once the model is built, <see cref="IEntityType" /> represents a read-only view of the same metadata.
///     </para>
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see> for more information and examples.
/// </remarks>
public interface IConventionEntityType : IReadOnlyEntityType, IConventionTypeBase
{
    /// <summary>
    ///     Gets the configuration source for this entity type.
    /// </summary>
    /// <returns>The configuration source.</returns>
    ConfigurationSource GetConfigurationSource();

    /// <summary>
    ///     Gets the model this entity belongs to.
    /// </summary>
    new IConventionModel Model { get; }

    /// <summary>
    ///     Gets the builder that can be used to configure this entity type.
    /// </summary>
    /// <exception cref="InvalidOperationException">If the entity type has been removed from the model.</exception>
    new IConventionEntityTypeBuilder Builder { get; }

    /// <summary>
    ///     Gets the base type of this entity type. Returns <see langword="null" /> if this is not a derived type in an inheritance hierarchy.
    /// </summary>
    new IConventionEntityType? BaseType { get; }

    /// <summary>
    ///     Gets a value indicating whether the entity type has no keys.
    ///     If <see langword="true" /> it will only be usable for queries.
    /// </summary>
    bool IsKeyless { get; }

    /// <summary>
    ///     Sets the change tracking strategy to use for this entity type. This strategy indicates how the
    ///     context detects changes to properties for an instance of the entity type.
    /// </summary>
    /// <param name="changeTrackingStrategy">The strategy to use.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The configured value.</returns>
    ChangeTrackingStrategy? SetChangeTrackingStrategy(ChangeTrackingStrategy? changeTrackingStrategy, bool fromDataAnnotation = false);

    /// <summary>
    ///     Returns the configuration source for <see cref="IReadOnlyEntityType.GetChangeTrackingStrategy" />.
    /// </summary>
    /// <returns>The configuration source for <see cref="IReadOnlyEntityType.GetChangeTrackingStrategy" />.</returns>
    ConfigurationSource? GetChangeTrackingStrategyConfigurationSource();

    /// <summary>
    ///     Sets the LINQ expression filter automatically applied to queries for this entity type.
    /// </summary>
    /// <param name="queryFilter">The LINQ expression filter.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The configured filter.</returns>
    LambdaExpression? SetQueryFilter(LambdaExpression? queryFilter, bool fromDataAnnotation = false);

    /// <summary>
    ///     Returns the configuration source for <see cref="IReadOnlyEntityType.GetQueryFilter" />.
    /// </summary>
    /// <returns>The configuration source for <see cref="IReadOnlyEntityType.GetQueryFilter" />.</returns>
    ConfigurationSource? GetQueryFilterConfigurationSource();

    /// <summary>
    ///     Returns the property that will be used for storing a discriminator value.
    /// </summary>
    /// <returns>The property that will be used for storing a discriminator value.</returns>
    new IConventionProperty? FindDiscriminatorProperty()
        => (IConventionProperty?)((IReadOnlyEntityType)this).FindDiscriminatorProperty();

    /// <summary>
    ///     Sets the <see cref="IReadOnlyProperty" /> that will be used for storing a discriminator value.
    /// </summary>
    /// <param name="property">The property to set.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The discriminator property.</returns>
    IConventionProperty? SetDiscriminatorProperty(IReadOnlyProperty? property, bool fromDataAnnotation = false);

    /// <summary>
    ///     Gets the <see cref="ConfigurationSource" /> for the discriminator property.
    /// </summary>
    /// <returns>The <see cref="ConfigurationSource" /> or <see langword="null" /> if no discriminator property has been set.</returns>
    ConfigurationSource? GetDiscriminatorPropertyConfigurationSource();

    /// <summary>
    ///     Sets the value indicating whether the discriminator mapping is complete.
    /// </summary>
    /// <param name="complete">The value indicating whether the discriminator mapping is complete.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The configured value.</returns>
    bool? SetDiscriminatorMappingComplete(bool? complete, bool fromDataAnnotation = false)
        => (bool?)SetOrRemoveAnnotation(CoreAnnotationNames.DiscriminatorMappingComplete, complete, fromDataAnnotation)?.Value;

    /// <summary>
    ///     Gets the <see cref="ConfigurationSource" /> for the discriminator value completeness.
    /// </summary>
    /// <returns>The <see cref="ConfigurationSource" /> or <see langword="null" /> if discriminator completeness has not been set.</returns>
    ConfigurationSource? GetDiscriminatorMappingCompleteConfigurationSource()
        => FindAnnotation(CoreAnnotationNames.DiscriminatorMappingComplete)?.GetConfigurationSource();

    /// <summary>
    ///     Sets the discriminator value for this entity type.
    /// </summary>
    /// <param name="value">The value to set.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The configured value.</returns>
    object? SetDiscriminatorValue(object? value, bool fromDataAnnotation = false)
        => SetAnnotation(CoreAnnotationNames.DiscriminatorValue, value, fromDataAnnotation)
            ?.Value;

    /// <summary>
    ///     Removes the discriminator value for this entity type.
    /// </summary>
    /// <returns>The removed discriminator value.</returns>
    object? RemoveDiscriminatorValue()
        => RemoveAnnotation(CoreAnnotationNames.DiscriminatorValue)?.Value;

    /// <summary>
    ///     Gets the <see cref="ConfigurationSource" /> for the discriminator value.
    /// </summary>
    /// <returns>The <see cref="ConfigurationSource" /> or <see langword="null" /> if no discriminator value has been set.</returns>
    ConfigurationSource? GetDiscriminatorValueConfigurationSource()
        => FindAnnotation(CoreAnnotationNames.DiscriminatorValue)
            ?.GetConfigurationSource();

    /// <summary>
    ///     Sets the base type of this entity type. Returns <see langword="null" /> if this is not a derived type in an inheritance hierarchy.
    /// </summary>
    /// <param name="entityType">The base entity type.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The new base type.</returns>
    IConventionEntityType? SetBaseType(IConventionEntityType? entityType, bool fromDataAnnotation = false);

    /// <summary>
    ///     Returns the configuration source for the BaseType property.
    /// </summary>
    /// <returns>The configuration source for the BaseType property.</returns>
    ConfigurationSource? GetBaseTypeConfigurationSource();

    /// <summary>
    ///     Gets all types in the model from which a given entity type derives, starting with the root.
    /// </summary>
    /// <returns>
    ///     The base types.
    /// </returns>
    new IEnumerable<IConventionEntityType> GetAllBaseTypes()
        => GetAllBaseTypesAscending().Reverse();

    /// <summary>
    ///     Gets all types in the model from which a given entity type derives, starting with the closest one.
    /// </summary>
    /// <returns>
    ///     The base types.
    /// </returns>
    new IEnumerable<IConventionEntityType> GetAllBaseTypesAscending()
        => GetAllBaseTypesInclusiveAscending().Skip(1);

    /// <summary>
    ///     Returns all base types of the given <see cref="IReadOnlyEntityType" />, including the type itself, top to bottom.
    /// </summary>
    /// <returns>Base types.</returns>
    new IEnumerable<IConventionEntityType> GetAllBaseTypesInclusive()
        => GetAllBaseTypesInclusiveAscending().Reverse();

    /// <summary>
    ///     Returns all base types of the given entity type, including the type itself, bottom to top.
    /// </summary>
    /// <returns>Base types.</returns>
    new IEnumerable<IConventionEntityType> GetAllBaseTypesInclusiveAscending()
        => ((IReadOnlyEntityType)this).GetAllBaseTypesInclusiveAscending().Cast<IConventionEntityType>();

    /// <summary>
    ///     Gets all types in the model that derive from a given entity type.
    /// </summary>
    /// <returns>The derived types.</returns>
    new IEnumerable<IConventionEntityType> GetDerivedTypes()
        => ((IReadOnlyEntityType)this).GetDerivedTypes().Cast<IConventionEntityType>();

    /// <summary>
    ///     Returns all derived types of this entity type, including the type itself.
    /// </summary>
    /// <returns>Derived types.</returns>
    new IEnumerable<IConventionEntityType> GetDerivedTypesInclusive()
        => ((IReadOnlyEntityType)this).GetDerivedTypesInclusive().Cast<IConventionEntityType>();

    /// <summary>
    ///     Gets all types in the model that directly derive from a given entity type.
    /// </summary>
    /// <returns>The derived types.</returns>
    new IEnumerable<IConventionEntityType> GetDirectlyDerivedTypes()
        => ((IReadOnlyEntityType)this).GetDirectlyDerivedTypes().Cast<IConventionEntityType>();

    /// <summary>
    ///     Gets the root base type for a given entity type.
    /// </summary>
    /// <returns>
    ///     The root base type. If the given entity type is not a derived type, then the same entity type is returned.
    /// </returns>
    new IConventionEntityType GetRootType()
        => (IConventionEntityType)((IReadOnlyEntityType)this).GetRootType();

    /// <summary>
    ///     Returns the closest entity type that is a parent of both given entity types. If one of the given entities is
    ///     a parent of the other, that parent is returned. Returns <see langword="null" /> if the two entity types aren't
    ///     in the same hierarchy.
    /// </summary>
    /// <param name="otherEntityType">Another entity type.</param>
    /// <returns>
    ///     The closest common parent of this entity type and <paramref name="otherEntityType" />,
    ///     or <see langword="null" /> if they have not common parent.
    /// </returns>
    new IConventionEntityType? FindClosestCommonParent(IReadOnlyEntityType otherEntityType)
        => (IConventionEntityType?)((IReadOnlyEntityType)this).FindClosestCommonParent(otherEntityType);

    /// <summary>
    ///     Gets the least derived type between the specified two.
    /// </summary>
    /// <param name="otherEntityType">The other entity type to compare with.</param>
    /// <returns>
    ///     The least derived type between the specified two.
    ///     If the given entity types are not related, then <see langword="null" /> is returned.
    /// </returns>
    new IConventionEntityType? LeastDerivedType(IReadOnlyEntityType otherEntityType)
        => (IConventionEntityType?)((IReadOnlyEntityType)this).LeastDerivedType(otherEntityType);

    /// <summary>
    ///     Sets a value indicating whether the entity type has no keys.
    ///     When set to <see langword="true" /> it will only be usable for queries.
    ///     <see langword="null" /> to reset to default.
    /// </summary>
    /// <param name="keyless">A value indicating whether the entity type to has no keys.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The new configuration value.</returns>
    bool? SetIsKeyless(bool? keyless, bool fromDataAnnotation = false);

    /// <summary>
    ///     Returns the configuration source for the IsKeyless property.
    /// </summary>
    /// <returns>The configuration source for the IsKeyless property.</returns>
    ConfigurationSource? GetIsKeylessConfigurationSource();

    /// <summary>
    ///     Sets the primary key for this entity type.
    /// </summary>
    /// <param name="properties">The properties that make up the primary key.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The newly created key.</returns>
    IConventionKey? SetPrimaryKey(IReadOnlyList<IConventionProperty>? properties, bool fromDataAnnotation = false);

    /// <summary>
    ///     Sets the primary key for this entity type.
    /// </summary>
    /// <param name="property">The primary key property.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The newly created key.</returns>
    IConventionKey? SetPrimaryKey(
        IConventionProperty? property,
        bool fromDataAnnotation = false)
        => SetPrimaryKey(property == null ? null : new[] { property }, fromDataAnnotation);

    /// <summary>
    ///     Gets primary key for this entity type. Returns <see langword="null" /> if no primary key is defined.
    /// </summary>
    /// <returns>The primary key, or <see langword="null" /> if none is defined.</returns>
    new IConventionKey? FindPrimaryKey();

    /// <summary>
    ///     Returns the configuration source for the primary key.
    /// </summary>
    /// <returns>The configuration source for the primary key.</returns>
    ConfigurationSource? GetPrimaryKeyConfigurationSource();

    /// <summary>
    ///     Adds a new alternate key to this entity type.
    /// </summary>
    /// <param name="property">The property to use as an alternate key.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The newly created key.</returns>
    IConventionKey? AddKey(IConventionProperty property, bool fromDataAnnotation = false)
        => AddKey(new[] { property }, fromDataAnnotation);

    /// <summary>
    ///     Adds a new alternate key to this entity type.
    /// </summary>
    /// <param name="properties">The properties that make up the alternate key.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The newly created key.</returns>
    IConventionKey? AddKey(IReadOnlyList<IConventionProperty> properties, bool fromDataAnnotation = false);

    /// <summary>
    ///     Gets the primary or alternate key that is defined on the given properties.
    ///     Returns <see langword="null" /> if no key is defined for the given properties.
    /// </summary>
    /// <param name="properties">The properties that make up the key.</param>
    /// <returns>The key, or <see langword="null" /> if none is defined.</returns>
    new IConventionKey? FindKey(IReadOnlyList<IReadOnlyProperty> properties);

    /// <summary>
    ///     Gets the primary or alternate key that is defined on the given property. Returns <see langword="null" /> if no key is defined
    ///     for the given property.
    /// </summary>
    /// <param name="property">The property that the key is defined on.</param>
    /// <returns>The key, or null if none is defined.</returns>
    new IConventionKey? FindKey(IReadOnlyProperty property)
        => FindKey(new[] { property });

    /// <summary>
    ///     Gets all keys declared on the given <see cref="IReadOnlyEntityType" />.
    /// </summary>
    /// <remarks>
    ///     This method does not return keys declared on base types.
    ///     It is useful when iterating over all entity types to avoid processing the same key more than once.
    ///     Use <see cref="GetKeys" /> to also return keys declared on base types.
    /// </remarks>
    /// <returns>Declared keys.</returns>
    new IEnumerable<IConventionKey> GetDeclaredKeys()
        => ((IReadOnlyEntityType)this).GetDeclaredKeys().Cast<IConventionKey>();

    /// <summary>
    ///     Gets the primary and alternate keys for this entity type.
    /// </summary>
    /// <returns>The primary and alternate keys.</returns>
    new IEnumerable<IConventionKey> GetKeys();

    /// <summary>
    ///     Removes a primary or alternate key from this entity type.
    /// </summary>
    /// <param name="properties">The properties that make up the key.</param>
    /// <returns>The key that was removed.</returns>
    IConventionKey? RemoveKey(IReadOnlyList<IReadOnlyProperty> properties);

    /// <summary>
    ///     Removes a primary or alternate key from this entity type.
    /// </summary>
    /// <param name="key">The key to be removed.</param>
    /// <returns>The removed key, or <see langword="null" /> if the key was not found.</returns>
    IConventionKey? RemoveKey(IReadOnlyKey key);

    /// <summary>
    ///     Adds a new relationship to this entity type.
    /// </summary>
    /// <param name="property">The property that the foreign key is defined on.</param>
    /// <param name="principalKey">The primary or alternate key that is referenced.</param>
    /// <param name="principalEntityType">
    ///     The entity type that the relationship targets. This may be different from the type that <paramref name="principalKey" />
    ///     is defined on when the relationship targets a derived type in an inheritance hierarchy (since the key is defined on the
    ///     base type of the hierarchy).
    /// </param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The newly created foreign key.</returns>
    IConventionForeignKey? AddForeignKey(
        IConventionProperty property,
        IConventionKey principalKey,
        IConventionEntityType principalEntityType,
        bool fromDataAnnotation = false)
        => AddForeignKey(new[] { property }, principalKey, principalEntityType, fromDataAnnotation);

    /// <summary>
    ///     Adds a new relationship to this entity type.
    /// </summary>
    /// <param name="properties">The properties that the foreign key is defined on.</param>
    /// <param name="principalKey">The primary or alternate key that is referenced.</param>
    /// <param name="principalEntityType">
    ///     The entity type that the relationship targets. This may be different from the type that <paramref name="principalKey" />
    ///     is defined on when the relationship targets a derived type in an inheritance hierarchy (since the key is defined on the
    ///     base type of the hierarchy).
    /// </param>
    /// <param name="setComponentConfigurationSource">
    ///     Indicates whether the configuration source should be set for the properties, principal key and principal end.
    /// </param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The newly created foreign key.</returns>
    IConventionForeignKey? AddForeignKey(
        IReadOnlyList<IConventionProperty> properties,
        IConventionKey principalKey,
        IConventionEntityType principalEntityType,
        bool setComponentConfigurationSource = true,
        bool fromDataAnnotation = false);

    /// <summary>
    ///     Gets the foreign key for the given properties that points to a given primary or alternate key.
    ///     Returns <see langword="null" /> if no foreign key is found.
    /// </summary>
    /// <param name="properties">The properties that the foreign key is defined on.</param>
    /// <param name="principalKey">The primary or alternate key that is referenced.</param>
    /// <param name="principalEntityType">
    ///     The entity type that the relationship targets. This may be different from the type that <paramref name="principalKey" />
    ///     is defined on when the relationship targets a derived type in an inheritance hierarchy (since the key is defined on the
    ///     base type of the hierarchy).
    /// </param>
    /// <returns>The foreign key, or <see langword="null" /> if none is defined.</returns>
    new IConventionForeignKey? FindForeignKey(
        IReadOnlyList<IReadOnlyProperty> properties,
        IReadOnlyKey principalKey,
        IReadOnlyEntityType principalEntityType);

    /// <summary>
    ///     Gets the foreign keys defined on the given property. Only foreign keys that are defined on exactly the specified
    ///     property are returned. Composite foreign keys that include the specified property are not returned.
    /// </summary>
    /// <param name="property">The property to find the foreign keys on.</param>
    /// <returns>The foreign keys.</returns>
    new IEnumerable<IConventionForeignKey> FindForeignKeys(IReadOnlyProperty property)
        => FindForeignKeys(new[] { property });

    /// <summary>
    ///     Gets the foreign keys defined on the given properties. Only foreign keys that are defined on exactly the specified
    ///     set of properties are returned.
    /// </summary>
    /// <param name="properties">The properties to find the foreign keys on.</param>
    /// <returns>The foreign keys.</returns>
    new IEnumerable<IConventionForeignKey> FindForeignKeys(IReadOnlyList<IReadOnlyProperty> properties)
        => ((IReadOnlyEntityType)this).FindForeignKeys(properties).Cast<IConventionForeignKey>();

    /// <summary>
    ///     Gets the foreign key for the given properties that points to a given primary or alternate key. Returns <see langword="null" />
    ///     if no foreign key is found.
    /// </summary>
    /// <param name="property">The property that the foreign key is defined on.</param>
    /// <param name="principalKey">The primary or alternate key that is referenced.</param>
    /// <param name="principalEntityType">
    ///     The entity type that the relationship targets. This may be different from the type that <paramref name="principalKey" />
    ///     is defined on when the relationship targets a derived type in an inheritance hierarchy (since the key is defined on the
    ///     base type of the hierarchy).
    /// </param>
    /// <returns>The foreign key, or <see langword="null" /> if none is defined.</returns>
    new IConventionForeignKey? FindForeignKey(
        IReadOnlyProperty property,
        IReadOnlyKey principalKey,
        IReadOnlyEntityType principalEntityType)
        => FindForeignKey(new[] { property }, principalKey, principalEntityType);

    /// <summary>
    ///     Gets the foreign keys declared on this entity type using the given properties.
    /// </summary>
    /// <param name="properties">The properties to find the foreign keys on.</param>
    /// <returns>Declared foreign keys.</returns>
    new IEnumerable<IConventionForeignKey> FindDeclaredForeignKeys(
        IReadOnlyList<IReadOnlyProperty> properties)
        => ((IReadOnlyEntityType)this).FindDeclaredForeignKeys(properties).Cast<IConventionForeignKey>();

    /// <summary>
    ///     Gets all foreign keys declared on this entity type.
    /// </summary>
    /// <remarks>
    ///     This method does not return foreign keys declared on base types.
    ///     It is useful when iterating over all entity types to avoid processing the same foreign key more than once.
    ///     Use <see cref="GetForeignKeys" /> to also return foreign keys declared on base types.
    /// </remarks>
    /// <returns>Declared foreign keys.</returns>
    new IEnumerable<IConventionForeignKey> GetDeclaredForeignKeys()
        => ((IReadOnlyEntityType)this).GetDeclaredForeignKeys().Cast<IConventionForeignKey>();

    /// <summary>
    ///     Gets all foreign keys declared on the types derived from this entity type.
    /// </summary>
    /// <returns>Derived foreign keys.</returns>
    new IEnumerable<IConventionForeignKey> GetDerivedForeignKeys()
        => ((IReadOnlyEntityType)this).GetDerivedForeignKeys().Cast<IConventionForeignKey>();

    /// <summary>
    ///     Gets the foreign keys defined on this entity type.
    /// </summary>
    /// <returns>The foreign keys defined on this entity type.</returns>
    new IEnumerable<IConventionForeignKey> GetForeignKeys();

    /// <summary>
    ///     Gets all foreign keys that target a given entity type (i.e. foreign keys where the given entity type
    ///     is the principal).
    /// </summary>
    /// <returns>The foreign keys that reference the given entity type.</returns>
    new IEnumerable<IConventionForeignKey> GetDeclaredReferencingForeignKeys()
        => ((IReadOnlyEntityType)this).GetDeclaredReferencingForeignKeys().Cast<IConventionForeignKey>();

    /// <summary>
    ///     Gets all foreign keys that target a given entity type (i.e. foreign keys where the given entity type
    ///     or a type it's derived from is the principal).
    /// </summary>
    /// <returns>The foreign keys that reference the given entity type.</returns>
    new IEnumerable<IConventionForeignKey> GetReferencingForeignKeys()
        => ((IReadOnlyEntityType)this).GetReferencingForeignKeys().Cast<IConventionForeignKey>();

    /// <summary>
    ///     Returns the relationship to the owner if this is an owned type or <see langword="null" /> otherwise.
    /// </summary>
    /// <returns>The relationship to the owner if this is an owned type or <see langword="null" /> otherwise.</returns>
    new IConventionForeignKey? FindOwnership()
        => (IConventionForeignKey?)((IReadOnlyEntityType)this).FindOwnership();

    /// <summary>
    ///     Removes a foreign key from this entity type.
    /// </summary>
    /// <param name="properties">The properties that the foreign key is defined on.</param>
    /// <param name="principalKey">The primary or alternate key that is referenced.</param>
    /// <param name="principalEntityType">
    ///     The entity type that the relationship targets. This may be different from the type that <paramref name="principalKey" />
    ///     is defined on when the relationship targets a derived type in an inheritance hierarchy (since the key is defined on the
    ///     base type of the hierarchy).
    /// </param>
    /// <returns>The foreign key that was removed.</returns>
    IConventionForeignKey? RemoveForeignKey(
        IReadOnlyList<IReadOnlyProperty> properties,
        IConventionKey principalKey,
        IConventionEntityType principalEntityType);

    /// <summary>
    ///     Removes a foreign key from this entity type.
    /// </summary>
    /// <param name="foreignKey">The foreign key to be removed.</param>
    /// <returns>The removed foreign key, or <see langword="null" /> if the index was not found.</returns>
    IConventionForeignKey? RemoveForeignKey(IReadOnlyForeignKey foreignKey);

    /// <summary>
    ///     Gets a navigation property on the given entity type. Returns <see langword="null" /> if no navigation property is found.
    /// </summary>
    /// <param name="memberInfo">The navigation property on the entity class.</param>
    /// <returns>The navigation property, or <see langword="null" /> if none is found.</returns>
    new IConventionNavigation? FindNavigation(MemberInfo memberInfo)
        => FindNavigation(Check.NotNull(memberInfo, nameof(memberInfo)).GetSimpleMemberName());

    /// <summary>
    ///     Gets a navigation property on the given entity type. Returns <see langword="null" /> if no navigation property is found.
    /// </summary>
    /// <param name="name">The name of the navigation property on the entity class.</param>
    /// <returns>The navigation property, or <see langword="null" /> if none is found.</returns>
    new IConventionNavigation? FindNavigation(string name)
        => (IConventionNavigation?)((IReadOnlyEntityType)this).FindNavigation(name);

    /// <summary>
    ///     Gets a navigation property on the given entity type. Does not return navigation properties defined on a base type.
    ///     Returns <see langword="null" /> if no navigation property is found.
    /// </summary>
    /// <param name="name">The name of the navigation property on the entity class.</param>
    /// <returns>The navigation property, or <see langword="null" /> if none is found.</returns>
    new IConventionNavigation? FindDeclaredNavigation(string name)
        => (IConventionNavigation?)((IReadOnlyEntityType)this).FindDeclaredNavigation(Check.NotNull(name, nameof(name)));

    /// <summary>
    ///     Gets all navigation properties declared on this entity type.
    /// </summary>
    /// <remarks>
    ///     This method does not return navigation properties declared on base types.
    ///     It is useful when iterating over all entity types to avoid processing the same navigation property more than once.
    ///     Use <see cref="GetNavigations" /> to also return navigation properties declared on base types.
    /// </remarks>
    /// <returns>Declared navigation properties.</returns>
    new IEnumerable<IConventionNavigation> GetDeclaredNavigations()
        => ((IReadOnlyEntityType)this).GetDeclaredNavigations().Cast<IConventionNavigation>();

    /// <summary>
    ///     Gets all navigation properties declared on the types derived from this entity type.
    /// </summary>
    /// <remarks>
    ///     This method does not return navigation properties declared on the given entity type itself.
    ///     Use <see cref="GetNavigations" /> to return navigation properties declared on this
    ///     and base entity typed types.
    /// </remarks>
    /// <returns>Derived navigation properties.</returns>
    new IEnumerable<IConventionNavigation> GetDerivedNavigations()
        => ((IReadOnlyEntityType)this).GetDerivedNavigations().Cast<IConventionNavigation>();

    /// <summary>
    ///     Gets all navigation properties on the given entity type.
    /// </summary>
    /// <returns>All navigation properties on the given entity type.</returns>
    new IEnumerable<IConventionNavigation> GetNavigations()
        => ((IReadOnlyEntityType)this).GetNavigations().Cast<IConventionNavigation>();

    /// <summary>
    ///     Adds a new skip navigation property to this entity type.
    /// </summary>
    /// <param name="name">The name of the skip navigation property to add.</param>
    /// <param name="memberInfo">
    ///     <para>
    ///         The corresponding CLR type member or <see langword="null" /> for a shadow navigation.
    ///     </para>
    ///     <para>
    ///         An indexer with a <see cref="string" /> parameter and <see cref="object" /> return type can be used.
    ///     </para>
    /// </param>
    /// <param name="targetEntityType">The entity type that the skip navigation property will hold an instance(s) of.</param>
    /// <param name="collection">Whether the navigation property is a collection property.</param>
    /// <param name="onDependent">
    ///     Whether the navigation property is defined on the dependent side of the underlying foreign key.
    /// </param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The newly created skip navigation property.</returns>
    IConventionSkipNavigation? AddSkipNavigation(
        string name,
        MemberInfo? memberInfo,
        IConventionEntityType targetEntityType,
        bool collection,
        bool onDependent,
        bool fromDataAnnotation = false);

    /// <summary>
    ///     Gets a skip navigation property on this entity type. Returns <see langword="null" /> if no navigation property is found.
    /// </summary>
    /// <param name="memberInfo">The navigation property on the entity class.</param>
    /// <returns>The navigation property, or <see langword="null" /> if none is found.</returns>
    new IConventionSkipNavigation? FindSkipNavigation(MemberInfo memberInfo)
        => (IConventionSkipNavigation?)((IReadOnlyEntityType)this).FindSkipNavigation(memberInfo);

    /// <summary>
    ///     Gets a skip navigation property on this entity type. Returns <see langword="null" /> if no skip navigation property is found.
    /// </summary>
    /// <param name="name">The name of the navigation property on the entity class.</param>
    /// <returns>The navigation property, or <see langword="null" /> if none is found.</returns>
    new IConventionSkipNavigation? FindSkipNavigation(string name);

    /// <summary>
    ///     Gets a skip navigation property on this entity type. Does not return skip navigation properties defined on a base type.
    ///     Returns <see langword="null" /> if no skip navigation property is found.
    /// </summary>
    /// <param name="name">The name of the navigation property on the entity class.</param>
    /// <returns>The navigation property, or <see langword="null" /> if none is found.</returns>
    new IConventionSkipNavigation? FindDeclaredSkipNavigation(string name)
        => (IConventionSkipNavigation?)((IReadOnlyEntityType)this).FindDeclaredSkipNavigation(name);

    /// <summary>
    ///     Gets the skip navigation properties declared on this entity type.
    /// </summary>
    /// <remarks>
    ///     This method does not return skip navigation properties declared declared on base types.
    ///     It is useful when iterating over all entity types to avoid processing the same foreign key more than once.
    ///     Use <see cref="GetSkipNavigations" /> to also return skip navigation properties declared on base types.
    /// </remarks>
    /// <returns>Declared foreign keys.</returns>
    new IEnumerable<IConventionSkipNavigation> GetDeclaredSkipNavigations()
        => ((IReadOnlyEntityType)this).GetDeclaredSkipNavigations().Cast<IConventionSkipNavigation>();

    /// <summary>
    ///     Gets all skip navigation properties declared on the types derived from this entity type.
    /// </summary>
    /// <remarks>
    ///     This method does not return skip navigation properties declared on the given entity type itself.
    ///     Use <see cref="GetSkipNavigations" /> to return skip navigation properties declared on this
    ///     and base entity typed types.
    /// </remarks>
    /// <returns>Derived skip navigation properties.</returns>
    new IEnumerable<IConventionSkipNavigation> GetDerivedSkipNavigations()
        => ((IReadOnlyEntityType)this).GetDerivedSkipNavigations().Cast<IConventionSkipNavigation>();

    /// <summary>
    ///     Gets all skip navigation properties on this entity type.
    /// </summary>
    /// <returns>All skip navigation properties on this entity type.</returns>
    new IEnumerable<IConventionSkipNavigation> GetSkipNavigations();

    /// <summary>
    ///     Removes a skip navigation property from this entity type.
    /// </summary>
    /// <param name="navigation">The skip navigation to be removed.</param>
    /// <returns>The removed skip navigation, or <see langword="null" /> if the skip navigation was not found.</returns>
    IConventionSkipNavigation? RemoveSkipNavigation(IReadOnlySkipNavigation navigation);

    /// <summary>
    ///     Adds an index to this entity type.
    /// </summary>
    /// <param name="property">The property to be indexed.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The newly created index.</returns>
    IConventionIndex? AddIndex(IConventionProperty property, bool fromDataAnnotation = false)
        => AddIndex(new[] { property }, fromDataAnnotation);

    /// <summary>
    ///     Adds an unnamed index to this entity type.
    /// </summary>
    /// <param name="properties">The properties that are to be indexed.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The newly created index.</returns>
    IConventionIndex? AddIndex(IReadOnlyList<IConventionProperty> properties, bool fromDataAnnotation = false);

    /// <summary>
    ///     Adds a named index to this entity type.
    /// </summary>
    /// <param name="property">The property to be indexed.</param>
    /// <param name="name">The name of the index.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The newly created index.</returns>
    IConventionIndex? AddIndex(
        IConventionProperty property,
        string name,
        bool fromDataAnnotation = false)
        => AddIndex(new[] { property }, name, fromDataAnnotation);

    /// <summary>
    ///     Adds a named index to this entity type.
    /// </summary>
    /// <param name="properties">The properties that are to be indexed.</param>
    /// <param name="name">The name of the index.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The newly created index.</returns>
    IConventionIndex? AddIndex(
        IReadOnlyList<IConventionProperty> properties,
        string name,
        bool fromDataAnnotation = false);

    /// <summary>
    ///     Gets the unnamed index defined on the given property. Returns <see langword="null" /> if no such index is defined.
    /// </summary>
    /// <remarks>
    ///     Named indexes will not be returned even if the list of properties matches.
    /// </remarks>
    /// <param name="property">The property to find the index on.</param>
    /// <returns>The index, or <see langword="null" /> if none is found.</returns>
    new IConventionIndex? FindIndex(IReadOnlyProperty property)
        => FindIndex(new[] { property });

    /// <summary>
    ///     Gets the unnamed index defined on the given properties. Returns <see langword="null" /> if no index is defined.
    /// </summary>
    /// <remarks>
    ///     Named indexes will not be returned even if the list of properties matches.
    /// </remarks>
    /// <param name="properties">The properties to find the index on.</param>
    /// <returns>The index, or <see langword="null" /> if none is found.</returns>
    new IConventionIndex? FindIndex(IReadOnlyList<IReadOnlyProperty> properties);

    /// <summary>
    ///     Gets the index with the given name. Returns <see langword="null" /> if no such index exists.
    /// </summary>
    /// <param name="name">The name of the index to find.</param>
    /// <returns>The index, or <see langword="null" /> if none is found.</returns>
    new IConventionIndex? FindIndex(string name);

    /// <summary>
    ///     Gets all indexes declared on this entity type.
    /// </summary>
    /// <remarks>
    ///     This method does not return indexes declared on base types.
    ///     It is useful when iterating over all entity types to avoid processing the same index more than once.
    ///     Use <see cref="GetIndexes" /> to also return indexes declared on base types.
    /// </remarks>
    /// <returns>Declared indexes.</returns>
    new IEnumerable<IConventionIndex> GetDeclaredIndexes()
        => ((IReadOnlyEntityType)this).GetDeclaredIndexes().Cast<IConventionIndex>();

    /// <summary>
    ///     Gets all indexes declared on the types derived from this entity type.
    /// </summary>
    /// <returns>Derived indexes.</returns>
    new IEnumerable<IConventionIndex> GetDerivedIndexes()
        => ((IReadOnlyEntityType)this).GetDerivedIndexes().Cast<IConventionIndex>();

    /// <summary>
    ///     Gets the indexes defined on this entity type.
    /// </summary>
    /// <returns>The indexes defined on this entity type.</returns>
    new IEnumerable<IConventionIndex> GetIndexes();

    /// <summary>
    ///     Removes an index from this entity type.
    /// </summary>
    /// <param name="properties">The properties that make up the index.</param>
    /// <returns>The index that was removed.</returns>
    IConventionIndex? RemoveIndex(IReadOnlyList<IReadOnlyProperty> properties);

    /// <summary>
    ///     Removes an index from this entity type.
    /// </summary>
    /// <param name="index">The index to remove.</param>
    /// <returns>The removed index, or <see langword="null" /> if the index was not found.</returns>
    IConventionIndex? RemoveIndex(IReadOnlyIndex index);

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
    ///         The corresponding CLR type member or <see langword="null" /> for a shadow property.
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
        MemberInfo? memberInfo,
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
    ///     This API only finds scalar properties and does not find navigation properties. Use
    ///     <see cref="FindNavigation(string)" /> to find a navigation property.
    /// </remarks>
    /// <param name="name">The name of the property.</param>
    /// <returns>The property, or <see langword="null" /> if none is found.</returns>
    new IConventionProperty? FindProperty(string name);

    /// <summary>
    ///     Gets the properties defined on this entity type.
    /// </summary>
    /// <remarks>
    ///     This API only returns scalar properties and does not return navigation properties. Use
    ///     <see cref="GetNavigations()" /> to get navigation properties.
    /// </remarks>
    /// <returns>The properties defined on this entity type.</returns>
    new IEnumerable<IConventionProperty> GetProperties();

    /// <summary>
    ///     Gets a property on the given entity type. Returns <see langword="null" /> if no property is found.
    /// </summary>
    /// <remarks>
    ///     This API only finds scalar properties and does not find navigation properties. Use
    ///     <see cref="FindNavigation(MemberInfo)" /> to find a navigation property.
    /// </remarks>
    /// <param name="memberInfo">The property on the entity class.</param>
    /// <returns>The property, or <see langword="null" /> if none is found.</returns>
    new IConventionProperty? FindProperty(MemberInfo memberInfo)
        => (IConventionProperty?)((IReadOnlyEntityType)this).FindProperty(memberInfo);

    /// <summary>
    ///     Finds matching properties on the given entity type. Returns <see langword="null" /> if any property is not found.
    /// </summary>
    /// <remarks>
    ///     This API only finds scalar properties and does not find navigation or service properties.
    /// </remarks>
    /// <param name="propertyNames">The property names.</param>
    /// <returns>The properties, or <see langword="null" /> if any property is not found.</returns>
    new IReadOnlyList<IConventionProperty>? FindProperties(IReadOnlyList<string> propertyNames)
        => (IReadOnlyList<IConventionProperty>?)((IReadOnlyEntityType)this).FindProperties(propertyNames);

    /// <summary>
    ///     Gets a property with the given name.
    /// </summary>
    /// <remarks>
    ///     This API only finds scalar properties and does not find navigation properties. Use
    ///     <see cref="FindNavigation(string)" /> to find a navigation property.
    /// </remarks>
    /// <param name="name">The property name.</param>
    /// <returns>The property.</returns>
    new IConventionProperty GetProperty(string name)
        => (IConventionProperty)((IReadOnlyEntityType)this).GetProperty(name);

    /// <summary>
    ///     Finds a property declared on the type with the given name.
    ///     Does not return properties defined on a base type.
    /// </summary>
    /// <param name="name">The property name.</param>
    /// <returns>The property, or <see langword="null" /> if none is found.</returns>
    new IConventionProperty? FindDeclaredProperty(string name)
        => (IConventionProperty?)((IReadOnlyEntityType)this).FindDeclaredProperty(name);

    /// <summary>
    ///     Gets all non-navigation properties declared on this entity type.
    /// </summary>
    /// <remarks>
    ///     This method does not return properties declared on base types.
    ///     It is useful when iterating over all entity types to avoid processing the same property more than once.
    ///     Use <see cref="GetProperties" /> to also return properties declared on base types.
    /// </remarks>
    /// <returns>Declared non-navigation properties.</returns>
    new IEnumerable<IConventionProperty> GetDeclaredProperties()
        => ((IReadOnlyEntityType)this).GetDeclaredProperties().Cast<IConventionProperty>();

    /// <summary>
    ///     Gets all non-navigation properties declared on the types derived from this entity type.
    /// </summary>
    /// <remarks>
    ///     This method does not return properties declared on the given entity type itself.
    ///     Use <see cref="GetProperties" /> to return properties declared on this
    ///     and base entity typed types.
    /// </remarks>
    /// <returns>Derived non-navigation properties.</returns>
    new IEnumerable<IConventionProperty> GetDerivedProperties()
        => ((IReadOnlyEntityType)this).GetDerivedProperties().Cast<IConventionProperty>();

    /// <summary>
    ///     Removes a property from this entity type.
    /// </summary>
    /// <param name="name">The name of the property to remove.</param>
    /// <returns>The property that was removed.</returns>
    IConventionProperty? RemoveProperty(string name);

    /// <summary>
    ///     Removes a property from this entity type.
    /// </summary>
    /// <param name="property">The property to remove.</param>
    /// <returns>The removed property, or <see langword="null" /> if the property was not found.</returns>
    IConventionProperty? RemoveProperty(IReadOnlyProperty property);

    /// <summary>
    ///     Adds a service property to this entity type.
    /// </summary>
    /// <param name="memberInfo">The <see cref="PropertyInfo" /> or <see cref="FieldInfo" /> of the property to add.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The newly created service property.</returns>
    IConventionServiceProperty AddServiceProperty(MemberInfo memberInfo, bool fromDataAnnotation = false);

    /// <summary>
    ///     Adds a service property to this entity type.
    /// </summary>
    /// <param name="serviceType">The type of the service.</param>
    /// <param name="memberInfo">The <see cref="PropertyInfo" /> or <see cref="FieldInfo" /> of the property to add.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The newly created service property.</returns>
    IConventionServiceProperty AddServiceProperty(Type serviceType, MemberInfo memberInfo, bool fromDataAnnotation = false);

    /// <summary>
    ///     Gets the service property with a given name.
    ///     Returns <see langword="null" /> if no property with the given name is defined.
    /// </summary>
    /// <remarks>
    ///     This API only finds service properties and does not find scalar or navigation properties.
    /// </remarks>
    /// <param name="name">The name of the service property.</param>
    /// <returns>The service property, or <see langword="null" /> if none is found.</returns>
    new IConventionServiceProperty? FindServiceProperty(string name);

    /// <summary>
    ///     Gets all service properties declared on this entity type.
    /// </summary>
    /// <remarks>
    ///     This method does not return properties declared on base types.
    ///     It is useful when iterating over all entity types to avoid processing the same property more than once.
    ///     Use <see cref="GetServiceProperties" /> to also return properties declared on base types.
    /// </remarks>
    /// <returns>Declared service properties.</returns>
    new IEnumerable<IConventionServiceProperty> GetDeclaredServiceProperties()
        => ((IReadOnlyEntityType)this).GetDeclaredServiceProperties().Cast<IConventionServiceProperty>();

    /// <summary>
    ///     Gets all service properties declared on the types derived from this entity type.
    /// </summary>
    /// <remarks>
    ///     This method does not return service properties declared on the given entity type itself.
    ///     Use <see cref="GetServiceProperties" /> to return service properties declared on this
    ///     and base entity typed types.
    /// </remarks>
    /// <returns>Derived service properties.</returns>
    new IEnumerable<IConventionServiceProperty> GetDerivedServiceProperties()
        => ((IReadOnlyEntityType)this).GetDerivedServiceProperties().Cast<IConventionServiceProperty>();

    /// <summary>
    ///     Gets all the service properties defined on this entity type.
    /// </summary>
    /// <remarks>
    ///     This API only returns service properties and does not return scalar or navigation properties.
    /// </remarks>
    /// <returns>The service properties defined on this entity type.</returns>
    new IEnumerable<IConventionServiceProperty> GetServiceProperties();

    /// <summary>
    ///     Removes a service property from this entity type.
    /// </summary>
    /// <param name="name">The name of the property to remove.</param>
    /// <returns>The property that was removed, or <see langword="null" /> if the property was not found.</returns>
    IConventionServiceProperty? RemoveServiceProperty(string name);

    /// <summary>
    ///     Removes a service property from this entity type.
    /// </summary>
    /// <param name="property">The property to remove.</param>
    /// <returns>The removed property, or <see langword="null" /> if the property was not found.</returns>
    IConventionServiceProperty? RemoveServiceProperty(IReadOnlyServiceProperty property);

    /// <summary>
    ///     Finds a trigger with the given name.
    /// </summary>
    /// <param name="name">The trigger name.</param>
    /// <returns>The trigger or <see langword="null" /> if no trigger with the given name was found.</returns>
    new IConventionTrigger? FindDeclaredTrigger(string name);

    /// <summary>
    ///     Returns the declared triggers on the entity type.
    /// </summary>
    new IEnumerable<IConventionTrigger> GetDeclaredTriggers();

    /// <summary>
    ///     Creates a new trigger with the given name on entity type. Throws an exception if a trigger with the same name exists on the same
    ///     entity type.
    /// </summary>
    /// <param name="name">The trigger name.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The trigger.</returns>
    IConventionTrigger? AddTrigger(
        string name,
        bool fromDataAnnotation = false);

    /// <summary>
    ///     Removes the trigger with the given name.
    /// </summary>
    /// <param name="name">The trigger name.</param>
    /// <returns>
    ///     The removed trigger or <see langword="null" /> if no trigger with the given name was found
    ///     or the existing trigger was configured from a higher source.
    /// </returns>
    IConventionTrigger? RemoveTrigger(string name);
}
