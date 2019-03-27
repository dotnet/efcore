// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
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
        ///     Gets all types in the model that derive from a given entity type.
        /// </summary>
        /// <param name="entityType"> The base type to find types that derive from. </param>
        /// <returns> The derived types. </returns>
        public static IEnumerable<IMutableEntityType> GetDerivedTypes([NotNull] this IMutableEntityType entityType)
            => ((IEntityType)entityType).GetDerivedTypes().Cast<IMutableEntityType>();

        /// <summary>
        ///     Gets the root base type for a given entity type.
        /// </summary>
        /// <param name="entityType"> The type to find the root of. </param>
        /// <returns>
        ///     The root base type. If the given entity type is not a derived type, then the same entity type is returned.
        /// </returns>
        public static IMutableEntityType RootType([NotNull] this IMutableEntityType entityType)
            => (IMutableEntityType)((IEntityType)entityType).RootType();

        /// <summary>
        ///     Sets the primary key for this entity.
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
        ///     Gets the existing primary key of an entity, or sets it if one is not defined.
        /// </summary>
        /// <param name="entityType"> The entity type to get or set the key on. </param>
        /// <param name="property"> The property to set as the primary key if one is not already defined. </param>
        /// <returns> The existing or newly created key. </returns>
        public static IMutableKey GetOrSetPrimaryKey(
            [NotNull] this IMutableEntityType entityType, [NotNull] IMutableProperty property)
            => entityType.GetOrSetPrimaryKey(new[] { property });

        /// <summary>
        ///     Gets the existing primary key of an entity, or sets it if one is not defined.
        /// </summary>
        /// <param name="entityType"> The entity type to get or set the key on. </param>
        /// <param name="properties"> The properties to set as the primary key if one is not already defined. </param>
        /// <returns> The existing or newly created key. </returns>
        public static IMutableKey GetOrSetPrimaryKey(
            [NotNull] this IMutableEntityType entityType, [NotNull] IReadOnlyList<IMutableProperty> properties)
        {
            Check.NotNull(entityType, nameof(entityType));

            return entityType.SetPrimaryKey(properties);
        }

        /// <summary>
        ///     Gets the primary or alternate key that is defined on the given property. Returns null if no key is defined
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
        {
            Check.NotNull(entityType, nameof(entityType));

            return entityType.AddKey(new[] { property });
        }

        /// <summary>
        ///     Gets the existing alternate key defined on a property, or creates a new one if one is not
        ///     already defined.
        /// </summary>
        /// <param name="entityType"> The entity type to get or create the alternate key on. </param>
        /// <param name="property"> The property that is used as the alternate key. </param>
        /// <returns> The existing or newly created alternate key. </returns>
        public static IMutableKey GetOrAddKey(
            [NotNull] this IMutableEntityType entityType, [NotNull] IMutableProperty property)
            => entityType.GetOrAddKey(new[] { property });

        /// <summary>
        ///     Gets the existing alternate key defined on a set of properties, or creates a new one if one is not
        ///     already defined.
        /// </summary>
        /// <param name="entityType"> The entity type to get or create the alternate key on. </param>
        /// <param name="properties"> The properties that are used as the alternate key. </param>
        /// <returns> The existing or newly created alternate key. </returns>
        public static IMutableKey GetOrAddKey(
            [NotNull] this IMutableEntityType entityType, [NotNull] IReadOnlyList<IMutableProperty> properties)
        {
            Check.NotNull(entityType, nameof(entityType));

            return entityType.FindKey(properties) ?? entityType.AddKey(properties);
        }

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
            => ((IEntityType)entityType).FindForeignKeys(properties).Cast<IMutableForeignKey>();

        /// <summary>
        ///     Gets the foreign key for the given properties that points to a given primary or alternate key. Returns null
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
        /// <returns> The foreign key, or null if none is defined. </returns>
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
        ///     Gets all foreign keys that target a given entity type (i.e. foreign keys where the given entity type
        ///     is the principal).
        /// </summary>
        /// <param name="entityType"> The entity type to find the foreign keys for. </param>
        /// <returns> The foreign keys that reference the given entity type. </returns>
        public static IEnumerable<IMutableForeignKey> GetReferencingForeignKeys([NotNull] this IMutableEntityType entityType)
            => ((IEntityType)entityType).GetReferencingForeignKeys().Cast<IMutableForeignKey>();

        /// <summary>
        ///     Adds a new relationship to this entity.
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
        ///     Gets an existing relationship, or creates a new one if one is not already defined.
        /// </summary>
        /// <param name="entityType"> The entity type to get or add the foreign key to. </param>
        /// <param name="property"> The property that the foreign key is defined on. </param>
        /// <param name="principalKey"> The primary or alternate key that is referenced. </param>
        /// <param name="principalEntityType">
        ///     The entity type that the relationship targets. This may be different from the type that <paramref name="principalKey" />
        ///     is defined on when the relationship targets a derived type in an inheritance hierarchy (since the key is defined on the
        ///     base type of the hierarchy).
        /// </param>
        /// <returns> The existing or newly created foreign key. </returns>
        public static IMutableForeignKey GetOrAddForeignKey(
            [NotNull] this IMutableEntityType entityType,
            [NotNull] IMutableProperty property,
            [NotNull] IMutableKey principalKey,
            [NotNull] IMutableEntityType principalEntityType)
            => entityType.GetOrAddForeignKey(new[] { property }, principalKey, principalEntityType);

        /// <summary>
        ///     Gets an existing relationship, or creates a new one if one is not already defined.
        /// </summary>
        /// <param name="entityType"> The entity type to get or add the foreign key to. </param>
        /// <param name="properties"> The properties that the foreign key is defined on. </param>
        /// <param name="principalKey"> The primary or alternate key that is referenced. </param>
        /// <param name="principalEntityType">
        ///     The entity type that the relationship targets. This may be different from the type that <paramref name="principalKey" />
        ///     is defined on when the relationship targets a derived type in an inheritance hierarchy (since the key is defined on the
        ///     base type of the hierarchy).
        /// </param>
        /// <returns> The existing or newly created foreign key. </returns>
        public static IMutableForeignKey GetOrAddForeignKey(
            [NotNull] this IMutableEntityType entityType,
            [NotNull] IReadOnlyList<IMutableProperty> properties,
            [NotNull] IMutableKey principalKey,
            [NotNull] IMutableEntityType principalEntityType)
        {
            Check.NotNull(entityType, nameof(entityType));

            return entityType.FindForeignKey(properties, principalKey, principalEntityType)
                   ?? entityType.AddForeignKey(properties, principalKey, principalEntityType);
        }

        /// <summary>
        ///     Gets a navigation property on the given entity type. Returns null if no navigation property is found.
        /// </summary>
        /// <param name="entityType"> The entity type to find the navigation property on. </param>
        /// <param name="propertyInfo"> The navigation property on the entity class. </param>
        /// <returns> The navigation property, or null if none is found. </returns>
        public static IMutableNavigation FindNavigation(
            [NotNull] this IMutableEntityType entityType, [NotNull] PropertyInfo propertyInfo)
        {
            Check.NotNull(entityType, nameof(entityType));
            Check.NotNull(propertyInfo, nameof(propertyInfo));

            return entityType.FindNavigation(propertyInfo.GetSimpleMemberName());
        }

        /// <summary>
        ///     Gets a navigation property on the given entity type. Returns null if no navigation property is found.
        /// </summary>
        /// <param name="entityType"> The entity type to find the navigation property on. </param>
        /// <param name="name"> The name of the navigation property on the entity class. </param>
        /// <returns> The navigation property, or null if none is found. </returns>
        public static IMutableNavigation FindNavigation([NotNull] this IMutableEntityType entityType, [NotNull] string name)
            => (IMutableNavigation)((IEntityType)entityType).FindNavigation(name);

        /// <summary>
        ///     Gets all navigation properties on the given entity type.
        /// </summary>
        /// <param name="entityType"> The entity type to get navigation properties for. </param>
        /// <returns> All navigation properties on the given entity type. </returns>
        public static IEnumerable<IMutableNavigation> GetNavigations([NotNull] this IMutableEntityType entityType)
            => ((IEntityType)entityType).GetNavigations().Cast<IMutableNavigation>();

        /// <summary>
        ///     <para>
        ///         Gets a property on the given entity type. Returns null if no property is found.
        ///     </para>
        ///     <para>
        ///         This API only finds scalar properties and does not find navigation properties. Use
        ///         <see cref="FindNavigation(IMutableEntityType, PropertyInfo)" /> to find a navigation property.
        ///     </para>
        /// </summary>
        /// <param name="entityType"> The entity type to find the property on. </param>
        /// <param name="propertyInfo"> The property on the entity class. </param>
        /// <returns> The property, or null if none is found. </returns>
        public static IMutableProperty FindProperty([NotNull] this IMutableEntityType entityType, [NotNull] PropertyInfo propertyInfo)
        {
            Check.NotNull(entityType, nameof(entityType));
            Check.NotNull(propertyInfo, nameof(propertyInfo));

            return entityType.FindProperty(propertyInfo.GetSimpleMemberName());
        }

        /// <summary>
        ///     Adds a property to this entity.
        /// </summary>
        /// <param name="entityType"> The entity type to add the property to. </param>
        /// <param name="propertyInfo"> The corresponding property in the entity class. </param>
        /// <returns> The newly created property. </returns>
        public static IMutableProperty AddProperty(
            [NotNull] this IMutableEntityType entityType, [NotNull] PropertyInfo propertyInfo)
        {
            Check.NotNull(entityType, nameof(entityType));
            Check.NotNull(propertyInfo, nameof(propertyInfo));

            return entityType.AsEntityType().AddProperty(propertyInfo);
        }

        /// <summary>
        ///     Gets the property with the given name, or creates a new one if one is not already defined.
        /// </summary>
        /// <param name="entityType"> The entity type to get or add the property to. </param>
        /// <param name="name"> The name of the property. </param>
        /// <param name="propertyType"> The type of value the property will hold. </param>
        /// <returns> The existing or newly created property. </returns>
        /// <remarks> The returned property might not have the specified type. </remarks>
        public static IMutableProperty GetOrAddProperty(
            [NotNull] this IMutableEntityType entityType, [NotNull] string name, [CanBeNull] Type propertyType)
            => entityType.FindProperty(name) ?? entityType.AddProperty(name, propertyType);

        /// <summary>
        ///     Gets the property with the given name, or creates a new one if one is not already defined.
        /// </summary>
        /// <param name="entityType"> The entity type to get or add the property to. </param>
        /// <param name="propertyInfo"> The corresponding property in the entity class. </param>
        /// <returns> The existing or newly created property. </returns>
        /// <remarks> The returned property might not have the specified type. </remarks>
        public static IMutableProperty GetOrAddProperty([NotNull] this IMutableEntityType entityType, [NotNull] PropertyInfo propertyInfo)
            => entityType.FindProperty(propertyInfo) ?? entityType.AddProperty(propertyInfo);

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
        ///     Adds an index to this entity.
        /// </summary>
        /// <param name="entityType"> The entity type to add the index to. </param>
        /// <param name="property"> The property to be indexed. </param>
        /// <returns> The newly created index. </returns>
        public static IMutableIndex AddIndex(
            [NotNull] this IMutableEntityType entityType, [NotNull] IMutableProperty property)
        {
            Check.NotNull(entityType, nameof(entityType));

            return entityType.AddIndex(new[] { property });
        }

        /// <summary>
        ///     Gets the index defined on the given property or creates a new one if one is not already defined.
        /// </summary>
        /// <param name="entityType"> The entity type to get or add the index to. </param>
        /// <param name="property"> The property to be indexed. </param>
        /// <returns> The existing or newly created index. </returns>
        public static IMutableIndex GetOrAddIndex(
            [NotNull] this IMutableEntityType entityType, [NotNull] IMutableProperty property)
            => entityType.GetOrAddIndex(new[] { property });

        /// <summary>
        ///     Gets the index defined on the given property or creates a new one if one is not already defined.
        /// </summary>
        /// <param name="entityType"> The entity type to get or add the index to. </param>
        /// <param name="properties"> The properties to be indexed. </param>
        /// <returns> The existing or newly created index. </returns>
        public static IMutableIndex GetOrAddIndex(
            [NotNull] this IMutableEntityType entityType, [NotNull] IReadOnlyList<IMutableProperty> properties)
        {
            Check.NotNull(entityType, nameof(entityType));

            return entityType.FindIndex(properties) ?? entityType.AddIndex(properties);
        }

        /// <summary>
        ///     <para>
        ///         Sets the <see cref="PropertyAccessMode" /> to use for properties and navigations of this entity type.
        ///     </para>
        ///     <para>
        ///         Note that individual properties and navigations can override this access mode. The value set here will
        ///         be used for any property or navigation for which no override has been specified.
        ///     </para>
        /// </summary>
        /// <param name="entityType"> The entity type for which to set the access mode. </param>
        /// <param name="propertyAccessMode"> The <see cref="PropertyAccessMode" />, or null to clear the mode set.</param>
        public static void SetPropertyAccessMode(
            [NotNull] this IMutableEntityType entityType, PropertyAccessMode? propertyAccessMode)
        {
            Check.NotNull(entityType, nameof(entityType));

            entityType[CoreAnnotationNames.PropertyAccessModeAnnotation] = propertyAccessMode;
        }

        /// <summary>
        ///     <para>
        ///         Sets the <see cref="PropertyAccessMode" /> to use for navigations of this entity type.
        ///     </para>
        ///     <para>
        ///         Note that individual navigations can override this access mode. The value set here will
        ///         be used for any navigation for which no override has been specified.
        ///     </para>
        /// </summary>
        /// <param name="entityType"> The entity type for which to set the access mode. </param>
        /// <param name="propertyAccessMode"> The <see cref="PropertyAccessMode" />, or null to clear the mode set.</param>
        public static void SetNavigationAccessMode(
            [NotNull] this IMutableEntityType entityType, PropertyAccessMode? propertyAccessMode)
        {
            Check.NotNull(entityType, nameof(entityType));

            entityType[CoreAnnotationNames.NavigationAccessModeAnnotation] = propertyAccessMode;
        }

        /// <summary>
        ///     Sets the change tracking strategy to use for this entity type. This strategy indicates how the
        ///     context detects changes to properties for an instance of the entity type.
        /// </summary>
        /// <param name="entityType"> The entity type to set the change tracking strategy for. </param>
        /// <param name="changeTrackingStrategy"> The strategy to use. </param>
        public static void SetChangeTrackingStrategy(
            [NotNull] this IMutableEntityType entityType,
            ChangeTrackingStrategy changeTrackingStrategy)
            => Check.NotNull(entityType, nameof(entityType)).AsEntityType().ChangeTrackingStrategy = changeTrackingStrategy;
    }
}
