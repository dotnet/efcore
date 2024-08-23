// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Cosmos.Internal;
using Microsoft.EntityFrameworkCore.Cosmos.Storage.Internal;

namespace Microsoft.EntityFrameworkCore.Cosmos.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class CosmosQuerySqlGenerator(ITypeMappingSource typeMappingSource) : SqlExpressionVisitor
{
    private readonly IndentedStringBuilder _sqlBuilder = new();
    private IReadOnlyDictionary<string, object> _parameterValues = null!;
    private List<SqlParameter> _sqlParameters = null!;
    private ParameterNameGenerator _parameterNameGenerator = null!;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual CosmosSqlQuery GetSqlQuery(
        SelectExpression selectExpression,
        IReadOnlyDictionary<string, object> parameterValues)
    {
        _sqlBuilder.Clear();
        _parameterValues = parameterValues;
        _sqlParameters = [];
        _parameterNameGenerator = new ParameterNameGenerator();

        Visit(selectExpression);

        return new CosmosSqlQuery(_sqlBuilder.ToString(), _sqlParameters);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitEntityProjection(EntityProjectionExpression entityProjectionExpression)
    {
        Visit(entityProjectionExpression.Object);

        return entityProjectionExpression;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitExists(ExistsExpression existsExpression)
    {
        _sqlBuilder.AppendLine("EXISTS (");

        using (_sqlBuilder.Indent())
        {
            Visit(existsExpression.Subquery);
        }

        _sqlBuilder.Append(")");

        return existsExpression;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitObjectArray(ObjectArrayExpression objectArrayExpression)
    {
        GenerateArray(objectArrayExpression.Subquery);
        return objectArrayExpression;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitScalarArray(ScalarArrayExpression scalarArrayExpression)
    {
        GenerateArray(scalarArrayExpression.Subquery);
        return scalarArrayExpression;
    }

    private void GenerateArray(SelectExpression subquery)
    {
        _sqlBuilder.AppendLine("ARRAY(");

        using (_sqlBuilder.Indent())
        {
            Visit(subquery);
        }

        _sqlBuilder.Append(")");
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitObjectArrayAccess(ObjectArrayAccessExpression objectArrayAccessExpression)
    {
        Visit(objectArrayAccessExpression.Object);

        _sqlBuilder
            .Append("[\"")
            .Append(objectArrayAccessExpression.PropertyName)
            .Append("\"]");

        return objectArrayAccessExpression;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitObjectArrayIndex(ObjectArrayIndexExpression objectArrayIndexExpression)
    {
        Visit(objectArrayIndexExpression.Array);
        _sqlBuilder.Append("[");
        Visit(objectArrayIndexExpression.Index);
        _sqlBuilder.Append("]");

        return objectArrayIndexExpression;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitScalarAccess(ScalarAccessExpression scalarAccessExpression)
    {
        Visit(scalarAccessExpression.Object);

        // TODO: Remove check once __jObject is translated to the access root in a better fashion.
        // See issue #17670 and related issue #14121.
        if (scalarAccessExpression.PropertyName.Length > 0)
        {
            _sqlBuilder
                .Append("[\"")
                .Append(scalarAccessExpression.PropertyName)
                .Append("\"]");
        }

        return scalarAccessExpression;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitObjectAccess(ObjectAccessExpression objectAccessExpression)
    {
        Visit(objectAccessExpression.Object);

        _sqlBuilder
            .Append("[\"")
            .Append(objectAccessExpression.PropertyName)
            .Append("\"]");

        return objectAccessExpression;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitScalarSubquery(ScalarSubqueryExpression scalarSubqueryExpression)
    {
        _sqlBuilder.AppendLine("(");
        using (_sqlBuilder.Indent())
        {
            Visit(scalarSubqueryExpression.Subquery);
        }

        _sqlBuilder.Append(")");

        return scalarSubqueryExpression;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitProjection(ProjectionExpression projectionExpression)
    {
        GenerateProjection(projectionExpression, objectProjectionStyle: false);
        return projectionExpression;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    private void GenerateProjection(ProjectionExpression projectionExpression, bool objectProjectionStyle)
    {
        // If the SELECT has a single projection with IsValueProjection, prepend the VALUE keyword (without VALUE, Cosmos projects a JSON
        // object containing the value).
        if (projectionExpression.IsValueProjection)
        {
            _sqlBuilder.Append("VALUE ");
            Visit(projectionExpression.Expression);
            return;
        }

        if (objectProjectionStyle)
        {
            _sqlBuilder.Append('"').Append(projectionExpression.Alias).Append("\" : ");
        }

        Visit(projectionExpression.Expression);

        if (!objectProjectionStyle
            && !string.IsNullOrEmpty(projectionExpression.Alias)
            && projectionExpression.Alias != projectionExpression.Name)
        {
            _sqlBuilder.Append(" AS " + projectionExpression.Alias);
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitObjectReference(ObjectReferenceExpression objectReferenceExpression)
    {
        _sqlBuilder.Append(objectReferenceExpression.Name);

        return objectReferenceExpression;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitValueReference(ScalarReferenceExpression scalarReferenceExpression)
    {
        _sqlBuilder.Append(scalarReferenceExpression.Name);

        return scalarReferenceExpression;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitSelect(SelectExpression selectExpression)
    {
        _sqlBuilder.Append("SELECT ");

        if (selectExpression.IsDistinct)
        {
            _sqlBuilder.Append("DISTINCT ");
        }

        if (selectExpression.Projection is { Count: > 0 } projections)
        {
            Check.DebugAssert(
                projections.Count == 1 || !projections.Any(p => p.IsValueProjection),
                "Multiple projections with IsValueProjection");

            // If there's only one projection, we simply project it directly (SELECT VALUE c["Id"]); this happens in GenerateProjection().
            // Otherwise, we'll project a JSON object wrapping the multiple projections. Cosmos has two syntaxes for doing so:
            // 1. Project out a JSON object as a value (SELECT VALUE { 'a': a, 'b': b }), or
            // 2. Project a set of properties with optional AS+aliases (SELECT 'a' AS a, 'b' AS b)
            // Both methods produce the exact same results; we usually prefer the 1st, but in some cases we use the 2nd.
            if (projections.Count > 1
                && projections.Any(p => !string.IsNullOrEmpty(p.Alias) && p.Alias != p.Name)
                && !projections.Any(p => p.Expression is SqlFunctionExpression)) // Aggregates are not allowed
            {
                _sqlBuilder.AppendLine("VALUE").AppendLine("{").IncrementIndent();
                GenerateList(projections, e => GenerateProjection(e, objectProjectionStyle: true), joinAction: sql => sql.AppendLine(","));
                _sqlBuilder.AppendLine().DecrementIndent().Append("}");
            }
            else
            {
                GenerateList(projections, e => Visit(e));
            }
        }
        else
        {
            _sqlBuilder.Append('1');
        }

        var sources = selectExpression.Sources;
        if (sources.Count > 0)
        {
            _sqlBuilder.AppendLine().Append("FROM ");

            Visit(sources[0]);

            for (var i = 1; i < sources.Count; i++)
            {
                _sqlBuilder.AppendLine().Append("JOIN ");

                Visit(sources[i]);
            }
        }

        if (selectExpression.Predicate != null)
        {
            _sqlBuilder.AppendLine().Append("WHERE ");
            Visit(selectExpression.Predicate);
        }

        if (selectExpression.Orderings.Any())
        {
            _sqlBuilder.AppendLine().Append("ORDER BY ");

            GenerateList(selectExpression.Orderings, e => Visit(e));
        }

        if (selectExpression.Offset != null
            || selectExpression.Limit != null)
        {
            _sqlBuilder.AppendLine().Append("OFFSET ");

            if (selectExpression.Offset != null)
            {
                Visit(selectExpression.Offset);
            }
            else
            {
                _sqlBuilder.Append('0');
            }

            _sqlBuilder.Append(" LIMIT ");

            if (selectExpression.Limit != null)
            {
                Visit(selectExpression.Limit);
            }
            else
            {
                // TODO: See Issue#18923
                throw new InvalidOperationException(CosmosStrings.OffsetRequiresLimit);
            }
        }

        return selectExpression;
    }

    /// <inheritdoc />
    protected override Expression VisitFromSql(FromSqlExpression fromSqlExpression)
    {
        var sql = fromSqlExpression.Sql;

        string[] substitutions;

        switch (fromSqlExpression.Arguments)
        {
            case ParameterExpression { Name: not null } parameterExpression
                when _parameterValues.TryGetValue(parameterExpression.Name, out var parameterValue)
                && parameterValue is object[] parameterValues:
            {
                substitutions = new string[parameterValues.Length];
                for (var i = 0; i < parameterValues.Length; i++)
                {
                    var parameterName = _parameterNameGenerator.GenerateNext();
                    _sqlParameters.Add(new SqlParameter(parameterName, parameterValues[i]));
                    substitutions[i] = parameterName;
                }

                break;
            }

            case ConstantExpression { Value: object[] constantValues }:
            {
                substitutions = new string[constantValues.Length];
                for (var i = 0; i < constantValues.Length; i++)
                {
                    var value = constantValues[i];
                    var typeMapping = typeMappingSource.FindMapping(value.GetType());
                    Check.DebugAssert(typeMapping is not null, "Could not find type mapping for FromSql parameter");
                    substitutions[i] = ((CosmosTypeMapping)typeMapping).GenerateConstant(value);
                }

                break;
            }

            default:
                throw new ArgumentOutOfRangeException(
                    nameof(fromSqlExpression),
                    fromSqlExpression.Arguments,
                    CosmosStrings.InvalidFromSqlArguments(
                        fromSqlExpression.Arguments.GetType(),
                        fromSqlExpression.Arguments is ConstantExpression constantExpression
                            ? constantExpression.Value?.GetType()
                            : null));
        }

        // ReSharper disable once CoVariantArrayConversion
        // InvariantCulture not needed since substitutions are all strings
        sql = string.Format(sql, substitutions);

        _sqlBuilder.AppendLine("(");

        using (_sqlBuilder.Indent())
        {
            _sqlBuilder.AppendLines(sql);
        }

        _sqlBuilder.Append(")");

        return fromSqlExpression;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitOrdering(OrderingExpression orderingExpression)
    {
        Visit(orderingExpression.Expression);

        if (!orderingExpression.IsAscending)
        {
            _sqlBuilder.Append(" DESC");
        }

        return orderingExpression;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitSqlBinary(SqlBinaryExpression sqlBinaryExpression)
    {
        if (sqlBinaryExpression.OperatorType is ExpressionType.ArrayIndex)
        {
            Visit(sqlBinaryExpression.Left);
            _sqlBuilder.Append('[');
            Visit(sqlBinaryExpression.Right);
            _sqlBuilder.Append(']');

            return sqlBinaryExpression;
        }

        var op = sqlBinaryExpression.OperatorType switch
        {
            // Arithmetic
            ExpressionType.Add => " + ",
            ExpressionType.Subtract => " - ",
            ExpressionType.Multiply => " * ",
            ExpressionType.Divide => " / ",
            ExpressionType.Modulo => " % ",

            // Bitwise >>> (zero-fill right shift) not available in C#
            ExpressionType.Or => " | ",
            ExpressionType.And => " & ",
            ExpressionType.ExclusiveOr => " ^ ",
            ExpressionType.LeftShift => " << ",
            ExpressionType.RightShift => " >> ",

            // Logical
            ExpressionType.AndAlso => " AND ",
            ExpressionType.OrElse => " OR ",

            // Comparison
            ExpressionType.Equal => " = ",
            ExpressionType.NotEqual => " != ",
            ExpressionType.GreaterThan => " > ",
            ExpressionType.GreaterThanOrEqual => " >= ",
            ExpressionType.LessThan => " < ",
            ExpressionType.LessThanOrEqual => " <= ",

            // Other
            ExpressionType.Coalesce => " ?? ",

            _ => throw new UnreachableException($"Unsupported unary OperatorType: {sqlBinaryExpression.OperatorType}")
        };

        _sqlBuilder.Append('(');
        Visit(sqlBinaryExpression.Left);

        if (sqlBinaryExpression.OperatorType == ExpressionType.Add
            && sqlBinaryExpression.Left.Type == typeof(string))
        {
            op = " || ";
        }
        else if (sqlBinaryExpression.OperatorType == ExpressionType.ExclusiveOr
                 && sqlBinaryExpression.Type == typeof(bool))
        {
            op = " != ";
        }

        _sqlBuilder.Append(op);

        Visit(sqlBinaryExpression.Right);
        _sqlBuilder.Append(')');

        return sqlBinaryExpression;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitObjectBinary(ObjectBinaryExpression objectBinaryExpression)
    {
        var op = objectBinaryExpression.OperatorType switch
        {
            ExpressionType.Coalesce => " ?? ",

            _ => throw new UnreachableException($"Unsupported unary OperatorType: {objectBinaryExpression.OperatorType}")
        };

        _sqlBuilder.Append('(');
        Visit(objectBinaryExpression.Left);
        _sqlBuilder.Append(op);
        Visit(objectBinaryExpression.Right);
        _sqlBuilder.Append(')');

        return objectBinaryExpression;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitSqlUnary(SqlUnaryExpression sqlUnaryExpression)
    {
        var op = sqlUnaryExpression.OperatorType switch
        {
            ExpressionType.UnaryPlus => "+",
            ExpressionType.Negate => "-",
            ExpressionType.Not => "~",

            _ => throw new UnreachableException($"Unsupported unary OperatorType: {sqlUnaryExpression.OperatorType}")
        };

        if (sqlUnaryExpression.OperatorType == ExpressionType.Not
            && sqlUnaryExpression.Operand.Type == typeof(bool))
        {
            if (sqlUnaryExpression.Operand is InExpression inExpression)
            {
                GenerateIn(inExpression, negated: true);

                return sqlUnaryExpression;
            }

            op = "NOT";
        }

        _sqlBuilder.Append(op);

        _sqlBuilder.Append('(');
        Visit(sqlUnaryExpression.Operand);
        _sqlBuilder.Append(')');

        return sqlUnaryExpression;
    }

    private void GenerateList<T>(
        IReadOnlyList<T> items,
        Action<T> generationAction,
        Action<IndentedStringBuilder>? joinAction = null)
    {
        joinAction ??= (isb => isb.Append(", "));

        for (var i = 0; i < items.Count; i++)
        {
            if (i > 0)
            {
                joinAction(_sqlBuilder);
            }

            generationAction(items[i]);
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitSqlConstant(SqlConstantExpression sqlConstantExpression)
    {
        Check.DebugAssert(sqlConstantExpression.TypeMapping is not null, "SqlConstantExpression without a type mapping");
        _sqlBuilder.Append(((CosmosTypeMapping)sqlConstantExpression.TypeMapping).GenerateConstant(sqlConstantExpression.Value));

        return sqlConstantExpression;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitFragment(FragmentExpression fragmentExpression)
    {
        _sqlBuilder.Append(fragmentExpression.Fragment);

        return fragmentExpression;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitSqlConditional(SqlConditionalExpression sqlConditionalExpression)
    {
        _sqlBuilder.Append('(');
        Visit(sqlConditionalExpression.Test);
        _sqlBuilder.Append(" ? ");
        Visit(sqlConditionalExpression.IfTrue);
        _sqlBuilder.Append(" : ");
        Visit(sqlConditionalExpression.IfFalse);
        _sqlBuilder.Append(')');

        return sqlConditionalExpression;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitSqlParameter(SqlParameterExpression sqlParameterExpression)
    {
        var parameterName = $"@{sqlParameterExpression.Name}";

        if (_sqlParameters.All(sp => sp.Name != parameterName))
        {
            Check.DebugAssert(sqlParameterExpression.TypeMapping is not null, "SqlParameterExpression without a type mapping.");

            _sqlParameters.Add(
                new SqlParameter(
                    parameterName,
                    ((CosmosTypeMapping)sqlParameterExpression.TypeMapping)
                    .GenerateJToken(_parameterValues[sqlParameterExpression.Name])));
        }

        _sqlBuilder.Append(parameterName);

        return sqlParameterExpression;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected sealed override Expression VisitIn(InExpression inExpression)
    {
        GenerateIn(inExpression, negated: false);

        return inExpression;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitArrayConstant(ArrayConstantExpression arrayConstantExpression)
    {
        _sqlBuilder.Append("[");

        var items = arrayConstantExpression.Items;
        for (var i = 0; i < items.Count; i++)
        {
            if (i > 0)
            {
                _sqlBuilder.Append(", ");
            }

            Visit(items[i]);
        }

        _sqlBuilder.Append("]");

        return arrayConstantExpression;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected sealed override Expression VisitSource(SourceExpression sourceExpression)
    {
        // https://learn.microsoft.com/azure/cosmos-db/nosql/query/from
        if (sourceExpression.WithIn)
        {
            if (sourceExpression.Alias is null)
            {
                throw new UnreachableException("Alias cannot be null when WithIn is true");
            }

            _sqlBuilder
                .Append(sourceExpression.Alias)
                .Append(" IN ");

            VisitContainerExpression(sourceExpression.Expression);
        }
        else
        {
            VisitContainerExpression(sourceExpression.Expression);

            if (sourceExpression.Alias is not null)
            {
                _sqlBuilder
                    .Append(" ")
                    .Append(sourceExpression.Alias);
            }
        }

        return sourceExpression;

        void VisitContainerExpression(Expression containerExpression)
        {
            var subquery = containerExpression is SelectExpression;
            var simpleValueProjectionSubquery = containerExpression is SelectExpression
            {
                Sources: [],
                Predicate: null,
                Offset: null,
                Limit: null,
                Orderings: [],
                IsDistinct: false,
                Projection.Count: 1
            };

            if (subquery)
            {
                if (simpleValueProjectionSubquery)
                {
                    _sqlBuilder.Append("(");
                }
                else
                {
                    _sqlBuilder.AppendLine("(").IncrementIndent();
                }
            }

            Visit(sourceExpression.Expression);

            if (subquery)
            {
                if (simpleValueProjectionSubquery)
                {
                    _sqlBuilder.Append(")");
                }
                else
                {
                    _sqlBuilder.DecrementIndent().Append(")");
                }
            }
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual void GenerateIn(InExpression inExpression, bool negated)
    {
        Check.DebugAssert(
            inExpression.ValuesParameter is null,
            "InExpression.ValuesParameter must have been expanded to constants before SQL generation (in "
            + "InExpressionValuesExpandingExpressionVisitor)");
        Check.DebugAssert(inExpression.Values is not null, "Missing Values on InExpression");

        Visit(inExpression.Item);
        _sqlBuilder.Append(negated ? " NOT IN (" : " IN (");
        GenerateList(inExpression.Values, e => Visit(e));
        _sqlBuilder.Append(')');
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitObjectFunction(ObjectFunctionExpression objectFunctionExpression)
    {
        GenerateFunction(objectFunctionExpression.Name, objectFunctionExpression.Arguments);
        return objectFunctionExpression;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitSqlFunction(SqlFunctionExpression sqlFunctionExpression)
    {
        GenerateFunction(sqlFunctionExpression.Name, sqlFunctionExpression.Arguments);
        return sqlFunctionExpression;
    }

    private void GenerateFunction(string name, IReadOnlyList<Expression> arguments)
    {
        _sqlBuilder.Append(name);
        _sqlBuilder.Append('(');
        GenerateList(arguments, e => Visit(e));
        _sqlBuilder.Append(')');
    }

    private sealed class ParameterNameGenerator
    {
        private int _count;

        public string GenerateNext()
            => "@p" + _count++;
    }
}
