// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     Extension methods for <see cref="IEntityType" /> for the in-memory provider.
    /// </summary>
    public static class InMemoryEntityTypeExtensions
    {
        /// <summary>
        ///     Gets the LINQ query used as the default source for queries of this type.
        /// </summary>
        /// <param name="entityType"> The entity type to get the in-memory query for. </param>
        /// <returns> The LINQ query used as the default source. </returns>
        public static LambdaExpression GetInMemoryQuery([NotNull] this IEntityType entityType)
#pragma warning disable EF1001 // Internal EF Core API usage.
#pragma warning disable CS0612 // Type or member is obsolete
            => (LambdaExpression)Check.NotNull(entityType, nameof(entityType))[CoreAnnotationNames.DefiningQuery];
#pragma warning restore CS0612 // Type or member is obsolete
#pragma warning restore EF1001 // Internal EF Core API usage.

        /// <summary>
        ///     Sets the LINQ query used as the default source for queries of this type.
        /// </summary>
        /// <param name="entityType"> The entity type. </param>
        /// <param name="inMemoryQuery"> The LINQ query used as the default source. </param>
        public static void SetInMemoryQuery(
            [NotNull] this IMutableEntityType entityType,
            [CanBeNull] LambdaExpression inMemoryQuery)
            => Check.NotNull(entityType, nameof(entityType))
#pragma warning disable EF1001 // Internal EF Core API usage.
#pragma warning disable CS0612 // Type or member is obsolete
                .SetOrRemoveAnnotation(CoreAnnotationNames.DefiningQuery, inMemoryQuery);
#pragma warning restore CS0612 // Type or member is obsolete
#pragma warning restore EF1001 // Internal EF Core API usage.

        /// <summary>
        ///     Sets the LINQ query used as the default source for queries of this type.
        /// </summary>
        /// <param name="entityType"> The entity type. </param>
        /// <param name="inMemoryQuery"> The LINQ query used as the default source. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> The configured entity type. </returns>
        public static LambdaExpression SetInMemoryQuery(
            [NotNull] this IConventionEntityType entityType,
            [CanBeNull] LambdaExpression inMemoryQuery,
            bool fromDataAnnotation = false)
            => (LambdaExpression)Check.NotNull(entityType, nameof(entityType))
#pragma warning disable EF1001 // Internal EF Core API usage.
#pragma warning disable CS0612 // Type or member is obsolete
                .SetOrRemoveAnnotation(CoreAnnotationNames.DefiningQuery, inMemoryQuery, fromDataAnnotation)
#pragma warning restore CS0612 // Type or member is obsolete
#pragma warning restore EF1001 // Internal EF Core API usage.
                ?.Value;

        /// <summary>
        ///     Returns the configuration source for <see cref="GetInMemoryQuery" />.
        /// </summary>
        /// <param name="entityType"> The entity type. </param>
        /// <returns> The configuration source for <see cref="GetInMemoryQuery" />. </returns>
        public static ConfigurationSource? GetDefiningQueryConfigurationSource([NotNull] this IConventionEntityType entityType)
#pragma warning disable EF1001 // Internal EF Core API usage.
#pragma warning disable CS0612 // Type or member is obsolete
            => entityType.FindAnnotation(CoreAnnotationNames.DefiningQuery)?.GetConfigurationSource();
#pragma warning restore CS0612 // Type or member is obsolete
#pragma warning restore EF1001 // Internal EF Core API usage.
    }
}
