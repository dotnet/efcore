// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Sqlite.Query.SqlExpressions.Internal;

namespace Microsoft.EntityFrameworkCore.Sqlite.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class SqliteQuerySqlGenerator : QuerySqlGenerator
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqliteQuerySqlGenerator(QuerySqlGeneratorDependencies dependencies)
        : base(dependencies)
    {
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
        // Sqlite doesn't support parentheses around set operation operands
        => Visit(operand);

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

        Visit(jsonEachExpression.JsonExpression);

        var path = jsonEachExpression.Path;

        if (path is not null)
        {
            Sql.Append(", ");

            // Note the difference with the JSONPATH rendering in VisitJsonScalar below, where we take advantage of SQLite's ->> operator
            // (we can't do that here).
            Sql.Append("'$");

            var inJsonpathString = true;

            for (var i = 0; i < path.Count; i++)
            {
                switch (path[i])
                {
                    case { PropertyName: string propertyName }:
                        Sql.Append(".").Append(propertyName);
                        break;

                    case { ArrayIndex: SqlExpression arrayIndex }:
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

            if (inJsonpathString)
            {
                Sql.Append("'");
            }
        }

        Sql.Append(")");

        Sql.Append(AliasSeparator).Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(jsonEachExpression.Alias));
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
            var pathSegment = path[i];
            var isLast = i == path.Count - 1;

            switch (pathSegment)
            {
                case { PropertyName: string propertyName }:
                    if (inJsonpathString)
                    {
                        Sql.Append(".").Append(propertyName);
                        continue;
                    }

                    Sql.Append(" ->> ");

                    // No need to start a $. JSONPATH string if we're the last segment or the next segment isn't a constant
                    if (isLast || path[i + 1] is { ArrayIndex: not null and not SqlConstantExpression })
                    {
                        Sql.Append("'").Append(propertyName).Append("'");
                        continue;
                    }

                    Sql.Append("'$.").Append(propertyName);
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

                    Check.DebugAssert(pathSegment.ArrayIndex is not null, "pathSegment.ArrayIndex is not null");

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
