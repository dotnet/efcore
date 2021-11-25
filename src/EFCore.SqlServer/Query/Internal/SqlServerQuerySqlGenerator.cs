// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class SqlServerQuerySqlGenerator : QuerySqlGenerator
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqlServerQuerySqlGenerator(QuerySqlGeneratorDependencies dependencies)
        : base(dependencies)
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override void GenerateTop(SelectExpression selectExpression)
    {
        if (selectExpression.Limit != null
            && selectExpression.Offset == null)
        {
            Sql.Append("TOP(");

            Visit(selectExpression.Limit);

            Sql.Append(") ");
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override void GenerateOrderings(SelectExpression selectExpression)
    {
        base.GenerateOrderings(selectExpression);

        // In SQL Server, if an offset is specified, then an ORDER BY clause must also exist.
        // Generate a fake one.
        if (!selectExpression.Orderings.Any() && selectExpression.Offset != null)
        {
            Sql.AppendLine().Append("ORDER BY (SELECT 1)");
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override void GenerateLimitOffset(SelectExpression selectExpression)
    {
        // Note: For Limit without Offset, SqlServer generates TOP()
        if (selectExpression.Offset != null)
        {
            Sql.AppendLine()
                .Append("OFFSET ");

            Visit(selectExpression.Offset);

            Sql.Append(" ROWS");

            if (selectExpression.Limit != null)
            {
                Sql.Append(" FETCH NEXT ");

                Visit(selectExpression.Limit);

                Sql.Append(" ROWS ONLY");
            }
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitExtension(Expression extensionExpression)
    {
        if (extensionExpression is TableExpression tableExpression
            && tableExpression.FindAnnotation(SqlServerSqlExpressionAnnotationNames.TemporalOperationType) != null)
        {
            Sql.Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(tableExpression.Name, tableExpression.Schema))
                .Append(" FOR SYSTEM_TIME ");

            var temporalOperationType = (TemporalOperationType)tableExpression[SqlServerSqlExpressionAnnotationNames.TemporalOperationType]!;

            switch (temporalOperationType)
            {
                case TemporalOperationType.All:
                    Sql.Append("ALL");
                    break;

                case TemporalOperationType.AsOf:
                    var pointInTime = (DateTime)tableExpression[SqlServerSqlExpressionAnnotationNames.TemporalAsOfPointInTime]!;
        
                    Sql.Append("AS OF ")
                        .Append(Sql.TypeMappingSource.GetMapping(typeof(DateTime)).GenerateSqlLiteral(pointInTime));
                    break;

                case TemporalOperationType.Between:
                case TemporalOperationType.ContainedIn:
                case TemporalOperationType.FromTo:
                    var from = Sql.TypeMappingSource.GetMapping(typeof(DateTime)).GenerateSqlLiteral(
                        (DateTime)tableExpression[SqlServerSqlExpressionAnnotationNames.TemporalRangeOperationFrom]!);

                    var to = Sql.TypeMappingSource.GetMapping(typeof(DateTime)).GenerateSqlLiteral(
                        (DateTime)tableExpression[SqlServerSqlExpressionAnnotationNames.TemporalRangeOperationTo]!);

                    switch (temporalOperationType)
                    {
                        case TemporalOperationType.FromTo:
                            Sql.Append($"FROM {from} TO {to}");
                            break;

                        case TemporalOperationType.Between:
                            Sql.Append($"BETWEEN {from} AND {to}");
                            break;

                        case TemporalOperationType.ContainedIn:
                            Sql.Append($"CONTAINED IN ({from}, {to})");
                            break;

                        default:
                            throw new InvalidOperationException(tableExpression.Print());
                    }

                    break;

                default:
                    throw new InvalidOperationException(tableExpression.Print());
            }

            if (tableExpression.Alias != null)
            {
                Sql.Append(AliasSeparator)
                    .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(tableExpression.Alias));
            }

            return tableExpression;
        }

        return base.VisitExtension(extensionExpression);
    }

    /// <inheritdoc />
    protected override void CheckComposableSqlTrimmed(ReadOnlySpan<char> sql)
    {
        base.CheckComposableSqlTrimmed(sql);

        if (sql.StartsWith("WITH", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(RelationalStrings.FromSqlNonComposable);
        }
    }
}
