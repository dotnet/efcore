// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Represents an entity type in a model.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
/// </remarks>
public interface IEntityType : IReadOnlyEntityType, ITypeBase
{
    /// <summary>
    ///     Gets the base type of this entity type. Returns <see langword="null" /> if this is not a derived type in an inheritance
    ///     hierarchy.
    /// </summary>
    new IEntityType? BaseType { get; }

    /// <summary>
    ///     Gets the <see cref="InstantiationBinding" /> for the preferred constructor when creating instances with only service
    ///     properties initialized.
    /// </summary>
    InstantiationBinding? ServiceOnlyConstructorBinding { get; }

    /// <summary>
    ///     Returns the <see cref="IProperty" /> that will be used for storing a discriminator value.
    /// </summary>
    new IProperty? FindDiscriminatorProperty()
        => (IProperty?)((IReadOnlyEntityType)this).FindDiscriminatorProperty();

    /// <summary>
    ///     Gets the root base type for a given entity type.
    /// </summary>
    /// <returns>
    ///     The root base type. If the given entity type is not a derived type, then the same entity type is returned.
    /// </returns>
    new IEntityType GetRootType()
        => (IEntityType)((IReadOnlyEntityType)this).GetRootType();

    /// <summary>
    ///     Gets all types in the model from which a given entity type derives, starting with the root.
    /// </summary>
    /// <returns>The base types.</returns>
    new IEnumerable<IEntityType> GetAllBaseTypes()
        => GetAllBaseTypesAscending().Reverse();

    /// <summary>
    ///     Returns all base types of the given entity type, including the type itself, top to bottom.
    /// </summary>
    /// <returns>The base types.</returns>
    new IEnumerable<IEntityType> GetAllBaseTypesInclusive()
        => GetAllBaseTypesInclusiveAscending().Reverse();

    /// <summary>
    ///     Gets all types in the model from which a given entity type derives, starting with the closest one.
    /// </summary>
    /// <returns>The base types.</returns>
    new IEnumerable<IEntityType> GetAllBaseTypesAscending()
        => GetAllBaseTypesInclusiveAscending().Skip(1);

    /// <summary>
    ///     Returns all base types of the given entity type, including the type itself, bottom to top.
    /// </summary>
    /// <returns>The base types.</returns>
    new IEnumerable<IEntityType> GetAllBaseTypesInclusiveAscending()
        => ((IReadOnlyEntityType)this).GetAllBaseTypesInclusiveAscending().Cast<IEntityType>();

    /// <summary>
    ///     Gets all types in the model that derive from this entity type.
    /// </summary>
    /// <returns>The derived types.</returns>
    new IEnumerable<IEntityType> GetDerivedTypes()
        => ((IReadOnlyEntityType)this).GetDerivedTypes().Cast<IEntityType>();

    /// <summary>
    ///     Returns all derived types of this entity type, including the type itself.
    /// </summary>
    /// <returns>Derived types.</returns>
    new IEnumerable<IEntityType> GetDerivedTypesInclusive()
        => ((IReadOnlyEntityType)this).GetDerivedTypesInclusive().Cast<IEntityType>();

    /// <summary>
    ///     Gets all types in the model that directly derive from this entity type.
    /// </summary>
    /// <returns>The derived types.</returns>
    new IEnumerable<IEntityType> GetDirectlyDerivedTypes();

    /// <summary>
    ///     Returns all the derived types of this entity type, including the type itself,
    ///     which are not <see langword="abstract" />.
    /// </summary>
    /// <returns>Non-abstract, derived types.</returns>
    new IEnumerable<IEntityType> GetConcreteDerivedTypesInclusive()
        => ((IReadOnlyEntityType)this).GetConcreteDerivedTypesInclusive().Cast<IEntityType>();

    /// <summary>
    ///     Gets the primary or alternate key that is defined on the given property. Returns <see langword="null" /> if no key is defined
    ///     for the given property.
    /// </summary>
    /// <param name="property">The property that the key is defined on.</param>
    /// <returns>The key, or null if none is defined.</returns>
    new IKey? FindKey(IReadOnlyProperty property)
        => FindKey(new[] { property });

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
    IEntityType? FindClosestCommonParent(IEntityType otherEntityType)
        => (IEntityType?)((IReadOnlyEntityType)this).FindClosestCommonParent(otherEntityType);

