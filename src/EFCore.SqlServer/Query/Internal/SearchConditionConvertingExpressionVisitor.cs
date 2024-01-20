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
public class SearchConditionConvertingExpressionVisitor : SqlExpressionVisitor
{
    private bool _isSearchCondition;
    private readonly ISqlExpressionFactory _sqlExpressionFactory;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SearchConditionConvertingExpressionVisitor(
        ISqlExpressionFactory sqlExpressionFactory)
    {
        _sqlExpressionFactory = sqlExpressionFactory;
    }

    private SqlExpression ApplyConversion(SqlExpression sqlExpression, bool condition)
        => _isSearchCondition
            ? ConvertToSearchCondition(sqlExpression, condition)
            : ConvertToValue(sqlExpression, condition);

    private SqlExpression ConvertToSearchCondition(SqlExpression sqlExpression, bool condition)
        => condition
            ? sqlExpression
            : BuildCompareToExpression(sqlExpression);

    private SqlExpression ConvertToValue(SqlExpression sqlExpression, bool condition)
        => condition
            ? _sqlExpressionFactory.Case(
                new[]
                {
                    new CaseWhenClause(
                        SimplifyNegatedBinary(sqlExpression),
                        _sqlExpressionFactory.ApplyDefaultTypeMapping(_sqlExpressionFactory.Constant(true)))
                },
                _sqlExpressionFactory.Constant(false))
            : sqlExpression;

    private SqlExpression BuildCompareToExpression(SqlExpression sqlExpression)
        => sqlExpression is SqlConstantExpression { Value: bool boolValue }
            ? _sqlExpressionFactory.Equal(
                boolValue
                    ? _sqlExpressionFactory.Constant(1)
                    : _sqlExpressionFactory.Constant(0),
                _sqlExpressionFactory.Constant(1))
            : _sqlExpressionFactory.Equal(
                sqlExpression,
                _sqlExpressionFactory.Constant(true));

