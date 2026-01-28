// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Globalization;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.SqlServer.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Metadata.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Query.Internal.SqlExpressions;
using Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;

namespace Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class SqlServerQuerySqlGenerator(
    QuerySqlGeneratorDependencies dependencies,
    IRelationalTypeMappingSource typeMappingSource,
    ISqlServerSingletonOptions sqlServerSingletonOptions)
    : QuerySqlGenerator(dependencies)
{
    private readonly IRelationalTypeMappingSource _typeMappingSource = typeMappingSource;
    private readonly ISqlGenerationHelper _sqlGenerationHelper = dependencies.SqlGenerationHelper;
    private readonly ISqlServerSingletonOptions _sqlServerSingletonOptions = sqlServerSingletonOptions;

    private bool _withinTable;


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
    protected override Expression VisitCollate(CollateExpression collateExpression)
    {
        Visit(collateExpression.Operand);

        // SQL Server collation docs: https://learn.microsoft.com/sql/relational-databases/collations/collation-and-unicode-support

        // The default behavior in QuerySqlGenerator is to quote collation names, but SQL Server does not support that.
        // Instead, make sure the collation name only contains a restricted set of characters.
        foreach (var c in collateExpression.Collation)
        {
            if (!char.IsLetterOrDigit(c) && c != '_')
            {
                throw new InvalidOperationException(SqlServerStrings.InvalidCollationName(collateExpression.Collation));
            }
        }

        Sql
            .Append(" COLLATE ")
            .Append(collateExpression.Collation);

        return collateExpression;
    }

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
            RelationalStrings.ExecuteOperationWithUnsupportedOperatorInSqlGeneration(
                nameof(EntityFrameworkQueryableExtensions.ExecuteDelete)));
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
    protected override Expression VisitTableValuedFunction(TableValuedFunctionExpression function)
    {
        switch (function)
        {
            // The VECTOR_SEARCH() function requires some special syntax (specifically named parameters), as well as
            // some special handling for its column parameter, which needs to be written out without a table alias.
            case
            {
                Name: "VECTOR_SEARCH",
                Arguments:
                [
                    TableExpression table,
                    ColumnExpression column,
                    SqlExpression similarTo,
                    SqlConstantExpression { Value: string } metric,
                    SqlExpression topN
                ]
            }:
                // VECTOR_SEARCH(
                //     TABLE = [Articles] AS t,
                //     COLUMN = [Vector],
                //     SIMILAR_TO = @qv,
                //     METRIC = 'Cosine',
                //     TOP_N = 3
                // )
                Sql.AppendLine("VECTOR_SEARCH(");

                using (Sql.Indent())
                {
                    Sql.Append("TABLE = ");
                    VisitTable(table);
                    Sql.AppendLine(",");

                    // SQL Server requires only the column name here, without a table alias (COLUMN = [Vector], not COLUMN = [b].[Vector]).
                    // Since ColumnExpression requires a non-nullable table alias, we handle this here in a special way.
                    Check.DebugAssert(column.TableAlias == table.Alias);
                    Sql.Append("COLUMN = ").Append(_sqlGenerationHelper.DelimitIdentifier(column.Name)).AppendLine(",");

                    Sql.Append("SIMILAR_TO = ");
                    Visit(similarTo);
                    Sql.AppendLine(",");

                    Sql.Append("METRIC = ");
                    Visit(metric);
                    Sql.AppendLine(",");

                    Sql.Append("TOP_N = ");
                    Visit(topN);
                    Sql.AppendLine();
                }

                Sql.Append(")")
                    .Append(AliasSeparator)
                    .Append(_sqlGenerationHelper.DelimitIdentifier(function.Alias));

                return function;

            // FREETEXTTABLE and CONTAINSTABLE full-text search functions
            // Syntax: FREETEXTTABLE/CONTAINSTABLE(table, column, 'search_string' [, LANGUAGE language_term] [, top_n_by_rank])
            case
            {
                Name: "FREETEXTTABLE" or "CONTAINSTABLE",
                Arguments: [TableExpression table, var columnsArgument, SqlExpression searchText, ..]
            }:
            {
                Sql.Append(function.Name).Append("(");

                // Table name
                Sql.Append(_sqlGenerationHelper.DelimitIdentifier(table.Name, table.Schema));
                Sql.Append(", ");

                // Column(s) - NewArrayExpression containing ColumnExpressions (empty array means "*")
                switch (columnsArgument)
                {
                    case NewArrayExpression { Expressions: [] }:
                        // Empty array means all columns
                        Sql.Append("*");
                        break;

                    case NewArrayExpression { Expressions: [ColumnExpression singleColumn] }:
                        // Single column - just write the delimited name
                        Sql.Append(_sqlGenerationHelper.DelimitIdentifier(singleColumn.Name));
                        break;

                    case NewArrayExpression { Expressions: IReadOnlyList<Expression> columns }:
                        // Multiple columns - wrap in parentheses
                        Sql.Append("(");

                        for (var i = 0; i < columns.Count; i++)
                        {
                            if (i > 0)
                            {
                                Sql.Append(", ");
                            }

                            Sql.Append(_sqlGenerationHelper.DelimitIdentifier(((ColumnExpression)columns[i]).Name));
                        }

                        Sql.Append(")");
                        break;

                    default:
                        throw new UnreachableException();
                }

                Sql.Append(", ");

                // Search text
                Visit(searchText);

                // Check remaining arguments for LANGUAGE and top_n
                var arguments = function.Arguments;
                var argIndex = 3;
                if (arguments.Count > argIndex && arguments[argIndex] is SqlConstantExpression { Value: string } languageTerm)
                {
                    Sql.Append(", LANGUAGE ");
                    Visit(languageTerm);
                    argIndex++;
                }

                if (arguments.Count > argIndex)
                {
                    Sql.Append(", ");
                    Visit(arguments[argIndex]);
                }

                Sql.Append(")")
                    .Append(AliasSeparator)
                    .Append(_sqlGenerationHelper.DelimitIdentifier(function.Alias));

                return function;
            }

            default:
                return base.VisitTableValuedFunction(function);
        }
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

            for (var i = 0; i < updateExpression.ColumnValueSetters.Count; i++)
            {
                var (column, value) = updateExpression.ColumnValueSetters[i];

                if (i == 1)
                {
                    Sql.IncrementIndent();
                }

                if (i > 0)
                {
                    Sql.AppendLine(",");
                }

                // SQL Server 2025 modify method (https://learn.microsoft.com/sql/t-sql/data-types/json-data-type#modify-method)
                // This requires special handling since modify isn't a standard setter of the form SET x = y, but rather just
                // SET [x].modify(...).
                if (value is SqlFunctionExpression
                    {
                        Name: "modify",
                        IsBuiltIn: true,
                        Instance: ColumnExpression { TypeMapping.StoreType: "json" } instance
                    })
                {
                    Visit(value);
                    continue;
                }

                Visit(column);
                Sql.Append(" = ");
                Visit(value);
            }

            if (updateExpression.ColumnValueSetters.Count > 1)
            {
                Sql.DecrementIndent();
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
            RelationalStrings.ExecuteOperationWithUnsupportedOperatorInSqlGeneration(
                nameof(EntityFrameworkQueryableExtensions.ExecuteUpdate)));
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
    ///     Generates SQL for a constant.
    /// </summary>
    /// <param name="sqlConstantExpression">The <see cref="SqlConstantExpression" /> for which to generate SQL.</param>
    protected override Expression VisitSqlConstant(SqlConstantExpression sqlConstantExpression)
    {
        // Certain JSON functions (e.g. JSON_MODIFY()) accept a JSONPATH argument - this is (currently) flown here as a
        // SqlConstantExpression over IReadOnlyList<PathSegment>. Render that to a string here.
        if (sqlConstantExpression is { Value: IReadOnlyList<PathSegment> path })
        {
            GenerateJsonPath(path);
            return sqlConstantExpression;
        }

        return base.VisitSqlConstant(sqlConstantExpression);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitSqlFunction(SqlFunctionExpression sqlFunctionExpression)
    {
        switch (sqlFunctionExpression)
        {
            case { IsBuiltIn: true, Arguments: not null }
                when string.Equals(sqlFunctionExpression.Name, "COALESCE", StringComparison.OrdinalIgnoreCase):
            {
                var type = sqlFunctionExpression.Type;
                var typeMapping = sqlFunctionExpression.TypeMapping;
                var defaultTypeMapping = _typeMappingSource.FindMapping(type);

                // ISNULL always return a value having the same type as its first
                // argument. Ideally we would convert the argument to have the
                // desired type and type mapping, but currently EFCore has some
                // trouble in computing types of non-homogeneous expressions
                // (tracked in https://github.com/dotnet/efcore/issues/15586). To
                // stay on the safe side we only use ISNULL if:
                //  - all sub-expressions have the same type as the expression
                //  - all sub-expressions have the same type mapping as the expression
                //  - the expression is using the default type mapping (combined
                //    with the two above, this implies that all of the expressions
                //    are using the default type mapping of the type)
                if (defaultTypeMapping == typeMapping
                    && sqlFunctionExpression.Arguments.All(a => a.Type == type && a.TypeMapping == typeMapping))
                {
                    var head = sqlFunctionExpression.Arguments[0];
                    sqlFunctionExpression = (SqlFunctionExpression)sqlFunctionExpression
                        .Arguments
                        .Skip(1)
                        .Aggregate(
                            head, (l, r) => new SqlFunctionExpression(
                                "ISNULL",
                                arguments: [l, r],
                                nullable: true,
                                argumentsPropagateNullability: [false, false],
                                sqlFunctionExpression.Type,
                                sqlFunctionExpression.TypeMapping
                            ));
                }

                return base.VisitSqlFunction(sqlFunctionExpression);
            }

            case SqlServerJsonObjectExpression jsonObject:
            {
                Sql.Append("JSON_OBJECT(");

                for (var i = 0; i < jsonObject.PropertyNames.Count; i++)
                {
                    if (i > 0)
                    {
                        Sql.Append(", ");
                    }

                    Sql.Append("'").Append(jsonObject.PropertyNames[i]).Append("': ");
                    Visit(jsonObject.Arguments![i]);
                }

                Sql.Append(")");

                return sqlFunctionExpression;
            }

            // SQL Server 2025 modify method (https://learn.microsoft.com/sql/t-sql/data-types/json-data-type#modify-method)
            // We get here only from within UPDATE setters.
            // We generate the syntax here manually rather than just using the regular function visitation logic since
            // the JSON column (function instance) needs to be rendered *without* the column, unlike elsewhere.
            case
            {
                Name: "modify",
                IsBuiltIn: true,
                Instance: ColumnExpression { TypeMapping.StoreType: "json" } jsonColumn,
                Arguments: [SqlConstantExpression { Value: IReadOnlyList<PathSegment> jsonPath }, var item]
            }:
            {
                Sql
                    .Append(_sqlGenerationHelper.DelimitIdentifier(jsonColumn.Name))
                    .Append(".modify(");
                GenerateJsonPath(jsonPath);
                Sql.Append(", ");
                Visit(item);
                Sql.Append(")");

                return sqlFunctionExpression;
            }
        }

        return base.VisitSqlFunction(sqlFunctionExpression);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override void GenerateValues(ValuesExpression valuesExpression)
    {
        if (valuesExpression.RowValues is null)
        {
            throw new UnreachableException();
        }

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
    protected override void GenerateFrom(SelectExpression selectExpression)
    {
        // SQL Server always requires column names to be specified in table subqueries, as opposed to e.g. scalar subqueries (this isn't
        // a requirement in other databases). So we must use visitor state to track whether we're (directly) within a table subquery, and
        // generate "1 AS empty" instead of just "1".
        _withinTable = true;
        base.GenerateFrom(selectExpression);
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

        // Hack: we currently use JsonScalarExpression to represent both JSON_VALUE and JSON_QUERY in the SQL tree
        // (see #36392), so we need to differentiate between the two here.
        // We use JSON_QUERY() to project out sub-documents, so either when the result is a structural type,
        // or when it is a primitive collection (array).
        var jsonQuery = jsonScalarExpression.TypeMapping is SqlServerStructuralJsonTypeMapping
            || jsonScalarExpression.TypeMapping?.ElementTypeMapping is not null;

        // SQL Server 2025 introduced the RETURNING clause for JSON_VALUE: JSON_VALUE(json, '$.foo' RETURNING int).
        // This is better than adding a cast, as this:
        // 1. Allows us to get the desired type directly (potentially more efficient, possibly index usage too)
        // 2. Supports big strings (otherwise JSON_VALUE always returns nvarchar(4000))
        // 3. Can do JSON-specific decoding (e.g. base64 for varbinary)
        // Note that RETURNING is only (currently) supported over the json type (not nvarchar(max)).
        // Note that we don't need to check the compatibility level - if the json type is being used, then RETURNING is supported.
        var useJsonValueReturningClause = !jsonQuery
            && jsonScalarExpression.Json.TypeMapping?.StoreType is "json"
            // The following types aren't supported by the JSON_VALUE() RETURNING clause (#36627).
            // Note that for varbinary we already transform the JSON_VALUE() into OPENJSON() earlier, in SqlServerJsonPostprocessor.
            && jsonScalarExpression.TypeMapping?.StoreType.ToLower(CultureInfo.InvariantCulture)
                is not ("uniqueidentifier" or "geometry" or "geography" or "datetime");

        // For JSON_VALUE(), if we can use the RETURNING clause, always do that.
        // Otherwise, JSON_VALUE always returns nvarchar(4000) (https://learn.microsoft.com/sql/t-sql/functions/json-value-transact-sql),
        // so we cast the result to the expected type - except if it's a string (since the cast interferes with indexes over
        // the JSON property).
        var useWrappingCast = !jsonQuery && !useJsonValueReturningClause && jsonScalarExpression.TypeMapping is not StringTypeMapping;

        if (jsonQuery)
        {
            Sql.Append("JSON_QUERY(");
        }
        else
        {
            if (useWrappingCast)
            {
                Sql.Append("CAST(");
            }

            Sql.Append("JSON_VALUE(");
        }

        Visit(jsonScalarExpression.Json);

        Sql.Append(", ");
        GenerateJsonPath(jsonScalarExpression.Path);

        if (useJsonValueReturningClause)
        {
            Sql.Append(" RETURNING ");
            Sql.Append(jsonScalarExpression.TypeMapping!.StoreType);
        }

        Sql.Append(")");

        if (useWrappingCast)
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
                case { PropertyName: { } propertyName }:
                    Sql.Append(".").Append(Dependencies.SqlGenerationHelper.DelimitJsonPathElement(propertyName));
                    break;

                case { ArrayIndex: { } arrayIndex }:
                    Sql.Append("[");

                    // JSON functions such as JSON_VALUE only support arbitrary expressions for the path parameter in SQL Server 2017 and
                    // above; before that, arguments must be constant strings.
                    if (arrayIndex is SqlConstantExpression)
                    {
                        Visit(arrayIndex);
                    }
                    else
                    {
                        switch (_sqlServerSingletonOptions.EngineType)
                        {
                            case SqlServerEngineType.SqlServer when _sqlServerSingletonOptions.SqlServerCompatibilityLevel >= 140:
                            case SqlServerEngineType.AzureSql when _sqlServerSingletonOptions.AzureSqlCompatibilityLevel >= 140:
                            case SqlServerEngineType.AzureSynapse:
                                Sql.Append("' + CAST(");
                                Visit(arrayIndex);
                                Sql.Append(" AS ");
                                Sql.Append(_typeMappingSource.GetMapping(typeof(string)).StoreType);
                                Sql.Append(") + '");
                                break;
                            case SqlServerEngineType.SqlServer:
                                throw new InvalidOperationException(
                                    SqlServerStrings.JsonValuePathExpressionsNotSupported(
                                        _sqlServerSingletonOptions.SqlServerCompatibilityLevel));
                            case SqlServerEngineType.AzureSql:
                                throw new InvalidOperationException(
                                    SqlServerStrings.JsonValuePathExpressionsNotSupported(
                                        _sqlServerSingletonOptions.AzureSqlCompatibilityLevel));
                        }
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

        Visit(openJsonExpression.Json);

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
                ExpressionType.Add => (700, true),
                ExpressionType.Subtract => (700, false),
                ExpressionType.And => (700, true),
                ExpressionType.Or => (700, true),
                ExpressionType.ExclusiveOr => (700, true),
                ExpressionType.LeftShift => (700, true),
                ExpressionType.RightShift => (700, true),
                ExpressionType.LessThan => (500, false),
                ExpressionType.LessThanOrEqual => (500, false),
                ExpressionType.GreaterThan => (500, false),
                ExpressionType.GreaterThanOrEqual => (500, false),
                ExpressionType.Equal => (500, false),
                ExpressionType.NotEqual => (500, false),
                ExpressionType.AndAlso => (200, true),
                ExpressionType.OrElse => (100, true),

                _ => default,
            },

            SqlUnaryExpression sqlUnaryExpression => sqlUnaryExpression.OperatorType switch
            {
                ExpressionType.Convert => (1300, false),
                ExpressionType.OnesComplement => (1200, false),
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