    /// <summary>
    ///     Gets the least derived type between the specified two.
    /// </summary>
    /// <param name="otherEntityType">The other entity type to compare with.</param>
    /// <returns>
    ///     The least derived type between the specified two.
    ///     If the given entity types are not related, then <see langword="null" /> is returned.
    /// </returns>
    IEntityType? LeastDerivedType(IEntityType otherEntityType)
        => (IEntityType?)((IReadOnlyEntityType)this).LeastDerivedType(otherEntityType);

    /// <summary>
    ///     Gets primary key for this entity type. Returns <see langword="null" /> if no primary key is defined.
    /// </summary>
    /// <returns>The primary key, or <see langword="null" /> if none is defined.</returns>
    new IKey? FindPrimaryKey();

    /// <summary>
    ///     Gets the primary or alternate key that is defined on the given properties.
    ///     Returns <see langword="null" /> if no key is defined for the given properties.
    /// </summary>
    /// <param name="properties">The properties that make up the key.</param>
    /// <returns>The key, or <see langword="null" /> if none is defined.</returns>
    new IKey? FindKey(IReadOnlyList<IReadOnlyProperty> properties);

    /// <summary>
    ///     Gets all keys declared on the given <see cref="IReadOnlyEntityType" />.
    /// </summary>
    /// <remarks>
    ///     This method does not return keys declared on base types.
    ///     It is useful when iterating over all entity types to avoid processing the same key more than once.
    ///     Use <see cref="GetKeys" /> to also return keys declared on base types.
    /// </remarks>
    /// <returns>Declared keys.</returns>
    new IEnumerable<IKey> GetDeclaredKeys();

    /// <summary>
    ///     Gets the primary and alternate keys for this entity type.
    /// </summary>
    /// <returns>The primary and alternate keys.</returns>
    new IEnumerable<IKey> GetKeys();

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
    new IForeignKey? FindForeignKey(
        IReadOnlyList<IReadOnlyProperty> properties,
        IReadOnlyKey principalKey,
        IReadOnlyEntityType principalEntityType);

    /// <summary>
    ///     Gets the foreign keys defined on the given property. Only foreign keys that are defined on exactly the specified
    ///     property are returned. Composite foreign keys that include the specified property are not returned.
    /// </summary>
    /// <param name="property">The property to find the foreign keys on.</param>
    /// <returns>The foreign keys.</returns>
    new IEnumerable<IForeignKey> FindForeignKeys(IReadOnlyProperty property)
        => FindForeignKeys(new[] { property });

    /// <summary>
    ///     Gets the foreign keys defined on the given properties. Only foreign keys that are defined on exactly the specified
    ///     set of properties are returned.
    /// </summary>
    /// <param name="properties">The properties to find the foreign keys on.</param>
    /// <returns>The foreign keys.</returns>
    new IEnumerable<IForeignKey> FindForeignKeys(IReadOnlyList<IReadOnlyProperty> properties);

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
    new IForeignKey? FindForeignKey(
        IReadOnlyProperty property,
        IReadOnlyKey principalKey,
        IReadOnlyEntityType principalEntityType)
        => FindForeignKey(new[] { property }, principalKey, principalEntityType);

    /// <summary>
    ///     Gets the foreign keys declared on the given <see cref="IEntityType" /> using the given properties.
    /// </summary>
    /// <param name="properties">The properties to find the foreign keys on.</param>
    /// <returns>Declared foreign keys.</returns>
    new IEnumerable<IForeignKey> FindDeclaredForeignKeys(IReadOnlyList<IReadOnlyProperty> properties);

    /// <summary>
    ///     Gets all foreign keys declared on the given <see cref="IEntityType" />.
    /// </summary>
    /// <remarks>
    ///     This method does not return foreign keys declared on base types.
    ///     It is useful when iterating over all entity types to avoid processing the same foreign key more than once.
    ///     Use <see cref="GetForeignKeys" /> to also return foreign keys declared on base types.
    /// </remarks>
    /// <returns>Declared foreign keys.</returns>
    new IEnumerable<IForeignKey> GetDeclaredForeignKeys();

    /// <summary>
    ///     Gets all foreign keys declared on the types derived from the given <see cref="IEntityType" />.
    /// </summary>
    /// <returns>Derived foreign keys.</returns>
    new IEnumerable<IForeignKey> GetDerivedForeignKeys();

