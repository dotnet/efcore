// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.SqlServer.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.SqlServer.Migrations.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class SqlServerMigrationsAnnotationProvider : MigrationsAnnotationProvider
{
    /// <summary>
    ///     Initializes a new instance of this class.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this service.</param>
#pragma warning disable EF1001 // Internal EF Core API usage.
    public SqlServerMigrationsAnnotationProvider(MigrationsAnnotationProviderDependencies dependencies)
#pragma warning restore EF1001 // Internal EF Core API usage.
        : base(dependencies)
    {
    }

    /// <inheritdoc />
    public override IEnumerable<IAnnotation> ForRemove(IRelationalModel model)
        => model.GetAnnotations().Where(a => a.Name != SqlServerAnnotationNames.EditionOptions);

    /// <inheritdoc />
    public override IEnumerable<IAnnotation> ForRemove(ITable table)
        => table.GetAnnotations();

    /// <inheritdoc />
    public override IEnumerable<IAnnotation> ForRemove(IUniqueConstraint constraint)
    {
        if (constraint.Table[SqlServerAnnotationNames.IsTemporal] as bool? == true)
        {
            yield return new Annotation(SqlServerAnnotationNames.IsTemporal, true);

            yield return new Annotation(
                SqlServerAnnotationNames.TemporalPeriodStartColumnName,
                constraint.Table[SqlServerAnnotationNames.TemporalPeriodStartColumnName]);

            yield return new Annotation(
                SqlServerAnnotationNames.TemporalPeriodEndColumnName,
                constraint.Table[SqlServerAnnotationNames.TemporalPeriodEndColumnName]);

            yield return new Annotation(
                SqlServerAnnotationNames.TemporalHistoryTableName,
                constraint.Table[SqlServerAnnotationNames.TemporalHistoryTableName]);

            yield return new Annotation(
                SqlServerAnnotationNames.TemporalHistoryTableSchema,
                constraint.Table[SqlServerAnnotationNames.TemporalHistoryTableSchema]);
        }
    }

    /// <inheritdoc />
    public override IEnumerable<IAnnotation> ForRemove(IColumn column)
    {
        if (column.Table[SqlServerAnnotationNames.IsTemporal] as bool? == true)
        {
            yield return new Annotation(SqlServerAnnotationNames.IsTemporal, true);

            yield return new Annotation(
                SqlServerAnnotationNames.TemporalHistoryTableName,
                column.Table[SqlServerAnnotationNames.TemporalHistoryTableName]);

            yield return new Annotation(
                SqlServerAnnotationNames.TemporalHistoryTableSchema,
                column.Table[SqlServerAnnotationNames.TemporalHistoryTableSchema]);

            if (column[SqlServerAnnotationNames.TemporalPeriodStartColumnName] is string periodStartColumnName)
            {
                yield return new Annotation(
                    SqlServerAnnotationNames.TemporalPeriodStartColumnName,
                    periodStartColumnName);
            }

            if (column[SqlServerAnnotationNames.TemporalPeriodEndColumnName] is string periodEndColumnName)
            {
                yield return new Annotation(
                    SqlServerAnnotationNames.TemporalPeriodEndColumnName,
                    periodEndColumnName);
            }
        }
    }

    /// <inheritdoc />
    public override IEnumerable<IAnnotation> ForRename(ITable table)
    {
        if (table[SqlServerAnnotationNames.IsTemporal] as bool? == true)
        {
            yield return new Annotation(SqlServerAnnotationNames.IsTemporal, true);

            yield return new Annotation(
                SqlServerAnnotationNames.TemporalHistoryTableName,
                table[SqlServerAnnotationNames.TemporalHistoryTableName]);

            yield return new Annotation(
                SqlServerAnnotationNames.TemporalHistoryTableSchema,
                table[SqlServerAnnotationNames.TemporalHistoryTableSchema]);
        }
    }
}
