// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Sqlite.Storage.Internal;

namespace Microsoft.EntityFrameworkCore.Sqlite.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class SqliteQuerySqlGenerator(QuerySqlGeneratorDependencies dependencies) : QuerySqlGenerator(dependencies)
{
    /// <inheritdoc />
    protected override void GenerateRootCommand(Expression queryExpression)
    {
        if (queryExpression is not SelectExpression { Predicate: ColumnExpression flag, Tables: [TableExpression table] } select
            || table.FindAnnotation(SqliteParameterBasedSqlProcessor.NonCoveringBooleanFilter) == null)
        {
            base.GenerateRootCommand(queryExpression);
            return;
        }

        GenerateTagsHeaderComment(select.Tags);
        var helper = Dependencies.SqlGenerationHelper;
        var name = "ef_density";
        while (name.Equals(table.Name, StringComparison.OrdinalIgnoreCase) || name.Equals(table.Alias, StringComparison.OrdinalIgnoreCase))
        {
            name += "_";
        }

        var gate = helper.DelimitIdentifier(name);
        var source = helper.DelimitIdentifier(table.Name, table.Schema);
        var column = helper.DelimitIdentifier(flag.Name);
        if (Sql is SqliteRelationalCommandBuilderFactory.Builder builder)
        {
            // Only sample density if the Boolean index can skip more than 1,024 zero or NULL entries.
            // Both queries independently evaluate the original predicate if the table changes between reads.
            var probe = Dependencies.RelationalCommandBuilderFactory.Create();
            if (select.Tags.Count > 0)
            {
                // The builder contains only the query tag header at this point.
                probe.Append(builder.ToString());
            }

            builder.IndexProbe = probe.Append("SELECT 1 FROM ").Append(source).Append(" WHERE ").Append(column).AppendLine(" = 0")
                .Append("UNION ALL SELECT 1 FROM ").Append(source).Append(" WHERE ").Append(column).Append(" IS NULL LIMIT 1 OFFSET 1024")
                .Build();
            builder.ScanQuery = new QuerySqlGenerator(Dependencies).GetCommand(select);
        }

        // Estimate density with bounded table reads so streaming one result never requires
        // counting all rows or walking a table-sized fraction of the Boolean index.
        Sql.Append("WITH ").Append(gate).AppendLine("(dense) AS (");
        if (table.Table is ITable { PrimaryKey.Columns: [{ StoreType: "INTEGER" } key] })
        {
            // Sample across the key range so a cluster at one position does not decide the plan.
            var id = helper.DelimitIdentifier(key.Name);
            Sql.AppendLine("    SELECT SUM((SELECT COUNT(*) FROM (")
                .Append("        SELECT ").Append(column).Append(" FROM ").Append(source).Append(" WHERE ").Append(id).AppendLine(" >= (")
                .Append("            (SELECT MIN(").Append(id).Append(") FROM ").Append(source).Append(") * ((16 - ")
                .Append(gate).AppendLine(".column1) / 16.0)")
                .Append("            + (SELECT MAX(").Append(id).Append(") FROM ").Append(source).Append(") * (")
                .Append(gate).AppendLine(".column1 / 16.0))")
                .Append("        ORDER BY ").Append(id).Append(" LIMIT 100) WHERE ").Append(column).AppendLine(")) > 100")
                .Append("    FROM (VALUES (1), (3), (5), (7), (9), (11), (13), (15)) AS ").Append(gate).AppendLine(")");
        }
        else
        {
            // Without an integer key, sample in table order rather than in Boolean index order.
            Sql.Append("    SELECT EXISTS (SELECT 1 FROM (SELECT ").Append(column).Append(" FROM ").Append(source)
                .Append(" NOT INDEXED LIMIT 400) WHERE ").Append(column).AppendLine(" LIMIT 1 OFFSET 50))");
        }

        // CROSS JOIN keeps the one-row decision outside the table loop; the inactive arm does no row lookups.
        void GenerateArm(bool dense)
        {
            Sql.Append("SELECT ");
            GenerateProjection(select);
            Sql.AppendLine().Append("FROM ").Append(gate).Append(" CROSS JOIN ");
            Visit(table);
            Sql.AppendLine().Append("WHERE ").Append(dense ? "" : "NOT ").Append(gate).Append(".dense AND ");
            if (!dense)
            {
                Sql.Append("(");
                Visit(flag);
                Sql.Append(" > 0 OR ");
                Visit(flag);
                Sql.Append(" < 0) AND ");
            }

            Visit(flag);
        }

        GenerateArm(dense: false);
        Sql.AppendLine().AppendLine("UNION ALL");
        GenerateArm(dense: true);
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
            case GlobExpression globExpression:
                GenerateGlob(globExpression);
                return extensionExpression;

            case RegexpExpression regexpExpression:
                GenerateRegexp(regexpExpression);
                return extensionExpression;

            case JsonEachExpression jsonEachExpression:
                GenerateJsonEach(jsonEachExpression);
                return extensionExpression;

            case SqliteAggregateFunctionExpression aggregateFunctionExpression:
                GenerateAggregateFunction(aggregateFunctionExpression);
                return extensionExpression;

            case MergeColumnReferenceExpression mergeColumnReference:
                if (mergeColumnReference.FromExcluded)
                {
                    Sql.Append("excluded.");
                }

                Sql.Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(mergeColumnReference.ColumnName));
                return extensionExpression;

