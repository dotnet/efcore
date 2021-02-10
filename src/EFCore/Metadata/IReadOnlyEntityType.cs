// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
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
    public interface IReadOnlyEntityType : IReadOnlyTypeBase
    {
        /// <summary>
        ///     Gets the base type of this entity type. Returns <see langword="null" /> if this is not a derived type in an inheritance hierarchy.
        /// </summary>
        IReadOnlyEntityType? BaseType { get; }

        /// <summary>
        ///     Gets the name of the defining navigation.
        /// </summary>
        [Obsolete("Entity types with defining navigations have been replaced by shared-type entity types")]
        string? DefiningNavigationName => null;

        /// <summary>
        ///     Gets the defining entity type.
        /// </summary>
        [Obsolete("Entity types with defining navigations have been replaced by shared-type entity types")]
        IReadOnlyEntityType? DefiningEntityType => null;

        /// <summary>
        ///     Gets a value indicating whether this entity type has a defining navigation.
        /// </summary>
        /// <returns> <see langword="true" /> if this entity type has a defining navigation. </returns>
        [Obsolete("Entity types with defining navigations have been replaced by shared-type entity types")]
        public bool HasDefiningNavigation() => HasSharedClrType;

        /// <summary>
        ///     Gets primary key for this entity type. Returns <see langword="null" /> if no primary key is defined.
        /// </summary>
        /// <returns> The primary key, or <see langword="null" /> if none is defined. </returns>
        IReadOnlyKey? FindPrimaryKey();

        /// <summary>
        ///     Gets the primary or alternate key that is defined on the given properties.
        ///     Returns <see langword="null" /> if no key is defined for the given properties.
        /// </summary>
        /// <param name="properties"> The properties that make up the key. </param>
        /// <returns> The key, or <see langword="null" /> if none is defined. </returns>
        IReadOnlyKey? FindKey([NotNull] IReadOnlyList<IReadOnlyProperty> properties);

        /// <summary>
        ///     Gets the primary and alternate keys for this entity type.
        /// </summary>
        /// <returns> The primary and alternate keys. </returns>
        IEnumerable<IReadOnlyKey> GetKeys();

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
        IReadOnlyForeignKey? FindForeignKey(
            [NotNull] IReadOnlyList<IReadOnlyProperty> properties,
            [NotNull] IReadOnlyKey principalKey,
            [NotNull] IReadOnlyEntityType principalEntityType);

        /// <summary>
        ///     Gets the foreign keys defined on this entity type.
        /// </summary>
        /// <returns> The foreign keys defined on this entity type. </returns>
        IEnumerable<IReadOnlyForeignKey> GetForeignKeys();

        /// <summary>
        ///     Gets a skip navigation property on this entity type. Returns <see langword="null" /> if no navigation property is found.
        /// </summary>
        /// <param name="memberInfo"> The navigation property on the entity class. </param>
        /// <returns> The navigation property, or <see langword="null" /> if none is found. </returns>
        IReadOnlySkipNavigation? FindSkipNavigation([NotNull] MemberInfo memberInfo)
            => FindSkipNavigation(Check.NotNull(memberInfo, nameof(memberInfo)).GetSimpleMemberName());

        /// <summary>
        ///     Gets a skip navigation property on this entity type. Returns <see langword="null" /> if no skip navigation property is found.
        /// </summary>
        /// <param name="name"> The name of the navigation property on the entity class. </param>
        /// <returns> The navigation property, or <see langword="null" /> if none is found. </returns>
        IReadOnlySkipNavigation? FindSkipNavigation([NotNull] string name);

        /// <summary>
        ///     <para>
        ///         Gets a skip navigation property on this entity type.
        ///     </para>
        ///     <para>
        ///         Does not return skip navigation properties defined on a base type.
        ///         Returns <see langword="null" /> if no skip navigation property is found.
        ///     </para>
        /// </summary>
        /// <param name="name"> The name of the navigation property on the entity class. </param>
        /// <returns> The navigation property, or <see langword="null" /> if none is found. </returns>
        IReadOnlySkipNavigation? FindDeclaredSkipNavigation([NotNull] string name)
        {
            var navigation = FindSkipNavigation(name);
            return navigation?.DeclaringEntityType == this ? navigation : null;
        }

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
        /// <returns> Declared skip navigations. </returns>
        IEnumerable<IReadOnlySkipNavigation> GetDeclaredSkipNavigations()
            => GetSkipNavigations().Where(n => n.DeclaringEntityType == this);

        /// <summary>
        ///     Gets the skip navigation properties on this entity type.
        /// </summary>
        /// <returns> All skip navigation properties on this entity type. </returns>
        IEnumerable<IReadOnlySkipNavigation> GetSkipNavigations();

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
        IReadOnlyIndex? FindIndex([NotNull] IReadOnlyList<IReadOnlyProperty> properties);

        /// <summary>
        ///     Gets the index with the given name. Returns <see langword="null" /> if no such index exists.
        /// </summary>
        /// <param name="name"> The name of the index to find. </param>
        /// <returns> The index, or <see langword="null" /> if none is found. </returns>
        IReadOnlyIndex? FindIndex([NotNull] string name);

        /// <summary>
        ///     Gets the indexes defined on this entity type.
        /// </summary>
        /// <returns> The indexes defined on this entity type. </returns>
        IEnumerable<IReadOnlyIndex> GetIndexes();

        /// <summary>
        ///     <para>
        ///         Gets the property with a given name. Returns <see langword="null" /> if no property with the given name is defined.
        ///     </para>
        ///     <para>
        ///         This API only finds scalar properties and does not find navigation properties. Use
        ///         <see cref="EntityTypeExtensions.FindNavigation(IReadOnlyEntityType, string)" /> to find a navigation property.
        ///     </para>
        /// </summary>
        /// <param name="name"> The name of the property. </param>
        /// <returns> The property, or <see langword="null" /> if none is found. </returns>
        IReadOnlyProperty? FindProperty([NotNull] string name);

        /// <summary>
        ///     <para>
        ///         Gets the properties defined on this entity type.
        ///     </para>
        ///     <para>
        ///         This API only returns scalar properties and does not return navigation properties. Use
        ///         <see cref="EntityTypeExtensions.GetNavigations(IReadOnlyEntityType)" /> to get navigation properties.
        ///     </para>
        /// </summary>
        /// <returns> The properties defined on this entity type. </returns>
        IEnumerable<IReadOnlyProperty> GetProperties();

        /// <summary>
        ///     <para>
        ///         Gets the <see cref="IReadOnlyServiceProperty" /> with a given name.
        ///         Returns <see langword="null" /> if no property with the given name is defined.
        ///     </para>
        ///     <para>
        ///         This API only finds service properties and does not find scalar or navigation properties.
        ///     </para>
        /// </summary>
        /// <param name="name"> The name of the property. </param>
        /// <returns> The service property, or <see langword="null" /> if none is found. </returns>
        IReadOnlyServiceProperty? FindServiceProperty([NotNull] string name);

        /// <summary>
        ///     <para>
        ///         Gets all the <see cref="IReadOnlyServiceProperty" /> defined on this entity type.
        ///     </para>
        ///     <para>
        ///         This API only returns service properties and does not return scalar or navigation properties.
        ///     </para>
        /// </summary>
        /// <returns> The service properties defined on this entity type. </returns>
        IEnumerable<IReadOnlyServiceProperty> GetServiceProperties();
    }
}
