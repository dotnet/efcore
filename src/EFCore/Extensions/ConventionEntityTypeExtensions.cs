// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
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
    ///     Extension methods for <see cref="IConventionEntityType" />.
    /// </summary>
    public static class ConventionEntityTypeExtensions
    {
        /// <summary>
        ///     Gets the root base type for a given entity type.
        /// </summary>
        /// <param name="entityType"> The type to find the root of. </param>
        /// <returns>
        ///     The root base type. If the given entity type is not a derived type, then the same entity type is returned.
        /// </returns>
        public static IConventionEntityType GetRootType([NotNull] this IConventionEntityType entityType)
            => (IConventionEntityType)((IEntityType)entityType).GetRootType();

        /// <summary>
        ///     Gets the root base type for a given entity type.
        /// </summary>
        /// <param name="entityType"> The type to find the root of. </param>
        /// <returns>
        ///     The root base type. If the given entity type is not a derived type, then the same entity type is returned.
        /// </returns>
        [Obsolete("Use GetRootType")]
        public static IConventionEntityType RootType([NotNull] this IConventionEntityType entityType)
            => (IConventionEntityType)((IEntityType)entityType).GetRootType();

        /// <summary>
        ///     Gets all types in the model that derive from a given entity type.
        /// </summary>
        /// <param name="entityType"> The base type to find types that derive from. </param>
        /// <returns> The derived types. </returns>
        public static IEnumerable<IConventionEntityType> GetDerivedTypes([NotNull] this IConventionEntityType entityType)
            => ((EntityType)entityType).GetDerivedTypes();

        /// <summary>
        ///     Returns all derived types of the given <see cref="IConventionEntityType" />, including the type itself.
        /// </summary>
        /// <param name="entityType"> The entity type. </param>
        /// <returns> Derived types. </returns>
        public static IEnumerable<IConventionEntityType> GetDerivedTypesInclusive([NotNull] this IConventionEntityType entityType)
            => ((EntityType)entityType).GetDerivedTypesInclusive();

        /// <summary>
        ///     Gets all types in the model that directly derive from a given entity type.
        /// </summary>
        /// <param name="entityType"> The base type to find types that derive from. </param>
        /// <returns> The derived types. </returns>
        public static IEnumerable<IConventionEntityType> GetDirectlyDerivedTypes([NotNull] this IConventionEntityType entityType)
            => ((EntityType)entityType).GetDirectlyDerivedTypes();

        /// <summary>
        ///     Returns all base types of the given <see cref="IEntityType" />, including the type itself, top to bottom.
        /// </summary>
        /// <param name="entityType"> The entity type. </param>
        /// <returns> Base types. </returns>
        public static IEnumerable<IConventionEntityType> GetAllBaseTypesInclusive([NotNull] this IConventionEntityType entityType)
            => GetAllBaseTypesInclusiveAscending(entityType).Reverse();

        /// <summary>
        ///     Returns all base types of the given <see cref="IEntityType" />, including the type itself, bottom to top.
        /// </summary>
        /// <param name="entityType"> The entity type. </param>
        /// <returns> Base types. </returns>
        public static IEnumerable<IConventionEntityType> GetAllBaseTypesInclusiveAscending([NotNull] this IConventionEntityType entityType)
        {
            Check.NotNull(entityType, nameof(entityType));

            while (entityType != null)
            {
                yield return entityType;
                entityType = entityType.BaseType;
            }
        }

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
        public static IEnumerable<IConventionKey> GetDeclaredKeys([NotNull] this IConventionEntityType entityType)
            => ((EntityType)entityType).GetDeclaredKeys();

        /// <summary>
        ///     <para>
        ///         Gets all non-navigation properties declared on the given <see cref="IConventionEntityType" />.
        ///     </para>
        ///     <para>
        ///         This method does not return properties declared on base types.
        ///         It is useful when iterating over all entity types to avoid processing the same property more than once.
        ///         Use <see cref="IConventionEntityType.GetProperties" /> to also return properties declared on base types.
        ///     </para>
        /// </summary>
        /// <param name="entityType"> The entity type. </param>
        /// <returns> Declared non-navigation properties. </returns>
        public static IEnumerable<IConventionProperty> GetDeclaredProperties([NotNull] this IConventionEntityType entityType)
            => ((EntityType)entityType).GetDeclaredProperties();

        /// <summary>
        ///     <para>
        ///         Gets all navigation properties declared on the given <see cref="IConventionEntityType" />.
        ///     </para>
        ///     <para>
        ///         This method does not return navigation properties declared on base types.
        ///         It is useful when iterating over all entity types to avoid processing the same navigation property more than once.
        ///         Use <see cref="GetNavigations" /> to also return navigation properties declared on base types.
        ///     </para>
        /// </summary>
        /// <param name="entityType"> The entity type. </param>
        /// <returns> Declared navigation properties. </returns>
        public static IEnumerable<IConventionNavigation> GetDeclaredNavigations([NotNull] this IConventionEntityType entityType)
            => ((EntityType)entityType).GetDeclaredNavigations();

        /// <summary>
        ///     <para>
        ///         Gets all service properties declared on the given <see cref="IConventionEntityType" />.
        ///     </para>
        ///     <para>
        ///         This method does not return properties declared on base types.
        ///         It is useful when iterating over all entity types to avoid processing the same property more than once.
        ///         Use <see cref="IConventionEntityType.GetServiceProperties" /> to also return properties declared on base types.
        ///     </para>
        /// </summary>
        /// <param name="entityType"> The entity type. </param>
        /// <returns> Declared service properties. </returns>
        public static IEnumerable<IConventionServiceProperty> GetDeclaredServiceProperties([NotNull] this IConventionEntityType entityType)
            => ((EntityType)entityType).GetDeclaredServiceProperties();

        /// <summary>
        ///     <para>
        ///         Gets all indexes declared on the given <see cref="IConventionEntityType" />.
        ///     </para>
        ///     <para>
        ///         This method does not return indexes declared on base types.
        ///         It is useful when iterating over all entity types to avoid processing the same index more than once.
        ///         Use <see cref="IConventionEntityType.GetForeignKeys" /> to also return indexes declared on base types.
        ///     </para>
        /// </summary>
        /// <param name="entityType"> The entity type. </param>
        /// <returns> Declared indexes. </returns>
        public static IEnumerable<IConventionIndex> GetDeclaredIndexes([NotNull] this IConventionEntityType entityType)
            => ((EntityType)entityType).GetDeclaredIndexes();

        /// <summary>
        ///     Removes a property from this entity type.
        /// </summary>
        /// <param name="entityType"> The entity type. </param>
        /// <param name="name"> The name of the property to remove. </param>
        /// <returns> The property that was removed. </returns>
        public static IConventionProperty RemoveProperty([NotNull] this IConventionEntityType entityType, [NotNull] string name)
            => ((EntityType)entityType).RemoveProperty(name);

        /// <summary>
        ///     Sets the primary key for this entity type.
        /// </summary>
        /// <param name="entityType"> The entity type to set the key on. </param>
        /// <param name="property"> The primary key property. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> The newly created key. </returns>
        public static IConventionKey SetPrimaryKey(
            [NotNull] this IConventionEntityType entityType,
            [CanBeNull] IConventionProperty property,
            bool fromDataAnnotation = false)
        {
            Check.NotNull(entityType, nameof(entityType));

            return entityType.SetPrimaryKey(property == null ? null : new[] { property }, fromDataAnnotation);
        }

        /// <summary>
        ///     Gets the primary or alternate key that is defined on the given property. Returns <c>null</c> if no key is defined
        ///     for the given property.
        /// </summary>
        /// <param name="entityType"> The entity type to find the key on. </param>
        /// <param name="property"> The property that the key is defined on. </param>
        /// <returns> The key, or null if none is defined. </returns>
        public static IConventionKey FindKey([NotNull] this IConventionEntityType entityType, [NotNull] IProperty property)
        {
            Check.NotNull(entityType, nameof(entityType));

            return entityType.FindKey(new[] { property });
        }

        /// <summary>
        ///     Adds a new alternate key to this entity type.
        /// </summary>
        /// <param name="entityType"> The entity type to add the alternate key to. </param>
        /// <param name="property"> The property to use as an alternate key. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> The newly created key. </returns>
        public static IConventionKey AddKey(
            [NotNull] this IConventionEntityType entityType,
            [NotNull] IConventionProperty property,
            bool fromDataAnnotation = false)
            => Check.NotNull(entityType, nameof(entityType)).AddKey(new[] { property }, fromDataAnnotation);

        /// <summary>
        ///     Removes a primary or alternate key from this entity type.
        /// </summary>
        /// <param name="entityType"> The entity type to add remove the key from. </param>
        /// <param name="properties"> The properties that make up the key. </param>
        /// <returns> The key that was removed. </returns>
        public static IConventionKey RemoveKey(
            [NotNull] this IConventionEntityType entityType,
            [NotNull] IReadOnlyList<IConventionProperty> properties)
            => ((EntityType)entityType).RemoveKey(properties);

        /// <summary>
        ///     <para>
        ///         Gets all foreign keys declared on the given <see cref="IConventionEntityType" />.
        ///     </para>
        ///     <para>
        ///         This method does not return foreign keys declared on base types.
        ///         It is useful when iterating over all entity types to avoid processing the same foreign key more than once.
        ///         Use <see cref="IConventionEntityType.GetForeignKeys" /> to also return foreign keys declared on base types.
        ///     </para>
        /// </summary>
        /// <param name="entityType"> The entity type. </param>
        /// <returns> Declared foreign keys. </returns>
        public static IEnumerable<IConventionForeignKey> GetDeclaredForeignKeys([NotNull] this IConventionEntityType entityType)
            => ((EntityType)entityType).GetDeclaredForeignKeys();

        /// <summary>
        ///     <para>
        ///         Gets all foreign keys declared on the types derived from the given <see cref="IConventionEntityType" />.
        ///     </para>
        ///     <para>
        ///         This method does not return foreign keys declared on the given entity type itself.
        ///         Use <see cref="IConventionEntityType.GetForeignKeys" /> to return foreign keys declared on this
        ///         and base entity typed types.
        ///     </para>
        /// </summary>
        /// <param name="entityType"> The entity type. </param>
        /// <returns> Derived foreign keys. </returns>
        public static IEnumerable<IConventionForeignKey> GetDerivedForeignKeys([NotNull] this IConventionEntityType entityType)
            => ((EntityType)entityType).GetDerivedForeignKeys();

        /// <summary>
        ///     Gets the foreign keys defined on the given property. Only foreign keys that are defined on exactly the specified
        ///     property are returned. Composite foreign keys that include the specified property are not returned.
        /// </summary>
        /// <param name="entityType"> The entity type to find the foreign keys on. </param>
        /// <param name="property"> The property to find the foreign keys on. </param>
        /// <returns> The foreign keys. </returns>
        public static IEnumerable<IConventionForeignKey> FindForeignKeys(
            [NotNull] this IConventionEntityType entityType, [NotNull] IProperty property)
            => entityType.FindForeignKeys(new[] { property });

        /// <summary>
        ///     Gets the foreign keys defined on the given properties. Only foreign keys that are defined on exactly the specified
        ///     set of properties are returned.
        /// </summary>
        /// <param name="entityType"> The entity type to find the foreign keys on. </param>
        /// <param name="properties"> The properties to find the foreign keys on. </param>
        /// <returns> The foreign keys. </returns>
        public static IEnumerable<IConventionForeignKey> FindForeignKeys(
            [NotNull] this IConventionEntityType entityType, [NotNull] IReadOnlyList<IProperty> properties)
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
        public static IConventionForeignKey FindForeignKey(
            [NotNull] this IConventionEntityType entityType,
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
        public static IEnumerable<IConventionForeignKey> FindDeclaredForeignKeys(
            [NotNull] this IConventionEntityType entityType,
            [NotNull] IReadOnlyList<IProperty> properties)
            => ((EntityType)entityType).FindDeclaredForeignKeys(properties);

        /// <summary>
        ///     Gets all foreign keys that target a given entity type (i.e. foreign keys where the given entity type
        ///     or a type it's derived from is the principal).
        /// </summary>
        /// <param name="entityType"> The entity type to find the foreign keys for. </param>
        /// <returns> The foreign keys that reference the given entity type. </returns>
        public static IEnumerable<IConventionForeignKey> GetReferencingForeignKeys([NotNull] this IConventionEntityType entityType)
            => ((EntityType)entityType).GetReferencingForeignKeys();

        /// <summary>
        ///     Gets all foreign keys that target a given entity type (i.e. foreign keys where the given entity type
        ///     is the principal).
        /// </summary>
        /// <param name="entityType"> The entity type to find the foreign keys for. </param>
        /// <returns> The foreign keys that reference the given entity type. </returns>
        public static IEnumerable<IConventionForeignKey> GetDeclaredReferencingForeignKeys([NotNull] this IConventionEntityType entityType)
            => ((EntityType)entityType).GetDeclaredReferencingForeignKeys();

        /// <summary>
        ///     Returns the relationship to the owner if this is an owned type or <c>null</c> otherwise.
        /// </summary>
        /// <param name="entityType"> The entity type to find the foreign keys on. </param>
        /// <returns> The relationship to the owner if this is an owned type or <c>null</c> otherwise. </returns>
        public static IConventionForeignKey FindOwnership([NotNull] this IConventionEntityType entityType)
            => ((EntityType)entityType).FindOwnership();

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
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> The newly created foreign key. </returns>
        public static IConventionForeignKey AddForeignKey(
            [NotNull] this IConventionEntityType entityType,
            [NotNull] IConventionProperty property,
            [NotNull] IConventionKey principalKey,
            [NotNull] IConventionEntityType principalEntityType,
            bool fromDataAnnotation = false)
            => Check.NotNull(entityType, nameof(entityType))
                .AddForeignKey(new[] { property }, principalKey, principalEntityType, fromDataAnnotation);

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
        public static IConventionForeignKey RemoveForeignKey(
            [NotNull] this IConventionEntityType entityType,
            [NotNull] IReadOnlyList<IConventionProperty> properties,
            [NotNull] IConventionKey principalKey,
            [NotNull] IConventionEntityType principalEntityType)
            => ((EntityType)entityType).RemoveForeignKey(properties, principalKey, principalEntityType);

        /// <summary>
        ///     Gets a navigation property on the given entity type. Returns <c>null</c> if no navigation property is found.
        /// </summary>
        /// <param name="entityType"> The entity type to find the navigation property on. </param>
        /// <param name="memberInfo"> The navigation property on the entity class. </param>
        /// <returns> The navigation property, or <c>null</c> if none is found. </returns>
        public static IConventionNavigation FindNavigation(
            [NotNull] this IConventionEntityType entityType, [NotNull] MemberInfo memberInfo)
            => Check.NotNull(entityType, nameof(entityType))
                .FindNavigation(Check.NotNull(memberInfo, nameof(memberInfo)).GetSimpleMemberName());

        /// <summary>
        ///     Gets a navigation property on the given entity type. Returns <c>null</c> if no navigation property is found.
        /// </summary>
        /// <param name="entityType"> The entity type to find the navigation property on. </param>
        /// <param name="name"> The name of the navigation property on the entity class. </param>
        /// <returns> The navigation property, or <c>null</c> if none is found. </returns>
        public static IConventionNavigation FindNavigation([NotNull] this IConventionEntityType entityType, [NotNull] string name)
            => ((EntityType)entityType).FindNavigation(name);

        /// <summary>
        ///     Gets a navigation property on the given entity type. Does not return navigation properties defined on a base type.
        ///     Returns <c>null</c> if no navigation property is found.
        /// </summary>
        /// <param name="entityType"> The entity type to find the navigation property on. </param>
        /// <param name="name"> The name of the navigation property on the entity class. </param>
        /// <returns> The navigation property, or <c>null</c> if none is found. </returns>
        public static IConventionNavigation FindDeclaredNavigation([NotNull] this IConventionEntityType entityType, [NotNull] string name)
            => ((EntityType)entityType).FindDeclaredNavigation(Check.NotNull(name, nameof(name)));

        /// <summary>
        ///     Returns the defining navigation if one exists or <c>null</c> otherwise.
        /// </summary>
        /// <param name="entityType"> The entity type to find the defining navigation for. </param>
        /// <returns> The defining navigation if one exists or <c>null</c> otherwise. </returns>
        public static IConventionNavigation FindDefiningNavigation([NotNull] this IConventionEntityType entityType)
            => (IConventionNavigation)((IEntityType)entityType).FindDefiningNavigation();

        /// <summary>
        ///     Gets all navigation properties on the given entity type.
        /// </summary>
        /// <param name="entityType"> The entity type to get navigation properties for. </param>
        /// <returns> All navigation properties on the given entity type. </returns>
        public static IEnumerable<IConventionNavigation> GetNavigations([NotNull] this IConventionEntityType entityType)
            => ((EntityType)entityType).GetNavigations();

        /// <summary>
        ///     <para>
        ///         Gets a property on the given entity type. Returns <c>null</c> if no property is found.
        ///     </para>
        ///     <para>
        ///         This API only finds scalar properties and does not find navigation properties. Use
        ///         <see cref="FindNavigation(IConventionEntityType, MemberInfo)" /> to find a navigation property.
        ///     </para>
        /// </summary>
        /// <param name="entityType"> The entity type to find the property on. </param>
        /// <param name="memberInfo"> The property on the entity class. </param>
        /// <returns> The property, or <c>null</c> if none is found. </returns>
        public static IConventionProperty FindProperty([NotNull] this IConventionEntityType entityType, [NotNull] MemberInfo memberInfo)
        {
            Check.NotNull(entityType, nameof(entityType));
            Check.NotNull(memberInfo, nameof(memberInfo));

            return entityType.FindProperty(memberInfo.GetSimpleMemberName());
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
        public static IReadOnlyList<IConventionProperty> FindProperties(
            [NotNull] this IConventionEntityType entityType,
            [NotNull] IReadOnlyList<string> propertyNames)
            => ((EntityType)entityType).FindProperties(Check.NotNull(propertyNames, nameof(propertyNames)));

        /// <summary>
        ///     Finds a property declared on the type with the given name.
        ///     Does not return properties defined on a base type.
        /// </summary>
        /// <param name="entityType"> The entity type to find the property on. </param>
        /// <param name="name"> The property name. </param>
        /// <returns> The property, or <c>null</c> if none is found. </returns>
        public static IConventionProperty FindDeclaredProperty([NotNull] this IConventionEntityType entityType, [NotNull] string name)
            => ((EntityType)entityType).FindDeclaredProperty(name);

        /// <summary>
        ///     Adds a property to this entity type.
        /// </summary>
        /// <param name="entityType"> The entity type to add the property to. </param>
        /// <param name="memberInfo"> The corresponding member on the entity class. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> The newly created property. </returns>
        public static IConventionProperty AddProperty(
            [NotNull] this IConventionEntityType entityType,
            [NotNull] MemberInfo memberInfo,
            bool fromDataAnnotation = false)
            => Check.NotNull(entityType, nameof(entityType)).AddProperty(memberInfo.GetSimpleMemberName(), memberInfo.GetMemberType(),
                memberInfo, setTypeConfigurationSource: true, fromDataAnnotation);

        /// <summary>
        ///     Adds a property to this entity type.
        /// </summary>
        /// <param name="entityType"> The entity type to add the property to. </param>
        /// <param name="name"> The name of the property to add. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> The newly created property. </returns>
        public static IConventionProperty AddProperty(
            [NotNull] this IConventionEntityType entityType, [NotNull] string name,
            bool fromDataAnnotation = false)
            => ((EntityType)entityType).AddProperty(name, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     Adds a property to this entity type.
        /// </summary>
        /// <param name="entityType"> The entity type to add the property to. </param>
        /// <param name="name"> The name of the property to add. </param>
        /// <param name="propertyType"> The type of value the property will hold. </param>
        /// <param name="setTypeConfigurationSource"> Indicates whether the type configuration source should be set. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> The newly created property. </returns>
        public static IConventionProperty AddProperty(
            [NotNull] this IConventionEntityType entityType, [NotNull] string name, [NotNull] Type propertyType,
            bool setTypeConfigurationSource = true, bool fromDataAnnotation = false)
            => entityType.AddProperty(name, propertyType, null, setTypeConfigurationSource, fromDataAnnotation);


        /// <summary>
        ///     Gets the index defined on the given property. Returns null if no index is defined.
        /// </summary>
        /// <param name="entityType"> The entity type to find the index on. </param>
        /// <param name="property"> The property to find the index on. </param>
        /// <returns> The index, or null if none is found. </returns>
        public static IConventionIndex FindIndex([NotNull] this IConventionEntityType entityType, [NotNull] IProperty property)
        {
            Check.NotNull(entityType, nameof(entityType));

            return entityType.FindIndex(new[] { property });
        }

        /// <summary>
        ///     Adds an index to this entity type.
        /// </summary>
        /// <param name="entityType"> The entity type to add the index to. </param>
        /// <param name="property"> The property to be indexed. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> The newly created index. </returns>
        public static IConventionIndex AddIndex(
            [NotNull] this IConventionEntityType entityType,
            [NotNull] IConventionProperty property,
            bool fromDataAnnotation = false)
            => Check.NotNull(entityType, nameof(entityType)).AddIndex(new[] { property }, fromDataAnnotation);

        /// <summary>
        ///     Removes an index from this entity type.
        /// </summary>
        /// <param name="entityType"> The entity type to remove the index from. </param>
        /// <param name="properties"> The properties that make up the index. </param>
        /// <returns> The index that was removed. </returns>
        public static IConventionIndex RemoveIndex(
            [NotNull] this IConventionEntityType entityType,
            [NotNull] IReadOnlyList<IConventionProperty> properties)
            => ((EntityType)entityType).RemoveIndex(properties);

        /// <summary>
        ///     Sets the change tracking strategy to use for this entity type. This strategy indicates how the
        ///     context detects changes to properties for an instance of the entity type.
        /// </summary>
        /// <param name="entityType"> The entity type to set the change tracking strategy for. </param>
        /// <param name="changeTrackingStrategy"> The strategy to use. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        public static void SetChangeTrackingStrategy(
            [NotNull] this IConventionEntityType entityType,
            ChangeTrackingStrategy? changeTrackingStrategy,
            bool fromDataAnnotation = false)
            => Check.NotNull(entityType, nameof(entityType)).AsEntityType()
                .SetChangeTrackingStrategy(
                    changeTrackingStrategy,
                    fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     Returns the configuration source for <see cref="EntityTypeExtensions.GetChangeTrackingStrategy" />.
        /// </summary>
        /// <param name="entityType"> The entity type to find configuration source for. </param>
        /// <returns> The configuration source for <see cref="EntityTypeExtensions.GetChangeTrackingStrategy" />. </returns>
        public static ConfigurationSource? GetChangeTrackingStrategyConfigurationSource([NotNull] this IConventionEntityType entityType)
            => entityType.FindAnnotation(CoreAnnotationNames.ChangeTrackingStrategy)?.GetConfigurationSource();

        /// <summary>
        ///     Sets the LINQ expression filter automatically applied to queries for this entity type.
        /// </summary>
        /// <param name="entityType"> The entity type to set the query filter for. </param>
        /// <param name="queryFilter"> The LINQ expression filter. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        public static void SetQueryFilter(
            [NotNull] this IConventionEntityType entityType,
            [CanBeNull] LambdaExpression queryFilter,
            bool fromDataAnnotation = false)
            => Check.NotNull(entityType, nameof(entityType)).AsEntityType()
                .SetQueryFilter(
                    queryFilter,
                    fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     Returns the configuration source for <see cref="EntityTypeExtensions.GetQueryFilter" />.
        /// </summary>
        /// <param name="entityType"> The entity type to find configuration source for. </param>
        /// <returns> The configuration source for <see cref="EntityTypeExtensions.GetQueryFilter" />. </returns>
        public static ConfigurationSource? GetQueryFilterConfigurationSource([NotNull] this IConventionEntityType entityType)
            => entityType.FindAnnotation(CoreAnnotationNames.QueryFilter)?.GetConfigurationSource();

        /// <summary>
        ///     Sets the LINQ query used as the default source for queries of this type.
        /// </summary>
        /// <param name="entityType"> The entity type to set the defining query for. </param>
        /// <param name="definingQuery"> The LINQ query used as the default source. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        public static void SetDefiningQuery(
            [NotNull] this IConventionEntityType entityType,
            [CanBeNull] LambdaExpression definingQuery,
            bool fromDataAnnotation = false)
            => Check.NotNull(entityType, nameof(entityType)).AsEntityType()
                .SetDefiningQuery(
                    definingQuery,
                    fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     Returns the configuration source for <see cref="EntityTypeExtensions.GetDefiningQuery" />.
        /// </summary>
        /// <param name="entityType"> The entity type to find configuration source for. </param>
        /// <returns> The configuration source for <see cref="EntityTypeExtensions.GetDefiningQuery" />. </returns>
        public static ConfigurationSource? GetDefiningQueryConfigurationSource([NotNull] this IConventionEntityType entityType)
            => entityType.FindAnnotation(CoreAnnotationNames.DefiningQuery)?.GetConfigurationSource();

        /// <summary>
        ///     Returns the <see cref="IConventionProperty" /> that will be used for storing a discriminator value.
        /// </summary>
        /// <param name="entityType"> The entity type to get the discriminator property for. </param>
        public static IConventionProperty GetDiscriminatorProperty([NotNull] this IConventionEntityType entityType)
            => (IConventionProperty)((IEntityType)entityType).GetDiscriminatorProperty();

        /// <summary>
        ///     Sets the <see cref="IProperty" /> that will be used for storing a discriminator value.
        /// </summary>
        /// <param name="entityType"> The entity type to set the discriminator property for. </param>
        /// <param name="property"> The property to set. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        public static void SetDiscriminatorProperty(
            [NotNull] this IConventionEntityType entityType, [CanBeNull] IProperty property, bool fromDataAnnotation = false)
            => Check.NotNull(entityType, nameof(entityType)).AsEntityType()
                .SetDiscriminatorProperty(
                    property,
                    fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     Gets the <see cref="ConfigurationSource" /> for the discriminator property.
        /// </summary>
        /// <param name="entityType"> The entity type to find configuration source for. </param>
        /// <returns> The <see cref="ConfigurationSource" /> or <c>null</c> if no discriminator property has been set. </returns>
        public static ConfigurationSource? GetDiscriminatorPropertyConfigurationSource([NotNull] this IConventionEntityType entityType)
            => entityType.FindAnnotation(CoreAnnotationNames.DiscriminatorProperty)
                ?.GetConfigurationSource();

        /// <summary>
        ///     Sets the discriminator value for this entity type.
        /// </summary>
        /// <param name="entityType"> The entity type to set the discriminator value for. </param>
        /// <param name="value"> The value to set. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        public static void SetDiscriminatorValue(
            [NotNull] this IConventionEntityType entityType, [CanBeNull] object value, bool fromDataAnnotation = false)
        {
            entityType.AsEntityType().CheckDiscriminatorValue(entityType, value);

            entityType.SetAnnotation(CoreAnnotationNames.DiscriminatorValue, value, fromDataAnnotation);
        }

        /// <summary>
        ///     Removes the discriminator value for this entity type.
        /// </summary>
        public static void RemoveDiscriminatorValue([NotNull] this IConventionEntityType entityType)
            => entityType.RemoveAnnotation(CoreAnnotationNames.DiscriminatorValue);

        /// <summary>
        ///     Gets the <see cref="ConfigurationSource" /> for the discriminator value.
        /// </summary>
        /// <returns> The <see cref="ConfigurationSource" /> or <c>null</c> if no discriminator value has been set. </returns>
        public static ConfigurationSource? GetDiscriminatorValueConfigurationSource([NotNull] this IConventionEntityType entityType)
            => entityType.FindAnnotation(CoreAnnotationNames.DiscriminatorValue)
                ?.GetConfigurationSource();
    }
}
