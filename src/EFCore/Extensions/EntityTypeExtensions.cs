// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Linq.Expressions;
using JetBrains.Annotations;
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
    [Obsolete("Use IReadOnlyEntityType")]
    public static class EntityTypeExtensions
    {
        /// <summary>
        ///     Gets the unique name for the given <see cref="IReadOnlyTypeBase" />.
        /// </summary>
        /// <param name="type"> The entity type. </param>
        /// <returns> The full name. </returns>
        [DebuggerStepThrough]
        [Obsolete("Use Name property")]
        public static string FullName([NotNull] this IReadOnlyTypeBase type) => type.Name;

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
    }
}
