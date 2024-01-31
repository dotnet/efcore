// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.SqlServer.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Metadata.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;

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
    private readonly ISqlGenerationHelper _sqlGenerationHelper;
    private readonly int _sqlServerCompatibilityLevel;

    private bool _withinTable;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqlServerQuerySqlGenerator(
        QuerySqlGeneratorDependencies dependencies,
        IRelationalTypeMappingSource typeMappingSource,
        ISqlServerSingletonOptions sqlServerSingletonOptions)
        : base(dependencies)
    {
        _typeMappingSource = typeMappingSource;
        _sqlGenerationHelper = dependencies.SqlGenerationHelper;
        _sqlServerCompatibilityLevel = sqlServerSingletonOptions.CompatibilityLevel;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override bool TryGenerateWithoutWrappingSelect(SelectExpression selectExpression)
        // SQL Server doesn't support VALUES as a top-level statement, so we need to wrap the VALUES in a SELECT:
        // SELECT 1 AS x UNION VALUES (2), (3) -- simple
        // SELECT 1 AS x UNION SELECT * FROM (VALUES (2), (3)) AS f(x) -- SQL Server
        => selectExpression.Tables is not [ValuesExpression]
            && base.TryGenerateWithoutWrappingSelect(selectExpression);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitDelete(DeleteExpression deleteExpression)
    {
        var selectExpression = deleteExpression.SelectExpression;

        if (selectExpression is
            {
                GroupBy: [],
                Having: null,
                Projection: [],
                Orderings: [],
                Offset: null
            })
        {
            Sql.Append("DELETE ");
            GenerateTop(selectExpression);

            _withinTable = true;
            Sql.AppendLine($"FROM {Dependencies.SqlGenerationHelper.DelimitIdentifier(deleteExpression.Table.Alias)}");

            Sql.Append("FROM ");
            GenerateList(selectExpression.Tables, e => Visit(e), sql => sql.AppendLine());
            _withinTable = false;

            if (selectExpression.Predicate != null)
            {
                Sql.AppendLine().Append("WHERE ");

                Visit(selectExpression.Predicate);
            }

            GenerateLimitOffset(selectExpression);

            return deleteExpression;
        }

        throw new InvalidOperationException(
            RelationalStrings.ExecuteOperationWithUnsupportedOperatorInSqlGeneration(nameof(RelationalQueryableExtensions.ExecuteDelete)));
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitSelect(SelectExpression selectExpression)
    {
        // SQL Server always requires column names to be specified in table subqueries, as opposed to e.g. scalar subqueries (this isn't
        // a requirement in databases). So we must use visitor state to track whether we're (directly) within a table subquery, and
        // generate "1 AS empty" instead of just "1".
        var parentWithinTable = _withinTable;
        base.VisitSelect(selectExpression);
        _withinTable = parentWithinTable;
        return selectExpression;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitUpdate(UpdateExpression updateExpression)
    {
        var selectExpression = updateExpression.SelectExpression;

        if (selectExpression is
            {
                GroupBy: [],
                Having: null,
                Projection: [],
                Orderings: [],
                Offset: null
            })
        {
            Sql.Append("UPDATE ");
            GenerateTop(selectExpression);

            Sql.AppendLine($"{Dependencies.SqlGenerationHelper.DelimitIdentifier(updateExpression.Table.Alias)}");
            Sql.Append("SET ");
            Visit(updateExpression.ColumnValueSetters[0].Column);
            Sql.Append(" = ");
            Visit(updateExpression.ColumnValueSetters[0].Value);

            using (Sql.Indent())
            {
                foreach (var columnValueSetter in updateExpression.ColumnValueSetters.Skip(1))
                {
                    Sql.AppendLine(",");
                    Visit(columnValueSetter.Column);
                    Sql.Append(" = ");
                    Visit(columnValueSetter.Value);
                }
            }

            _withinTable = true;
            Sql.AppendLine().Append("FROM ");
            GenerateList(selectExpression.Tables, e => Visit(e), sql => sql.AppendLine());
            _withinTable = false;

            if (selectExpression.Predicate != null)
            {
                Sql.AppendLine().Append("WHERE ");
                Visit(selectExpression.Predicate);
            }

            return updateExpression;
        }

        throw new InvalidOperationException(
            RelationalStrings.ExecuteOperationWithUnsupportedOperatorInSqlGeneration(nameof(RelationalQueryableExtensions.ExecuteUpdate)));
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitValues(ValuesExpression valuesExpression)
    {
        base.VisitValues(valuesExpression);

        // SQL Server VALUES supports setting the projects column names: FROM (VALUES (1), (2)) AS v(foo)
        Sql.Append("(");

        for (var i = 0; i < valuesExpression.ColumnNames.Count; i++)
        {
            if (i > 0)
            {
                Sql.Append(", ");
            }

            Sql.Append(_sqlGenerationHelper.DelimitIdentifier(valuesExpression.ColumnNames[i]));
        }

        Sql.Append(")");

        return valuesExpression;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override void GenerateValues(ValuesExpression valuesExpression)
    {
        if (valuesExpression.RowValues.Count == 0)
        {
            throw new InvalidOperationException(RelationalStrings.EmptyCollectionNotSupportedAsInlineQueryRoot);
        }

        // SQL Server supports providing the names of columns projected out of VALUES: (VALUES (1, 3), (2, 4)) AS x(a, b)
        // (this is implemented in VisitValues above).
        // But since other databases sometimes don't, the default relational implementation is complex, involving a SELECT for the first row
        // and a UNION All on the rest. Override to do the nice simple thing.

        var rowValues = valuesExpression.RowValues;

        Sql.Append("VALUES ");

        for (var i = 0; i < rowValues.Count; i++)
        {
            if (i > 0)
            {
                Sql.Append(", ");
            }

            Visit(valuesExpression.RowValues[i]);
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override void GenerateTop(SelectExpression selectExpression)
    {
        var parentWithinTable = _withinTable;
        _withinTable = false;

        if (selectExpression is { Limit: not null, Offset: null })
        {
            Sql.Append("TOP(");

            Visit(selectExpression.Limit);

            Sql.Append(") ");
        }

        _withinTable = parentWithinTable;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override void GenerateProjection(SelectExpression selectExpression)
    {
        // SQL Server always requires column names to be specified in table subqueries, as opposed to e.g. scalar subqueries (this isn't
        // a requirement in databases). So we must use visitor state to track whether we're (directly) within a table subquery, and
        // generate "1 AS empty" instead of just "1".
        if (selectExpression.Projection.Count == 0)
        {
            Sql.Append(_withinTable ? "1 AS empty" : "1");
        }
        else
        {
            var parentWithinTable = _withinTable;
            _withinTable = false;
            GenerateList(selectExpression.Projection, e => Visit(e));
            _withinTable = parentWithinTable;
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override void GenerateTables(SelectExpression selectExpression)
    {
        // SQL Server always requires column names to be specified in table subqueries, as opposed to e.g. scalar subqueries (this isn't
        // a requirement in databases). So we must use visitor state to track whether we're (directly) within a table subquery, and
        // generate "1 AS empty" instead of just "1".
        _withinTable = true;
        base.GenerateTables(selectExpression);
        _withinTable = false;
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
    protected virtual Expression VisitSqlServerAggregateFunction(SqlServerAggregateFunctionExpression aggregateFunctionExpression)
    {
        Sql.Append(aggregateFunctionExpression.Name);

        Sql.Append("(");
        GenerateList(aggregateFunctionExpression.Arguments, e => Visit(e));
        Sql.Append(")");

        if (aggregateFunctionExpression.Orderings.Count > 0)
        {
            Sql.Append(" WITHIN GROUP (ORDER BY ");
            GenerateList(aggregateFunctionExpression.Orderings, e => Visit(e));
            Sql.Append(")");
        }

        return aggregateFunctionExpression;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitExtension(Expression extensionExpression)
    {
        switch (extensionExpression)
        {
            case TableExpression tableExpression
                when tableExpression.FindAnnotation(SqlServerAnnotationNames.TemporalOperationType) != null:
            {
                Sql.Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(tableExpression.Name, tableExpression.Schema))
                    .Append(" FOR SYSTEM_TIME ");

                var temporalOperationType = (TemporalOperationType)tableExpression
                    .FindAnnotation(SqlServerAnnotationNames.TemporalOperationType)!.Value!;

                switch (temporalOperationType)
                {
                    case TemporalOperationType.All:
                        Sql.Append("ALL");
                        break;

                    case TemporalOperationType.AsOf:
                        var pointInTime =
                            (DateTime)tableExpression.FindAnnotation(SqlServerAnnotationNames.TemporalAsOfPointInTime)!.Value!;

                        Sql.Append("AS OF ")
                            .Append(_typeMappingSource.GetMapping(typeof(DateTime)).GenerateSqlLiteral(pointInTime));
                        break;

                    case TemporalOperationType.Between:
                    case TemporalOperationType.ContainedIn:
                    case TemporalOperationType.FromTo:
                        var from = _typeMappingSource.GetMapping(typeof(DateTime)).GenerateSqlLiteral(
                            (DateTime)tableExpression.FindAnnotation(SqlServerAnnotationNames.TemporalRangeOperationFrom)!.Value!);

                        var to = _typeMappingSource.GetMapping(typeof(DateTime)).GenerateSqlLiteral(
                            (DateTime)tableExpression.FindAnnotation(SqlServerAnnotationNames.TemporalRangeOperationTo)!.Value!);

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

            case SqlServerAggregateFunctionExpression aggregateFunctionExpression:
                return VisitSqlServerAggregateFunction(aggregateFunctionExpression);

            case SqlServerOpenJsonExpression openJsonExpression:
                return VisitOpenJsonExpression(openJsonExpression);
        }

        return base.VisitExtension(extensionExpression);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitJsonScalar(JsonScalarExpression jsonScalarExpression)
    {
        // TODO: Stop producing empty JsonScalarExpressions, #30768
        var path = jsonScalarExpression.Path;
        if (path.Count == 0)
        {
            Visit(jsonScalarExpression.Json);
            return jsonScalarExpression;
        }

        if (jsonScalarExpression.TypeMapping is SqlServerJsonTypeMapping
            || jsonScalarExpression.TypeMapping?.ElementTypeMapping is not null)
        {
            Sql.Append("JSON_QUERY(");
        }
        else
        {
            // JSON_VALUE always returns nvarchar(4000) (https://learn.microsoft.com/sql/t-sql/functions/json-value-transact-sql),
            // so we cast the result to the expected type - except if it's a string (since the cast interferes with indexes over
            // the JSON property).
            Sql.Append(jsonScalarExpression.TypeMapping is StringTypeMapping ? "JSON_VALUE(" : "CAST(JSON_VALUE(");
        }

        Visit(jsonScalarExpression.Json);

        Sql.Append(", ");
        GenerateJsonPath(jsonScalarExpression.Path);
        Sql.Append(")");

        if (jsonScalarExpression.TypeMapping is not SqlServerJsonTypeMapping and not StringTypeMapping)
        {
            Sql.Append(" AS ");
            Sql.Append(jsonScalarExpression.TypeMapping!.StoreType);
            Sql.Append(")");
        }

        return jsonScalarExpression;
    }

    private void GenerateJsonPath(IReadOnlyList<PathSegment> path)
    {
        Sql.Append("'$");

        foreach (var pathSegment in path)
        {
            switch (pathSegment)
            {
                case { PropertyName: string propertyName }:
                    Sql.Append(".").Append(propertyName);
                    break;

                case { ArrayIndex: SqlExpression arrayIndex }:
                    Sql.Append("[");

                    // JSON functions such as JSON_VALUE only support arbitrary expressions for the path parameter in SQL Server 2017 and
                    // above; before that, arguments must be constant strings.
                    if (arrayIndex is SqlConstantExpression)
                    {
                        Visit(arrayIndex);
                    }
                    else if (_sqlServerCompatibilityLevel >= 140)
                    {
                        Sql.Append("' + CAST(");
                        Visit(arrayIndex);
                        Sql.Append(" AS ");
                        Sql.Append(_typeMappingSource.GetMapping(typeof(string)).StoreType);
                        Sql.Append(") + '");
                    }
                    else
                    {
                        throw new InvalidOperationException(
                            SqlServerStrings.JsonValuePathExpressionsNotSupported(_sqlServerCompatibilityLevel));
                    }

                    Sql.Append("]");
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        Sql.Append("'");
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual Expression VisitOpenJsonExpression(SqlServerOpenJsonExpression openJsonExpression)
    {
        // OPENJSON docs: https://learn.microsoft.com/sql/t-sql/functions/openjson-transact-sql

        // The second argument is the JSON path, which is represented as a list of PathSegments, from which we generate a SQL jsonpath
        // expression.
        Sql.Append("OPENJSON(");

        Visit(openJsonExpression.JsonExpression);

        if (openJsonExpression.Path is not null)
        {
            Sql.Append(", ");
            GenerateJsonPath(openJsonExpression.Path);
        }

        Sql.Append(")");

        if (openJsonExpression.ColumnInfos is not null)
        {
            Sql.Append(" WITH (");

            if (openJsonExpression.ColumnInfos is [var singleColumnInfo])
            {
                GenerateColumnInfo(singleColumnInfo);
            }
            else
            {
                Sql.AppendLine();
                using var _ = Sql.Indent();

                for (var i = 0; i < openJsonExpression.ColumnInfos.Count; i++)
                {
                    var columnInfo = openJsonExpression.ColumnInfos[i];

                    if (i > 0)
                    {
                        Sql.AppendLine(",");
                    }

                    GenerateColumnInfo(columnInfo);
                }

                Sql.AppendLine();
            }

            Sql.Append(")");

            void GenerateColumnInfo(SqlServerOpenJsonExpression.ColumnInfo columnInfo)
            {
                Sql
                    .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(columnInfo.Name))
                    .Append(" ")
                    .Append(columnInfo.TypeMapping.StoreType);

                if (columnInfo.Path is not null)
                {
                    Sql.Append(" ");
                    GenerateJsonPath(columnInfo.Path);
                }

                if (columnInfo.AsJson)
                {
                    Sql.Append(" AS JSON");
                }
            }
        }

        Sql.Append(AliasSeparator).Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(openJsonExpression.Alias));

        return openJsonExpression;
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

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override bool TryGetOperatorInfo(SqlExpression expression, out int precedence, out bool isAssociative)
    {
        // See https://docs.microsoft.com/sql/t-sql/language-elements/operator-precedence-transact-sql, although that list is very partial
        (precedence, isAssociative) = expression switch
        {
            SqlBinaryExpression sqlBinaryExpression => sqlBinaryExpression.OperatorType switch
            {
                ExpressionType.Multiply => (900, true),
                ExpressionType.Divide => (900, false),
                ExpressionType.Modulo => (900, false),
                ExpressionType.Add => (800, true),
                ExpressionType.Subtract => (800, false),
                ExpressionType.And => (700, true),
                ExpressionType.Or => (700, true),
                ExpressionType.LeftShift => (700, true),
                ExpressionType.RightShift => (700, true),
                ExpressionType.LessThan => (600, false),
                ExpressionType.LessThanOrEqual => (600, false),
                ExpressionType.GreaterThan => (600, false),
                ExpressionType.GreaterThanOrEqual => (600, false),
                ExpressionType.Equal => (500, false),
                ExpressionType.NotEqual => (500, false),
                ExpressionType.AndAlso => (200, true),
                ExpressionType.OrElse => (100, true),

                _ => default,
            },

            SqlUnaryExpression sqlUnaryExpression => sqlUnaryExpression.OperatorType switch
            {
                ExpressionType.Convert => (1300, false),
                ExpressionType.Not when sqlUnaryExpression.Type != typeof(bool) => (1200, false),
                ExpressionType.Negate => (1100, false),
                ExpressionType.Equal => (500, false), // IS NULL
                ExpressionType.NotEqual => (500, false), // IS NOT NULL
                ExpressionType.Not when sqlUnaryExpression.Type == typeof(bool) => (300, false),

                _ => default,
            },

            CollateExpression => (900, false),
            LikeExpression => (350, false),
            AtTimeZoneExpression => (1200, false),

            // On SQL Server, JsonScalarExpression renders as a function (JSON_VALUE()), so there's never a need for parentheses.
            JsonScalarExpression => (9999, false),

            _ => default,
        };

        return precedence != default;
    }

    private void GenerateList<T>(
        IReadOnlyList<T> items,
        Action<T> generationAction,
        Action<IRelationalCommandBuilder>? joinAction = null)
    {
        joinAction ??= (isb => isb.Append(", "));

        for (var i = 0; i < items.Count; i++)
        {
            if (i > 0)
            {
                joinAction(Sql);
            }

            generationAction(items[i]);
        }
    }
}
