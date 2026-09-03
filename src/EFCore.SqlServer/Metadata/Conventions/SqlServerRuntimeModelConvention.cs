// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.SqlServer.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

/// <summary>
///     A convention that creates an optimized copy of the mutable model.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see>, and
///     <see href="https://aka.ms/efcore-docs-sqlserver">Accessing SQL Server and Azure SQL databases with EF Core</see>
///     for more information and examples.
/// </remarks>
public class SqlServerRuntimeModelConvention : RelationalRuntimeModelConvention
{
    /// <summary>
    ///     Creates a new instance of <see cref="SqlServerRuntimeModelConvention" />.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this convention.</param>
    /// <param name="relationalDependencies"> Parameter object containing relational dependencies for this convention.</param>
    public SqlServerRuntimeModelConvention(
        ProviderConventionSetBuilderDependencies dependencies,
        RelationalConventionSetBuilderDependencies relationalDependencies)
        : base(dependencies, relationalDependencies)
    {
    }

    /// <inheritdoc />
    protected override void ProcessModelAnnotations(
        Dictionary<string, object?> annotations,
        IModel model,
        RuntimeModel runtimeModel,
        bool runtime)
    {
        base.ProcessModelAnnotations(annotations, model, runtimeModel, runtime);

        if (!runtime)
        {
            annotations.Remove(SqlServerAnnotationNames.IdentityIncrement);
            annotations.Remove(SqlServerAnnotationNames.IdentitySeed);
            annotations.Remove(SqlServerAnnotationNames.MaxDatabaseSize);
            annotations.Remove(SqlServerAnnotationNames.PerformanceLevelSql);
            annotations.Remove(SqlServerAnnotationNames.ServiceTierSql);
        }
    }

    /// <inheritdoc />
    protected override void ProcessPropertyAnnotations(
        Dictionary<string, object?> annotations,
        IProperty property,
        RuntimeProperty runtimeProperty,
        bool runtime)
    {
        base.ProcessPropertyAnnotations(annotations, property, runtimeProperty, runtime);

        if (!runtime)
        {
            annotations.Remove(SqlServerAnnotationNames.IdentityIncrement);
            annotations.Remove(SqlServerAnnotationNames.IdentitySeed);
            annotations.Remove(SqlServerAnnotationNames.Sparse);

            if (!annotations.ContainsKey(SqlServerAnnotationNames.ValueGenerationStrategy))
            {
                annotations[SqlServerAnnotationNames.ValueGenerationStrategy] = property.GetValueGenerationStrategy();
            }
        }
    }

    /// <inheritdoc />
    protected override void ProcessPropertyOverridesAnnotations(
        Dictionary<string, object?> annotations,
        IRelationalPropertyOverrides propertyOverrides,
        RuntimeRelationalPropertyOverrides runtimePropertyOverrides,
        bool runtime)
    {
        base.ProcessPropertyOverridesAnnotations(annotations, propertyOverrides, runtimePropertyOverrides, runtime);

        if (!runtime)
        {
            annotations.Remove(SqlServerAnnotationNames.IdentityIncrement);
            annotations.Remove(SqlServerAnnotationNames.IdentitySeed);
        }
    }

    /// <inheritdoc />
    protected override void ProcessIndexAnnotations(
        Dictionary<string, object?> annotations,
        IIndex index,
        RuntimeIndex runtimeIndex,
        bool runtime)
    {
        base.ProcessIndexAnnotations(annotations, index, runtimeIndex, runtime);

        if (!runtime)
        {
            annotations.Remove(SqlServerAnnotationNames.Clustered);
            annotations.Remove(SqlServerAnnotationNames.CreatedOnline);
            annotations.Remove(SqlServerAnnotationNames.Include);
            annotations.Remove(SqlServerAnnotationNames.FillFactor);
            annotations.Remove(SqlServerAnnotationNames.SortInTempDb);
            annotations.Remove(SqlServerAnnotationNames.DataCompression);
        }
    }

    /// <inheritdoc />
    protected override void ProcessKeyAnnotations(
        Dictionary<string, object?> annotations,
        IKey key,
        RuntimeKey runtimeKey,
        bool runtime)
    {
        base.ProcessKeyAnnotations(annotations, key, runtimeKey, runtime);

        if (!runtime)
        {
            annotations.Remove(SqlServerAnnotationNames.Clustered);
            annotations.Remove(SqlServerAnnotationNames.FillFactor);
        }
    }

    /// <inheritdoc />
    protected override void ProcessEntityTypeAnnotations(
        Dictionary<string, object?> annotations,
        IEntityType entityType,
        RuntimeEntityType runtimeEntityType,
        bool runtime)
    {
        base.ProcessEntityTypeAnnotations(annotations, entityType, runtimeEntityType, runtime);

        if (!runtime)
        {
            annotations.Remove(SqlServerAnnotationNames.TemporalHistoryTableName);
            annotations.Remove(SqlServerAnnotationNames.TemporalHistoryTableSchema);
            annotations.Remove(SqlServerAnnotationNames.TemporalPeriodEndColumnName);
            annotations.Remove(SqlServerAnnotationNames.TemporalPeriodEndPropertyName);
            annotations.Remove(SqlServerAnnotationNames.TemporalPeriodStartColumnName);
            annotations.Remove(SqlServerAnnotationNames.TemporalPeriodStartPropertyName);
        }
    }
}
