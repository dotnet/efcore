// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable once CheckNamespace

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

/// <summary>
///     A convention that configures tables with triggers to not use the OUTPUT clause when saving changes.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see>, and
///     <see href="https://aka.ms/efcore-docs-sqlserver">Accessing SQL Server and Azure SQL databases with EF Core</see>
///     for more information and examples.
/// </remarks>
public class SqlServerOutputClauseConvention : ITriggerAddedConvention, ITriggerRemovedConvention
{
    /// <summary>
    ///     Creates a new instance of <see cref="SqlServerDbFunctionConvention" />.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this convention.</param>
    /// <param name="relationalDependencies"> Parameter object containing relational dependencies for this convention.</param>
    public SqlServerOutputClauseConvention(
        ProviderConventionSetBuilderDependencies dependencies,
        RelationalConventionSetBuilderDependencies relationalDependencies)
    {
        Dependencies = dependencies;
        RelationalDependencies = relationalDependencies;
    }

    /// <summary>
    ///     Dependencies for this service.
    /// </summary>
    protected virtual ProviderConventionSetBuilderDependencies Dependencies { get; }

    /// <summary>
    ///     Relational provider-specific dependencies for this service.
    /// </summary>
    protected virtual RelationalConventionSetBuilderDependencies RelationalDependencies { get; }

    /// <inheritdoc />
    public virtual void ProcessTriggerAdded(IConventionTriggerBuilder triggerBuilder, IConventionContext<IConventionTriggerBuilder> context)
    {
        var trigger = triggerBuilder.Metadata;
        var entityType = trigger.EntityType;
        var triggerTableIdentifier = StoreObjectIdentifier.Table(trigger.GetTableName(), trigger.GetTableSchema());

        entityType.UseSqlOutputClause(false, triggerTableIdentifier);
    }

    /// <inheritdoc />
    public virtual void ProcessTriggerRemoved(
        IConventionEntityTypeBuilder entityTypeBuilder,
        IConventionTrigger trigger,
        IConventionContext<IConventionTrigger> context)
    {
        var entityType = entityTypeBuilder.Metadata;
        var triggerTableIdentifier = StoreObjectIdentifier.Table(trigger.GetTableName(), trigger.GetTableSchema());

        if (!entityType.GetDeclaredTriggers().Any(
                t => t.GetTableName() == trigger.GetTableName() && t.GetTableSchema() == trigger.GetTableSchema()))
        {
            entityType.UseSqlOutputClause(null, triggerTableIdentifier);
        }
    }
}
