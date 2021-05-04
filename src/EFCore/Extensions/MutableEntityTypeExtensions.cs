// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     Extension methods for <see cref="IMutableEntityType" />.
    /// </summary>
    [Obsolete("Use IMutableEntityType")]
    public static class MutableEntityTypeExtensions
    {
        /// <summary>
        ///     Returns the defining navigation if one exists or <see langword="null" /> otherwise.
        /// </summary>
        /// <param name="entityType"> The entity type. </param>
        /// <returns> The defining navigation if one exists or <see langword="null" /> otherwise. </returns>
        [Obsolete("Entity types with defining navigations have been replaced by shared-type entity types")]
        public static IMutableNavigation? FindDefiningNavigation(this IMutableEntityType entityType)
            => (IMutableNavigation?)((IEntityType)entityType).FindDefiningNavigation();

        /// <summary>
        ///     Sets the LINQ query used as the default source for queries of this type.
        /// </summary>
        /// <param name="entityType"> The entity type. </param>
        /// <param name="definingQuery"> The LINQ query used as the default source. </param>
        [Obsolete("Use InMemoryEntityTypeExtensions.SetInMemoryQuery")]
        public static void SetDefiningQuery(
            this IMutableEntityType entityType,
            LambdaExpression? definingQuery)
            => ((EntityType)entityType).SetDefiningQuery(definingQuery, ConfigurationSource.Explicit);
    }
}
