// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     Extension methods for <see cref="IReadOnlyEntityType" /> for the in-memory provider.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
///     <see href="https://aka.ms/efcore-docs-in-memory">The EF Core in-memory database provider</see> for more information and examples.
/// </remarks>
public static class InMemoryEntityTypeExtensions
{
    /// <summary>
    ///     Gets the LINQ query used as the default source for queries of this type.
    /// </summary>
    /// <param name="entityType">The entity type to get the in-memory query for.</param>
    /// <returns>The LINQ query used as the default source.</returns>
    public static LambdaExpression? GetInMemoryQuery(this IReadOnlyEntityType entityType)
#pragma warning disable EF1001 // Internal EF Core API usage.
#pragma warning disable CS0612 // Type or member is obsolete
        => (LambdaExpression?)entityType[CoreAnnotationNames.DefiningQuery];
#pragma warning restore CS0612 // Type or member is obsolete
#pragma warning restore EF1001 // Internal EF Core API usage.

    /// <summary>
    ///     Sets the LINQ query used as the default source for queries of this type.
    /// </summary>
    /// <param name="entityType">The entity type.</param>
    /// <param name="inMemoryQuery">The LINQ query used as the default source.</param>
    public static void SetInMemoryQuery(
        this IMutableEntityType entityType,
        LambdaExpression? inMemoryQuery)
        => entityType
#pragma warning disable EF1001 // Internal EF Core API usage.
#pragma warning disable CS0612 // Type or member is obsolete
            .SetOrRemoveAnnotation(CoreAnnotationNames.DefiningQuery, inMemoryQuery);
#pragma warning restore CS0612 // Type or member is obsolete
#pragma warning restore EF1001 // Internal EF Core API usage.

    /// <summary>
    ///     Sets the LINQ query used as the default source for queries of this type.
    /// </summary>
    /// <param name="entityType">The entity type.</param>
    /// <param name="inMemoryQuery">The LINQ query used as the default source.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The configured entity type.</returns>
    public static LambdaExpression? SetInMemoryQuery(
        this IConventionEntityType entityType,
        LambdaExpression? inMemoryQuery,
        bool fromDataAnnotation = false)
        => (LambdaExpression?)entityType
#pragma warning disable EF1001 // Internal EF Core API usage.
#pragma warning disable CS0612 // Type or member is obsolete
            .SetOrRemoveAnnotation(CoreAnnotationNames.DefiningQuery, inMemoryQuery, fromDataAnnotation)
#pragma warning restore CS0612 // Type or member is obsolete
#pragma warning restore EF1001 // Internal EF Core API usage.
            ?.Value;

    /// <summary>
    ///     Returns the configuration source for <see cref="GetInMemoryQuery" />.
    /// </summary>
    /// <param name="entityType">The entity type.</param>
    /// <returns>The configuration source for <see cref="GetInMemoryQuery" />.</returns>
    public static ConfigurationSource? GetDefiningQueryConfigurationSource(this IConventionEntityType entityType)
#pragma warning disable EF1001 // Internal EF Core API usage.
#pragma warning disable CS0612 // Type or member is obsolete
        => entityType.FindAnnotation(CoreAnnotationNames.DefiningQuery)?.GetConfigurationSource();
#pragma warning restore CS0612 // Type or member is obsolete
#pragma warning restore EF1001 // Internal EF Core API usage.
}
