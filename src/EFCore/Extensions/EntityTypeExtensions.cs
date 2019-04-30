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
using Microsoft.EntityFrameworkCore.Infrastructure;
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
        ///     Checks if this entity type represents an abstract type.
        /// </summary>
        /// <param name="type"> The entity type. </param>
        /// <returns> True if the type is abstract, false otherwise. </returns>
        [DebuggerStepThrough]
        public static bool IsAbstract([NotNull] this ITypeBase type)
            => type.ClrType?.GetTypeInfo().IsAbstract ?? false;

        /// <summary>
        ///     Returns all derived types of the given <see cref="IEntityType" />, including the type itself.
        /// </summary>
        /// <param name="entityType"> The entity type. </param>
        /// <returns> Derived types. </returns>
        public static IEnumerable<IEntityType> GetDerivedTypesInclusive([NotNull] this IEntityType entityType)
            => new[] { entityType }.Concat(entityType.GetDerivedTypes());

        /// <summary>
        ///     Returns all base types of the given <see cref="IEntityType" />, including the type itself.
        /// </summary>
        /// <param name="entityType"> The entity type. </param>
        /// <returns> Base types. </returns>
        public static IEnumerable<IEntityType> GetAllBaseTypesInclusive([NotNull] this IEntityType entityType)
            => new List<IEntityType>(entityType.GetAllBaseTypes())
            {
                entityType
            };

        /// <summary>
        ///     <para>
        ///         Gets all foreign keys declared on the given <see cref="IEntityType" />.
        ///     </para>
        ///     <para>
        ///         This method does not return foreign keys declared on derived types.
        ///         It is useful when iterating over all entity types to avoid processing the same foreign key more than once.
        ///         Use <see cref="IEntityType.GetForeignKeys" /> to also return foreign keys declared on derived types.
        ///     </para>
        /// </summary>
        /// <param name="entityType"> The entity type. </param>
        /// <returns> Declared foreign keys. </returns>
        public static IEnumerable<IForeignKey> GetDeclaredForeignKeys([NotNull] this IEntityType entityType)
            => entityType.AsEntityType().GetDeclaredForeignKeys();

        /// <summary>
        ///     <para>
        ///         Gets all navigation properties declared on the given <see cref="IEntityType" />.
        ///     </para>
        ///     <para>
        ///         This method does not return navigation properties declared on derived types.
        ///         It is useful when iterating over all entity types to avoid processing the same navigation property more than once.
        ///         Use <see cref="GetNavigations" /> to also return navigation properties declared on derived types.
        ///     </para>
        /// </summary>
        /// <param name="entityType"> The entity type. </param>
        /// <returns> Declared navigation properties. </returns>
        public static IEnumerable<INavigation> GetDeclaredNavigations([NotNull] this IEntityType entityType)
            => entityType.GetDeclaredForeignKeys()
                .Concat(entityType.GetDeclaredReferencingForeignKeys())
                .SelectMany(foreignKey => foreignKey.FindNavigationsFrom(entityType))
                .Distinct()
                .OrderBy(m => m.Name);

        /// <summary>
        ///     <para>
        ///         Gets all non-navigation properties declared on the given <see cref="IEntityType" />.
        ///     </para>
        ///     <para>
        ///         This method does not return properties declared on derived types.
        ///         It is useful when iterating over all entity types to avoid processing the same property more than once.
        ///         Use <see cref="IEntityType.GetProperties" /> to also return properties declared on derived types.
        ///     </para>
        /// </summary>
        /// <param name="entityType"> The entity type. </param>
        /// <returns> Declared non-navigation properties. </returns>
        public static IEnumerable<IProperty> GetDeclaredProperties([NotNull] this IEntityType entityType)
            => entityType.AsEntityType().GetDeclaredProperties();

        /// <summary>
        ///     <para>
        ///         Gets all service properties declared on the given <see cref="IEntityType" />.
        ///     </para>
        ///     <para>
        ///         This method does not return properties declared on derived types.
        ///         It is useful when iterating over all entity types to avoid processing the same property more than once.
        ///         Use <see cref="IEntityType.GetServiceProperties" /> to also return properties declared on derived types.
        ///     </para>
        /// </summary>
        /// <param name="entityType"> The entity type. </param>
        /// <returns> Declared service properties. </returns>
        public static IEnumerable<IServiceProperty> GetDeclaredServiceProperties([NotNull] this IEntityType entityType)
            => entityType.AsEntityType().GetDeclaredServiceProperties();

        /// <summary>
        ///     <para>
        ///         Gets all indexes declared on the given <see cref="IEntityType" />.
        ///     </para>
        ///     <para>
        ///         This method does not return indexes declared on derived types.
        ///         It is useful when iterating over all entity types to avoid processing the same index more than once.
        ///         Use <see cref="IEntityType.GetForeignKeys" /> to also return indexes declared on derived types.
        ///     </para>
        /// </summary>
        /// <param name="entityType"> The entity type. </param>
        /// <returns> Declared indexes. </returns>
        public static IEnumerable<IIndex> GetDeclaredIndexes([NotNull] this IEntityType entityType)
            => entityType.GetIndexes().Where(p => p.DeclaringEntityType == entityType);

        private static string DisplayNameDefault(this ITypeBase type)
            => type.ClrType != null
                ? type.ClrType.ShortDisplayName()
                : type.Name;

        /// <summary>
        ///     Gets the friendly display name for the given <see cref="ITypeBase" />.
        /// </summary>
        /// <param name="type"> The entity type. </param>
        /// <returns> The display name. </returns>
        [DebuggerStepThrough]
        public static string DisplayName([NotNull] this ITypeBase type)
        {
            if (!(type is IEntityType entityType)
                || !entityType.HasDefiningNavigation())
            {
                return type.DisplayNameDefault();
            }

            var builder = new StringBuilder();
            var path = new Stack<string>();
            var root = entityType;
            while (true)
            {
                var definingNavigationName = root.DefiningNavigationName;
                if (definingNavigationName == null)
                {
                    break;
                }

                root = root.DefiningEntityType;
                path.Push("#");
                path.Push(definingNavigationName);
                path.Push(".");
                path.Push(root.DisplayNameDefault());
            }

            if (root != entityType)
            {
                builder.AppendJoin(path, "");
            }

            builder.Append(type.DisplayNameDefault());
            return builder.ToString();
        }

        /// <summary>
        ///     Gets a short name for the given <see cref="ITypeBase" /> that can be used in other identifiers.
        /// </summary>
        /// <param name="type"> The entity type. </param>
        /// <returns> The short name. </returns>
        [DebuggerStepThrough]
        public static string ShortName([NotNull] this ITypeBase type)
        {
            if (type.ClrType != null)
            {
                return type.ClrType.ShortDisplayName();
            }

            var plusIndex = type.Name.LastIndexOf("+", StringComparison.Ordinal);
            var dotIndex = type.Name.LastIndexOf(".", StringComparison.Ordinal);
            return plusIndex == -1
                ? dotIndex == -1
                    ? type.Name
                    : type.Name.Substring(dotIndex + 1, type.Name.Length - dotIndex - 1)
                : type.Name.Substring(plusIndex + 1, type.Name.Length - plusIndex - 1);
        }

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
        ///     <c>true</c> if <paramref name="derivedType" /> derives from (or is the same as) <paramref name="entityType" />,
        ///     otherwise <c>false</c>.
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
        ///     Determines if an entity type derives from (but is not the same as) a given entity type.
        /// </summary>
        /// <param name="entityType"> The derived entity type. </param>
        /// <param name="baseType"> The entity type to check if it is a base type of <paramref name="entityType" />. </param>
        /// <returns>
        ///     <c>true</c> if <paramref name="entityType" /> derives from (but is not the same as) <paramref name="baseType" />,
        ///     otherwise <c>false</c>.
        /// </returns>
        public static bool IsStrictlyDerivedFrom([NotNull] this IEntityType entityType, [NotNull] IEntityType baseType)
        {
            Check.NotNull(entityType, nameof(entityType));
            Check.NotNull(baseType, nameof(baseType));

            if (entityType == baseType)
            {
                return false;
            }

            return baseType.IsAssignableFrom(entityType);
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
        ///     Gets the primary or alternate key that is defined on the given property. Returns <c>null</c> if no key is defined
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
        ///     Returns the relationship to the owner if this is an owned type or <c>null</c> otherwise.
        /// </summary>
        /// <param name="entityType"> The entity type to find the foreign keys on. </param>
        /// <returns> The relationship to the owner if this is an owned type or <c>null</c> otherwise. </returns>
        public static IForeignKey FindOwnership([NotNull] this IEntityType entityType)
            => ((EntityType)entityType).FindOwnership();

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
        public static ChangeTrackingStrategy GetChangeTrackingStrategy([NotNull] this IEntityType entityType)
        {
            Check.NotNull(entityType, nameof(entityType));

            return (ChangeTrackingStrategy?)entityType[CoreAnnotationNames.ChangeTrackingStrategy]
                   ?? entityType.Model.GetChangeTrackingStrategy();
        }

        /// <summary>
        ///     Gets the data stored in the model for the given entity type.
        /// </summary>
        /// <param name="entityType"> The entity type. </param>
        /// <param name="providerValues"> If true, then provider values are used. </param>
        /// <returns> The data. </returns>
        public static IEnumerable<IDictionary<string, object>> GetSeedData(
            [NotNull] this IEntityType entityType, bool providerValues = false)
            => entityType.AsEntityType().GetSeedData(providerValues);

        /// <summary>
        ///     Gets the LINQ expression filter automatically applied to queries for this entity type.
        /// </summary>
        /// <param name="entityType"> The entity type to get the query filter for. </param>
        /// <returns> The LINQ expression filter. </returns>
        public static LambdaExpression GetQueryFilter([NotNull] this IEntityType entityType)
        {
            Check.NotNull(entityType, nameof(entityType));

            return (LambdaExpression)entityType[CoreAnnotationNames.QueryFilter];
        }

        /// <summary>
        ///     Gets the LINQ query used as the default source for queries of this type.
        /// </summary>
        /// <param name="entityType"> The entity type to get the defining query for. </param>
        /// <returns> The LINQ query used as the default source. </returns>
        public static LambdaExpression GetDefiningQuery([NotNull] this IEntityType entityType)
        {
            Check.NotNull(entityType, nameof(entityType));

            return (LambdaExpression)entityType[CoreAnnotationNames.DefiningQuery];
        }
    }
}
