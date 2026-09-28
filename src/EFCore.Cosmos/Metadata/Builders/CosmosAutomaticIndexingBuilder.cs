// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Cosmos.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>
///     A fluent builder used to configure the Cosmos container's automatic indexing policy. Returned by
///     <c>HasAutomaticIndexing</c> on <see cref="EntityTypeBuilder" />; each call to <see cref="Except(string)" />
///     adds a path to the container's <c>ExcludedPaths</c>.
/// </summary>
/// <remarks>
///     See <see href="https://learn.microsoft.com/azure/cosmos-db/index-policy">Indexing policies in Azure Cosmos DB</see>
///     for more information.
/// </remarks>
public class CosmosAutomaticIndexingBuilder : IInfrastructure<IConventionEntityTypeBuilder>
{
    /// <summary>
    ///     Creates a new <see cref="CosmosAutomaticIndexingBuilder" />. Typically obtained via
    ///     <see cref="CosmosEntityTypeBuilderExtensions.HasAutomaticIndexing(EntityTypeBuilder, bool)" /> rather than constructed directly.
    /// </summary>
    /// <param name="entityTypeBuilder">The entity type builder being configured.</param>
    public CosmosAutomaticIndexingBuilder(EntityTypeBuilder entityTypeBuilder)
    {
        Check.NotNull(entityTypeBuilder);

        EntityTypeBuilder = entityTypeBuilder;
    }

    /// <summary>
    ///     The entity type builder used to back this automatic-indexing configuration.
    /// </summary>
    protected virtual EntityTypeBuilder EntityTypeBuilder { get; }

    /// <inheritdoc />
    IConventionEntityTypeBuilder IInfrastructure<IConventionEntityTypeBuilder>.Instance
        => ((IInfrastructure<IConventionEntityTypeBuilder>)EntityTypeBuilder).Instance;

    /// <summary>
    ///     Adds a path to the container's <c>ExcludedPaths</c>. The path must use Cosmos indexing-policy syntax
    ///     (e.g., <c>/secret/?</c> for a leaf, <c>/items/[]/*</c> for an array sub-tree). Throws if automatic
    ///     indexing was disabled via <c>HasAutomaticIndexing(false)</c>.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://learn.microsoft.com/azure/cosmos-db/index-policy">Indexing policies in Azure Cosmos DB</see>
    ///     for more information.
    /// </remarks>
    /// <param name="path">The path to exclude from indexing.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public virtual CosmosAutomaticIndexingBuilder Except(string path)
    {
        Check.NotEmpty(path);

        if (EntityTypeBuilder.Metadata.GetAutomaticIndexingEnabled() == false)
        {
            throw new InvalidOperationException(CosmosStrings.AutomaticIndexingExceptionWhileDisabled);
        }

        var current = EntityTypeBuilder.Metadata.GetAutomaticIndexingExceptions();
        var updated = new List<string>((current?.Count ?? 0) + 1);
        if (current is not null)
        {
            updated.AddRange(current);
        }

        updated.Add(path);
        EntityTypeBuilder.Metadata.SetAutomaticIndexingExceptions(updated);

        return this;
    }
}

/// <summary>
///     A generic fluent builder used to configure the Cosmos container's automatic indexing policy. Returned by
///     <c>HasAutomaticIndexing</c> on <see cref="EntityTypeBuilder{TEntity}" />.
/// </summary>
/// <typeparam name="TEntity">The entity type being configured.</typeparam>
public class CosmosAutomaticIndexingBuilder<TEntity> : CosmosAutomaticIndexingBuilder
    where TEntity : class
{
    /// <summary>
    ///     Creates a new <see cref="CosmosAutomaticIndexingBuilder{TEntity}" />.
    /// </summary>
    /// <param name="entityTypeBuilder">The entity type builder being configured.</param>
    public CosmosAutomaticIndexingBuilder(EntityTypeBuilder<TEntity> entityTypeBuilder)
        : base(entityTypeBuilder)
    {
    }

    /// <inheritdoc cref="CosmosAutomaticIndexingBuilder.Except(string)" />
    public new virtual CosmosAutomaticIndexingBuilder<TEntity> Except(string path)
    {
        base.Except(path);
        return this;
    }
}