            default:
                return base.VisitExtension(extensionExpression);
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override void VisitMerge(MergeExpression mergeExpression)
    {
        var helper = Dependencies.SqlGenerationHelper;

        Sql.Append("INSERT INTO ")
            .Append(helper.DelimitIdentifier(mergeExpression.Table.Name, mergeExpression.Table.Schema));

        // A target table named "excluded" would shadow the excluded pseudo-row in DO UPDATE, so give it another name.
        if (string.Equals(mergeExpression.Table.Name, "excluded", StringComparison.OrdinalIgnoreCase))
        {
            var alias = mergeExpression.Table.Alias is { } tableAlias
                && !string.Equals(tableAlias, "excluded", StringComparison.OrdinalIgnoreCase)
                    ? tableAlias
                    : "t";
            Sql.Append(AliasSeparator).Append(helper.DelimitIdentifier(alias));
        }

        Sql.Append(" (");
        AppendDelimitedColumns(mergeExpression.InsertColumns);
        Sql.AppendLine(")");

        if (mergeExpression.SourceRows.Count == 0)
        {
            // SQL VALUES doesn't allow an empty set. With no source rows there is nothing to merge, so emit an
            // INSERT ... SELECT ... WHERE 0 that inserts (and returns) nothing, leaving the affected-row count at 0.
            Sql.Append("SELECT ");
            for (var i = 0; i < mergeExpression.InsertColumns.Count; i++)
            {
                if (i > 0)
                {
                    Sql.Append(", ");
                }

                Sql.Append("NULL");
            }

            Sql.Append(" WHERE 0");
        }
        else
        {
            Sql.Append("VALUES ");
            for (var r = 0; r < mergeExpression.SourceRows.Count; r++)
            {
                if (r > 0)
                {
                    Sql.Append(",").AppendLine();
                }

                Visit(mergeExpression.SourceRows[r]);
            }

            Sql.AppendLine().Append("ON CONFLICT (");
            AppendDelimitedColumns(mergeExpression.ConflictColumns);
            Sql.Append(")");

            if (mergeExpression.UpdateSetters is null)
            {
                Sql.Append(" DO NOTHING");
            }
            else
            {
                Sql.AppendLine(" DO UPDATE SET");
                for (var i = 0; i < mergeExpression.UpdateSetters.Count; i++)
                {
                    if (i > 0)
                    {
                        Sql.Append(",").AppendLine();
                    }

                    var setter = mergeExpression.UpdateSetters[i];
                    Sql.Append(helper.DelimitIdentifier(setter.ColumnName)).Append(" = ");
                    Visit(setter.Value);
                }
            }
        }

        if (mergeExpression.Returning is not null)
        {
            Sql.AppendLine().Append("RETURNING ");
            for (var i = 0; i < mergeExpression.Returning.Count; i++)
            {
                if (i > 0)
                {
                    Sql.Append(", ");
                }

                Visit(mergeExpression.Returning[i].Expression);
            }
        }

        Sql.Append(helper.StatementTerminator);

        void AppendDelimitedColumns(IReadOnlyList<string> columns)
        {
            for (var i = 0; i < columns.Count; i++)
            {
                if (i > 0)
                {
                    Sql.Append(", ");
                }

                Sql.Append(helper.DelimitIdentifier(columns[i]));
            }
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override string GetOperator(SqlBinaryExpression binaryExpression)
        => binaryExpression.OperatorType == ExpressionType.Add
            && binaryExpression.Type == typeof(string)
                ? " || "
                : base.GetOperator(binaryExpression);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override void GenerateLimitOffset(SelectExpression selectExpression)
    {
        if (selectExpression.Limit != null
            || selectExpression.Offset != null)
        {
            Sql.AppendLine()
                .Append("LIMIT ");

            Visit(
                selectExpression.Limit
                ?? new SqlConstantExpression(-1, selectExpression.Offset!.TypeMapping));

            if (selectExpression.Offset != null)
            {
                Sql.Append(" OFFSET ");

                Visit(selectExpression.Offset);
            }
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override void GenerateSetOperationOperand(SetOperationBase setOperation, SelectExpression operand)
    {
        // Most databases support parentheses around set operations to determine evaluation order, but SQLite does not;
        // however, we can instead wrap the nested set operation in a SELECT * FROM () to achieve the same effect.
        // The following is a copy-paste of the base implementation from QuerySqlGenerator, adding the SELECT.

        if (
            // INTERSECT has higher precedence over UNION and EXCEPT, but otherwise evaluation is left-to-right.
            // To preserve evaluation order, add parentheses whenever a set operation is nested within a different set operation
            // - including different distinctness.
            // In addition, EXCEPT is non-commutative (unlike UNION/INTERSECT), so add parentheses for that case too (see #36105).
            (TryUnwrapBareSetOperation(operand, out var nestedSetOperation)
                && (nestedSetOperation is ExceptExpression
                    || nestedSetOperation.GetType() != setOperation.GetType()
                    || nestedSetOperation.IsDistinct != setOperation.IsDistinct))
            ||
            // ValuesExpression with multiple rows uses UNION ALL by default.
            // We wrap it in a SELECT * FROM () to ensure that the rows are treated as a single set operation.
            (operand is { Tables: [ValuesExpression { RowValues.Count: > 1 }] }))
        {
            Sql.AppendLine("SELECT * FROM (");

            using (Sql.Indent())
            {
                Visit(operand);
            }

            Sql.AppendLine().Append(")");
        }
        else
        {
            Visit(operand);
        }

        static bool TryUnwrapBareSetOperation(SelectExpression selectExpression, [NotNullWhen(true)] out SetOperationBase? setOperation)
        {
            if (selectExpression is
                {
                    Tables: [SetOperationBase s],
                    Predicate: null,
                    Orderings: [],
                    Offset: null,
                    Limit: null,
                    IsDistinct: false,
                    Having: null,
                    GroupBy: []
                }
                && selectExpression.Projection.Count == s.Source1.Projection.Count
                && selectExpression.Projection.Select((pe, index) => pe.Expression is ColumnExpression column
                        && column.TableAlias == s.Alias
                        && column.Name == s.Source1.Projection[index].Alias)
                    .All(e => e))
            {
                setOperation = s;
                return true;
            }

            setOperation = null;
            return false;
        }
    }

    private void GenerateGlob(GlobExpression globExpression, bool negated = false)
    {
        Visit(globExpression.Match);

        if (negated)
        {
            Sql.Append(" NOT");
        }

        Sql.Append(" GLOB ");
        Visit(globExpression.Pattern);
    }

    private void GenerateRegexp(RegexpExpression regexpExpression, bool negated = false)
    {
        Visit(regexpExpression.Match);

        if (negated)
        {
            Sql.Append(" NOT");
        }

        Sql.Append(" REGEXP ");
        Visit(regexpExpression.Pattern);
    }

    private void GenerateAggregateFunction(SqliteAggregateFunctionExpression aggregateFunctionExpression)
    {
        Sql.Append(aggregateFunctionExpression.Name).Append("(");

        for (var i = 0; i < aggregateFunctionExpression.Arguments.Count; i++)
        {
            if (i > 0)
            {
                Sql.Append(", ");
            }

            Visit(aggregateFunctionExpression.Arguments[i]);
        }

        // Unlike SQL Server's "WITHIN GROUP (ORDER BY ...)", SQLite renders the ordering inside the function
        // parentheses: group_concat(value, separator ORDER BY ...). Supported since SQLite 3.44.0.
        if (aggregateFunctionExpression.Orderings.Count > 0)
        {
            Sql.Append(" ORDER BY ");

            for (var i = 0; i < aggregateFunctionExpression.Orderings.Count; i++)
            {
                if (i > 0)
                {
                    Sql.Append(", ");
                }

                Visit(aggregateFunctionExpression.Orderings[i]);
            }
        }

        Sql.Append(")");
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual void GenerateJsonEach(JsonEachExpression jsonEachExpression)
    {
        // json_each docs: https://www.sqlite.org/json1.html#jeach

        // json_each is a regular table-valued function; however, since it accepts an (optional) JSONPATH argument - which we represent
        // as IReadOnlyList<PathSegment>, and that can only be rendered as a string here in the QuerySqlGenerator, we have a special
        // expression type for it.
        Sql.Append("json_each(");

        Visit(jsonEachExpression.Json);

        var path = jsonEachExpression.Path;

        if (path is not null)
        {
            Sql.Append(", ");

            GenerateJsonPath(path);
        }

        Sql.Append(")");

        Sql.Append(AliasSeparator).Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(jsonEachExpression.Alias));
    }

    private void GenerateJsonPath(IReadOnlyList<PathSegment> path)
    {
        Sql.Append("'$");

        for (var i = 0; i < path.Count; i++)
        {
            switch (path[i])
            {
                case { PropertyName: { } propertyName }:
                    Sql.Append(".").Append(propertyName);
                    break;

                case { ArrayIndex: { } arrayIndex }:
                    Sql.Append("[");

                    if (arrayIndex is SqlConstantExpression)
                    {
                        Visit(arrayIndex);
                    }
                    else
                    {
                        Sql.Append("' || ");
                        Visit(arrayIndex);
                        Sql.Append(" || '");
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
    ///     Generates SQL for a constant.
    /// </summary>
    /// <param name="sqlConstantExpression">The <see cref="SqlConstantExpression" /> for which to generate SQL.</param>
    protected override Expression VisitSqlConstant(SqlConstantExpression sqlConstantExpression)
    {
        // Certain JSON functions (e.g. json_set()) accept a JSONPATH argument - this is (currently) flown here as a SqlConstantExpression
        // over IReadOnlyList<PathSegment>. Render that to a string here.
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
    protected override Expression VisitJsonScalar(JsonScalarExpression jsonScalarExpression)
    {
        Visit(jsonScalarExpression.Json);

        // TODO: Stop producing empty JsonScalarExpressions, #30768
        var path = jsonScalarExpression.Path;
        if (path.Count == 0)
        {
            return jsonScalarExpression;
        }

        var inJsonpathString = false;

        for (var i = 0; i < path.Count; i++)
        {
            // Note that we don't use GenerateJsonPath() to generate the JSONPATH string here, since we take advantage of SQLite's ->> operator
            // for JsonScalarExpression.
            var pathSegment = path[i];
            var isLast = i == path.Count - 1;

            switch (pathSegment)
            {
                case { PropertyName: { } propertyName }:
                    if (inJsonpathString)
                    {
                        Sql.Append(".").Append(Dependencies.SqlGenerationHelper.DelimitJsonPathElement(propertyName));
                        continue;
                    }

                    Sql.Append(" ->> ");

                    var delimitedPropertyName = Dependencies.SqlGenerationHelper.DelimitJsonPathElement(propertyName);

                    // No need to start a $. JSONPATH string if we're the last segment or the next segment isn't a constant
                    if (isLast || path[i + 1] is { ArrayIndex: not null and not SqlConstantExpression })
                    {
                        // A plain label is matched literally by SQLite, so a name that had to be quoted needs a full JSONPATH;
                        // otherwise the quotes become part of the key being looked up.
                        Sql.Append(delimitedPropertyName == propertyName ? "'" : "'$.").Append(delimitedPropertyName).Append("'");
                        continue;
                    }

                    Sql.Append("'$.").Append(delimitedPropertyName);
                    inJsonpathString = true;
                    continue;

                case { ArrayIndex: SqlConstantExpression arrayIndex }:
                    if (inJsonpathString)
                    {
                        Sql.Append("[");
                        Visit(pathSegment.ArrayIndex);
                        Sql.Append("]");
                        continue;
                    }

                    Sql.Append(" ->> ");

                    // No need to start a $. JSONPATH string if we're the last segment or the next segment isn't a constant
                    if (isLast || path[i + 1] is { ArrayIndex: not null and not SqlConstantExpression })
                    {
                        Visit(arrayIndex);
                        continue;
                    }

                    Sql.Append("'$[");
                    Visit(arrayIndex);
                    Sql.Append("]");
                    inJsonpathString = true;
                    continue;

                default:
                    if (inJsonpathString)
                    {
                        Sql.Append("'");
                        inJsonpathString = false;
                    }

                    Sql.Append(" ->> ");

                    Check.DebugAssert(pathSegment.ArrayIndex is not null);

                    var requiresParentheses = RequiresParentheses(jsonScalarExpression, pathSegment.ArrayIndex);
                    if (requiresParentheses)
                    {
                        Sql.Append("(");
                    }

                    Visit(pathSegment.ArrayIndex);

                    if (requiresParentheses)
                    {
                        Sql.Append(")");
                    }

                    continue;
            }
        }

        if (inJsonpathString)
        {
            Sql.Append("'");
        }

        return jsonScalarExpression;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitSqlUnary(SqlUnaryExpression sqlUnaryExpression)
    {
        switch (sqlUnaryExpression.OperatorType)
        {
            case ExpressionType.Convert:
                if (sqlUnaryExpression.Operand.Type == typeof(char)
                    && sqlUnaryExpression.Type.IsInteger())
                {
                    Sql.Append("unicode(");
                    Visit(sqlUnaryExpression.Operand);
                    Sql.Append(")");

                    return sqlUnaryExpression;
                }

                if (sqlUnaryExpression.Operand.Type.IsInteger()
                    && sqlUnaryExpression.Type == typeof(char))
                {
                    Sql.Append("char(");
                    Visit(sqlUnaryExpression.Operand);
                    Sql.Append(")");

                    return sqlUnaryExpression;
                }

                goto default;

            case ExpressionType.Not when sqlUnaryExpression.Type == typeof(bool):
                switch (sqlUnaryExpression.Operand)
                {
                    case GlobExpression globExpression:
                        GenerateGlob(globExpression, negated: true);
                        return sqlUnaryExpression;

                    case RegexpExpression regexpExpression:
                        GenerateRegexp(regexpExpression, negated: true);
                        return sqlUnaryExpression;
                }

                goto default;

            default:
                return base.VisitSqlUnary(sqlUnaryExpression);
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
        // See https://sqlite.org/lang_expr.html#operators_and_parse_affecting_attributes
        (precedence, isAssociative) = expression switch
        {
            SqlBinaryExpression sqlBinaryExpression => sqlBinaryExpression.OperatorType switch
            {
                ExpressionType.Multiply => (900, true),
                ExpressionType.Divide => (900, false),
                ExpressionType.Modulo => (900, false),
                ExpressionType.Add when sqlBinaryExpression.Type == typeof(string) => (1000, true),
                ExpressionType.Add when sqlBinaryExpression.Type != typeof(string) => (800, true),
                ExpressionType.Subtract => (800, false),
                ExpressionType.And => (600, true),
                ExpressionType.Or => (600, true),
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
                ExpressionType.Not when sqlUnaryExpression.Type != typeof(bool) => (1200, false),
                ExpressionType.Negate => (1200, false),
                ExpressionType.Equal => (500, false), // IS NULL
                ExpressionType.NotEqual => (500, false), // IS NOT NULL
                ExpressionType.Not when sqlUnaryExpression.Type == typeof(bool) => (300, false),

                _ => default,
            },

            CollateExpression => (1100, false),
            LikeExpression => (500, false),
            JsonScalarExpression => (1000, true),

            _ => default,
        };

        return precedence != default;
    }
}
