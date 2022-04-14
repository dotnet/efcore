// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     Extension methods for <see cref="EntityTypeBuilder" /> for the in-memory provider.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
///     <see href="https://aka.ms/efcore-docs-in-memory">The EF Core in-memory database provider</see> for more information and examples.
/// </remarks>
public static class InMemoryEntityTypeBuilderExtensions
{
    /// <summary>
    ///     Configures a query used to provide data for an entity type.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///     <see href="https://aka.ms/efcore-docs-in-memory">The EF Core in-memory database provider</see> for more information and examples.
    /// </remarks>
    /// <param name="entityTypeBuilder">The builder for the entity type being configured.</param>
    /// <param name="query">The query that will provide the underlying data for the entity type.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static EntityTypeBuilder ToInMemoryQuery(
        this EntityTypeBuilder entityTypeBuilder,
        LambdaExpression? query)
    {
        Check.NotNull(query, nameof(query));

        entityTypeBuilder.Metadata.SetInMemoryQuery(query);

        return entityTypeBuilder;
    }

    /// <summary>
    ///     Configures a query used to provide data for an entity type.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///     <see href="https://aka.ms/efcore-docs-in-memory">The EF Core in-memory database provider</see> for more information and examples.
    /// </remarks>
    /// <param name="entityTypeBuilder">The builder for the entity type being configured.</param>
    /// <param name="query">The query that will provide the underlying data for the entity type.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static EntityTypeBuilder<TEntity> ToInMemoryQuery<TEntity>(
        this EntityTypeBuilder<TEntity> entityTypeBuilder,
        Expression<Func<IQueryable<TEntity>>> query)
        where TEntity : class
    {
        Check.NotNull(query, nameof(query));

        entityTypeBuilder.Metadata.SetInMemoryQuery(query);

        return entityTypeBuilder;
    }

    /// <summary>
    ///     Configures a query used to provide data for an entity type.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///     <see href="https://aka.ms/efcore-docs-in-memory">The EF Core in-memory database provider</see> for more information and examples.
    /// </remarks>
    /// <param name="entityTypeBuilder">The builder for the entity type being configured.</param>
    /// <param name="query">The query that will provide the underlying data for the entity type.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     The same builder instance if the query was set, <see langword="null" /> otherwise.
    /// </returns>
    public static IConventionEntityTypeBuilder? ToInMemoryQuery(
        this IConventionEntityTypeBuilder entityTypeBuilder,
        LambdaExpression? query,
        bool fromDataAnnotation = false)
    {
        if (CanSetInMemoryQuery(entityTypeBuilder, query, fromDataAnnotation))
        {
            entityTypeBuilder.Metadata.SetInMemoryQuery(query, fromDataAnnotation);

            return entityTypeBuilder;
        }

        return null;
    }

    /// <summary>
    ///     Returns a value indicating whether the given in-memory query can be set from the current configuration source.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///     <see href="https://aka.ms/efcore-docs-in-memory">The EF Core in-memory database provider</see> for more information and examples.
    /// </remarks>
    /// <param name="entityTypeBuilder">The builder for the entity type being configured.</param>
    /// <param name="query">The query that will provide the underlying data for the keyless entity type.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the given in-memory query can be set.</returns>
    public static bool CanSetInMemoryQuery(
        this IConventionEntityTypeBuilder entityTypeBuilder,
        LambdaExpression? query,
        bool fromDataAnnotation = false)
#pragma warning disable EF1001 // Internal EF Core API usage.
#pragma warning disable CS0612 // Type or member is obsolete
        => entityTypeBuilder.CanSetAnnotation(CoreAnnotationNames.DefiningQuery, query, fromDataAnnotation);
#pragma warning restore CS0612 // Type or member is obsolete
#pragma warning restore EF1001 // Internal EF Core API usage.
}