    private SqlExpression SimplifyNegatedBinary(SqlExpression sqlExpression)
    {
        if (sqlExpression is SqlUnaryExpression { OperatorType: ExpressionType.Not } sqlUnaryExpression
            && sqlUnaryExpression.Type == typeof(bool)
            && sqlUnaryExpression.Operand is SqlBinaryExpression
            {
                OperatorType: ExpressionType.Equal or ExpressionType.NotEqual
            } sqlBinaryOperand)
        {
            if (sqlBinaryOperand.Left.Type == typeof(bool)
                && sqlBinaryOperand.Right.Type == typeof(bool)
                && (sqlBinaryOperand.Left is SqlConstantExpression
                    || sqlBinaryOperand.Right is SqlConstantExpression))
            {
                var constant = sqlBinaryOperand.Left as SqlConstantExpression ?? (SqlConstantExpression)sqlBinaryOperand.Right;
                if (sqlBinaryOperand.Left is SqlConstantExpression)
                {
                    return _sqlExpressionFactory.MakeBinary(
                        ExpressionType.Equal,
                        _sqlExpressionFactory.Constant(!(bool)constant.Value!, constant.TypeMapping),
                        sqlBinaryOperand.Right,
                        sqlBinaryOperand.TypeMapping)!;
                }

                return _sqlExpressionFactory.MakeBinary(
                    ExpressionType.Equal,
                    sqlBinaryOperand.Left,
                    _sqlExpressionFactory.Constant(!(bool)constant.Value!, constant.TypeMapping),
                    sqlBinaryOperand.TypeMapping)!;
            }

            return _sqlExpressionFactory.MakeBinary(
                sqlBinaryOperand.OperatorType == ExpressionType.Equal
                    ? ExpressionType.NotEqual
                    : ExpressionType.Equal,
                sqlBinaryOperand.Left,
                sqlBinaryOperand.Right,
                sqlBinaryOperand.TypeMapping)!;
        }

        return sqlExpression;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitCase(CaseExpression caseExpression)
    {
        var parentSearchCondition = _isSearchCondition;

        var testIsCondition = caseExpression.Operand == null;
        _isSearchCondition = false;
        var operand = (SqlExpression?)Visit(caseExpression.Operand);
        var whenClauses = new List<CaseWhenClause>();
        foreach (var whenClause in caseExpression.WhenClauses)
        {
            _isSearchCondition = testIsCondition;
            var test = (SqlExpression)Visit(whenClause.Test);
            _isSearchCondition = false;
            var result = (SqlExpression)Visit(whenClause.Result);
            whenClauses.Add(new CaseWhenClause(test, result));
        }

        _isSearchCondition = false;
        var elseResult = (SqlExpression?)Visit(caseExpression.ElseResult);

        _isSearchCondition = parentSearchCondition;

        return ApplyConversion(caseExpression.Update(operand, whenClauses, elseResult), condition: false);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitCollate(CollateExpression collateExpression)
    {
        var parentSearchCondition = _isSearchCondition;
        _isSearchCondition = false;
        var operand = (SqlExpression)Visit(collateExpression.Operand);
        _isSearchCondition = parentSearchCondition;

        return ApplyConversion(collateExpression.Update(operand), condition: false);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitColumn(ColumnExpression columnExpression)
        => ApplyConversion(columnExpression, condition: false);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitDelete(DeleteExpression deleteExpression)
        => deleteExpression.Update(deleteExpression.Table, (SelectExpression)Visit(deleteExpression.SelectExpression));

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitDistinct(DistinctExpression distinctExpression)
    {
        var parentSearchCondition = _isSearchCondition;
        _isSearchCondition = false;
        var operand = (SqlExpression)Visit(distinctExpression.Operand);
        _isSearchCondition = parentSearchCondition;

        return ApplyConversion(distinctExpression.Update(operand), condition: false);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitExists(ExistsExpression existsExpression)
    {
        var parentSearchCondition = _isSearchCondition;
        _isSearchCondition = false;
        var subquery = (SelectExpression)Visit(existsExpression.Subquery);
        _isSearchCondition = parentSearchCondition;

        return ApplyConversion(existsExpression.Update(subquery), condition: true);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitFromSql(FromSqlExpression fromSqlExpression)
        => fromSqlExpression;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitIn(InExpression inExpression)
    {
        var parentSearchCondition = _isSearchCondition;

        _isSearchCondition = false;
        var item = (SqlExpression)Visit(inExpression.Item);
        var subquery = (SelectExpression?)Visit(inExpression.Subquery);

        var values = inExpression.Values;
        SqlExpression[]? newValues = null;
        if (values is not null)
        {
            for (var i = 0; i < values.Count; i++)
            {
                var value = values[i];
                var newValue = (SqlExpression)Visit(value);

                if (newValue != value && newValues is null)
                {
                    newValues = new SqlExpression[values.Count];
                    for (var j = 0; j < i; j++)
                    {
                        newValues[j] = values[j];
                    }
                }

                if (newValues is not null)
                {
                    newValues[i] = newValue;
                }
            }
        }

        var valuesParameter = (SqlParameterExpression?)Visit(inExpression.ValuesParameter);
        _isSearchCondition = parentSearchCondition;

        return ApplyConversion(inExpression.Update(item, subquery, newValues ?? values, valuesParameter), condition: true);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitLike(LikeExpression likeExpression)
    {
        var parentSearchCondition = _isSearchCondition;
        _isSearchCondition = false;
        var match = (SqlExpression)Visit(likeExpression.Match);
        var pattern = (SqlExpression)Visit(likeExpression.Pattern);
        var escapeChar = (SqlExpression?)Visit(likeExpression.EscapeChar);
        _isSearchCondition = parentSearchCondition;

        return ApplyConversion(likeExpression.Update(match, pattern, escapeChar), condition: true);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitSelect(SelectExpression selectExpression)
    {
        var parentSearchCondition = _isSearchCondition;

        _isSearchCondition = false;

        var projections = this.VisitAndConvert(selectExpression.Projection);
        var tables = this.VisitAndConvert(selectExpression.Tables);
        var groupBy = this.VisitAndConvert(selectExpression.GroupBy);
        var orderings = this.VisitAndConvert(selectExpression.Orderings);
        var offset = (SqlExpression?)Visit(selectExpression.Offset);
        var limit = (SqlExpression?)Visit(selectExpression.Limit);

        _isSearchCondition = true;

        var predicate = (SqlExpression?)Visit(selectExpression.Predicate);
        var havingExpression = (SqlExpression?)Visit(selectExpression.Having);

        _isSearchCondition = parentSearchCondition;

        return selectExpression.Update(projections, tables, predicate, groupBy, havingExpression, orderings, limit, offset);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitAtTimeZone(AtTimeZoneExpression atTimeZoneExpression)
    {
        var parentSearchCondition = _isSearchCondition;
        _isSearchCondition = false;
        var operand = (SqlExpression)Visit(atTimeZoneExpression.Operand);
        var timeZone = (SqlExpression)Visit(atTimeZoneExpression.TimeZone);
        _isSearchCondition = parentSearchCondition;

        return atTimeZoneExpression.Update(operand, timeZone);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitSqlBinary(SqlBinaryExpression sqlBinaryExpression)
    {
        var parentIsSearchCondition = _isSearchCondition;

        switch (sqlBinaryExpression.OperatorType)
        {
            // Only logical operations need conditions on both sides
            case ExpressionType.AndAlso:
            case ExpressionType.OrElse:
                _isSearchCondition = true;
                break;
            default:
                _isSearchCondition = false;
                break;
        }

        var newLeft = (SqlExpression)Visit(sqlBinaryExpression.Left);
        var newRight = (SqlExpression)Visit(sqlBinaryExpression.Right);

        _isSearchCondition = parentIsSearchCondition;

        sqlBinaryExpression = sqlBinaryExpression.Update(newLeft, newRight);
        var condition = sqlBinaryExpression.OperatorType is ExpressionType.AndAlso
            or ExpressionType.OrElse
            or ExpressionType.Equal
            or ExpressionType.NotEqual
            or ExpressionType.GreaterThan
            or ExpressionType.GreaterThanOrEqual
            or ExpressionType.LessThan
            or ExpressionType.LessThanOrEqual;

        return ApplyConversion(sqlBinaryExpression, condition);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitSqlUnary(SqlUnaryExpression sqlUnaryExpression)
    {
        var parentSearchCondition = _isSearchCondition;
        bool resultCondition;
        switch (sqlUnaryExpression.OperatorType)
        {
            case ExpressionType.Not
                when sqlUnaryExpression.Type == typeof(bool):
            {
                _isSearchCondition = true;
                resultCondition = true;
                break;
            }

            case ExpressionType.Not:
                _isSearchCondition = false;
                resultCondition = false;
                break;

            case ExpressionType.Convert:
            case ExpressionType.Negate:
                _isSearchCondition = false;
                resultCondition = false;
                break;

            case ExpressionType.Equal:
            case ExpressionType.NotEqual:
                _isSearchCondition = false;
                resultCondition = true;
                break;

            default:
                throw new InvalidOperationException(
                    RelationalStrings.UnsupportedOperatorForSqlExpression(
                        sqlUnaryExpression.OperatorType, typeof(SqlUnaryExpression)));
        }

        var operand = (SqlExpression)Visit(sqlUnaryExpression.Operand);

        _isSearchCondition = parentSearchCondition;

        return SimplifyNegatedBinary(
            ApplyConversion(
                sqlUnaryExpression.Update(operand),
                condition: resultCondition));
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitSqlConstant(SqlConstantExpression sqlConstantExpression)
        => ApplyConversion(sqlConstantExpression, condition: false);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitSqlFragment(SqlFragmentExpression sqlFragmentExpression)
        => sqlFragmentExpression;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitSqlFunction(SqlFunctionExpression sqlFunctionExpression)
    {
        var parentSearchCondition = _isSearchCondition;
        _isSearchCondition = false;
        var instance = (SqlExpression?)Visit(sqlFunctionExpression.Instance);
        SqlExpression[]? arguments = default;
        if (!sqlFunctionExpression.IsNiladic)
        {
            arguments = new SqlExpression[sqlFunctionExpression.Arguments.Count];
            for (var i = 0; i < arguments.Length; i++)
            {
                arguments[i] = (SqlExpression)Visit(sqlFunctionExpression.Arguments[i]);
            }
        }

        _isSearchCondition = parentSearchCondition;
        var newFunction = sqlFunctionExpression.Update(instance, arguments);

        var condition = sqlFunctionExpression.Name is "FREETEXT" or "CONTAINS";

        return ApplyConversion(newFunction, condition);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitTableValuedFunction(TableValuedFunctionExpression tableValuedFunctionExpression)
    {
        var parentSearchCondition = _isSearchCondition;
        _isSearchCondition = false;

        var arguments = new SqlExpression[tableValuedFunctionExpression.Arguments.Count];
        for (var i = 0; i < arguments.Length; i++)
        {
            arguments[i] = (SqlExpression)Visit(tableValuedFunctionExpression.Arguments[i]);
        }

        _isSearchCondition = parentSearchCondition;
        return tableValuedFunctionExpression.Update(arguments);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitSqlParameter(SqlParameterExpression sqlParameterExpression)
        => ApplyConversion(sqlParameterExpression, condition: false);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitTable(TableExpression tableExpression)
        => tableExpression;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitProjection(ProjectionExpression projectionExpression)
    {
        var expression = (SqlExpression)Visit(projectionExpression.Expression);

        return projectionExpression.Update(expression);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitOrdering(OrderingExpression orderingExpression)
    {
        var parentSearchCondition = _isSearchCondition;
        _isSearchCondition = false;

        var expression = (SqlExpression)Visit(orderingExpression.Expression);

        _isSearchCondition = parentSearchCondition;

        return orderingExpression.Update(expression);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitCrossJoin(CrossJoinExpression crossJoinExpression)
    {
        var parentSearchCondition = _isSearchCondition;
        _isSearchCondition = false;
        var table = (TableExpressionBase)Visit(crossJoinExpression.Table);
        _isSearchCondition = parentSearchCondition;

        return crossJoinExpression.Update(table);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitCrossApply(CrossApplyExpression crossApplyExpression)
    {
        var parentSearchCondition = _isSearchCondition;
        _isSearchCondition = false;
        var table = (TableExpressionBase)Visit(crossApplyExpression.Table);
        _isSearchCondition = parentSearchCondition;

        return crossApplyExpression.Update(table);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitOuterApply(OuterApplyExpression outerApplyExpression)
    {
        var parentSearchCondition = _isSearchCondition;
        _isSearchCondition = false;
        var table = (TableExpressionBase)Visit(outerApplyExpression.Table);
        _isSearchCondition = parentSearchCondition;

        return outerApplyExpression.Update(table);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitInnerJoin(InnerJoinExpression innerJoinExpression)
    {
        var parentSearchCondition = _isSearchCondition;
        _isSearchCondition = false;
        var table = (TableExpressionBase)Visit(innerJoinExpression.Table);
        _isSearchCondition = true;
        var joinPredicate = (SqlExpression)Visit(innerJoinExpression.JoinPredicate);
        _isSearchCondition = parentSearchCondition;

        return innerJoinExpression.Update(table, joinPredicate);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitLeftJoin(LeftJoinExpression leftJoinExpression)
    {
        var parentSearchCondition = _isSearchCondition;
        _isSearchCondition = false;
        var table = (TableExpressionBase)Visit(leftJoinExpression.Table);
        _isSearchCondition = true;
        var joinPredicate = (SqlExpression)Visit(leftJoinExpression.JoinPredicate);
        _isSearchCondition = parentSearchCondition;

        return leftJoinExpression.Update(table, joinPredicate);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitScalarSubquery(ScalarSubqueryExpression scalarSubqueryExpression)
    {
        var parentSearchCondition = _isSearchCondition;
        var subquery = (SelectExpression)Visit(scalarSubqueryExpression.Subquery);
        _isSearchCondition = parentSearchCondition;

        return ApplyConversion(scalarSubqueryExpression.Update(subquery), condition: false);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitRowNumber(RowNumberExpression rowNumberExpression)
    {
        var parentSearchCondition = _isSearchCondition;
        _isSearchCondition = false;
        var partitions = new List<SqlExpression>();
        foreach (var partition in rowNumberExpression.Partitions)
        {
            var newPartition = (SqlExpression)Visit(partition);
            partitions.Add(newPartition);
        }

        var orderings = new List<OrderingExpression>();
        foreach (var ordering in rowNumberExpression.Orderings)
        {
            var newOrdering = (OrderingExpression)Visit(ordering);
            orderings.Add(newOrdering);
        }

        _isSearchCondition = parentSearchCondition;

        return ApplyConversion(rowNumberExpression.Update(partitions, orderings), condition: false);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitRowValue(RowValueExpression rowValueExpression)
    {
        var parentSearchCondition = _isSearchCondition;
        _isSearchCondition = false;

        var values = new SqlExpression[rowValueExpression.Values.Count];
        for (var i = 0; i < values.Length; i++)
        {
            values[i] = (SqlExpression)Visit(rowValueExpression.Values[i]);
        }

        _isSearchCondition = parentSearchCondition;
        return rowValueExpression.Update(values);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitExcept(ExceptExpression exceptExpression)
    {
        var parentSearchCondition = _isSearchCondition;
        _isSearchCondition = false;
        var source1 = (SelectExpression)Visit(exceptExpression.Source1);
        var source2 = (SelectExpression)Visit(exceptExpression.Source2);
        _isSearchCondition = parentSearchCondition;

        return exceptExpression.Update(source1, source2);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitIntersect(IntersectExpression intersectExpression)
    {
        var parentSearchCondition = _isSearchCondition;
        _isSearchCondition = false;
        var source1 = (SelectExpression)Visit(intersectExpression.Source1);
        var source2 = (SelectExpression)Visit(intersectExpression.Source2);
        _isSearchCondition = parentSearchCondition;

        return intersectExpression.Update(source1, source2);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitUnion(UnionExpression unionExpression)
    {
        var parentSearchCondition = _isSearchCondition;
        _isSearchCondition = false;
        var source1 = (SelectExpression)Visit(unionExpression.Source1);
        var source2 = (SelectExpression)Visit(unionExpression.Source2);
        _isSearchCondition = parentSearchCondition;

        return unionExpression.Update(source1, source2);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitUpdate(UpdateExpression updateExpression)
    {
        var selectExpression = (SelectExpression)Visit(updateExpression.SelectExpression);
        var parentSearchCondition = _isSearchCondition;
        _isSearchCondition = false;
        List<ColumnValueSetter>? columnValueSetters = null;
        for (var (i, n) = (0, updateExpression.ColumnValueSetters.Count); i < n; i++)
        {
            var columnValueSetter = updateExpression.ColumnValueSetters[i];
            var newValue = (SqlExpression)Visit(columnValueSetter.Value);
            if (columnValueSetters != null)
            {
                columnValueSetters.Add(new ColumnValueSetter(columnValueSetter.Column, newValue));
            }
            else if (!ReferenceEquals(newValue, columnValueSetter.Value))
            {
                columnValueSetters = [];
                for (var j = 0; j < i; j++)
                {
                    columnValueSetters.Add(updateExpression.ColumnValueSetters[j]);
                }

                columnValueSetters.Add(new ColumnValueSetter(columnValueSetter.Column, newValue));
            }
        }

        _isSearchCondition = parentSearchCondition;
        return updateExpression.Update(selectExpression, columnValueSetters ?? updateExpression.ColumnValueSetters);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitJsonScalar(JsonScalarExpression jsonScalarExpression)
        => ApplyConversion(jsonScalarExpression, condition: false);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitValues(ValuesExpression valuesExpression)
    {
        var parentSearchCondition = _isSearchCondition;
        _isSearchCondition = false;

        var rowValues = new RowValueExpression[valuesExpression.RowValues.Count];
        for (var i = 0; i < rowValues.Length; i++)
        {
            rowValues[i] = (RowValueExpression)Visit(valuesExpression.RowValues[i]);
        }

        _isSearchCondition = parentSearchCondition;
        return valuesExpression.Update(rowValues);
    }
}
