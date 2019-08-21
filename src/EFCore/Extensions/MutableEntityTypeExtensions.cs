// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     Extension methods for <see cref="IMutableEntityType" />.
    /// </summary>
    public static class MutableEntityTypeExtensions
    {
        /// <summary>
        ///     Gets the root base type for a given entity type.
        /// </summary>
        /// <param name="entityType"> The type to find the root of. </param>
        /// <returns>
        ///     The root base type. If the given entity type is not a derived type, then the same entity type is returned.
        /// </returns>
        public static IMutableEntityType GetRootType([NotNull] this IMutableEntityType entityType)
            => (IMutableEntityType)((IEntityType)entityType).GetRootType();

        /// <summary>
        ///     Gets the root base type for a given entity type.
        /// </summary>
        /// <param name="entityType"> The type to find the root of. </param>
        /// <returns>
        ///     The root base type. If the given entity type is not a derived type, then the same entity type is returned.
        /// </returns>
        [Obsolete("Use GetRootType")]
        public static IMutableEntityType RootType([NotNull] this IMutableEntityType entityType)
            => (IMutableEntityType)((IEntityType)entityType).GetRootType();

        /// <summary>
        ///     Gets all types in the model that derive from a given entity type.
        /// </summary>
        /// <param name="entityType"> The base type to find types that derive from. </param>
        /// <returns> The derived types. </returns>
        public static IEnumerable<IMutableEntityType> GetDerivedTypes([NotNull] this IMutableEntityType entityType)
            => ((EntityType)entityType).GetDerivedTypes();

        /// <summary>
        ///     Returns all derived types of the given <see cref="IMutableEntityType" />, including the type itself.
        /// </summary>
        /// <param name="entityType"> The entity type. </param>
        /// <returns> Derived types. </returns>
        public static IEnumerable<IMutableEntityType> GetDerivedTypesInclusive([NotNull] this IMutableEntityType entityType)
            => ((EntityType)entityType).GetDerivedTypesInclusive();

        /// <summary>
        ///     Gets all types in the model that directly derive from a given entity type.
        /// </summary>
        /// <param name="entityType"> The base type to find types that derive from. </param>
        /// <returns> The derived types. </returns>
        public static IEnumerable<IMutableEntityType> GetDirectlyDerivedTypes([NotNull] this IMutableEntityType entityType)
            => ((EntityType)entityType).GetDirectlyDerivedTypes();

        /// <summary>
        ///     <para>
        ///         Gets all keys declared on the given <see cref="IEntityType" />.
        ///     </para>
        ///     <para>
        ///         This method does not return keys declared on base types.
        ///         It is useful when iterating over all entity types to avoid processing the same key more than once.
        ///         Use <see cref="IConventionEntityType.GetKeys" /> to also return keys declared on base types.
        ///     </para>
        /// </summary>
        /// <param name="entityType"> The entity type. </param>
        /// <returns> Declared keys. </returns>
        public static IEnumerable<IMutableKey> GetDeclaredKeys([NotNull] this IMutableEntityType entityType)
            => ((EntityType)entityType).GetDeclaredKeys();

        /// <summary>
        ///     <para>
        ///         Gets all non-navigation properties declared on the given <see cref="IMutableEntityType" />.
        ///     </para>
        ///     <para>
        ///         This method does not return properties declared on base types.
        ///         It is useful when iterating over all entity types to avoid processing the same property more than once.
        ///         Use <see cref="IMutableEntityType.GetProperties" /> to also return properties declared on base types.
        ///     </para>
        /// </summary>
        /// <param name="entityType"> The entity type. </param>
        /// <returns> Declared non-navigation properties. </returns>
        public static IEnumerable<IMutableProperty> GetDeclaredProperties([NotNull] this IMutableEntityType entityType)
            => ((EntityType)entityType).GetDeclaredProperties();

        /// <summary>
        ///     <para>
        ///         Gets all navigation properties declared on the given <see cref="IMutableEntityType" />.
        ///     </para>
        ///     <para>
        ///         This method does not return navigation properties declared on base types.
        ///         It is useful when iterating over all entity types to avoid processing the same navigation property more than once.
        ///         Use <see cref="GetNavigations" /> to also return navigation properties declared on base types.
        ///     </para>
        /// </summary>
        /// <param name="entityType"> The entity type. </param>
        /// <returns> Declared navigation properties. </returns>
        public static IEnumerable<IMutableNavigation> GetDeclaredNavigations([NotNull] this IMutableEntityType entityType)
            => ((EntityType)entityType).GetDeclaredNavigations();

        /// <summary>
        ///     <para>
        ///         Gets all service properties declared on the given <see cref="IMutableEntityType" />.
        ///     </para>
        ///     <para>
        ///         This method does not return properties declared on base types.
        ///         It is useful when iterating over all entity types to avoid processing the same property more than once.
        ///         Use <see cref="IMutableEntityType.GetServiceProperties" /> to also return properties declared on base types.
        ///     </para>
        /// </summary>
        /// <param name="entityType"> The entity type. </param>
        /// <returns> Declared service properties. </returns>
        public static IEnumerable<IMutableServiceProperty> GetDeclaredServiceProperties([NotNull] this IMutableEntityType entityType)
            => ((EntityType)entityType).GetDeclaredServiceProperties();

        /// <summary>
        ///     <para>
        ///         Gets all indexes declared on the given <see cref="IMutableEntityType" />.
        ///     </para>
        ///     <para>
        ///         This method does not return indexes declared on base types.
        ///         It is useful when iterating over all entity types to avoid processing the same index more than once.
        ///         Use <see cref="IMutableEntityType.GetForeignKeys" /> to also return indexes declared on base types.
        ///     </para>
        /// </summary>
        /// <param name="entityType"> The entity type. </param>
        /// <returns> Declared indexes. </returns>
        public static IEnumerable<IMutableIndex> GetDeclaredIndexes([NotNull] this IMutableEntityType entityType)
            => ((EntityType)entityType).GetDeclaredIndexes();

        /// <summary>
        ///     Removes a property from this entity type.
        /// </summary>
        /// <param name="entityType"> The entity type. </param>
        /// <param name="name"> The name of the property to remove. </param>
        /// <returns> The property that was removed. </returns>
        public static IMutableProperty RemoveProperty([NotNull] this IMutableEntityType entityType, [NotNull] string name)
            => ((EntityType)entityType).RemoveProperty(name);

        /// <summary>
        ///     Sets the primary key for this entity type.
        /// </summary>
        /// <param name="entityType"> The entity type to set the key on. </param>
        /// <param name="property"> The primary key property. </param>
        /// <returns> The newly created key. </returns>
        public static IMutableKey SetPrimaryKey(
            [NotNull] this IMutableEntityType entityType, [CanBeNull] IMutableProperty property)
        {
            Check.NotNull(entityType, nameof(entityType));

            return entityType.SetPrimaryKey(property == null ? null : new[] { property });
        }

        /// <summary>
        ///     Gets the primary or alternate key that is defined on the given property. Returns <c>null</c> if no key is defined
        ///     for the given property.
        /// </summary>
        /// <param name="entityType"> The entity type to find the key on. </param>
        /// <param name="property"> The property that the key is defined on. </param>
        /// <returns> The key, or null if none is defined. </returns>
        public static IMutableKey FindKey([NotNull] this IMutableEntityType entityType, [NotNull] IProperty property)
        {
            Check.NotNull(entityType, nameof(entityType));

            return entityType.FindKey(new[] { property });
        }

        /// <summary>
        ///     Adds a new alternate key to this entity type.
        /// </summary>
        /// <param name="entityType"> The entity type to add the alternate key to. </param>
        /// <param name="property"> The property to use as an alternate key. </param>
        /// <returns> The newly created key. </returns>
        public static IMutableKey AddKey(
            [NotNull] this IMutableEntityType entityType, [NotNull] IMutableProperty property)
            => Check.NotNull(entityType, nameof(entityType)).AddKey(new[] { property });

        /// <summary>
        ///     Removes a primary or alternate key from this entity type.
        /// </summary>
        /// <param name="entityType"> The entity type to add remove the key from. </param>
        /// <param name="properties"> The properties that make up the key. </param>
        /// <returns> The key that was removed. </returns>
        public static IMutableKey RemoveKey(
            [NotNull] this IMutableEntityType entityType, [NotNull] IReadOnlyList<IMutableProperty> properties)
            => ((EntityType)entityType).RemoveKey(properties);

        /// <summary>
        ///     <para>
        ///         Gets all foreign keys declared on the given <see cref="IMutableEntityType" />.
        ///     </para>
        ///     <para>
        ///         This method does not return foreign keys declared on base types.
        ///         It is useful when iterating over all entity types to avoid processing the same foreign key more than once.
        ///         Use <see cref="IMutableEntityType.GetForeignKeys" /> to also return foreign keys declared on base types.
        ///     </para>
        /// </summary>
        /// <param name="entityType"> The entity type. </param>
        /// <returns> Declared foreign keys. </returns>
        public static IEnumerable<IMutableForeignKey> GetDeclaredForeignKeys([NotNull] this IMutableEntityType entityType)
            => ((EntityType)entityType).GetDeclaredForeignKeys();

        /// <summary>
        ///     <para>
        ///         Gets all foreign keys declared on the types derived from the given <see cref="IMutableEntityType" />.
        ///     </para>
        ///     <para>
        ///         This method does not return foreign keys declared on the given entity type itself.
        ///         Use <see cref="IMutableEntityType.GetForeignKeys" /> to return foreign keys declared on this
        ///         and base entity typed types.
        ///     </para>
        /// </summary>
        /// <param name="entityType"> The entity type. </param>
        /// <returns> Derived foreign keys. </returns>
        public static IEnumerable<IMutableForeignKey> GetDerivedForeignKeys([NotNull] this IMutableEntityType entityType)
            => ((EntityType)entityType).GetDerivedForeignKeys();

        /// <summary>
        ///     Gets the foreign keys defined on the given property. Only foreign keys that are defined on exactly the specified
        ///     property are returned. Composite foreign keys that include the specified property are not returned.
        /// </summary>
        /// <param name="entityType"> The entity type to find the foreign keys on. </param>
        /// <param name="property"> The property to find the foreign keys on. </param>
        /// <returns> The foreign keys. </returns>
        public static IEnumerable<IMutableForeignKey> FindForeignKeys(
            [NotNull] this IMutableEntityType entityType, [NotNull] IProperty property)
            => entityType.FindForeignKeys(new[] { property });

        /// <summary>
        ///     Gets the foreign keys defined on the given properties. Only foreign keys that are defined on exactly the specified
        ///     set of properties are returned.
        /// </summary>
        /// <param name="entityType"> The entity type to find the foreign keys on. </param>
        /// <param name="properties"> The properties to find the foreign keys on. </param>
        /// <returns> The foreign keys. </returns>
        public static IEnumerable<IMutableForeignKey> FindForeignKeys(
            [NotNull] this IMutableEntityType entityType, [NotNull] IReadOnlyList<IProperty> properties)
            => ((EntityType)entityType).FindForeignKeys(properties);

        /// <summary>
        ///     Gets the foreign key for the given properties that points to a given primary or alternate key. Returns <c>null</c>
        ///     if no foreign key is found.
        /// </summary>
        /// <param name="entityType"> The entity type to find the foreign keys on. </param>
        /// <param name="property"> The property that the foreign key is defined on. </param>
        /// <param name="principalKey"> The primary or alternate key that is referenced. </param>
        /// <param name="principalEntityType">
        ///     The entity type that the relationship targets. This may be different from the type that <paramref name="principalKey" />
        ///     is defined on when the relationship targets a derived type in an inheritance hierarchy (since the key is defined on the
        ///     base type of the hierarchy).
        /// </param>
        /// <returns> The foreign key, or <c>null</c> if none is defined. </returns>
        public static IMutableForeignKey FindForeignKey(
            [NotNull] this IMutableEntityType entityType,
            [NotNull] IProperty property,
            [NotNull] IKey principalKey,
            [NotNull] IEntityType principalEntityType)
        {
            Check.NotNull(entityType, nameof(entityType));

            return entityType.FindForeignKey(new[] { property }, principalKey, principalEntityType);
        }

        /// <summary>
        ///      Gets the foreign keys declared on the given <see cref="IConventionEntityType" /> using the given properties.
        /// </summary>
        /// <param name="entityType"> The entity type. </param>
        /// <param name="properties"> The properties to find the foreign keys on. </param>
        /// <returns> Declared foreign keys. </returns>
        public static IEnumerable<IMutableForeignKey> FindDeclaredForeignKeys(
            [NotNull] this IMutableEntityType entityType,
            [NotNull] IReadOnlyList<IProperty> properties)
            => ((EntityType)entityType).FindDeclaredForeignKeys(properties);

        /// <summary>
        ///     Gets all foreign keys that target a given entity type (i.e. foreign keys where the given entity type
        ///     or a type it's derived from is the principal).
        /// </summary>
        /// <param name="entityType"> The entity type to find the foreign keys for. </param>
        /// <returns> The foreign keys that reference the given entity type. </returns>
        public static IEnumerable<IMutableForeignKey> GetReferencingForeignKeys([NotNull] this IMutableEntityType entityType)
            => ((EntityType)entityType).GetReferencingForeignKeys();

        /// <summary>
        ///     Gets all foreign keys that target a given entity type (i.e. foreign keys where the given entity type
        ///     is the principal).
        /// </summary>
        /// <param name="entityType"> The entity type to find the foreign keys for. </param>
        /// <returns> The foreign keys that reference the given entity type. </returns>
        public static IEnumerable<IMutableForeignKey> GetDeclaredReferencingForeignKeys([NotNull] this IMutableEntityType entityType)
            => ((EntityType)entityType).GetDeclaredReferencingForeignKeys();

        /// <summary>
        ///     Adds a new relationship to this entity type.
        /// </summary>
        /// <param name="entityType"> The entity type to add the foreign key to. </param>
        /// <param name="property"> The property that the foreign key is defined on. </param>
        /// <param name="principalKey"> The primary or alternate key that is referenced. </param>
        /// <param name="principalEntityType">
        ///     The entity type that the relationship targets. This may be different from the type that <paramref name="principalKey" />
        ///     is defined on when the relationship targets a derived type in an inheritance hierarchy (since the key is defined on the
        ///     base type of the hierarchy).
        /// </param>
        /// <returns> The newly created foreign key. </returns>
        public static IMutableForeignKey AddForeignKey(
            [NotNull] this IMutableEntityType entityType,
            [NotNull] IMutableProperty property,
            [NotNull] IMutableKey principalKey,
            [NotNull] IMutableEntityType principalEntityType)
        {
            Check.NotNull(entityType, nameof(entityType));

            return entityType.AddForeignKey(new[] { property }, principalKey, principalEntityType);
        }

        /// <summary>
        ///     Returns the relationship to the owner if this is an owned type or <c>null</c> otherwise.
        /// </summary>
        /// <param name="entityType"> The entity type to find the foreign keys on. </param>
        /// <returns> The relationship to the owner if this is an owned type or <c>null</c> otherwise. </returns>
        public static IMutableForeignKey FindOwnership([NotNull] this IMutableEntityType entityType)
            => ((EntityType)entityType).FindOwnership();

        /// <summary>
        ///     Removes a foreign key from this entity type.
        /// </summary>
        /// <param name="entityType"> The entity type to remove the foreign key from. </param>
        /// <param name="properties"> The properties that the foreign key is defined on. </param>
        /// <param name="principalKey"> The primary or alternate key that is referenced. </param>
        /// <param name="principalEntityType">
        ///     The entity type that the relationship targets. This may be different from the type that <paramref name="principalKey" />
        ///     is defined on when the relationship targets a derived type in an inheritance hierarchy (since the key is defined on the
        ///     base type of the hierarchy).
        /// </param>
        /// <returns> The foreign key that was removed. </returns>
        public static IMutableForeignKey RemoveForeignKey(
            [NotNull] this IMutableEntityType entityType,
            [NotNull] IReadOnlyList<IMutableProperty> properties,
            [NotNull] IMutableKey principalKey,
            [NotNull] IMutableEntityType principalEntityType)
            => ((EntityType)entityType).RemoveForeignKey(properties, principalKey, principalEntityType);

        /// <summary>
        ///     Gets a navigation property on the given entity type. Returns <c>null</c> if no navigation property is found.
        /// </summary>
        /// <param name="entityType"> The entity type to find the navigation property on. </param>
        /// <param name="memberInfo"> The navigation property on the entity class. </param>
        /// <returns> The navigation property, or <c>null</c> if none is found. </returns>
        public static IMutableNavigation FindNavigation(
            [NotNull] this IMutableEntityType entityType, [NotNull] MemberInfo memberInfo)
            => Check.NotNull(entityType, nameof(entityType))
                .FindNavigation(Check.NotNull(memberInfo, nameof(memberInfo)).GetSimpleMemberName());

        /// <summary>
        ///     Gets a navigation property on the given entity type. Returns <c>null</c> if no navigation property is found.
        /// </summary>
        /// <param name="entityType"> The entity type to find the navigation property on. </param>
        /// <param name="name"> The name of the navigation property on the entity class. </param>
        /// <returns> The navigation property, or <c>null</c> if none is found. </returns>
        public static IMutableNavigation FindNavigation([NotNull] this IMutableEntityType entityType, [NotNull] string name)
            => ((EntityType)entityType).FindNavigation(name);

        /// <summary>
        ///     Gets a navigation property on the given entity type. Does not return navigation properties defined on a base type.
        ///     Returns <c>null</c> if no navigation property is found.
        /// </summary>
        /// <param name="entityType"> The entity type to find the navigation property on. </param>
        /// <param name="name"> The name of the navigation property on the entity class. </param>
        /// <returns> The navigation property, or <c>null</c> if none is found. </returns>
        public static IMutableNavigation FindDeclaredNavigation([NotNull] this IMutableEntityType entityType, [NotNull] string name)
            => ((EntityType)entityType).FindDeclaredNavigation(Check.NotNull(name, nameof(name)));

        /// <summary>
        ///     Returns the defining navigation if one exists or <c>null</c> otherwise.
        /// </summary>
        /// <param name="entityType"> The entity type to find the defining navigation for. </param>
        /// <returns> The defining navigation if one exists or <c>null</c> otherwise. </returns>
        public static IMutableNavigation FindDefiningNavigation([NotNull] this IMutableEntityType entityType)
            => (IMutableNavigation)((IEntityType)entityType).FindDefiningNavigation();

        /// <summary>
        ///     Gets all navigation properties on the given entity type.
        /// </summary>
        /// <param name="entityType"> The entity type to get navigation properties for. </param>
        /// <returns> All navigation properties on the given entity type. </returns>
        public static IEnumerable<IMutableNavigation> GetNavigations([NotNull] this IMutableEntityType entityType)
        => ((EntityType)entityType).GetNavigations();

        /// <summary>
        ///     <para>
        ///         Gets a property on the given entity type. Returns <c>null</c> if no property is found.
        ///     </para>
        ///     <para>
        ///         This API only finds scalar properties and does not find navigation properties. Use
        ///         <see cref="FindNavigation(IMutableEntityType, MemberInfo)" /> to find a navigation property.
        ///     </para>
        /// </summary>
        /// <param name="entityType"> The entity type to find the property on. </param>
        /// <param name="propertyInfo"> The property on the entity class. </param>
        /// <returns> The property, or <c>null</c> if none is found. </returns>
        public static IMutableProperty FindProperty([NotNull] this IMutableEntityType entityType, [NotNull] PropertyInfo propertyInfo)
        {
            Check.NotNull(entityType, nameof(entityType));
            Check.NotNull(propertyInfo, nameof(propertyInfo));

            return entityType.FindProperty(propertyInfo.GetSimpleMemberName());
        }

        /// <summary>
        ///     <para>
        ///         Finds matching properties on the given entity type. Returns <c>null</c> if any property is not found.
        ///     </para>
        ///     <para>
        ///         This API only finds scalar properties and does not find navigation properties.
        ///     </para>
        /// </summary>
        /// <param name="entityType"> The entity type to find the properties on. </param>
        /// <param name="propertyNames"> The property names. </param>
        /// <returns> The properties, or <c>null</c> if any property is not found. </returns>
        public static IReadOnlyList<IMutableProperty> FindProperties(
            [NotNull] this IMutableEntityType entityType,
            [NotNull] IReadOnlyList<string> propertyNames)
            => ((EntityType)entityType).FindProperties(Check.NotNull(propertyNames, nameof(propertyNames)));

        /// <summary>
        ///     Finds a property declared on the type with the given name.
        ///     Does not return properties defined on a base type.
        /// </summary>
        /// <param name="entityType"> The entity type to find the property on. </param>
        /// <param name="name"> The property name. </param>
        /// <returns> The property, or <c>null</c> if none is found. </returns>
        public static IMutableProperty FindDeclaredProperty([NotNull] this IMutableEntityType entityType, [NotNull] string name)
            => ((EntityType)entityType).FindDeclaredProperty(name);

        /// <summary>
        ///     Adds a property to this entity type.
        /// </summary>
        /// <param name="entityType"> The entity type to add the property to. </param>
        /// <param name="memberInfo"> The corresponding member on the entity class. </param>
        /// <returns> The newly created property. </returns>
        public static IMutableProperty AddProperty(
            [NotNull] this IMutableEntityType entityType, [NotNull] MemberInfo memberInfo)
            => Check.NotNull(entityType, nameof(entityType))
                .AddProperty(memberInfo.GetSimpleMemberName(), memberInfo.GetMemberType(), memberInfo);

        /// <summary>
        ///     Adds a property to this entity type.
        /// </summary>
        /// <param name="entityType"> The entity type to add the property to. </param>
        /// <param name="name"> The name of the property to add. </param>
        /// <returns> The newly created property. </returns>
        public static IMutableProperty AddProperty(
            [NotNull] this IMutableEntityType entityType, [NotNull] string name)
            => ((EntityType)entityType).AddProperty(name, ConfigurationSource.Explicit);

        /// <summary>
        ///     Adds a property to this entity type.
        /// </summary>
        /// <param name="entityType"> The entity type to add the property to. </param>
        /// <param name="name"> The name of the property to add. </param>
        /// <param name="propertyType"> The type of value the property will hold. </param>
        /// <returns> The newly created property. </returns>
        public static IMutableProperty AddProperty(
            [NotNull] this IMutableEntityType entityType, [NotNull] string name, [NotNull] Type propertyType)
            => entityType.AddProperty(name, propertyType, null);

        /// <summary>
        ///     Gets the index defined on the given property. Returns null if no index is defined.
        /// </summary>
        /// <param name="entityType"> The entity type to find the index on. </param>
        /// <param name="property"> The property to find the index on. </param>
        /// <returns> The index, or null if none is found. </returns>
        public static IMutableIndex FindIndex([NotNull] this IMutableEntityType entityType, [NotNull] IProperty property)
        {
            Check.NotNull(entityType, nameof(entityType));

            return entityType.FindIndex(new[] { property });
        }

        /// <summary>
        ///     Adds an index to this entity type.
        /// </summary>
        /// <param name="entityType"> The entity type to add the index to. </param>
        /// <param name="property"> The property to be indexed. </param>
        /// <returns> The newly created index. </returns>
        public static IMutableIndex AddIndex(
            [NotNull] this IMutableEntityType entityType, [NotNull] IMutableProperty property)
            => Check.NotNull(entityType, nameof(entityType)).AddIndex(new[] { property });

        /// <summary>
        ///     Removes an index from this entity type.
        /// </summary>
        /// <param name="entityType"> The entity type to remove the index from. </param>
        /// <param name="properties"> The properties that make up the index. </param>
        /// <returns> The index that was removed. </returns>
        public static IMutableIndex RemoveIndex(
            [NotNull] this IMutableEntityType entityType,
            [NotNull] IReadOnlyList<IMutableProperty> properties)
            => ((EntityType)entityType).RemoveIndex(properties);

        /// <summary>
        ///     Sets the change tracking strategy to use for this entity type. This strategy indicates how the
        ///     context detects changes to properties for an instance of the entity type.
        /// </summary>
        /// <param name="entityType"> The entity type to set the change tracking strategy for. </param>
        /// <param name="changeTrackingStrategy"> The strategy to use. </param>
        public static void SetChangeTrackingStrategy(
            [NotNull] this IMutableEntityType entityType,
            ChangeTrackingStrategy? changeTrackingStrategy)
            => Check.NotNull(entityType, nameof(entityType)).AsEntityType()
                .SetChangeTrackingStrategy(changeTrackingStrategy, ConfigurationSource.Explicit);

        /// <summary>
        ///     Sets the LINQ expression filter automatically applied to queries for this entity type.
        /// </summary>
        /// <param name="entityType"> The entity type to set the query filter for. </param>
        /// <param name="queryFilter"> The LINQ expression filter. </param>
        public static void SetQueryFilter(
            [NotNull] this IMutableEntityType entityType,
            [CanBeNull] LambdaExpression queryFilter)
            => Check.NotNull(entityType, nameof(entityType)).AsEntityType()
                .SetQueryFilter(queryFilter, ConfigurationSource.Explicit);

        /// <summary>
        ///     Sets the LINQ query used as the default source for queries of this type.
        /// </summary>
        /// <param name="entityType"> The entity type to set the defining query for. </param>
        /// <param name="definingQuery"> The LINQ query used as the default source. </param>
        public static void SetDefiningQuery(
            [NotNull] this IMutableEntityType entityType,
            [CanBeNull] LambdaExpression definingQuery)
            => Check.NotNull(entityType, nameof(entityType)).AsEntityType()
                .SetDefiningQuery(definingQuery, ConfigurationSource.Explicit);

        /// <summary>
        ///     Returns the <see cref="IMutableProperty" /> that will be used for storing a discriminator value.
        /// </summary>
        /// <param name="entityType"> The entity type to get the discriminator property for. </param>
        public static IMutableProperty GetDiscriminatorProperty([NotNull] this IMutableEntityType entityType)
            => (IMutableProperty)((IEntityType)entityType).GetDiscriminatorProperty();

        /// <summary>
        ///     Sets the <see cref="IProperty" /> that will be used for storing a discriminator value.
        /// </summary>
        /// <param name="entityType"> The entity type to set the discriminator property for. </param>
        /// <param name="property"> The property to set. </param>
        public static void SetDiscriminatorProperty([NotNull] this IMutableEntityType entityType, [CanBeNull] IProperty property)
            => Check.NotNull(entityType, nameof(entityType)).AsEntityType()
                .SetDiscriminatorProperty(property, ConfigurationSource.Explicit);

        /// <summary>
        ///     Sets the discriminator value for this entity type.
        /// </summary>
        /// <param name="entityType"> The entity type to set the discriminator value for. </param>
        /// <param name="value"> The value to set. </param>
        public static void SetDiscriminatorValue([NotNull] this IMutableEntityType entityType, [CanBeNull] object value)
        {
            entityType.AsEntityType().CheckDiscriminatorValue(entityType, value);

            entityType.SetAnnotation(CoreAnnotationNames.DiscriminatorValue, value);
        }

        /// <summary>
        ///     Removes the discriminator value for this entity type.
        /// </summary>
        /// <param name="entityType"> The entity type to remove the discriminator value for. </param>
        public static void RemoveDiscriminatorValue([NotNull] this IMutableEntityType entityType)
            => entityType.RemoveAnnotation(CoreAnnotationNames.DiscriminatorValue);
    }
}
