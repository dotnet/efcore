// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Cosmos.Metadata.Conventions;

/// <summary>
///     A convention that configures non-owned foreign keys as unconstrained, because Azure Cosmos DB does not
///     enforce foreign key constraints between documents. Owned (embedded) relationships remain constrained.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see> and
///     <see href="https://aka.ms/efcore-docs-cosmos">Accessing Azure Cosmos DB with EF Core</see> for more information and examples.
/// </remarks>
public class CosmosUnconstrainedForeignKeyConvention : IModelFinalizingConvention
{
    /// <summary>
    ///     Creates a new instance of <see cref="CosmosUnconstrainedForeignKeyConvention" />.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this convention.</param>
    public CosmosUnconstrainedForeignKeyConvention(ProviderConventionSetBuilderDependencies dependencies)
        => Dependencies = dependencies;

    /// <summary>
    ///     Dependencies for this service.
    /// </summary>
    protected virtual ProviderConventionSetBuilderDependencies Dependencies { get; }

    /// <inheritdoc />
    public virtual void ProcessModelFinalizing(
        IConventionModelBuilder modelBuilder,
        IConventionContext<IConventionModelBuilder> context)
    {
        foreach (var entityType in modelBuilder.Metadata.GetEntityTypes())
        {
            // Snapshot with ToList(): setting IsConstrained dispatches OnForeignKeyConstrainednessChanged,
            // and the established pattern guards against a convention mutating foreign keys mid-iteration.
            foreach (var foreignKey in entityType.GetDeclaredForeignKeys().ToList())
            {
                // Owned (embedded) relationships keep their default constrained semantics: the principal
                // document is guaranteed to exist because the dependent is embedded in it. Everything else
                // in Cosmos is unconstrained (no foreign key constraints between documents).
                // Set at the convention configuration source so an explicit `.IsConstrained(true)` still wins.
                if (!foreignKey.IsOwnership)
                {
                    foreignKey.Builder.IsConstrained(false);
                }
            }
        }
    }
}