    /// <summary>
    ///     Gets the foreign keys defined on this entity type.
    /// </summary>
    /// <returns>The foreign keys defined on this entity type.</returns>
    new IEnumerable<IForeignKey> GetForeignKeys();

    /// <summary>
    ///     Gets all foreign keys that target a given entity type (i.e. foreign keys where the given entity type
    ///     or a type it's derived from is the principal).
    /// </summary>
    /// <returns>The foreign keys that reference the given entity type.</returns>
    new IEnumerable<IForeignKey> GetReferencingForeignKeys();

    /// <summary>
    ///     Gets all foreign keys that target a given entity type (i.e. foreign keys where the given entity type
    ///     is the principal).
    /// </summary>
    /// <returns>The foreign keys that reference the given entity type.</returns>
    new IEnumerable<IForeignKey> GetDeclaredReferencingForeignKeys();

    /// <summary>
    ///     Returns the relationship to the owner if this is an owned type or <see langword="null" /> otherwise.
    /// </summary>
    /// <returns>The relationship to the owner if this is an owned type or <see langword="null" /> otherwise.</returns>
    new IForeignKey? FindOwnership()
        => (IForeignKey?)((IReadOnlyEntityType)this).FindOwnership();

    /// <summary>
    ///     Gets a navigation property on the given entity type. Returns <see langword="null" /> if no navigation property is found.
    /// </summary>
    /// <param name="memberInfo">The navigation property on the entity class.</param>
    /// <returns>The navigation property, or <see langword="null" /> if none is found.</returns>
    new INavigation? FindNavigation(MemberInfo memberInfo)
        => FindNavigation(Check.NotNull(memberInfo, nameof(memberInfo)).GetSimpleMemberName());

    /// <summary>
    ///     Gets a navigation property on the given entity type. Returns <see langword="null" /> if no navigation property is found.
    /// </summary>
    /// <param name="name">The name of the navigation property on the entity class.</param>
    /// <returns>The navigation property, or <see langword="null" /> if none is found.</returns>
    new INavigation? FindNavigation(string name)
        => (INavigation?)((IReadOnlyEntityType)this).FindNavigation(name);

    /// <summary>
    ///     Gets a navigation property on the given entity type. Does not return navigation properties defined on a base type.
    ///     Returns <see langword="null" /> if no navigation property is found.
    /// </summary>
    /// <param name="name">The name of the navigation property on the entity class.</param>
    /// <returns>The navigation property, or <see langword="null" /> if none is found.</returns>
    new INavigation? FindDeclaredNavigation(string name);

    /// <summary>
    ///     Gets all navigation properties declared on the given <see cref="IEntityType" />.
    /// </summary>
    /// <remarks>
    ///     This method does not return navigation properties declared on base types.
    ///     It is useful when iterating over all entity types to avoid processing the same navigation property more than once.
    ///     Use <see cref="GetNavigations" /> to also return navigation properties declared on base types.
    /// </remarks>
    /// <returns>Declared navigation properties.</returns>
    new IEnumerable<INavigation> GetDeclaredNavigations();

    /// <summary>
    ///     Gets all navigation properties declared on the types derived from this entity type.
    /// </summary>
    /// <remarks>
    ///     This method does not return navigation properties declared on the given entity type itself.
    ///     Use <see cref="GetNavigations" /> to return navigation properties declared on this
    ///     and base entity typed types.
    /// </remarks>
    /// <returns>Derived navigation properties.</returns>
    new IEnumerable<INavigation> GetDerivedNavigations()
        => ((IReadOnlyEntityType)this).GetDerivedNavigations().Cast<INavigation>();

    /// <summary>
    ///     Gets all navigation properties declared on the base types and types derived from this entity type.
    /// </summary>
    /// <returns>Navigation properties.</returns>
    IEnumerable<INavigation> GetNavigationsInHierarchy()
        => GetAllBaseTypes().Concat(GetDerivedTypesInclusive())
            .SelectMany(t => t.GetDeclaredNavigations());

    /// <summary>
    ///     Gets all navigation properties on the given entity type.
    /// </summary>
    /// <returns>All navigation properties on the given entity type.</returns>
    new IEnumerable<INavigation> GetNavigations();

