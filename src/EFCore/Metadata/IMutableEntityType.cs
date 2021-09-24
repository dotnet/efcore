// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     <para>
    ///         Represents an entity type in an <see cref="IMutableModel" />.
    ///     </para>
    ///     <para>
    ///         This interface is used during model creation and allows the metadata to be modified.
    ///         Once the model is built, <see cref="IEntityType" /> represents a read-only view of the same metadata.
    ///     </para>
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information.
    /// </remarks>
    public interface IMutableEntityType : IReadOnlyEntityType, IMutableTypeBase
    {
        /// <summary>
        ///     Gets the model this entity belongs to.
        /// </summary>
        new IMutableModel Model { get; }

        /// <summary>
        ///     Gets or sets the base type of this entity type. Returns <see langword="null" /> if this is not a derived type in an inheritance
        ///     hierarchy.
        /// </summary>
        new IMutableEntityType? BaseType { get; set; }

        /// <summary>
        ///     Gets the defining entity type.
        /// </summary>
        [Obsolete("Entity types with defining navigations have been replaced by shared-type entity types")]
        new IMutableEntityType? DefiningEntityType
            => null;

        /// <summary>
        ///     Adds seed data to this entity type. It is used to generate data motion migrations.
        /// </summary>
        /// <param name="data">
        ///     An array of seed data represented by anonymous types or entities.
        /// </param>
        void AddData(IEnumerable<object> data);

        /// <summary>
        ///     Gets or sets a value indicating whether the entity type has no keys.
        ///     If set to <see langword="true" /> it will only be usable for queries.
        /// </summary>
        bool IsKeyless { get; set; }

        /// <summary>
        ///     Sets the change tracking strategy to use for this entity type. This strategy indicates how the
        ///     context detects changes to properties for an instance of the entity type.
        /// </summary>
        /// <param name="changeTrackingStrategy">The strategy to use.</param>
        void SetChangeTrackingStrategy(ChangeTrackingStrategy? changeTrackingStrategy);

        /// <summary>
        ///     Sets the LINQ expression filter automatically applied to queries for this entity type.
        /// </summary>
        /// <param name="queryFilter">The LINQ expression filter.</param>
        void SetQueryFilter(LambdaExpression? queryFilter);

        /// <summary>
        ///     Returns the property that will be used for storing a discriminator value.
        /// </summary>
        /// <returns>The property that will be used for storing a discriminator value.</returns>
        new IMutableProperty? FindDiscriminatorProperty()
            => (IMutableProperty?)((IReadOnlyEntityType)this).FindDiscriminatorProperty();

        /// <summary>
        ///     Sets the <see cref="IReadOnlyProperty" /> that will be used for storing a discriminator value.
        /// </summary>
        /// <param name="property">The property to set.</param>
        void SetDiscriminatorProperty(IReadOnlyProperty? property);

        /// <summary>
        ///     Sets the value indicating whether the discriminator mapping is complete.
        /// </summary>
        /// <param name="complete">The value indicating whether the discriminator mapping is complete.</param>
        void SetDiscriminatorMappingComplete(bool? complete)
            => SetOrRemoveAnnotation(CoreAnnotationNames.DiscriminatorMappingComplete, complete);

        /// <summary>
        ///     Sets the discriminator value for this entity type.
        /// </summary>
        /// <param name="value">The value to set.</param>
        void SetDiscriminatorValue(object? value)
            => SetAnnotation(CoreAnnotationNames.DiscriminatorValue, EntityType.CheckDiscriminatorValue(this, value));

        /// <summary>
        ///     Removes the discriminator value for this entity type.
        /// </summary>
        void RemoveDiscriminatorValue()
            => RemoveAnnotation(CoreAnnotationNames.DiscriminatorValue);

        /// <summary>
        ///     Gets all types in the model from which a given entity type derives, starting with the root.
        /// </summary>
        /// <returns>
        ///     The base types.
        /// </returns>
        new IEnumerable<IMutableEntityType> GetAllBaseTypes()
            => GetAllBaseTypesAscending().Reverse();

        /// <summary>
        ///     Gets all types in the model from which a given entity type derives, starting with the closest one.
        /// </summary>
        /// <returns>
        ///     The base types.
        /// </returns>
        new IEnumerable<IMutableEntityType> GetAllBaseTypesAscending()
            => GetAllBaseTypesInclusiveAscending().Skip(1);

        /// <summary>
        ///     Returns all base types of this entity type, including the type itself, top to bottom.
        /// </summary>
        /// <returns>Base types.</returns>
        new IEnumerable<IMutableEntityType> GetAllBaseTypesInclusive()
            => ((IReadOnlyEntityType)this).GetAllBaseTypesInclusive().Cast<IMutableEntityType>();

        /// <summary>
        ///     Returns all base types of the given entity type, including the type itself, bottom to top.
        /// </summary>
        /// <returns>Base types.</returns>
        new IEnumerable<IMutableEntityType> GetAllBaseTypesInclusiveAscending()
            => ((IReadOnlyEntityType)this).GetAllBaseTypesInclusiveAscending().Cast<IMutableEntityType>();

        /// <summary>
        ///     Gets all types in the model that derive from a given entity type.
        /// </summary>
        /// <returns>The derived types.</returns>
        new IEnumerable<IMutableEntityType> GetDerivedTypes()
            => ((IReadOnlyEntityType)this).GetDerivedTypes().Cast<IMutableEntityType>();

        /// <summary>
        ///     Returns all derived types of this entity type, including the type itself.
        /// </summary>
        /// <returns>Derived types.</returns>
        new IEnumerable<IMutableEntityType> GetDerivedTypesInclusive()
            => ((IReadOnlyEntityType)this).GetDerivedTypesInclusive().Cast<IMutableEntityType>();

        /// <summary>
        ///     Gets all types in the model that directly derive from a given entity type.
        /// </summary>
        /// <returns>The derived types.</returns>
        new IEnumerable<IMutableEntityType> GetDirectlyDerivedTypes()
            => ((IReadOnlyEntityType)this).GetDirectlyDerivedTypes().Cast<IMutableEntityType>();

        /// <summary>
        ///     Gets the root base type for a given entity type.
        /// </summary>
        /// <returns>
        ///     The root base type. If the given entity type is not a derived type, then the same entity type is returned.
        /// </returns>
        new IMutableEntityType GetRootType()
            => (IMutableEntityType)((IReadOnlyEntityType)this).GetRootType();

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
        new IMutableEntityType? FindClosestCommonParent(IReadOnlyEntityType otherEntityType)
            => (IMutableEntityType?)((IReadOnlyEntityType)this).FindClosestCommonParent(otherEntityType);

        /// <summary>
        ///     Gets the least derived type between the specified two.
        /// </summary>
        /// <param name="otherEntityType">The other entity type to compare with.</param>
        /// <returns>
        ///     The least derived type between the specified two.
        ///     If the given entity types are not related, then <see langword="null" /> is returned.
        /// </returns>
        new IMutableEntityType? LeastDerivedType(IReadOnlyEntityType otherEntityType)
            => (IMutableEntityType?)((IReadOnlyEntityType)this).LeastDerivedType(otherEntityType);

        /// <summary>
        ///     Sets the primary key for this entity type.
        /// </summary>
        /// <param name="properties">The properties that make up the primary key.</param>
        /// <returns>The newly created key.</returns>
        IMutableKey? SetPrimaryKey(IReadOnlyList<IMutableProperty>? properties);

        /// <summary>
        ///     Sets the primary key for this entity type.
        /// </summary>
        /// <param name="property">The primary key property.</param>
        /// <returns>The newly created key.</returns>
        IMutableKey? SetPrimaryKey(IMutableProperty? property)
            => SetPrimaryKey(property == null ? null : new[] { property });

        /// <summary>
        ///     Gets primary key for this entity type. Returns <see langword="null" /> if no primary key is defined.
        /// </summary>
        /// <returns>The primary key, or <see langword="null" /> if none is defined.</returns>
        new IMutableKey? FindPrimaryKey();

        /// <summary>
        ///     Adds a new alternate key to this entity type.
        /// </summary>
        /// <param name="property">The property to use as an alternate key.</param>
        /// <returns>The newly created key.</returns>
        IMutableKey AddKey(IMutableProperty property)
            => AddKey(new[] { property });

        /// <summary>
        ///     Adds a new alternate key to this entity type.
        /// </summary>
        /// <param name="properties">The properties that make up the alternate key.</param>
        /// <returns>The newly created key.</returns>
        IMutableKey AddKey(IReadOnlyList<IMutableProperty> properties);

        /// <summary>
        ///     Gets the primary or alternate key that is defined on the given property. Returns <see langword="null" /> if no key is defined
        ///     for the given property.
        /// </summary>
        /// <param name="property">The property that the key is defined on.</param>
        /// <returns>The key, or null if none is defined.</returns>
        new IMutableKey? FindKey(IReadOnlyProperty property)
            => FindKey(new[] { property });

        /// <summary>
        ///     Gets the primary or alternate key that is defined on the given properties.
        ///     Returns <see langword="null" /> if no key is defined for the given properties.
        /// </summary>
        /// <param name="properties">The properties that make up the key.</param>
        /// <returns>The key, or <see langword="null" /> if none is defined.</returns>
        new IMutableKey? FindKey(IReadOnlyList<IReadOnlyProperty> properties);

        /// <summary>
        ///     <para>
        ///         Gets all keys declared on the given <see cref="IReadOnlyEntityType" />.
        ///     </para>
        ///     <para>
        ///         This method does not return keys declared on base types.
        ///         It is useful when iterating over all entity types to avoid processing the same key more than once.
        ///         Use <see cref="GetKeys" /> to also return keys declared on base types.
        ///     </para>
        /// </summary>
        /// <returns>Declared keys.</returns>
        new IEnumerable<IMutableKey> GetDeclaredKeys()
            => ((IReadOnlyEntityType)this).GetDeclaredKeys().Cast<IMutableKey>();

        /// <summary>
        ///     Gets the primary and alternate keys for this entity type.
        /// </summary>
        /// <returns>The primary and alternate keys.</returns>
        new IEnumerable<IMutableKey> GetKeys();

        /// <summary>
        ///     Removes a primary or alternate key from this entity type.
        /// </summary>
        /// <param name="properties">The properties that make up the key.</param>
        /// <returns>The removed key, or <see langword="null" /> if the key was not found.</returns>
        IMutableKey? RemoveKey(IReadOnlyList<IReadOnlyProperty> properties);

        /// <summary>
        ///     Removes a primary or alternate key from this entity type.
        /// </summary>
        /// <param name="key">The key to be removed.</param>
        /// <returns>The removed key, or <see langword="null" /> if the key was not found.</returns>
        IMutableKey? RemoveKey(IReadOnlyKey key);

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
        /// <returns>The newly created foreign key.</returns>
        IMutableForeignKey AddForeignKey(
            IMutableProperty property,
            IMutableKey principalKey,
            IMutableEntityType principalEntityType)
            => AddForeignKey(new[] { property }, principalKey, principalEntityType);

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
        /// <returns>The newly created foreign key.</returns>
        IMutableForeignKey AddForeignKey(
            IReadOnlyList<IMutableProperty> properties,
            IMutableKey principalKey,
            IMutableEntityType principalEntityType);

        /// <summary>
        ///     Gets the foreign keys defined on the given property. Only foreign keys that are defined on exactly the specified
        ///     property are returned. Composite foreign keys that include the specified property are not returned.
        /// </summary>
        /// <param name="property">The property to find the foreign keys on.</param>
        /// <returns>The foreign keys.</returns>
        new IEnumerable<IMutableForeignKey> FindForeignKeys(IReadOnlyProperty property)
            => FindForeignKeys(new[] { property });

        /// <summary>
        ///     Gets the foreign keys defined on the given properties. Only foreign keys that are defined on exactly the specified
        ///     set of properties are returned.
        /// </summary>
        /// <param name="properties">The properties to find the foreign keys on.</param>
        /// <returns>The foreign keys.</returns>
        new IEnumerable<IMutableForeignKey> FindForeignKeys(IReadOnlyList<IReadOnlyProperty> properties)
            => ((IReadOnlyEntityType)this).FindForeignKeys(properties).Cast<IMutableForeignKey>();

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
        new IMutableForeignKey? FindForeignKey(
            IReadOnlyProperty property,
            IReadOnlyKey principalKey,
            IReadOnlyEntityType principalEntityType)
            => FindForeignKey(new[] { property }, principalKey, principalEntityType);

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
        new IMutableForeignKey? FindForeignKey(
            IReadOnlyList<IReadOnlyProperty> properties,
            IReadOnlyKey principalKey,
            IReadOnlyEntityType principalEntityType);

        /// <summary>
        ///     Gets the foreign keys declared on this entity type using the given properties.
        /// </summary>
        /// <param name="properties">The properties to find the foreign keys on.</param>
        /// <returns>Declared foreign keys.</returns>
        new IEnumerable<IMutableForeignKey> FindDeclaredForeignKeys(
            IReadOnlyList<IReadOnlyProperty> properties)
            => ((IReadOnlyEntityType)this).FindDeclaredForeignKeys(properties).Cast<IMutableForeignKey>();

        /// <summary>
        ///     <para>
        ///         Gets all foreign keys declared on this entity type.
        ///     </para>
        ///     <para>
        ///         This method does not return foreign keys declared on base types.
        ///         It is useful when iterating over all entity types to avoid processing the same foreign key more than once.
        ///         Use <see cref="GetForeignKeys" /> to also return foreign keys declared on base types.
        ///     </para>
        /// </summary>
        /// <returns>Declared foreign keys.</returns>
        new IEnumerable<IMutableForeignKey> GetDeclaredForeignKeys()
            => ((IReadOnlyEntityType)this).GetDeclaredForeignKeys().Cast<IMutableForeignKey>();

        /// <summary>
        ///     <para>
        ///         Gets all foreign keys declared on the types derived from this entity type.
        ///     </para>
        /// </summary>
        /// <returns>Derived foreign keys.</returns>
        new IEnumerable<IMutableForeignKey> GetDerivedForeignKeys()
            => ((IReadOnlyEntityType)this).GetDerivedForeignKeys().Cast<IMutableForeignKey>();

        /// <summary>
        ///     Gets the foreign keys defined on this entity type.
        /// </summary>
        /// <returns>The foreign keys defined on this entity type.</returns>
        new IEnumerable<IMutableForeignKey> GetForeignKeys();

        /// <summary>
        ///     Gets all foreign keys that target a given entity type (i.e. foreign keys where the given entity type
        ///     is the principal).
        /// </summary>
        /// <returns>The foreign keys that reference the given entity type.</returns>
        new IEnumerable<IMutableForeignKey> GetDeclaredReferencingForeignKeys()
            => ((IReadOnlyEntityType)this).GetDeclaredReferencingForeignKeys().Cast<IMutableForeignKey>();

        /// <summary>
        ///     Gets all foreign keys that target a given entity type (i.e. foreign keys where the given entity type
        ///     or a type it's derived from is the principal).
        /// </summary>
        /// <returns>The foreign keys that reference the given entity type.</returns>
        new IEnumerable<IMutableForeignKey> GetReferencingForeignKeys()
            => ((IReadOnlyEntityType)this).GetReferencingForeignKeys().Cast<IMutableForeignKey>();

        /// <summary>
        ///     Returns the relationship to the owner if this is an owned type or <see langword="null" /> otherwise.
        /// </summary>
        /// <returns>The relationship to the owner if this is an owned type or <see langword="null" /> otherwise.</returns>
        new IMutableForeignKey? FindOwnership()
            => (IMutableForeignKey?)((IReadOnlyEntityType)this).FindOwnership();

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
        /// <returns>The removed foreign key, or <see langword="null" /> if the index was not found.</returns>
        IMutableForeignKey? RemoveForeignKey(
            IReadOnlyList<IReadOnlyProperty> properties,
            IMutableKey principalKey,
            IMutableEntityType principalEntityType);

        /// <summary>
        ///     Removes a foreign key from this entity type.
        /// </summary>
        /// <param name="foreignKey">The foreign key to be removed.</param>
        /// <returns>The removed foreign key, or <see langword="null" /> if the index was not found.</returns>
        IMutableForeignKey? RemoveForeignKey(IReadOnlyForeignKey foreignKey);

        /// <summary>
        ///     Gets a navigation property on the given entity type. Returns <see langword="null" /> if no navigation property is found.
        /// </summary>
        /// <param name="memberInfo">The navigation property on the entity class.</param>
        /// <returns>The navigation property, or <see langword="null" /> if none is found.</returns>
        new IMutableNavigation? FindNavigation(MemberInfo memberInfo)
            => FindNavigation(Check.NotNull(memberInfo, nameof(memberInfo)).GetSimpleMemberName());

        /// <summary>
        ///     Gets a navigation property on the given entity type. Returns <see langword="null" /> if no navigation property is found.
        /// </summary>
        /// <param name="name">The name of the navigation property on the entity class.</param>
        /// <returns>The navigation property, or <see langword="null" /> if none is found.</returns>
        new IMutableNavigation? FindNavigation(string name)
            => (IMutableNavigation?)((IReadOnlyEntityType)this).FindNavigation(name);

        /// <summary>
        ///     Gets a navigation property on the given entity type. Does not return navigation properties defined on a base type.
        ///     Returns <see langword="null" /> if no navigation property is found.
        /// </summary>
        /// <param name="name">The name of the navigation property on the entity class.</param>
        /// <returns>The navigation property, or <see langword="null" /> if none is found.</returns>
        new IMutableNavigation? FindDeclaredNavigation(string name)
            => (IMutableNavigation?)((IReadOnlyEntityType)this).FindDeclaredNavigation(name);

        /// <summary>
        ///     <para>
        ///         Gets all navigation properties declared on this entity type.
        ///     </para>
        ///     <para>
        ///         This method does not return navigation properties declared on base types.
        ///         It is useful when iterating over all entity types to avoid processing the same navigation property more than once.
        ///         Use <see cref="GetNavigations" /> to also return navigation properties declared on base types.
        ///     </para>
        /// </summary>
        /// <returns>Declared navigation properties.</returns>
        new IEnumerable<IMutableNavigation> GetDeclaredNavigations()
            => ((IReadOnlyEntityType)this).GetDeclaredNavigations().Cast<IMutableNavigation>();

        /// <summary>
        ///     <para>
        ///         Gets all navigation properties declared on the types derived from this entity type.
        ///     </para>
        ///     <para>
        ///         This method does not return navigation properties declared on the given entity type itself.
        ///         Use <see cref="GetNavigations" /> to return navigation properties declared on this
        ///         and base entity typed types.
        ///     </para>
        /// </summary>
        /// <returns>Derived navigation properties.</returns>
        new IEnumerable<IMutableNavigation> GetDerivedNavigations()
            => ((IReadOnlyEntityType)this).GetDerivedNavigations().Cast<IMutableNavigation>();

        /// <summary>
        ///     Gets all navigation properties on the given entity type.
        /// </summary>
        /// <returns>All navigation properties on the given entity type.</returns>
        new IEnumerable<IMutableNavigation> GetNavigations()
            => ((IReadOnlyEntityType)this).GetNavigations().Cast<IMutableNavigation>();

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
        /// <returns>The newly created skip navigation property.</returns>
        IMutableSkipNavigation AddSkipNavigation(
            string name,
            MemberInfo? memberInfo,
            IMutableEntityType targetEntityType,
            bool collection,
            bool onDependent);

        /// <summary>
        ///     Gets a skip navigation property on this entity type. Returns <see langword="null" /> if no navigation property is found.
        /// </summary>
        /// <param name="memberInfo">The navigation property on the entity class.</param>
        /// <returns>The navigation property, or <see langword="null" /> if none is found.</returns>
        new IMutableSkipNavigation? FindSkipNavigation(MemberInfo memberInfo)
            => (IMutableSkipNavigation?)((IReadOnlyEntityType)this).FindSkipNavigation(memberInfo);

        /// <summary>
        ///     Gets a skip navigation property on this entity type. Returns <see langword="null" /> if no skip navigation property is found.
        /// </summary>
        /// <param name="name">The name of the navigation property on the entity class.</param>
        /// <returns>The navigation property, or <see langword="null" /> if none is found.</returns>
        new IMutableSkipNavigation? FindSkipNavigation(string name);

        /// <summary>
        ///     Gets a skip navigation property on this entity type. Does not return skip navigation properties defined on a base type.
        ///     Returns <see langword="null" /> if no skip navigation property is found.
        /// </summary>
        /// <param name="name">The name of the navigation property on the entity class.</param>
        /// <returns>The navigation property, or <see langword="null" /> if none is found.</returns>
        new IMutableSkipNavigation? FindDeclaredSkipNavigation(string name)
            => (IMutableSkipNavigation?)((IReadOnlyEntityType)this).FindDeclaredSkipNavigation(name);

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
        /// <returns>Declared foreign keys.</returns>
        new IEnumerable<IMutableSkipNavigation> GetDeclaredSkipNavigations()
            => ((IReadOnlyEntityType)this).GetDeclaredSkipNavigations().Cast<IMutableSkipNavigation>();

        /// <summary>
        ///     <para>
        ///         Gets all skip navigation properties declared on the types derived from this entity type.
        ///     </para>
        ///     <para>
        ///         This method does not return skip navigation properties declared on the given entity type itself.
        ///         Use <see cref="GetSkipNavigations" /> to return skip navigation properties declared on this
        ///         and base entity typed types.
        ///     </para>
        /// </summary>
        /// <returns>Derived skip navigation properties.</returns>
        new IEnumerable<IMutableSkipNavigation> GetDerivedSkipNavigations()
            => ((IReadOnlyEntityType)this).GetDerivedSkipNavigations().Cast<IMutableSkipNavigation>();

        /// <summary>
        ///     Gets the skip navigation properties on this entity type.
        /// </summary>
        /// <returns>The skip navigation properties on this entity type.</returns>
        new IEnumerable<IMutableSkipNavigation> GetSkipNavigations();

        /// <summary>
        ///     Removes a skip navigation properties from this entity type.
        /// </summary>
        /// <param name="navigation">The skip navigation to be removed.</param>
        /// <returns>The removed skip navigation, or <see langword="null" /> if the skip navigation was not found.</returns>
        IMutableSkipNavigation? RemoveSkipNavigation(IReadOnlySkipNavigation navigation);

        /// <summary>
        ///     Adds an unnamed index to this entity type.
        /// </summary>
        /// <param name="property">The property to be indexed.</param>
        /// <returns>The newly created index.</returns>
        IMutableIndex AddIndex(IMutableProperty property)
            => AddIndex(new[] { property });

        /// <summary>
        ///     Adds an unnamed index to this entity type.
        /// </summary>
        /// <param name="properties">The properties that are to be indexed.</param>
        /// <returns>The newly created index.</returns>
        IMutableIndex AddIndex(IReadOnlyList<IMutableProperty> properties);

        /// <summary>
        ///     Adds a named index to this entity type.
        /// </summary>
        /// <param name="property">The property to be indexed.</param>
        /// <param name="name">The name of the index.</param>
        /// <returns>The newly created index.</returns>
        IMutableIndex AddIndex(IMutableProperty property, string name)
            => AddIndex(new[] { property }, name);

        /// <summary>
        ///     Adds a named index to this entity type.
        /// </summary>
        /// <param name="properties">The properties that are to be indexed.</param>
        /// <param name="name">The name of the index.</param>
        /// <returns>The newly created index.</returns>
        IMutableIndex AddIndex(IReadOnlyList<IMutableProperty> properties, string name);

        /// <summary>
        ///     Gets the index defined on the given property. Returns <see langword="null" /> if no index is defined.
        /// </summary>
        /// <param name="property">The property to find the index on.</param>
        /// <returns>The index, or <see langword="null" /> if none is found.</returns>
        new IMutableIndex? FindIndex(IReadOnlyProperty property)
            => FindIndex(new[] { property });

        /// <summary>
        ///     <para>
        ///         Gets the unnamed index defined on the given properties. Returns <see langword="null" /> if no such index is defined.
        ///     </para>
        ///     <para>
        ///         Named indexes will not be returned even if the list of properties matches.
        ///     </para>
        /// </summary>
        /// <param name="properties">The properties to find the index on.</param>
        /// <returns>The index, or <see langword="null" /> if none is found.</returns>
        new IMutableIndex? FindIndex(IReadOnlyList<IReadOnlyProperty> properties);

        /// <summary>
        ///     Gets the index with the given name. Returns <see langword="null" /> if no such index exists.
        /// </summary>
        /// <param name="name">The name of the index.</param>
        /// <returns>The index, or <see langword="null" /> if none is found.</returns>
        new IMutableIndex? FindIndex(string name);

        /// <summary>
        ///     <para>
        ///         Gets all indexes declared on this entity type.
        ///     </para>
        ///     <para>
        ///         This method does not return indexes declared on base types.
        ///         It is useful when iterating over all entity types to avoid processing the same index more than once.
        ///         Use <see cref="GetIndexes" /> to also return indexes declared on base types.
        ///     </para>
        /// </summary>
        /// <returns>Declared indexes.</returns>
        new IEnumerable<IMutableIndex> GetDeclaredIndexes()
            => ((IReadOnlyEntityType)this).GetDeclaredIndexes().Cast<IMutableIndex>();

        /// <summary>
        ///     <para>
        ///         Gets all indexes declared on the types derived from this entity type.
        ///     </para>
        /// </summary>
        /// <returns>Derived indexes.</returns>
        new IEnumerable<IMutableIndex> GetDerivedIndexes()
            => ((IReadOnlyEntityType)this).GetDerivedIndexes().Cast<IMutableIndex>();

        /// <summary>
        ///     Gets the indexes defined on this entity type.
        /// </summary>
        /// <returns>The indexes defined on this entity type.</returns>
        new IEnumerable<IMutableIndex> GetIndexes();

        /// <summary>
        ///     Removes an index from this entity type.
        /// </summary>
        /// <param name="properties">The properties that make up the index.</param>
        /// <returns>The removed index, or <see langword="null" /> if the index was not found.</returns>
        IMutableIndex? RemoveIndex(IReadOnlyList<IReadOnlyProperty> properties);

        /// <summary>
        ///     Removes an index from this entity type.
        /// </summary>
        /// <param name="index">The index to remove.</param>
        /// <returns>The removed index, or <see langword="null" /> if the index was not found.</returns>
        IMutableIndex? RemoveIndex(IReadOnlyIndex index);

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
        /// <returns>The newly created property.</returns>
        IMutableProperty AddProperty(string name, Type propertyType, MemberInfo? memberInfo);

        /// <summary>
        ///     <para>
        ///         Gets a property on the given entity type. Returns <see langword="null" /> if no property is found.
        ///     </para>
        ///     <para>
        ///         This API only finds scalar properties and does not find navigation properties. Use
        ///         <see cref="FindNavigation(MemberInfo)" /> to find a navigation property.
        ///     </para>
        /// </summary>
        /// <param name="memberInfo">The property on the entity class.</param>
        /// <returns>The property, or <see langword="null" /> if none is found.</returns>
        new IMutableProperty? FindProperty(MemberInfo memberInfo)
            => (IMutableProperty?)((IReadOnlyEntityType)this).FindProperty(memberInfo);

        /// <summary>
        ///     <para>
        ///         Gets the property with a given name. Returns <see langword="null" /> if no property with the given name is defined.
        ///     </para>
        ///     <para>
        ///         This API only finds scalar properties and does not find navigation properties. Use
        ///         <see cref="FindNavigation(string)" /> to find
        ///         a navigation property.
        ///     </para>
        /// </summary>
        /// <param name="name">The name of the property.</param>
        /// <returns>The property, or <see langword="null" /> if none is found.</returns>
        new IMutableProperty? FindProperty(string name);

        /// <summary>
        ///     <para>
        ///         Finds matching properties on the given entity type. Returns <see langword="null" /> if any property is not found.
        ///     </para>
        ///     <para>
        ///         This API only finds scalar properties and does not find navigation or service properties.
        ///     </para>
        /// </summary>
        /// <param name="propertyNames">The property names.</param>
        /// <returns>The properties, or <see langword="null" /> if any property is not found.</returns>
        new IReadOnlyList<IMutableProperty>? FindProperties(IReadOnlyList<string> propertyNames)
            => (IReadOnlyList<IMutableProperty>?)((IReadOnlyEntityType)this).FindProperties(propertyNames);

        /// <summary>
        ///     Finds a property declared on the type with the given name.
        ///     Does not return properties defined on a base type.
        /// </summary>
        /// <param name="name">The property name.</param>
        /// <returns>The property, or <see langword="null" /> if none is found.</returns>
        new IMutableProperty? FindDeclaredProperty(string name)
            => (IMutableProperty?)((IReadOnlyEntityType)this).FindDeclaredProperty(name);

        /// <summary>
        ///     <para>
        ///         Gets a property with the given name.
        ///     </para>
        ///     <para>
        ///         This API only finds scalar properties and does not find navigation properties. Use
        ///         <see cref="FindNavigation(string)" /> to find a navigation property.
        ///     </para>
        /// </summary>
        /// <param name="name">The property name.</param>
        /// <returns>The property, or <see langword="null" /> if none is found.</returns>
        new IMutableProperty GetProperty(string name)
            => (IMutableProperty)((IReadOnlyEntityType)this).GetProperty(name);

        /// <summary>
        ///     Adds a property to this entity type.
        /// </summary>
        /// <param name="memberInfo">The corresponding member on the entity class.</param>
        /// <returns>The newly created property.</returns>
        IMutableProperty AddProperty(MemberInfo memberInfo)
            => AddProperty(memberInfo.GetSimpleMemberName(), memberInfo.GetMemberType(), memberInfo);

        /// <summary>
        ///     Adds a property to this entity type.
        /// </summary>
        /// <param name="name">The name of the property to add.</param>
        /// <returns>The newly created property.</returns>
        IMutableProperty AddProperty(string name);

        /// <summary>
        ///     Adds a property to this entity type.
        /// </summary>
        /// <param name="name">The name of the property to add.</param>
        /// <param name="propertyType">The type of value the property will hold.</param>
        /// <returns>The newly created property.</returns>
        IMutableProperty AddProperty(string name, Type propertyType);

        /// <summary>
        ///     Adds a property backed up by an indexer to this entity type.
        /// </summary>
        /// <param name="name">The name of the property to add.</param>
        /// <param name="propertyType">The type of value the property will hold.</param>
        /// <returns>The newly created property.</returns>
        IMutableProperty AddIndexerProperty(
            string name,
            Type propertyType)
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
        ///     <para>
        ///         Gets all non-navigation properties declared on this entity type.
        ///     </para>
        ///     <para>
        ///         This method does not return properties declared on base types.
        ///         It is useful when iterating over all entity types to avoid processing the same property more than once.
        ///         Use <see cref="GetProperties" /> to also return properties declared on base types.
        ///     </para>
        /// </summary>
        /// <returns>Declared non-navigation properties.</returns>
        new IEnumerable<IMutableProperty> GetDeclaredProperties()
            => ((IReadOnlyEntityType)this).GetDeclaredProperties().Cast<IMutableProperty>();

        /// <summary>
        ///     <para>
        ///         Gets all non-navigation properties declared on the types derived from this entity type.
        ///     </para>
        ///     <para>
        ///         This method does not return properties declared on the given entity type itself.
        ///         Use <see cref="GetProperties" /> to return properties declared on this
        ///         and base entity typed types.
        ///     </para>
        /// </summary>
        /// <returns>Derived non-navigation properties.</returns>
        new IEnumerable<IMutableProperty> GetDerivedProperties()
            => ((IReadOnlyEntityType)this).GetDerivedProperties().Cast<IMutableProperty>();

        /// <summary>
        ///     <para>
        ///         Gets the properties defined on this entity type.
        ///     </para>
        ///     <para>
        ///         This API only returns scalar properties and does not return navigation properties. Use
        ///         <see cref="GetNavigations()" /> to get navigation
        ///         properties.
        ///     </para>
        /// </summary>
        /// <returns>The properties defined on this entity type.</returns>
        new IEnumerable<IMutableProperty> GetProperties();

        /// <summary>
        ///     Removes a property from this entity type.
        /// </summary>
        /// <param name="name">The name of the property to remove.</param>
        /// <returns>The removed property, or <see langword="null" /> if the property was not found.</returns>
        IMutableProperty? RemoveProperty(string name);

        /// <summary>
        ///     Removes a property from this entity type.
        /// </summary>
        /// <param name="property">The property to remove.</param>
        /// <returns>The removed property, or <see langword="null" /> if the property was not found.</returns>
        IMutableProperty? RemoveProperty(IReadOnlyProperty property);

        /// <summary>
        ///     Adds a service property to this entity type.
        /// </summary>
        /// <param name="memberInfo">The <see cref="PropertyInfo" /> or <see cref="FieldInfo" /> of the property to add.</param>
        /// <returns>The newly created service property.</returns>
        IMutableServiceProperty AddServiceProperty(MemberInfo memberInfo);

        /// <summary>
        ///     <para>
        ///         Gets the service property with a given name.
        ///         Returns <see langword="null" /> if no property with the given name is defined.
        ///     </para>
        ///     <para>
        ///         This API only finds service properties and does not find scalar or navigation properties.
        ///     </para>
        /// </summary>
        /// <param name="name">The name of the service property.</param>
        /// <returns>The service property, or <see langword="null" /> if none is found.</returns>
        new IMutableServiceProperty? FindServiceProperty(string name);

        /// <summary>
        ///     <para>
        ///         Gets all service properties declared on this entity type.
        ///     </para>
        ///     <para>
        ///         This method does not return properties declared on base types.
        ///         It is useful when iterating over all entity types to avoid processing the same property more than once.
        ///         Use <see cref="GetServiceProperties" /> to also return properties declared on base types.
        ///     </para>
        /// </summary>
        /// <returns>Declared service properties.</returns>
        new IEnumerable<IMutableServiceProperty> GetDeclaredServiceProperties()
            => ((IReadOnlyEntityType)this).GetDeclaredServiceProperties().Cast<IMutableServiceProperty>();

        /// <summary>
        ///     <para>
        ///         Gets all service properties declared on the types derived from this entity type.
        ///     </para>
        ///     <para>
        ///         This method does not return service properties declared on the given entity type itself.
        ///         Use <see cref="GetServiceProperties" /> to return service properties declared on this
        ///         and base entity typed types.
        ///     </para>
        /// </summary>
        /// <returns>Derived service properties.</returns>
        new IEnumerable<IMutableServiceProperty> GetDerivedServiceProperties()
            => ((IReadOnlyEntityType)this).GetDerivedServiceProperties().Cast<IMutableServiceProperty>();

        /// <summary>
        ///     <para>
        ///         Gets all the service properties defined on this entity type.
        ///     </para>
        ///     <para>
        ///         This API only returns service properties and does not return scalar or navigation properties.
        ///     </para>
        /// </summary>
        /// <returns>The service properties defined on this entity type.</returns>
        new IEnumerable<IMutableServiceProperty> GetServiceProperties();

        /// <summary>
        ///     Removes a service property from this entity type.
        /// </summary>
        /// <param name="name">The name of the property to remove.</param>
        /// <returns>The property that was removed, or <see langword="null" /> if the property was not found.</returns>
        IMutableServiceProperty? RemoveServiceProperty(string name);

        /// <summary>
        ///     Removes a service property from this entity type.
        /// </summary>
        /// <param name="property">The property to remove.</param>
        /// <returns>The removed property, or <see langword="null" /> if the property was not found.</returns>
        IMutableServiceProperty? RemoveServiceProperty(IReadOnlyServiceProperty property);
    }
}
