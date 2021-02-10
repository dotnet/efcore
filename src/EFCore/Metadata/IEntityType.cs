// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

#nullable enable

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     Represents an entity type in a model.
    /// </summary>
    public interface IEntityType : IReadOnlyEntityType, ITypeBase
    {
        /// <summary>
        ///     Gets the base type of this entity type. Returns <see langword="null" /> if this is not a derived type in an inheritance
        ///     hierarchy.
        /// </summary>
        new IEntityType? BaseType { get; }

        /// <summary>
        ///     Gets the <see cref="InstantiationBinding"/> for the preferred constructor.
        /// </summary>
        InstantiationBinding? ConstructorBinding { get; }

        /// <summary>
        ///     Gets primary key for this entity type. Returns <see langword="null" /> if no primary key is defined.
        /// </summary>
        /// <returns> The primary key, or <see langword="null" /> if none is defined. </returns>
        new IKey? FindPrimaryKey();

        /// <summary>
        ///     Gets the primary or alternate key that is defined on the given properties.
        ///     Returns <see langword="null" /> if no key is defined for the given properties.
        /// </summary>
        /// <param name="properties"> The properties that make up the key. </param>
        /// <returns> The key, or <see langword="null" /> if none is defined. </returns>
        new IKey? FindKey([NotNull] IReadOnlyList<IReadOnlyProperty> properties);

        /// <summary>
        ///     Gets the primary and alternate keys for this entity type.
        /// </summary>
        /// <returns> The primary and alternate keys. </returns>
        new IEnumerable<IKey> GetKeys();

        /// <summary>
        ///     Gets the foreign key for the given properties that points to a given primary or alternate key.
        ///     Returns <see langword="null" /> if no foreign key is found.
        /// </summary>
        /// <param name="properties"> The properties that the foreign key is defined on. </param>
        /// <param name="principalKey"> The primary or alternate key that is referenced. </param>
        /// <param name="principalEntityType">
        ///     The entity type that the relationship targets. This may be different from the type that <paramref name="principalKey" />
        ///     is defined on when the relationship targets a derived type in an inheritance hierarchy (since the key is defined on the
        ///     base type of the hierarchy).
        /// </param>
        /// <returns> The foreign key, or <see langword="null" /> if none is defined. </returns>
        new IForeignKey? FindForeignKey(
            [NotNull] IReadOnlyList<IReadOnlyProperty> properties,
            [NotNull] IReadOnlyKey principalKey,
            [NotNull] IReadOnlyEntityType principalEntityType);

        /// <summary>
        ///     Gets the foreign keys defined on this entity type.
        /// </summary>
        /// <returns> The foreign keys defined on this entity type. </returns>
        new IEnumerable<IForeignKey> GetForeignKeys();

        /// <summary>
        ///     Gets a skip navigation property on this entity type. Returns <see langword="null" /> if no navigation property is found.
        /// </summary>
        /// <param name="memberInfo"> The navigation property on the entity class. </param>
        /// <returns> The navigation property, or <see langword="null" /> if none is found. </returns>
        new ISkipNavigation? FindSkipNavigation([NotNull] MemberInfo memberInfo)
            => (ISkipNavigation?)((IReadOnlyEntityType)this).FindSkipNavigation(memberInfo);

        /// <summary>
        ///     Gets a skip navigation property on this entity type. Returns <see langword="null" /> if no skip navigation property is found.
        /// </summary>
        /// <param name="name"> The name of the navigation property on the entity class. </param>
        /// <returns> The navigation property, or <see langword="null" /> if none is found. </returns>
        new ISkipNavigation? FindSkipNavigation([NotNull] string name);

        /// <summary>
        ///     Gets a skip navigation property on this entity type. Does not return skip navigation properties defined on a base type.
        ///     Returns <see langword="null" /> if no skip navigation property is found.
        /// </summary>
        /// <param name="name"> The name of the navigation property on the entity class. </param>
        /// <returns> The navigation property, or <see langword="null" /> if none is found. </returns>
        new ISkipNavigation? FindDeclaredSkipNavigation([NotNull] string name)
            => (ISkipNavigation?)((IReadOnlyEntityType)this).FindDeclaredSkipNavigation(name);

        /// <summary>
        ///     <para>
        ///         Gets all skip navigation properties declared on this entity type.
        ///     </para>
        ///     <para>
        ///         This method does not return skip navigation properties declared declared on base types.
        ///         It is useful when iterating over all entity types to avoid processing the same foreign key more than once.
        ///         Use <see cref="GetSkipNavigations" /> to also return skip navigation properties declared on base types.
        ///     </para>
        /// </summary>
        /// <returns> Declared foreign keys. </returns>
        new IEnumerable<ISkipNavigation> GetDeclaredSkipNavigations()
            => ((IReadOnlyEntityType)this).GetDeclaredSkipNavigations().Cast<ISkipNavigation>();

        /// <summary>
        ///     Gets the skip navigation properties on this entity type.
        /// </summary>
        /// <returns> The skip navigation properties on this entity type. </returns>
        new IEnumerable<ISkipNavigation> GetSkipNavigations();

        /// <summary>
        ///     <para>
        ///         Gets the unnamed index defined on the given properties. Returns <see langword="null" /> if no such index is defined.
        ///     </para>
        ///     <para>
        ///         Named indexes will not be returned even if the list of properties matches.
        ///     </para>
        /// </summary>
        /// <param name="properties"> The properties to find the index on. </param>
        /// <returns> The index, or <see langword="null" /> if none is found. </returns>
        new IIndex? FindIndex([NotNull] IReadOnlyList<IReadOnlyProperty> properties);

        /// <summary>
        ///     Gets the index with the given name. Returns <see langword="null" /> if no such index exists.
        /// </summary>
        /// <param name="name"> The name of the index. </param>
        /// <returns> The index, or <see langword="null" /> if none is found. </returns>
        new IIndex? FindIndex([NotNull] string name);

        /// <summary>
        ///     Gets the indexes defined on this entity type.
        /// </summary>
        /// <returns> The indexes defined on this entity type. </returns>
        new IEnumerable<IIndex> GetIndexes();

        /// <summary>
        ///     <para>
        ///         Gets the property with a given name. Returns <see langword="null" /> if no property with the given name is defined.
        ///     </para>
        ///     <para>
        ///         This API only finds scalar properties and does not find navigation properties. Use
        ///         <see cref="FindNavigation(string)" /> to find a navigation property.
        ///     </para>
        /// </summary>
        /// <param name="name"> The name of the property. </param>
        /// <returns> The property, or <see langword="null" /> if none is found. </returns>
        new IProperty? FindProperty([NotNull] string name);

        /// <summary>
        ///     <para>
        ///         Gets the properties defined on this entity type.
        ///     </para>
        ///     <para>
        ///         This API only returns scalar properties and does not return navigation properties. Use
        ///         <see cref="GetNavigations()" /> to get navigation properties.
        ///     </para>
        /// </summary>
        /// <returns> The properties defined on this entity type. </returns>
        new IEnumerable<IProperty> GetProperties();

        /// <summary>
        ///     Returns the properties contained in foreign keys.
        /// </summary>
        /// <returns> The properties contained in foreign keys. </returns>
        IEnumerable<IProperty> GetForeignKeyProperties();

        /// <summary>
        ///     Returns the properties that need a value to be generated when the entity entry transitions to the
        ///     <see cref="EntityState.Added"/> state.
        /// </summary>
        /// <returns> The properties that need a value to be generated on add.</returns>
        IEnumerable<IProperty> GetValueGeneratingProperties();

        /// <summary>
        ///     <para>
        ///         Gets the <see cref="IServiceProperty" /> with a given name.
        ///         Returns <see langword="null" /> if no property with the given name is defined.
        ///     </para>
        ///     <para>
        ///         This API only finds service properties and does not find scalar or navigation properties.
        ///     </para>
        /// </summary>
        /// <param name="name"> The name of the property. </param>
        /// <returns> The service property, or <see langword="null" /> if none is found. </returns>
        new IServiceProperty? FindServiceProperty([NotNull] string name);

        /// <summary>
        ///     <para>
        ///         Gets all the <see cref="IServiceProperty" /> defined on this entity type.
        ///     </para>
        ///     <para>
        ///         This API only returns service properties and does not return scalar or navigation properties.
        ///     </para>
        /// </summary>
        /// <returns> The service properties defined on this entity type. </returns>
        new IEnumerable<IServiceProperty> GetServiceProperties();

        /// <summary>
        ///     Returns all the derived types of the given <see cref="IEntityType" />, including the type itself,
        ///     which are not <see langword="abstract" />.
        /// </summary>
        /// <returns> Non-abstract, derived types. </returns>
        IEnumerable<IEntityType> GetConcreteDerivedTypesInclusive()
            => ((IReadOnlyEntityType)this).GetConcreteDerivedTypesInclusive().Cast<IEntityType>();

        /// <summary>
        ///     Returns the <see cref="IProperty" /> that will be used for storing a discriminator value.
        /// </summary>
        IProperty? GetDiscriminatorProperty()
            => (IProperty?)((IReadOnlyEntityType)this).GetDiscriminatorProperty();

        /// <summary>
        ///     Gets the root base type for a given entity type.
        /// </summary>
        /// <returns>
        ///     The root base type. If the given entity type is not a derived type, then the same entity type is returned.
        /// </returns>
        IEntityType GetRootType()
            => (IEntityType)((IReadOnlyEntityType)this).GetRootType();

        /// <summary>
        ///     Gets all types in the model from which a given entity type derives, starting with the root.
        /// </summary>
        /// <returns> The base types. </returns>
        IEnumerable<IEntityType> GetAllBaseTypes()
            => GetAllBaseTypesAscending().Reverse();

        /// <summary>
        ///     Returns all base types of the given entity type, including the type itself, top to bottom.
        /// </summary>
        /// <returns> The base types. </returns>
        IEnumerable<IEntityType> GetAllBaseTypesInclusive()
            => GetAllBaseTypesInclusiveAscending().Reverse();

        /// <summary>
        ///     Gets all types in the model from which a given entity type derives, starting with the closest one.
        /// </summary>
        /// <returns> The base types. </returns>
        IEnumerable<IEntityType> GetAllBaseTypesAscending()
            => GetAllBaseTypesInclusiveAscending().Skip(1);

        /// <summary>
        ///     Returns all base types of the given entity type, including the type itself, bottom to top.
        /// </summary>
        /// <returns> The base types. </returns>
        IEnumerable<IEntityType> GetAllBaseTypesInclusiveAscending()
            => ((IReadOnlyEntityType)this).GetAllBaseTypesInclusiveAscending().Cast<IEntityType>();

        /// <summary>
        ///     Gets all types in the model that derive from a given entity type.
        /// </summary>
        /// <returns> The derived types. </returns>
        IEnumerable<IEntityType> GetDerivedTypes()
            => ((IReadOnlyEntityType)this).GetDerivedTypes().Cast<IEntityType>();

        /// <summary>
        ///     Returns all derived types of the given <see cref="IEntityType" />, including the type itself.
        /// </summary>
        /// <returns> Derived types. </returns>
        IEnumerable<IEntityType> GetDerivedTypesInclusive()
            => ((IReadOnlyEntityType)this).GetDerivedTypesInclusive().Cast<IEntityType>();

        /// <summary>
        ///     Gets all types in the model that directly derive from a given entity type.
        /// </summary>
        /// <returns> The derived types. </returns>
        IEnumerable<IEntityType> GetDirectlyDerivedTypes();

        /// <summary>
        ///     <para>
        ///         Gets all keys declared on the given <see cref="IReadOnlyEntityType" />.
        ///     </para>
        ///     <para>
        ///         This method does not return keys declared on base types.
        ///         It is useful when iterating over all entity types to avoid processing the same key more than once.
        ///         Use <see cref="IConventionEntityType.GetKeys" /> to also return keys declared on base types.
        ///     </para>
        /// </summary>
        /// <returns> Declared keys. </returns>
        IEnumerable<IKey> GetDeclaredKeys();

        /// <summary>
        ///     Gets the primary or alternate key that is defined on the given property. Returns <see langword="null" /> if no key is defined
        ///     for the given property.
        ///     /// </summary>
        /// <param name="property"> The property that the key is defined on. </param>
        /// <returns> The key, or null if none is defined. </returns>
        IKey? FindKey([NotNull] IReadOnlyProperty property) => FindKey(new[] { property });

        /// <summary>
        ///     <para>
        ///         Gets all non-navigation properties declared on the given <see cref="IEntityType" />.
        ///     </para>
        ///     <para>
        ///         This method does not return properties declared on base types.
        ///         It is useful when iterating over all entity types to avoid processing the same property more than once.
        ///         Use <see cref="GetProperties" /> to also return properties declared on base types.
        ///     </para>
        /// </summary>
        /// <returns> Declared non-navigation properties. </returns>
        IEnumerable<IProperty> GetDeclaredProperties();

        /// <summary>
        ///     <para>
        ///         Gets all navigation properties declared on the given <see cref="IEntityType" />.
        ///     </para>
        ///     <para>
        ///         This method does not return navigation properties declared on base types.
        ///         It is useful when iterating over all entity types to avoid processing the same navigation property more than once.
        ///         Use <see cref="GetNavigations" /> to also return navigation properties declared on base types.
        ///     </para>
        /// </summary>
        /// <returns> Declared navigation properties. </returns>
        IEnumerable<INavigation> GetDeclaredNavigations();

        /// <summary>
        ///     <para>
        ///         Gets all service properties declared on the given <see cref="IEntityType" />.
        ///     </para>
        ///     <para>
        ///         This method does not return properties declared on base types.
        ///         It is useful when iterating over all entity types to avoid processing the same property more than once.
        ///         Use <see cref="GetServiceProperties" /> to also return properties declared on base types.
        ///     </para>
        /// </summary>
        /// <returns> Declared service properties. </returns>
        IEnumerable<IServiceProperty> GetDeclaredServiceProperties();

        /// <summary>
        ///     <para>
        ///         Gets all indexes declared on the given <see cref="IEntityType" />.
        ///     </para>
        ///     <para>
        ///         This method does not return indexes declared on base types.
        ///         It is useful when iterating over all entity types to avoid processing the same index more than once.
        ///         Use <see cref="GetIndexes" /> to also return indexes declared on base types.
        ///     </para>
        /// </summary>
        /// <returns> Declared indexes. </returns>
        IEnumerable<IIndex> GetDeclaredIndexes();

        /// <summary>
        ///     <para>
        ///         Gets all indexes declared on the types derived from the given <see cref="IEntityType" />.
        ///     </para>
        /// </summary>
        /// <returns> Derived indexes. </returns>
        IEnumerable<IIndex> GetDerivedIndexes();

        /// <summary>
        ///     <para>
        ///         Gets all foreign keys declared on the given <see cref="IEntityType" />.
        ///     </para>
        ///     <para>
        ///         This method does not return foreign keys declared on base types.
        ///         It is useful when iterating over all entity types to avoid processing the same foreign key more than once.
        ///         Use <see cref="GetForeignKeys" /> to also return foreign keys declared on base types.
        ///     </para>
        /// </summary>
        /// <returns> Declared foreign keys. </returns>
        IEnumerable<IForeignKey> GetDeclaredForeignKeys();

        /// <summary>
        ///     <para>
        ///         Gets all foreign keys declared on the types derived from the given <see cref="IEntityType" />.
        ///     </para>
        /// </summary>
        /// <returns> Derived foreign keys. </returns>
        IEnumerable<IForeignKey> GetDerivedForeignKeys();

        /// <summary>
        ///     Gets the foreign keys defined on the given property. Only foreign keys that are defined on exactly the specified
        ///     property are returned. Composite foreign keys that include the specified property are not returned.
        /// </summary>
        /// <param name="property"> The property to find the foreign keys on. </param>
        /// <returns> The foreign keys. </returns>
        IEnumerable<IForeignKey> FindForeignKeys([NotNull] IReadOnlyProperty property)
            => FindForeignKeys(new[] { property });

        /// <summary>
        ///     Gets the foreign keys defined on the given properties. Only foreign keys that are defined on exactly the specified
        ///     set of properties are returned.
        /// </summary>
        /// <param name="properties"> The properties to find the foreign keys on. </param>
        /// <returns> The foreign keys. </returns>
        IEnumerable<IForeignKey> FindForeignKeys([NotNull] IReadOnlyList<IReadOnlyProperty> properties);

        /// <summary>
        ///     Gets the foreign key for the given properties that points to a given primary or alternate key. Returns <see langword="null" />
        ///     if no foreign key is found.
        /// </summary>
        /// <param name="property"> The property that the foreign key is defined on. </param>
        /// <param name="principalKey"> The primary or alternate key that is referenced. </param>
        /// <param name="principalEntityType">
        ///     The entity type that the relationship targets. This may be different from the type that <paramref name="principalKey" />
        ///     is defined on when the relationship targets a derived type in an inheritance hierarchy (since the key is defined on the
        ///     base type of the hierarchy).
        /// </param>
        /// <returns> The foreign key, or <see langword="null" /> if none is defined. </returns>
        IForeignKey? FindForeignKey(
            [NotNull] IReadOnlyProperty property,
            [NotNull] IReadOnlyKey principalKey,
            [NotNull] IReadOnlyEntityType principalEntityType)
            => FindForeignKey(new[] { property }, principalKey, principalEntityType);

        /// <summary>
        ///     Gets the foreign keys declared on the given <see cref="IEntityType" /> using the given properties.
        /// </summary>
        /// <param name="properties"> The properties to find the foreign keys on. </param>
        /// <returns> Declared foreign keys. </returns>
        IEnumerable<IForeignKey> FindDeclaredForeignKeys([NotNull] IReadOnlyList<IReadOnlyProperty> properties);

        /// <summary>
        ///     Gets all foreign keys that target a given entity type (i.e. foreign keys where the given entity type
        ///     or a type it's derived from is the principal).
        /// </summary>
        /// <returns> The foreign keys that reference the given entity type. </returns>
        IEnumerable<IForeignKey> GetReferencingForeignKeys();

        /// <summary>
        ///     Gets all foreign keys that target a given entity type (i.e. foreign keys where the given entity type
        ///     is the principal).
        /// </summary>
        /// <returns> The foreign keys that reference the given entity type. </returns>
        IEnumerable<IForeignKey> GetDeclaredReferencingForeignKeys();

        /// <summary>
        ///     Returns the relationship to the owner if this is an owned type or <see langword="null" /> otherwise.
        /// </summary>
        /// <returns> The relationship to the owner if this is an owned type or <see langword="null" /> otherwise. </returns>
        IForeignKey? FindOwnership()
            => (IForeignKey?)((IReadOnlyEntityType)this).FindOwnership();

        /// <summary>
        ///     Gets a navigation property on the given entity type. Returns <see langword="null" /> if no navigation property is found.
        /// </summary>
        /// <param name="memberInfo"> The navigation property on the entity class. </param>
        /// <returns> The navigation property, or <see langword="null" /> if none is found. </returns>
        INavigation? FindNavigation([NotNull] MemberInfo memberInfo)
            => FindNavigation(Check.NotNull(memberInfo, nameof(memberInfo)).GetSimpleMemberName());

        /// <summary>
        ///     Gets a navigation property on the given entity type. Returns <see langword="null" /> if no navigation property is found.
        /// </summary>
        /// <param name="name"> The name of the navigation property on the entity class. </param>
        /// <returns> The navigation property, or <see langword="null" /> if none is found. </returns>
        INavigation? FindNavigation([NotNull] string name)
            => (INavigation?)((IReadOnlyEntityType)this).FindNavigation(name);

        /// <summary>
        ///     Gets a navigation property on the given entity type. Does not return navigation properties defined on a base type.
        ///     Returns <see langword="null" /> if no navigation property is found.
        /// </summary>
        /// <param name="name"> The name of the navigation property on the entity class. </param>
        /// <returns> The navigation property, or <see langword="null" /> if none is found. </returns>
        INavigation? FindDeclaredNavigation([NotNull] string name);

        /// <summary>
        ///     Gets all navigation properties on the given entity type.
        /// </summary>
        /// <returns> All navigation properties on the given entity type. </returns>
        IEnumerable<INavigation> GetNavigations()
            => ((IReadOnlyEntityType)this).GetNavigations().Cast<INavigation>();

        /// <summary>
        ///     <para>
        ///         Gets a property on the given entity type. Returns <see langword="null" /> if no property is found.
        ///     </para>
        ///     <para>
        ///         This API only finds scalar properties and does not find navigation properties. Use
        ///         <see cref="FindNavigation(MemberInfo)" /> to find a navigation property.
        ///     </para>
        /// </summary>
        /// <param name="memberInfo"> The property on the entity class. </param>
        /// <returns> The property, or <see langword="null" /> if none is found. </returns>
        IProperty? FindProperty([NotNull] MemberInfo memberInfo)
            => (IProperty?)((IReadOnlyEntityType)this).FindProperty(memberInfo);

        /// <summary>
        ///     <para>
        ///         Finds matching properties on the given entity type. Returns <see langword="null" /> if any property is not found.
        ///     </para>
        ///     <para>
        ///         This API only finds scalar properties and does not find navigation properties.
        ///     </para>
        /// </summary>
        /// <param name="propertyNames"> The property names. </param>
        /// <returns> The properties, or <see langword="null" /> if any property is not found. </returns>
        IReadOnlyList<IProperty>? FindProperties(
            [NotNull] IReadOnlyList<string> propertyNames)
            => (IReadOnlyList<IProperty>?)((IReadOnlyEntityType)this).FindProperties(propertyNames);

        /// <summary>
        ///     <para>
        ///         Gets a property with the given name.
        ///     </para>
        ///     <para>
        ///         This API only finds scalar properties and does not find navigation properties. Use
        ///         <see cref="FindNavigation(string)" /> to find a navigation property.
        ///     </para>
        /// </summary>
        /// <param name="name"> The property name. </param>
        /// <returns> The property, or <see langword="null" /> if none is found. </returns>
        IProperty GetProperty([NotNull] string name)
            => (IProperty)((IReadOnlyEntityType)this).GetProperty(name);

        /// <summary>
        ///     Finds a property declared on the type with the given name.
        ///     Does not return properties defined on a base type.
        /// </summary>
        /// <param name="name"> The property name. </param>
        /// <returns> The property, or <see langword="null" /> if none is found. </returns>
        IProperty? FindDeclaredProperty([NotNull] string name);

        /// <summary>
        ///     Gets the index defined on the given property. Returns null if no index is defined.
        /// </summary>
        /// <param name="property"> The property to find the index on. </param>
        /// <returns> The index, or null if none is found. </returns>
        IIndex? FindIndex([NotNull] IReadOnlyProperty property)
            => FindIndex(new[] { property });
    }
}