    /// <summary>
    ///     Gets a skip navigation property on this entity type. Returns <see langword="null" /> if no navigation property is found.
    /// </summary>
    /// <param name="memberInfo">The navigation property on the entity class.</param>
    /// <returns>The navigation property, or <see langword="null" /> if none is found.</returns>
    new ISkipNavigation? FindSkipNavigation(MemberInfo memberInfo)
        => (ISkipNavigation?)((IReadOnlyEntityType)this).FindSkipNavigation(memberInfo);

    /// <summary>
    ///     Gets a skip navigation property on this entity type. Returns <see langword="null" /> if no skip navigation property is found.
    /// </summary>
    /// <param name="name">The name of the navigation property on the entity class.</param>
    /// <returns>The navigation property, or <see langword="null" /> if none is found.</returns>
    new ISkipNavigation? FindSkipNavigation(string name);

    /// <summary>
    ///     Gets a skip navigation property on this entity type. Does not return skip navigation properties defined on a base type.
    ///     Returns <see langword="null" /> if no skip navigation property is found.
    /// </summary>
    /// <param name="name">The name of the navigation property on the entity class.</param>
    /// <returns>The navigation property, or <see langword="null" /> if none is found.</returns>
    new ISkipNavigation? FindDeclaredSkipNavigation(string name)
        => (ISkipNavigation?)((IReadOnlyEntityType)this).FindDeclaredSkipNavigation(name);

    /// <summary>
    ///     Gets all skip navigation properties declared on this entity type.
    /// </summary>
    /// <remarks>
    ///     This method does not return skip navigation properties declared declared on base types.
    ///     It is useful when iterating over all entity types to avoid processing the same foreign key more than once.
    ///     Use <see cref="GetSkipNavigations" /> to also return skip navigation properties declared on base types.
    /// </remarks>
    /// <returns>Declared foreign keys.</returns>
    new IEnumerable<ISkipNavigation> GetDeclaredSkipNavigations()
        => ((IReadOnlyEntityType)this).GetDeclaredSkipNavigations().Cast<ISkipNavigation>();

    /// <summary>
    ///     Gets all skip navigation properties declared on the types derived from this entity type.
    /// </summary>
    /// <remarks>
    ///     This method does not return skip navigation properties declared on the given entity type itself.
    ///     Use <see cref="GetSkipNavigations" /> to return skip navigation properties declared on this
    ///     and base entity typed types.
    /// </remarks>
    /// <returns>Derived skip navigation properties.</returns>
    new IEnumerable<ISkipNavigation> GetDerivedSkipNavigations()
        => ((IReadOnlyEntityType)this).GetDerivedSkipNavigations().Cast<ISkipNavigation>();

    /// <summary>
    ///     Gets the skip navigation properties on this entity type.
    /// </summary>
    /// <returns>The skip navigation properties on this entity type.</returns>
    new IEnumerable<ISkipNavigation> GetSkipNavigations();

    /// <summary>
    ///     Gets the unnamed index defined on the given properties. Returns <see langword="null" /> if no such index is defined.
    /// </summary>
    /// <remarks>
    ///     Named indexes will not be returned even if the list of properties matches.
    /// </remarks>
    /// <param name="properties">The properties to find the index on.</param>
    /// <returns>The index, or <see langword="null" /> if none is found.</returns>
    new IIndex? FindIndex(IReadOnlyList<IReadOnlyProperty> properties);

    /// <summary>
    ///     Gets the index with the given name. Returns <see langword="null" /> if no such index exists.
    /// </summary>
    /// <param name="name">The name of the index.</param>
    /// <returns>The index, or <see langword="null" /> if none is found.</returns>
    new IIndex? FindIndex(string name);

    /// <summary>
    ///     Gets the index defined on the given property. Returns <see langword="null" /> if no index is defined.
    /// </summary>
    /// <param name="property">The property to find the index on.</param>
    /// <returns>The index, or <see langword="null" /> if none is found.</returns>
    new IIndex? FindIndex(IReadOnlyProperty property)
        => FindIndex(new[] { property });

    /// <summary>
    ///     Gets all indexes declared on the given <see cref="IEntityType" />.
    /// </summary>
    /// <remarks>
    ///     This method does not return indexes declared on base types.
    ///     It is useful when iterating over all entity types to avoid processing the same index more than once.
    ///     Use <see cref="GetIndexes" /> to also return indexes declared on base types.
    /// </remarks>
    /// <returns>Declared indexes.</returns>
    new IEnumerable<IIndex> GetDeclaredIndexes();

