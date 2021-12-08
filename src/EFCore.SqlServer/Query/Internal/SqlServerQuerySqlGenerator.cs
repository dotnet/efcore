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
    private readonly IRelationalTypeMappingSource _typeMappingSource;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqlServerQuerySqlGenerator(QuerySqlGeneratorDependencies dependencies,
        IRelationalTypeMappingSource typeMappingSource)
        : base(dependencies)
    {
        _typeMappingSource = typeMappingSource;
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
        if (extensionExpression is TemporalTableExpression temporalTableExpression)
        {
            Sql.Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(temporalTableExpression.Name, temporalTableExpression.Schema))
                .Append(" FOR SYSTEM_TIME ");

            switch (temporalTableExpression)
            {
                case TemporalAsOfTableExpression asOf:
                    Sql.Append("AS OF ")
                        .Append(_typeMappingSource.GetMapping(typeof(DateTime)).GenerateSqlLiteral(asOf.PointInTime));
                    break;

                case TemporalFromToTableExpression fromTo:
                    Sql.Append("FROM ")
                        .Append(_typeMappingSource.GetMapping(typeof(DateTime)).GenerateSqlLiteral(fromTo.From))
                        .Append(" TO ")
                        .Append(_typeMappingSource.GetMapping(typeof(DateTime)).GenerateSqlLiteral(fromTo.To));
                    break;

                case TemporalBetweenTableExpression between:
                    Sql.Append("BETWEEN ")
                        .Append(_typeMappingSource.GetMapping(typeof(DateTime)).GenerateSqlLiteral(between.From))
                        .Append(" AND ")
                        .Append(_typeMappingSource.GetMapping(typeof(DateTime)).GenerateSqlLiteral(between.To));
                    break;

                case TemporalContainedInTableExpression containedIn:
                    Sql.Append("CONTAINED IN (")
                        .Append(_typeMappingSource.GetMapping(typeof(DateTime)).GenerateSqlLiteral(containedIn.From))
                        .Append(", ")
                        .Append(_typeMappingSource.GetMapping(typeof(DateTime)).GenerateSqlLiteral(containedIn.To))
                        .Append(")");
                    break;

                case TemporalAllTableExpression:
                    Sql.Append("ALL");
                    break;

                default:
                    throw new InvalidOperationException(temporalTableExpression.Print());
            }

            if (temporalTableExpression.Alias != null)
            {
                Sql.Append(AliasSeparator)
                    .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(temporalTableExpression.Alias));
            }

            return temporalTableExpression;
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
