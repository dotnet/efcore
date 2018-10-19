// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics;
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
    ///     Extension methods for <see cref="IEntityType" />.
    /// </summary>
    public static class EntityTypeExtensions
    {
        /// <summary>
        ///     Gets all types in the model that derive from a given entity type.
        /// </summary>
        /// <param name="entityType"> The base type to find types that derive from. </param>
        /// <returns> The derived types. </returns>
        public static IEnumerable<IEntityType> GetDerivedTypes([NotNull] this IEntityType entityType)
        {
            Check.NotNull(entityType, nameof(entityType));

            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var derivedType in entityType.Model.GetEntityTypes())
            {
                if (derivedType.BaseType != null
                    && derivedType != entityType
                    && entityType.IsAssignableFrom(derivedType))
                {
                    yield return derivedType;
                }
            }
        }

        /// <summary>
        ///     Gets the root base type for a given entity type.
        /// </summary>
        /// <param name="entityType"> The type to find the root of. </param>
        /// <returns>
        ///     The root base type. If the given entity type is not a derived type, then the same entity type is returned.
        /// </returns>
        public static IEntityType RootType([NotNull] this IEntityType entityType)
        {
            Check.NotNull(entityType, nameof(entityType));

            return entityType.BaseType?.RootType() ?? entityType;
        }

        /// <summary>
        ///     Determines if an entity type derives from (or is the same as) a given entity type.
        /// </summary>
        /// <param name="entityType"> The base entity type. </param>
        /// <param name="derivedType"> The entity type to check if it derives from <paramref name="entityType" />. </param>
        /// <returns>
        ///     True if <paramref name="derivedType" /> derives from (or is the same as) <paramref name="entityType" />, otherwise false.
        /// </returns>
        public static bool IsAssignableFrom([NotNull] this IEntityType entityType, [NotNull] IEntityType derivedType)
        {
            Check.NotNull(entityType, nameof(entityType));
            Check.NotNull(derivedType, nameof(derivedType));

            var baseType = derivedType;
            while (baseType != null)
            {
                if (baseType == entityType)
                {
                    return true;
                }

                baseType = baseType.BaseType;
            }

            return false;
        }

        /// <summary>
        ///     Gets the least derived type between the specified two.
        /// </summary>
        /// <param name="entityType"> The type to compare. </param>
        /// <param name="otherEntityType"> The other entity type to compare with. </param>
        /// <returns>
        ///     The least derived type between the specified two.
        ///     If the given entity types are not related, then <c>null</c> is returned.
        /// </returns>
        public static IEntityType LeastDerivedType([NotNull] this IEntityType entityType, [NotNull] IEntityType otherEntityType)
        {
            Check.NotNull(entityType, nameof(entityType));
            Check.NotNull(otherEntityType, nameof(otherEntityType));

            return entityType.IsAssignableFrom(otherEntityType)
                ? entityType
                : otherEntityType.IsAssignableFrom(entityType)
                    ? otherEntityType
                    : null;
        }

        /// <summary>
        ///     Gets a value indicating whether this entity type has a defining navigation.
        /// </summary>
        /// <returns> True if this entity type has a defining navigation. </returns>
        [DebuggerStepThrough]
        public static bool HasDefiningNavigation([NotNull] this IEntityType entityType)
            => entityType.DefiningEntityType != null;

        /// <summary>
        ///     Gets a value indicating whether this entity type is owned by another entity type.
        /// </summary>
        /// <returns> True if this entity type is owned by another entity type. </returns>
        [DebuggerStepThrough]
        public static bool IsOwned([NotNull] this IEntityType entityType)
            => entityType.GetForeignKeys().Any(fk => fk.IsOwnership);

        /// <summary>
        ///     Gets the primary or alternate key that is defined on the given property. Returns null if no key is defined
        ///     for the given property.
        /// </summary>
        /// <param name="entityType"> The entity type to find the key on. </param>
        /// <param name="property"> The property that the key is defined on. </param>
        /// <returns> The key, or null if none is defined. </returns>
        public static IKey FindKey([NotNull] this IEntityType entityType, [NotNull] IProperty property)
        {
            Check.NotNull(entityType, nameof(entityType));

            return entityType.FindKey(new[] { property });
        }

        /// <summary>
        ///     Gets the foreign keys defined on the given property. Only foreign keys that are defined on exactly the specified
        ///     property are returned. Composite foreign keys that include the specified property are not returned.
        /// </summary>
        /// <param name="entityType"> The entity type to find the foreign keys on. </param>
        /// <param name="property"> The property to find the foreign keys on. </param>
        /// <returns> The foreign keys. </returns>
        public static IEnumerable<IForeignKey> FindForeignKeys([NotNull] this IEntityType entityType, [NotNull] IProperty property)
            => entityType.FindForeignKeys(new[] { property });

        /// <summary>
        ///     Gets the foreign keys defined on the given properties. Only foreign keys that are defined on exactly the specified
        ///     set of properties are returned.
        /// </summary>
        /// <param name="entityType"> The entity type to find the foreign keys on. </param>
        /// <param name="properties"> The properties to find the foreign keys on. </param>
        /// <returns> The foreign keys. </returns>
        public static IEnumerable<IForeignKey> FindForeignKeys(
            [NotNull] this IEntityType entityType, [NotNull] IReadOnlyList<IProperty> properties)
        {
            Check.NotNull(entityType, nameof(entityType));
            Check.NotEmpty(properties, nameof(properties));
            Check.HasNoNulls(properties, nameof(properties));

            foreach (var foreignKey in entityType.GetForeignKeys())
            {
                if (PropertyListComparer.Instance.Equals(foreignKey.Properties, properties))
                {
                    yield return foreignKey;
                }
            }
        }

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
        public static IForeignKey FindForeignKey(
            [NotNull] this IEntityType entityType,
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
        public static IEnumerable<IForeignKey> GetReferencingForeignKeys([NotNull] this IEntityType entityType)
            => Check.NotNull(entityType, nameof(entityType)).AsEntityType().GetReferencingForeignKeys();

        /// <summary>
        ///     Gets a navigation property on the given entity type. Returns null if no navigation property is found.
        /// </summary>
        /// <param name="entityType"> The entity type to find the navigation property on. </param>
        /// <param name="propertyInfo"> The navigation property on the entity class. </param>
        /// <returns> The navigation property, or null if none is found. </returns>
        public static INavigation FindNavigation([NotNull] this IEntityType entityType, [NotNull] PropertyInfo propertyInfo)
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
        public static INavigation FindNavigation([NotNull] this IEntityType entityType, [NotNull] string name)
        {
            Check.NotNull(entityType, nameof(entityType));
            Check.NotNull(name, nameof(name));

            return entityType.GetNavigations().FirstOrDefault(n => n.Name.Equals(name));
        }

        /// <summary>
        ///     Gets all navigation properties on the given entity type.
        /// </summary>
        /// <param name="entityType"> The entity type to get navigation properties for. </param>
        /// <returns> All navigation properties on the given entity type. </returns>
        public static IEnumerable<INavigation> GetNavigations([NotNull] this IEntityType entityType)
            => Check.NotNull(entityType, nameof(entityType)).AsEntityType().GetNavigations();

        /// <summary>
        ///     <para>
        ///         Gets a property on the given entity type. Returns null if no property is found.
        ///     </para>
        ///     <para>
        ///         This API only finds scalar properties and does not find navigation properties. Use
        ///         <see cref="FindNavigation(IEntityType, PropertyInfo)" /> to find a navigation property.
        ///     </para>
        /// </summary>
        /// <param name="entityType"> The entity type to find the property on. </param>
        /// <param name="propertyInfo"> The property on the entity class. </param>
        /// <returns> The property, or null if none is found. </returns>
        public static IProperty FindProperty([NotNull] this IEntityType entityType, [NotNull] PropertyInfo propertyInfo)
        {
            Check.NotNull(entityType, nameof(entityType));
            Check.NotNull(propertyInfo, nameof(propertyInfo));

            return entityType.FindProperty(propertyInfo.GetSimpleMemberName());
        }

        /// <summary>
        ///     Gets the index defined on the given property. Returns null if no index is defined.
        /// </summary>
        /// <param name="entityType"> The entity type to find the index on. </param>
        /// <param name="property"> The property to find the index on. </param>
        /// <returns> The index, or null if none is found. </returns>
        public static IIndex FindIndex([NotNull] this IEntityType entityType, [NotNull] IProperty property)
        {
            Check.NotNull(entityType, nameof(entityType));

            return entityType.FindIndex(new[] { property });
        }

        /// <summary>
        ///     Gets the change tracking strategy being used for this entity type. This strategy indicates how the
        ///     context detects changes to properties for an instance of the entity type.
        /// </summary>
        /// <param name="entityType"> The entity type to get the change tracking strategy for. </param>
        /// <returns> The change tracking strategy. </returns>
        public static ChangeTrackingStrategy GetChangeTrackingStrategy(
            [NotNull] this IEntityType entityType)
            => ((EntityType)Check.NotNull(entityType, nameof(entityType))).ChangeTrackingStrategy;
    }
}
