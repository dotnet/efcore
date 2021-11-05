// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     Entity type extension methods for <see cref="IReadOnlyEntityType" />.
    /// </summary>
    [Obsolete("Use IReadOnlyEntityType")]
    public static class EntityTypeExtensions
    {
        /// <summary>
        ///     Gets the unique name for the given <see cref="IReadOnlyTypeBase" />.
        /// </summary>
        /// <param name="type">The entity type.</param>
        /// <returns>The full name.</returns>
        [DebuggerStepThrough]
        [Obsolete("Use Name property")]
        public static string FullName(this ITypeBase type)
            => type.Name;

        /// <summary>
        ///     Gets a value indicating whether this entity type has a defining navigation.
        /// </summary>
        /// <param name="entityType">The entity type.</param>
        /// <returns><see langword="true" /> if this entity type has a defining navigation.</returns>
        [DebuggerStepThrough]
        [Obsolete("Entity types with defining navigations have been replaced by shared-type entity types")]
        public static bool HasDefiningNavigation(this IEntityType entityType)
            => entityType.HasDefiningNavigation();

        /// <summary>
        ///     Returns the defining navigation if one exists or <see langword="null" /> otherwise.
        /// </summary>
        /// <param name="entityType">The entity type.</param>
        /// <returns>The defining navigation if one exists or <see langword="null" /> otherwise.</returns>
        [Obsolete("Entity types with defining navigations have been replaced by shared-type entity types")]
        public static INavigation? FindDefiningNavigation(this IEntityType entityType)
        {
            if (!entityType.HasDefiningNavigation())
            {
                return null;
            }

            var definingNavigation = (INavigation?)entityType.DefiningEntityType!.FindNavigation(entityType.DefiningNavigationName!);
            return definingNavigation?.TargetEntityType == entityType ? definingNavigation : null;
        }

        /// <summary>
        ///     Gets all navigation properties on the given entity type.
        /// </summary>
        /// <param name="entityType">The entity type.</param>
        /// <returns>All navigation properties on the given entity type.</returns>
        [Obsolete("Use IReadOnlyEntityType.GetNavigations")]
        public static IEnumerable<INavigation> GetNavigations(this IEntityType entityType)
            => entityType.GetNavigations();

        /// <summary>
        ///     Gets the LINQ query used as the default source for queries of this type.
        /// </summary>
        /// <param name="entityType">The entity type to get the defining query for.</param>
        /// <returns>The LINQ query used as the default source.</returns>
        [Obsolete("Use InMemoryEntityTypeExtensions.GetInMemoryQuery")]
        public static LambdaExpression? GetDefiningQuery(this IEntityType entityType)
            => (LambdaExpression?)entityType[CoreAnnotationNames.DefiningQuery];

        /// <summary>
        ///     Returns the closest entity type that is a parent of both given entity types. If one of the given entities is
        ///     a parent of the other, that parent is returned. Returns <see langword="null" /> if the two entity types aren't in the same hierarchy.
        /// </summary>
        /// <param name="entityType1">An entity type.</param>
        /// <param name="entityType2">Another entity type.</param>
        /// <returns>
        ///     The closest common parent of <paramref name="entityType1" /> and <paramref name="entityType2" />,
        ///     or null if they have not common parent.
        /// </returns>
        [Obsolete("Use IReadOnlyEntityType.FindClosestCommonParent")]
        public static IEntityType? GetClosestCommonParent(
            this IEntityType entityType1,
            IEntityType entityType2)
            => entityType1.FindClosestCommonParent(entityType2);

        /// <summary>
        ///     Returns the <see cref="IReadOnlyProperty" /> that will be used for storing a discriminator value.
        /// </summary>
        /// <param name="entityType">The entity type.</param>
        [Obsolete("Use IReadOnlyEntityType.FindDiscriminatorProperty")]
        public static IProperty? GetDiscriminatorProperty(this IEntityType entityType)
            => entityType.FindDiscriminatorProperty();
    }
}