    /// <summary>
    ///     Gets all indexes declared on the types derived from the given <see cref="IEntityType" />.
    /// </summary>
    /// <returns>Derived indexes.</returns>
    new IEnumerable<IIndex> GetDerivedIndexes();

    /// <summary>
    ///     Gets the indexes defined on this entity type.
    /// </summary>
    /// <returns>The indexes defined on this entity type.</returns>
    new IEnumerable<IIndex> GetIndexes();

    // The following methods are needed for binary compatibility
    #region DO NOT DELETE

    /// <summary>
    ///     Gets a property on the given entity type. Returns <see langword="null" /> if no property is found.
    /// </summary>
    /// <remarks>
    ///     This API only finds scalar properties and does not find navigation properties. Use
    ///     <see cref="FindNavigation(MemberInfo)" /> to find a navigation property.
    /// </remarks>
    /// <param name="memberInfo">The property on the entity class.</param>
    /// <returns>The property, or <see langword="null" /> if none is found.</returns>
    new IProperty? FindProperty(MemberInfo memberInfo)
        => (IProperty?)((IReadOnlyEntityType)this).FindProperty(memberInfo);

    /// <summary>
    ///     Gets the property with a given name. Returns <see langword="null" /> if no property with the given name is defined.
    /// </summary>
    /// <remarks>
    ///     This API only finds scalar properties and does not find navigation properties. Use
    ///     <see cref="FindNavigation(string)" /> to find a navigation property.
    /// </remarks>
    /// <param name="name">The name of the property.</param>
    /// <returns>The property, or <see langword="null" /> if none is found.</returns>
    new IProperty? FindProperty(string name);

    /// <summary>
    ///     Finds matching properties on the given entity type. Returns <see langword="null" /> if any property is not found.
    /// </summary>
    /// <remarks>
    ///     This API only finds scalar properties and does not find navigation properties.
    /// </remarks>
    /// <param name="propertyNames">The property names.</param>
    /// <returns>The properties, or <see langword="null" /> if any property is not found.</returns>
    new IReadOnlyList<IProperty>? FindProperties(
        IReadOnlyList<string> propertyNames)
        => (IReadOnlyList<IProperty>?)((IReadOnlyEntityType)this).FindProperties(propertyNames);

    /// <summary>
    ///     Gets a property with the given name.
    /// </summary>
    /// <remarks>
    ///     This API only finds scalar properties and does not find navigation properties. Use
    ///     <see cref="FindNavigation(string)" /> to find a navigation property.
    /// </remarks>
    /// <param name="name">The property name.</param>
    /// <returns>The property.</returns>
    new IProperty GetProperty(string name)
        => (IProperty)((IReadOnlyEntityType)this).GetProperty(name);

    /// <summary>
    ///     Finds a property declared on the type with the given name.
    ///     Does not return properties defined on a base type.
    /// </summary>
    /// <param name="name">The property name.</param>
    /// <returns>The property, or <see langword="null" /> if none is found.</returns>
    new IProperty? FindDeclaredProperty(string name);

    /// <summary>
    ///     Gets all non-navigation properties declared on the given <see cref="IEntityType" />.
    /// </summary>
    /// <remarks>
    ///     This method does not return properties declared on base types.
    ///     It is useful when iterating over all entity types to avoid processing the same property more than once.
    ///     Use <see cref="GetProperties" /> to also return properties declared on base types.
    /// </remarks>
    /// <returns>Declared non-navigation properties.</returns>
    new IEnumerable<IProperty> GetDeclaredProperties();

    /// <summary>
    ///     Gets all non-navigation properties declared on the types derived from this entity type.
    /// </summary>
    /// <remarks>
    ///     This method does not return properties declared on the given entity type itself.
    ///     Use <see cref="GetProperties" /> to return properties declared on this
    ///     and base entity typed types.
    /// </remarks>
    /// <returns>Derived non-navigation properties.</returns>
    new IEnumerable<IProperty> GetDerivedProperties()
        => ((IReadOnlyEntityType)this).GetDerivedProperties().Cast<IProperty>();

