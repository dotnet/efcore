// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     Extension methods for <see cref="IConventionEntityType" />.
    /// </summary>
    [Obsolete("Use IConventionEntityType")]
    public static class ConventionEntityTypeExtensions
    {
        /// <summary>
        ///     Returns the defining navigation if one exists or <see langword="null" /> otherwise.
        /// </summary>
        /// <param name="entityType">The entity type.</param>
        /// <returns>The defining navigation if one exists or <see langword="null" /> otherwise.</returns>
        [Obsolete("Entity types with defining navigations have been replaced by shared-type entity types")]
        public static IConventionNavigation? FindDefiningNavigation(this IConventionEntityType entityType)
            => (IConventionNavigation?)((IEntityType)entityType).FindDefiningNavigation();

        /// <summary>
        ///     Sets the LINQ query used as the default source for queries of this type.
        /// </summary>
        /// <param name="entityType">The entity type.</param>
        /// <param name="definingQuery">The LINQ query used as the default source.</param>
        /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
        [Obsolete("Use InMemoryEntityTypeExtensions.SetInMemoryQuery")]
        public static void SetDefiningQuery(
            this IConventionEntityType entityType,
            LambdaExpression? definingQuery,
            bool fromDataAnnotation = false)
            => ((EntityType)entityType).SetDefiningQuery(
                definingQuery,
                fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     Returns the configuration source for <see cref="EntityTypeExtensions.GetDefiningQuery" />.
        /// </summary>
        /// <param name="entityType">The entity type.</param>
        /// <returns>The configuration source for <see cref="EntityTypeExtensions.GetDefiningQuery" />.</returns>
        [Obsolete("Use InMemoryEntityTypeExtensions.GetInMemoryQueryConfigurationSource")]
        public static ConfigurationSource? GetDefiningQueryConfigurationSource(this IConventionEntityType entityType)
            => entityType.FindAnnotation(CoreAnnotationNames.DefiningQuery)?.GetConfigurationSource();
    }
}
