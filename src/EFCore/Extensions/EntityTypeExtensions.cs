// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

#nullable enable

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     Entity type extension methods for <see cref="IReadOnlyEntityType" />.
    /// </summary>
    public static class EntityTypeExtensions
    {
        /// <summary>
        ///     Returns all the derived types of the given <see cref="IReadOnlyEntityType" />, including the type itself,
        ///     which are not <see langword="abstract" />.
        /// </summary>
        /// <param name="entityType"> The entity type. </param>
        /// <returns> Non-abstract, derived types. </returns>
        public static IEnumerable<IReadOnlyEntityType> GetConcreteDerivedTypesInclusive([NotNull] this IReadOnlyEntityType entityType)
            => entityType.GetDerivedTypesInclusive().Where(et => !et.IsAbstract());

        /// <summary>
        ///     Checks if this entity type represents an abstract type.
        /// </summary>
        /// <param name="type"> The entity type. </param>
        /// <returns> <see langword="true" /> if the type is abstract, <see langword="false" /> otherwise. </returns>
        [DebuggerStepThrough]
        public static bool IsAbstract([NotNull] this IReadOnlyTypeBase type)
            => type.ClrType.IsAbstract;

        /// <summary>
        ///     Gets the root base type for a given entity type.
        /// </summary>
        /// <param name="entityType"> The type to find the root of. </param>
        /// <returns>
        ///     The root base type. If the given entity type is not a derived type, then the same entity type is returned.
        /// </returns>
        public static IReadOnlyEntityType GetRootType([NotNull] this IReadOnlyEntityType entityType)
        {
            Check.NotNull(entityType, nameof(entityType));

            return entityType.BaseType?.GetRootType() ?? entityType;
        }

        /// <summary>
        ///     Gets all types in the model from which a given entity type derives, starting with the root.
        /// </summary>
        /// <param name="entityType"> The entity type. </param>
        /// <returns>
        ///     The base types.
        /// </returns>
        public static IEnumerable<IReadOnlyEntityType> GetAllBaseTypes([NotNull] this IReadOnlyEntityType entityType)
            => entityType.GetAllBaseTypesAscending().Reverse();

        /// <summary>
        ///     Gets all types in the model from which a given entity type derives, starting with the closest one.
        /// </summary>
        /// <param name="entityType"> The entity type. </param>
        /// <returns>
        ///     The base types.
        /// </returns>
        public static IEnumerable<IReadOnlyEntityType> GetAllBaseTypesAscending([NotNull] this IReadOnlyEntityType entityType)
            => entityType.GetAllBaseTypesInclusiveAscending().Skip(1);

        /// <summary>
        ///     Gets all types in the model that derive from a given entity type.
        /// </summary>
        /// <param name="entityType"> The entity type. </param>
        /// <returns> The derived types. </returns>
        public static IEnumerable<IReadOnlyEntityType> GetDerivedTypes([NotNull] this IReadOnlyEntityType entityType)
            => Check.NotNull(entityType, nameof(entityType)).AsEntityType().GetDerivedTypes();

        /// <summary>
        ///     Returns all derived types of the given <see cref="IReadOnlyEntityType" />, including the type itself.
        /// </summary>
        /// <param name="entityType"> The entity type. </param>
        /// <returns> Derived types. </returns>
        public static IEnumerable<IReadOnlyEntityType> GetDerivedTypesInclusive([NotNull] this IReadOnlyEntityType entityType)
            => new[] { entityType }.Concat(entityType.GetDerivedTypes());

        /// <summary>
        ///     Gets all types in the model that directly derive from a given entity type.
        /// </summary>
        /// <param name="entityType"> The entity type. </param>
        /// <returns> The derived types. </returns>
        public static IEnumerable<IReadOnlyEntityType> GetDirectlyDerivedTypes([NotNull] this IReadOnlyEntityType entityType)
            => ((EntityType)entityType).GetDirectlyDerivedTypes();

        /// <summary>
        ///     Determines if this entity type derives from (or is the same as) a given entity type.
        /// </summary>
        /// <param name="entityType"> The base entity type. </param>
        /// <param name="derivedType"> The entity type to check if it derives from <paramref name="entityType" />. </param>
        /// <returns>
        ///     <see langword="true" /> if <paramref name="derivedType" /> derives from (or is the same as) <paramref name="entityType" />,
        ///     otherwise <see langword="false" />.
        /// </returns>
        public static bool IsAssignableFrom([NotNull] this IReadOnlyEntityType entityType, [NotNull] IReadOnlyEntityType derivedType)
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
        ///     Returns the closest entity type that is a parent of both given entity types. If one of the given entities is
        ///     a parent of the other, that parent is returned. Returns <see langword="null" /> if the two entity types aren't in the same hierarchy.
        /// </summary>
        /// <param name="entityType1"> An entity type.</param>
        /// <param name="entityType2"> Another entity type.</param>
        /// <returns>
        ///     The closest common parent of <paramref name="entityType1" /> and <paramref name="entityType2" />,
        ///     or null if they have not common parent.
        /// </returns>
        public static IReadOnlyEntityType? GetClosestCommonParent([NotNull] this IReadOnlyEntityType entityType1, [NotNull] IReadOnlyEntityType entityType2)
        {
            Check.NotNull(entityType1, nameof(entityType1));
            Check.NotNull(entityType2, nameof(entityType2));

            return entityType1
                .GetAllBaseTypesInclusiveAscending()
                .FirstOrDefault(i => entityType2.GetAllBaseTypesInclusiveAscending().Any(j => j == i));
        }

        /// <summary>
        ///     Determines if this entity type derives from (but is not the same as) a given entity type.
        /// </summary>
        /// <param name="entityType"> The entity type. </param>
        /// <param name="baseType"> The entity type to check if it is a base type of <paramref name="entityType" />. </param>
        /// <returns>
        ///     <see langword="true" /> if <paramref name="entityType" /> derives from (but is not the same as) <paramref name="baseType" />,
        ///     otherwise <see langword="false" />.
        /// </returns>
        public static bool IsStrictlyDerivedFrom([NotNull] this IReadOnlyEntityType entityType, [NotNull] IReadOnlyEntityType baseType)
        {
            Check.NotNull(entityType, nameof(entityType));
            Check.NotNull(baseType, nameof(baseType));

            return entityType == baseType ? false : baseType.IsAssignableFrom(entityType);
        }

        /// <summary>
        ///     Gets the least derived type between the specified two.
        /// </summary>
        /// <param name="entityType"> The entity type. </param>
        /// <param name="otherEntityType"> The other entity type to compare with. </param>
        /// <returns>
        ///     The least derived type between the specified two.
        ///     If the given entity types are not related, then <see langword="null" /> is returned.
        /// </returns>
        public static IReadOnlyEntityType? LeastDerivedType([NotNull] this IReadOnlyEntityType entityType, [NotNull] IReadOnlyEntityType otherEntityType)
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
        ///     Returns all base types of the given <see cref="IReadOnlyEntityType" />, including the type itself, top to bottom.
        /// </summary>
        /// <param name="entityType"> The entity type. </param>
        /// <returns> Base types. </returns>
        public static IEnumerable<IReadOnlyEntityType> GetAllBaseTypesInclusive([NotNull] this IReadOnlyEntityType entityType)
            => GetAllBaseTypesInclusiveAscending(entityType).Reverse();

        /// <summary>
        ///     Returns all base types of the given <see cref="IReadOnlyEntityType" />, including the type itself, bottom to top.
        /// </summary>
        /// <param name="entityType"> The entity type. </param>
        /// <returns> Base types. </returns>
        public static IEnumerable<IReadOnlyEntityType> GetAllBaseTypesInclusiveAscending([NotNull] this IReadOnlyEntityType entityType)
        {
            Check.NotNull(entityType, nameof(entityType));

            var tmp = (IReadOnlyEntityType?)entityType;
            while (tmp != null)
            {
                yield return tmp;
                tmp = tmp.BaseType;
            }
        }

        /// <summary>
        ///     <para>
        ///         Gets all keys declared on the given <see cref="IReadOnlyEntityType" />.
        ///     </para>
        ///     <para>
        ///         This method does not return keys declared on base types.
        ///         It is useful when iterating over all entity types to avoid processing the same key more than once.
        ///         Use <see cref="IReadOnlyEntityType.GetKeys" /> to also return keys declared on base types.
        ///     </para>
        /// </summary>
        /// <param name="entityType"> The entity type. </param>
        /// <returns> Declared keys. </returns>
        public static IEnumerable<IReadOnlyKey> GetDeclaredKeys([NotNull] this IReadOnlyEntityType entityType)
            => entityType.AsEntityType().GetDeclaredKeys();

        /// <summary>
        ///     <para>
        ///         Gets all foreign keys declared on the given <see cref="IReadOnlyEntityType" />.
        ///     </para>
        ///     <para>
        ///         This method does not return foreign keys declared on base types.
        ///         It is useful when iterating over all entity types to avoid processing the same foreign key more than once.
        ///         Use <see cref="IReadOnlyEntityType.GetForeignKeys" /> to also return foreign keys declared on base types.
        ///     </para>
        /// </summary>
        /// <param name="entityType"> The entity type. </param>
        /// <returns> Declared foreign keys. </returns>
        public static IEnumerable<IReadOnlyForeignKey> GetDeclaredForeignKeys([NotNull] this IReadOnlyEntityType entityType)
            => entityType.AsEntityType().GetDeclaredForeignKeys();

        /// <summary>
        ///     <para>
        ///         Gets all foreign keys declared on the types derived from the given <see cref="IReadOnlyEntityType" />.
        ///     </para>
        ///     <para>
        ///         This method does not return foreign keys declared on the given entity type itself.
        ///         Use <see cref="IReadOnlyEntityType.GetForeignKeys" /> to return foreign keys declared on this
        ///         and base entity typed types.
        ///     </para>
        /// </summary>
        /// <param name="entityType"> The entity type. </param>
        /// <returns> Derived foreign keys. </returns>
        public static IEnumerable<IReadOnlyForeignKey> GetDerivedForeignKeys([NotNull] this IReadOnlyEntityType entityType)
            => entityType.AsEntityType().GetDerivedForeignKeys();

        /// <summary>
        ///     <para>
        ///         Gets all navigation properties declared on the given <see cref="IReadOnlyEntityType" />.
        ///     </para>
        ///     <para>
        ///         This method does not return navigation properties declared on base types.
        ///         It is useful when iterating over all entity types to avoid processing the same navigation property more than once.
        ///         Use <see cref="GetNavigations" /> to also return navigation properties declared on base types.
        ///     </para>
        /// </summary>
        /// <param name="entityType"> The entity type. </param>
        /// <returns> Declared navigation properties. </returns>
        public static IEnumerable<IReadOnlyNavigation> GetDeclaredNavigations([NotNull] this IReadOnlyEntityType entityType)
            => ((EntityType)entityType).GetDeclaredNavigations();

        /// <summary>
        ///     <para>
        ///         Gets all non-navigation properties declared on the given <see cref="IReadOnlyEntityType" />.
        ///     </para>
        ///     <para>
        ///         This method does not return properties declared on base types.
        ///         It is useful when iterating over all entity types to avoid processing the same property more than once.
        ///         Use <see cref="IReadOnlyEntityType.GetProperties" /> to also return properties declared on base types.
        ///     </para>
        /// </summary>
        /// <param name="entityType"> The entity type. </param>
        /// <returns> Declared non-navigation properties. </returns>
        public static IEnumerable<IReadOnlyProperty> GetDeclaredProperties([NotNull] this IReadOnlyEntityType entityType)
            => entityType.AsEntityType().GetDeclaredProperties();

        /// <summary>
        ///     <para>
        ///         Gets all service properties declared on the given <see cref="IReadOnlyEntityType" />.
        ///     </para>
        ///     <para>
        ///         This method does not return properties declared on base types.
        ///         It is useful when iterating over all entity types to avoid processing the same property more than once.
        ///         Use <see cref="IReadOnlyEntityType.GetServiceProperties" /> to also return properties declared on base types.
        ///     </para>
        /// </summary>
        /// <param name="entityType"> The entity type. </param>
        /// <returns> Declared service properties. </returns>
        public static IEnumerable<IReadOnlyServiceProperty> GetDeclaredServiceProperties([NotNull] this IReadOnlyEntityType entityType)
            => entityType.AsEntityType().GetDeclaredServiceProperties();

        /// <summary>
        ///     <para>
        ///         Gets all indexes declared on the given <see cref="IReadOnlyEntityType" />.
        ///     </para>
        ///     <para>
        ///         This method does not return indexes declared on base types.
        ///         It is useful when iterating over all entity types to avoid processing the same index more than once.
        ///         Use <see cref="IReadOnlyEntityType.GetForeignKeys" /> to also return indexes declared on base types.
        ///     </para>
        /// </summary>
        /// <param name="entityType"> The entity type. </param>
        /// <returns> Declared indexes. </returns>
        public static IEnumerable<IReadOnlyIndex> GetDeclaredIndexes([NotNull] this IReadOnlyEntityType entityType)
            => ((EntityType)entityType).GetDeclaredIndexes();

        /// <summary>
        ///     <para>
        ///         Gets all indexes declared on the types derived from the given <see cref="IReadOnlyEntityType" />.
        ///     </para>
        /// </summary>
        /// <param name="entityType"> The entity type. </param>
        /// <returns> Derived indexes. </returns>
        public static IEnumerable<IReadOnlyIndex> GetDerivedIndexes([NotNull] this IReadOnlyEntityType entityType)
            => ((EntityType)entityType).GetDerivedIndexes();

        /// <summary>
        ///     <para>
        ///         Gets the unnamed index defined on the given property. Returns <see langword="null" /> if no such index is defined.
        ///     </para>
        ///     <para>
        ///         Named indexes will not be returned even if the list of properties matches.
        ///     </para>
        /// </summary>
        /// <param name="entityType"> The entity type. </param>
        /// <param name="property"> The property to find the index on. </param>
        /// <returns> The index, or null if none is found. </returns>
        public static IReadOnlyIndex? FindIndex([NotNull] this IReadOnlyEntityType entityType, [NotNull] IReadOnlyProperty property)
            => entityType.FindIndex(new[] { property });

        /// <summary>
        ///     Gets the friendly display name for the given <see cref="IReadOnlyTypeBase" />.
        /// </summary>
        /// <param name="type"> The entity type. </param>
        /// <returns> The display name. </returns>
        [DebuggerStepThrough]
        public static string DisplayName([NotNull] this IReadOnlyTypeBase type)
        {
            if (!type.HasSharedClrType)
            {
                return type.ClrType.ShortDisplayName();
            }

            var shortName = type.Name;
            var hashIndex = shortName.IndexOf("#", StringComparison.Ordinal);
            if (hashIndex == -1)
            {
                return type.Name + " (" + type.ClrType.ShortDisplayName() + ")";
            }

            var plusIndex = shortName.LastIndexOf("+", StringComparison.Ordinal);
            if (plusIndex != -1)
            {
                shortName = shortName[(plusIndex + 1)..];
            }
            else
            {
                var length = shortName.Length;
                var dotIndex = shortName.LastIndexOf(".", hashIndex, hashIndex + 1, StringComparison.Ordinal);
                if (dotIndex != -1)
                {
                    dotIndex = shortName.LastIndexOf(".", dotIndex - 1, dotIndex, StringComparison.Ordinal);
                    if (dotIndex != -1)
                    {
                        shortName = shortName[(dotIndex + 1)..];
                    }
                }
            }

            return shortName == type.Name
                       ? shortName + " (" + type.ClrType.ShortDisplayName() + ")"
                       : shortName;
        }

        /// <summary>
        ///     Gets the unique name for the given <see cref="IReadOnlyTypeBase" />.
        /// </summary>
        /// <param name="type"> The entity type. </param>
        /// <returns> The full name. </returns>
        [DebuggerStepThrough]
        public static string FullName([NotNull] this IReadOnlyTypeBase type) => type.Name;

        /// <summary>
        ///     Gets a short name for the given <see cref="IReadOnlyTypeBase" /> that can be used in other identifiers.
        /// </summary>
        /// <param name="type"> The entity type. </param>
        /// <returns> The short name. </returns>
        [DebuggerStepThrough]
        public static string ShortName([NotNull] this IReadOnlyTypeBase type)
        {
            if (!type.HasSharedClrType)
            {
                return type.ClrType.ShortDisplayName();
            }

            var hashIndex = type.Name.LastIndexOf("#", StringComparison.Ordinal);
            if (hashIndex == -1)
            {
                var plusIndex = type.Name.LastIndexOf("+", StringComparison.Ordinal);
                if (plusIndex == -1)
                {
                    var dotIndex = type.Name.LastIndexOf(".", StringComparison.Ordinal);
                    return dotIndex == -1
                            ? type.Name
                            : type.Name[(dotIndex + 1)..];
                }

                return type.Name[(plusIndex + 1)..];
            }

            return type.Name[(hashIndex + 1)..];
        }

        /// <summary>
        ///     Gets a value indicating whether this entity type has a defining navigation.
        /// </summary>
        /// <param name="entityType"> The entity type. </param>
        /// <returns> <see langword="true" /> if this entity type has a defining navigation. </returns>
        [DebuggerStepThrough]
        [Obsolete("Entity types with defining navigations have been replaced by shared-type entity types")]
        public static bool HasDefiningNavigation([NotNull] this IReadOnlyEntityType entityType)
            => entityType.HasDefiningNavigation();

        /// <summary>
        ///     Gets a value indicating whether this entity type is owned by another entity type.
        /// </summary>
        /// <param name="entityType"> The entity type. </param>
        /// <returns> <see langword="true" /> if this entity type is owned by another entity type. </returns>
        [DebuggerStepThrough]
        public static bool IsOwned([NotNull] this IReadOnlyEntityType entityType)
            => entityType.GetForeignKeys().Any(fk => fk.IsOwnership);

        /// <summary>
        ///     Gets a value indicating whether given entity type is in ownership path for this entity type.
        /// </summary>
        /// <param name="entityType"> The entity type. </param>
        /// <param name="targetType"> Entity type to search for in ownership path. </param>
        /// <returns>
        ///     <see langword="true" /> if <paramref name="targetType" /> is in ownership path of <paramref name="entityType" />,
        ///     otherwise <see langword="false" />.
        /// </returns>
        public static bool IsInOwnershipPath([NotNull] this IReadOnlyEntityType entityType, [NotNull] IReadOnlyEntityType targetType)
        {
            var owner = entityType;
            while (true)
            {
                var ownOwnership = owner.FindOwnership();
                if (ownOwnership == null)
                {
                    return false;
                }

                owner = ownOwnership.PrincipalEntityType;
                if (owner.IsAssignableFrom(targetType))
                {
                    return true;
                }
            }
        }

        /// <summary>
        ///     Gets the primary or alternate key that is defined on the given property. Returns <see langword="null" /> if no key is defined
        ///     for the given property.
        /// </summary>
        /// <param name="entityType"> The entity type. </param>
        /// <param name="property"> The property that the key is defined on. </param>
        /// <returns> The key, or null if none is defined. </returns>
        public static IReadOnlyKey? FindKey([NotNull] this IReadOnlyEntityType entityType, [NotNull] IReadOnlyProperty property)
            => entityType.FindKey(new[] { property });

        /// <summary>
        ///     Gets the foreign keys defined on the given property. Only foreign keys that are defined on exactly the specified
        ///     property are returned. Composite foreign keys that include the specified property are not returned.
        /// </summary>
        /// <param name="entityType"> The entity type. </param>
        /// <param name="property"> The property to find the foreign keys on. </param>
        /// <returns> The foreign keys. </returns>
        public static IEnumerable<IReadOnlyForeignKey> FindForeignKeys([NotNull] this IReadOnlyEntityType entityType, [NotNull] IReadOnlyProperty property)
            => entityType.FindForeignKeys(new[] { property });

        /// <summary>
        ///     Gets the foreign keys defined on the given properties. Only foreign keys that are defined on exactly the specified
        ///     set of properties are returned.
        /// </summary>
        /// <param name="entityType"> The entity type. </param>
        /// <param name="properties"> The properties to find the foreign keys on. </param>
        /// <returns> The foreign keys. </returns>
        public static IEnumerable<IReadOnlyForeignKey> FindForeignKeys(
            [NotNull] this IReadOnlyEntityType entityType,
            [NotNull] IReadOnlyList<IReadOnlyProperty> properties)
            => ((EntityType)entityType).FindForeignKeys(properties);

        /// <summary>
        ///     Gets the foreign key for the given properties that points to a given primary or alternate key. Returns <see langword="null" />
        ///     if no foreign key is found.
        /// </summary>
        /// <param name="entityType"> The entity type. </param>
        /// <param name="property"> The property that the foreign key is defined on. </param>
        /// <param name="principalKey"> The primary or alternate key that is referenced. </param>
        /// <param name="principalEntityType">
        ///     The entity type that the relationship targets. This may be different from the type that <paramref name="principalKey" />
        ///     is defined on when the relationship targets a derived type in an inheritance hierarchy (since the key is defined on the
        ///     base type of the hierarchy).
        /// </param>
        /// <returns> The foreign key, or <see langword="null" /> if none is defined. </returns>
        public static IReadOnlyForeignKey? FindForeignKey(
            [NotNull] this IReadOnlyEntityType entityType,
            [NotNull] IReadOnlyProperty property,
            [NotNull] IReadOnlyKey principalKey,
            [NotNull] IReadOnlyEntityType principalEntityType)
            => Check.NotNull(entityType, nameof(entityType))
                .FindForeignKey(
                    new[] { property }, principalKey, principalEntityType);

        /// <summary>
        ///     Gets all foreign keys that target a given entity type (i.e. foreign keys where the given entity type
        ///     is the principal).
        /// </summary>
        /// <param name="entityType"> The entity type. </param>
        /// <returns> The foreign keys that reference the given entity type. </returns>
        public static IEnumerable<IReadOnlyForeignKey> GetReferencingForeignKeys([NotNull] this IReadOnlyEntityType entityType)
            => Check.NotNull(entityType, nameof(entityType)).AsEntityType().GetReferencingForeignKeys();

        /// <summary>
        ///     Gets all foreign keys that target a given entity type (i.e. foreign keys where the given entity type
        ///     is the principal).
        /// </summary>
        /// <param name="entityType"> The entity type. </param>
        /// <returns> The foreign keys that reference the given entity type. </returns>
        public static IEnumerable<IReadOnlyForeignKey> GetDeclaredReferencingForeignKeys([NotNull] this IReadOnlyEntityType entityType)
            => Check.NotNull(entityType, nameof(entityType)).AsEntityType().GetDeclaredReferencingForeignKeys();

        /// <summary>
        ///     Returns the relationship to the owner if this is an owned type or <see langword="null" /> otherwise.
        /// </summary>
        /// <param name="entityType"> The entity type. </param>
        /// <returns> The relationship to the owner if this is an owned type or <see langword="null" /> otherwise. </returns>
        public static IReadOnlyForeignKey? FindOwnership([NotNull] this IReadOnlyEntityType entityType)
        {
            foreach (var foreignKey in entityType.GetForeignKeys())
            {
                if (foreignKey.IsOwnership)
                {
                    return foreignKey;
                }
            }

            return null;
        }

        /// <summary>
        ///     Gets a navigation property on the given entity type. Returns <see langword="null" /> if no navigation property is found.
        /// </summary>
        /// <param name="entityType"> The entity type. </param>
        /// <param name="memberInfo"> The navigation property on the entity class. </param>
        /// <returns> The navigation property, or <see langword="null" /> if none is found. </returns>
        public static IReadOnlyNavigation? FindNavigation([NotNull] this IReadOnlyEntityType entityType, [NotNull] MemberInfo memberInfo)
            => entityType.FindNavigation(Check.NotNull(memberInfo, nameof(memberInfo)).GetSimpleMemberName());

        /// <summary>
        ///     Gets a navigation property on the given entity type. Returns <see langword="null" /> if no navigation property is found.
        /// </summary>
        /// <param name="entityType"> The entity type. </param>
        /// <param name="name"> The name of the navigation property on the entity class. </param>
        /// <returns> The navigation property, or <see langword="null" /> if none is found. </returns>
        public static IReadOnlyNavigation? FindNavigation([NotNull] this IReadOnlyEntityType entityType, [NotNull] string name)
            => entityType.FindDeclaredNavigation(Check.NotEmpty(name, nameof(name))) ?? entityType.BaseType?.FindNavigation(name);

        /// <summary>
        ///     Gets a navigation property on the given entity type. Does not return navigation properties defined on a base type.
        ///     Returns <see langword="null" /> if no navigation property is found.
        /// </summary>
        /// <param name="entityType"> The entity type. </param>
        /// <param name="name"> The name of the navigation property on the entity class. </param>
        /// <returns> The navigation property, or <see langword="null" /> if none is found. </returns>
        public static IReadOnlyNavigation? FindDeclaredNavigation([NotNull] this IReadOnlyEntityType entityType, [NotNull] string name)
            => ((EntityType)entityType).FindDeclaredNavigation(Check.NotNull(name, nameof(name)));

        /// <summary>
        ///     Returns the defining navigation if one exists or <see langword="null" /> otherwise.
        /// </summary>
        /// <param name="entityType"> The entity type. </param>
        /// <returns> The defining navigation if one exists or <see langword="null" /> otherwise. </returns>
        [Obsolete("Entity types with defining navigations have been replaced by shared-type entity types")]
        public static IReadOnlyNavigation? FindDefiningNavigation([NotNull] this IReadOnlyEntityType entityType)
        {
            if (!entityType.HasDefiningNavigation())
            {
                return null;
            }

            var definingNavigation = entityType.DefiningEntityType!.FindNavigation(entityType.DefiningNavigationName!);
            return definingNavigation?.TargetEntityType == entityType ? definingNavigation : null;
        }

        /// <summary>
        ///     Gets all navigation properties on the given entity type.
        /// </summary>
        /// <param name="entityType"> The entity type. </param>
        /// <returns> All navigation properties on the given entity type. </returns>
        public static IEnumerable<IReadOnlyNavigation> GetNavigations([NotNull] this IReadOnlyEntityType entityType)
            => Check.NotNull(entityType, nameof(entityType)).AsEntityType().GetNavigations();

        /// <summary>
        ///     <para>
        ///         Gets a property with the given member info. Returns <see langword="null" /> if no property is found.
        ///     </para>
        ///     <para>
        ///         This API only finds scalar properties and does not find navigation properties. Use
        ///         <see cref="FindNavigation(IReadOnlyEntityType, MemberInfo)" /> to find a navigation property.
        ///     </para>
        /// </summary>
        /// <param name="entityType"> The entity type. </param>
        /// <param name="memberInfo"> The member on the entity class. </param>
        /// <returns> The property, or <see langword="null" /> if none is found. </returns>
        public static IReadOnlyProperty? FindProperty([NotNull] this IReadOnlyEntityType entityType, [NotNull] MemberInfo memberInfo)
            => (Check.NotNull(memberInfo, nameof(memberInfo)) as PropertyInfo)?.IsIndexerProperty() == true
                ? null
                : entityType.FindProperty(memberInfo.GetSimpleMemberName());

        /// <summary>
        ///     <para>
        ///         Gets a property with the given name.
        ///     </para>
        ///     <para>
        ///         This API only finds scalar properties and does not find navigation properties. Use
        ///         <see cref="FindNavigation(IReadOnlyEntityType, string)" /> to find a navigation property.
        ///     </para>
        /// </summary>
        /// <param name="entityType"> The entity type. </param>
        /// <param name="name"> The property name. </param>
        /// <returns> The property, or <see langword="null" /> if none is found. </returns>
        public static IReadOnlyProperty GetProperty([NotNull] this IReadOnlyEntityType entityType, [NotNull] string name)
        {
            Check.NotEmpty(name, nameof(name));

            var property = entityType.FindProperty(name);
            if (property == null)
            {
                if (entityType.FindNavigation(name) != null
                    || entityType.FindSkipNavigation(name) != null)
                {
                    throw new InvalidOperationException(
                        CoreStrings.PropertyIsNavigation(
                            name, entityType.DisplayName(),
                            nameof(EntityEntry.Property), nameof(EntityEntry.Reference), nameof(EntityEntry.Collection)));
                }

                throw new InvalidOperationException(CoreStrings.PropertyNotFound(name, entityType.DisplayName()));
            }

            return property;
        }

        /// <summary>
        ///     <para>
        ///         Finds matching properties on the given entity type. Returns <see langword="null" /> if any property is not found.
        ///     </para>
        ///     <para>
        ///         This API only finds scalar properties and does not find navigation properties.
        ///     </para>
        /// </summary>
        /// <param name="entityType"> The entity type. </param>
        /// <param name="propertyNames"> The property names. </param>
        /// <returns> The properties, or <see langword="null" /> if any property is not found. </returns>
        public static IReadOnlyList<IReadOnlyProperty>? FindProperties(
            [NotNull] this IReadOnlyEntityType entityType,
            [NotNull] IReadOnlyList<string> propertyNames)
            => ((EntityType)entityType).FindProperties(propertyNames);

        /// <summary>
        ///     Finds a property declared on the type with the given name.
        ///     Does not return properties defined on a base type.
        /// </summary>
        /// <param name="entityType"> The entity type. </param>
        /// <param name="name"> The property name. </param>
        /// <returns> The property, or <see langword="null" /> if none is found. </returns>
        public static IReadOnlyProperty? FindDeclaredProperty([NotNull] this IReadOnlyEntityType entityType, [NotNull] string name)
            => entityType.AsEntityType().FindDeclaredProperty(name);

        /// <summary>
        ///     Gets the change tracking strategy being used for this entity type. This strategy indicates how the
        ///     context detects changes to properties for an instance of the entity type.
        /// </summary>
        /// <param name="entityType"> The entity type. </param>
        /// <returns> The change tracking strategy. </returns>
        [DebuggerStepThrough]
        public static ChangeTrackingStrategy GetChangeTrackingStrategy([NotNull] this IReadOnlyEntityType entityType)
            => ((EntityType)entityType).GetChangeTrackingStrategy();

        /// <summary>
        ///     Gets the data stored in the model for the given entity type.
        /// </summary>
        /// <param name="entityType"> The entity type. </param>
        /// <param name="providerValues"> If true, then provider values are used. </param>
        /// <returns> The data. </returns>
        public static IEnumerable<IDictionary<string, object?>> GetSeedData(
            [NotNull] this IReadOnlyEntityType entityType,
            bool providerValues = false)
            => entityType.AsEntityType().GetSeedData(providerValues);

        /// <summary>
        ///     Gets the LINQ expression filter automatically applied to queries for this entity type.
        /// </summary>
        /// <param name="entityType"> The entity type to get the query filter for. </param>
        /// <returns> The LINQ expression filter. </returns>
        public static LambdaExpression? GetQueryFilter([NotNull] this IReadOnlyEntityType entityType)
        {
            Check.NotNull(entityType, nameof(entityType));

            return (LambdaExpression?)entityType[CoreAnnotationNames.QueryFilter];
        }

        /// <summary>
        ///     Gets the LINQ query used as the default source for queries of this type.
        /// </summary>
        /// <param name="entityType"> The entity type to get the defining query for. </param>
        /// <returns> The LINQ query used as the default source. </returns>
        [Obsolete("Use InMemoryEntityTypeExtensions.GetInMemoryQuery")]
        public static LambdaExpression? GetDefiningQuery([NotNull] this IReadOnlyEntityType entityType)
        {
            Check.NotNull(entityType, nameof(entityType));

            return (LambdaExpression?)entityType[CoreAnnotationNames.DefiningQuery];
        }

        /// <summary>
        ///     Returns the <see cref="IReadOnlyProperty" /> that will be used for storing a discriminator value.
        /// </summary>
        /// <param name="entityType"> The entity type. </param>
        public static IReadOnlyProperty? GetDiscriminatorProperty([NotNull] this IReadOnlyEntityType entityType)
        {
            if (entityType.BaseType != null)
            {
                return entityType.GetRootType().GetDiscriminatorProperty();
            }

            var propertyName = (string?)entityType[CoreAnnotationNames.DiscriminatorProperty];

            return propertyName == null ? null : entityType.FindProperty(propertyName);
        }

        /// <summary>
        ///     Returns the value indicating whether the discriminator mapping is complete for this entity type.
        /// </summary>
        /// <param name="entityType"> The entity type. </param>
        public static bool GetIsDiscriminatorMappingComplete([NotNull] this IReadOnlyEntityType entityType)
            => (bool?)entityType[CoreAnnotationNames.DiscriminatorMappingComplete]
                ?? GetDefaultIsDiscriminatorMappingComplete(entityType);

        private static bool GetDefaultIsDiscriminatorMappingComplete(IReadOnlyEntityType entityType)
            => true;

        /// <summary>
        ///     Returns the discriminator value for this entity type.
        /// </summary>
        /// <param name="entityType"> The entity type. </param>
        /// <returns> The discriminator value for this entity type. </returns>
        public static object? GetDiscriminatorValue([NotNull] this IReadOnlyEntityType entityType)
            => entityType[CoreAnnotationNames.DiscriminatorValue];

        /// <summary>
        ///     <para>
        ///         Creates a human-readable representation of the given metadata.
        ///     </para>
        ///     <para>
        ///         Warning: Do not rely on the format of the returned string.
        ///         It is designed for debugging only and may change arbitrarily between releases.
        ///     </para>
        /// </summary>
        /// <param name="entityType"> The entity type. </param>
        /// <param name="options"> Options for generating the string. </param>
        /// <param name="indent"> The number of indent spaces to use before each new line. </param>
        /// <returns> A human-readable representation. </returns>
        public static string ToDebugString(
            [NotNull] this IReadOnlyEntityType entityType,
            MetadataDebugStringOptions options,
            int indent = 0)
        {
            var builder = new StringBuilder();
            var indentString = new string(' ', indent);

            builder
                .Append(indentString)
                .Append("EntityType: ")
                .Append(entityType.DisplayName());

            if (entityType.BaseType != null)
            {
                builder.Append(" Base: ").Append(entityType.BaseType.DisplayName());
            }

            if (entityType.HasSharedClrType)
            {
                builder.Append(" CLR Type: ").Append(entityType.ClrType.ShortDisplayName());
            }

            if (entityType.IsAbstract())
            {
                builder.Append(" Abstract");
            }

            if (entityType.FindPrimaryKey() == null)
            {
                builder.Append(" Keyless");
            }

            if (entityType.GetChangeTrackingStrategy() != ChangeTrackingStrategy.Snapshot)
            {
                builder.Append(" ChangeTrackingStrategy.").Append(entityType.GetChangeTrackingStrategy());
            }

            if ((options & MetadataDebugStringOptions.SingleLine) == 0)
            {
                var properties = entityType.GetDeclaredProperties().ToList();
                if (properties.Count != 0)
                {
                    builder.AppendLine().Append(indentString).Append("  Properties: ");
                    foreach (var property in properties)
                    {
                        builder.AppendLine().Append(property.ToDebugString(options, indent + 4));
                    }
                }

                var navigations = entityType.GetDeclaredNavigations().ToList();
                if (navigations.Count != 0)
                {
                    builder.AppendLine().Append(indentString).Append("  Navigations: ");
                    foreach (var navigation in navigations)
                    {
                        builder.AppendLine().Append(navigation.ToDebugString(options, indent + 4));
                    }
                }

                var skipNavigations = entityType.GetDeclaredSkipNavigations().ToList();
                if (skipNavigations.Count != 0)
                {
                    builder.AppendLine().Append(indentString).Append("  Skip navigations: ");
                    foreach (var skipNavigation in skipNavigations)
                    {
                        builder.AppendLine().Append(skipNavigation.ToDebugString(options, indent + 4));
                    }
                }

                var serviceProperties = entityType.GetDeclaredServiceProperties().ToList();
                if (serviceProperties.Count != 0)
                {
                    builder.AppendLine().Append(indentString).Append("  Service properties: ");
                    foreach (var serviceProperty in serviceProperties)
                    {
                        builder.AppendLine().Append(serviceProperty.ToDebugString(options, indent + 4));
                    }
                }

                var keys = entityType.GetDeclaredKeys().ToList();
                if (keys.Count != 0)
                {
                    builder.AppendLine().Append(indentString).Append("  Keys: ");
                    foreach (var key in keys)
                    {
                        builder.AppendLine().Append(key.ToDebugString(options, indent + 4));
                    }
                }

                var fks = entityType.GetDeclaredForeignKeys().ToList();
                if (fks.Count != 0)
                {
                    builder.AppendLine().Append(indentString).Append("  Foreign keys: ");
                    foreach (var fk in fks)
                    {
                        builder.AppendLine().Append(fk.ToDebugString(options, indent + 4));
                    }
                }

                var indexes = entityType.GetDeclaredIndexes().ToList();
                if (indexes.Count != 0)
                {
                    builder.AppendLine().Append(indentString).Append("  Indexes: ");
                    foreach (var index in indexes)
                    {
                        builder.AppendLine().Append(index.ToDebugString(options, indent + 4));
                    }
                }

                if ((options & MetadataDebugStringOptions.IncludeAnnotations) != 0)
                {
                    builder.Append(entityType.AnnotationsToDebugString(indent: indent + 2));
                }
            }

            return builder.ToString();
        }
    }
}