    /// <summary>
    ///     Gets the properties defined on this entity type.
    /// </summary>
    /// <remarks>
    ///     This API only returns scalar properties and does not return navigation properties. Use
    ///     <see cref="GetNavigations()" /> to get navigation properties.
    /// </remarks>
    /// <returns>The properties defined on this entity type.</returns>
    new IEnumerable<IProperty> GetProperties();

    #endregion

    /// <summary>
    ///     Returns the properties contained in foreign keys.
    /// </summary>
    /// <returns>The properties contained in foreign keys.</returns>
    IEnumerable<IProperty> GetForeignKeyProperties();

    /// <summary>
    ///     Gets all properties declared on the base types and types derived from this entity type.
    /// </summary>
    /// <returns>The properties.</returns>
    IEnumerable<IProperty> ITypeBase.GetPropertiesInHierarchy()
        => GetAllBaseTypes().Concat(GetDerivedTypesInclusive())
            .SelectMany(t => t.GetDeclaredProperties());

    /// <summary>
    ///     Gets all properties declared on the base types and types derived from this entity type, including those on complex types.
    /// </summary>
    /// <returns>The properties.</returns>
    IEnumerable<IProperty> ITypeBase.GetFlattenedPropertiesInHierarchy()
        => GetAllBaseTypes().Concat(GetDerivedTypesInclusive())
            .SelectMany(t => t.GetFlattenedDeclaredProperties());

    /// <summary>
    ///     Returns the properties that need a value to be generated when the entity entry transitions to the
    ///     <see cref="EntityState.Added" /> state.
    /// </summary>
    /// <returns>The properties that need a value to be generated on add.</returns>
    IEnumerable<IProperty> GetValueGeneratingProperties();

    /// <summary>
    ///     Gets the service property with a given name.
    ///     Returns <see langword="null" /> if no property with the given name is defined.
    /// </summary>
    /// <remarks>
    ///     This API only finds service properties and does not find scalar or navigation properties.
    /// </remarks>
    /// <param name="name">The name of the service property.</param>
    /// <returns>The service property, or <see langword="null" /> if none is found.</returns>
    new IServiceProperty? FindServiceProperty(string name);

    /// <summary>
    ///     Gets all service properties declared on the given <see cref="IEntityType" />.
    /// </summary>
    /// <remarks>
    ///     This method does not return properties declared on base types.
    ///     It is useful when iterating over all entity types to avoid processing the same property more than once.
    ///     Use <see cref="GetServiceProperties" /> to also return properties declared on base types.
    /// </remarks>
    /// <returns>Declared service properties.</returns>
    new IEnumerable<IServiceProperty> GetDeclaredServiceProperties();

    /// <summary>
    ///     Gets all service properties declared on the types derived from this entity type.
    /// </summary>
    /// <remarks>
    ///     This method does not return service properties declared on the given entity type itself.
    ///     Use <see cref="GetServiceProperties" /> to return service properties declared on this
    ///     and base entity typed types.
    /// </remarks>
    /// <returns>Derived service properties.</returns>
    new IEnumerable<IServiceProperty> GetDerivedServiceProperties()
        => ((IReadOnlyEntityType)this).GetDerivedServiceProperties().Cast<IServiceProperty>();

    /// <summary>
    ///     Gets all the <see cref="IServiceProperty" /> defined on this entity type.
    /// </summary>
    /// <remarks>
    ///     This API only returns service properties and does not return scalar or navigation properties.
    /// </remarks>
    /// <returns>The service properties defined on this entity type.</returns>
    new IEnumerable<IServiceProperty> GetServiceProperties();

    /// <summary>
    ///     Finds a trigger with the given name.
    /// </summary>
    /// <param name="name">The trigger name.</param>
    /// <returns>The trigger or <see langword="null" /> if no trigger with the given name was found.</returns>
    new ITrigger? FindDeclaredTrigger(string name);

    /// <summary>
    ///     Returns the declared triggers on the entity type.
    /// </summary>
    new IEnumerable<ITrigger> GetDeclaredTriggers();

    internal const DynamicallyAccessedMemberTypes DynamicallyAccessedMemberTypes =
        System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.PublicConstructors
        | System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.NonPublicConstructors
        | System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.PublicProperties
        | System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.PublicFields
        | System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.NonPublicProperties
        | System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.NonPublicFields
        | System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.Interfaces;
}
