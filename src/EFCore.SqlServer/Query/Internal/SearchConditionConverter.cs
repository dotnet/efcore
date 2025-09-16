// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;

/// <summary>
///     <para>
///         A SQL Server visitor which converts boolean expressions that represent search conditions to bit values and vice versa, depending
///         on context:
///     </para>
///     <code>
///         WHERE b.SomeBitColumn => WHERE b.SomeBitColumn = 1
///         SELECT a LIKE b => SELECT CASE WHEN a LIKE b THEN 1 ELSE 0 END
///     </code>
/// </summary>
/// <remarks>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </remarks>
public class SearchConditionConverter(ISqlExpressionFactory sqlExpressionFactory) : ExpressionVisitor
{
    /// <summary>
    ///     Tracks whether we're in the larger context of a predicate, even the immediate context is not a search condition.
    /// </summary>
    /// <remarks>
    ///     This visitor tracks whether the immediate context is a search condition by passing a boolean flag down during visitation,
    ///     and making adjustments so that the SQL is correct. However, in some cases it's useful to know whether we're in the
    ///     larger context of a predicate - even if the immediate context isn't a search condition; we use this to make sure the
    ///     SQL is efficient (as opposed to correct), refraining from transformations which could prevent index usage.
    /// </remarks>
    private bool _inLargerPredicateContext;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [return: NotNullIfNotNull(nameof(expression))]
    public override Expression? Visit(Expression? expression)
        => Visit(expression, inSearchConditionContext: false);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [return: NotNullIfNotNull(nameof(expression))]
    protected virtual Expression? Visit(Expression? expression, bool inSearchConditionContext)
    {
        var parentOptimizeForPredicateContext = _inLargerPredicateContext;
        if (inSearchConditionContext)
        {
            _inLargerPredicateContext = true;
        }

        var result = expression switch
        {
            CaseExpression e => VisitCase(e, inSearchConditionContext),
            SelectExpression e => VisitSelect(e),
            SqlBinaryExpression e => VisitSqlBinary(e, inSearchConditionContext),
            SqlUnaryExpression e => VisitSqlUnary(e, inSearchConditionContext),
            PredicateJoinExpressionBase e => VisitPredicateJoin(e),

            // The following are search condition expressions: they can appear directly in a WHERE, and cannot e.g. be projected out
            // directly
            SqlExpression e and
                (ExistsExpression or InExpression or LikeExpression or SqlFunctionExpression { Name: "FREETEXT" or "CONTAINS" })
                => ApplyConversion((SqlExpression)base.VisitExtension(e), inSearchConditionContext, isExpressionSearchCondition: true),

            SqlExpression e => ApplyConversion(
                (SqlExpression)base.VisitExtension(e), inSearchConditionContext, isExpressionSearchCondition: false),

            _ => base.Visit(expression)
        };

        _inLargerPredicateContext = parentOptimizeForPredicateContext;
        return result;
    }

    private SqlExpression ApplyConversion(SqlExpression sqlExpression, bool inSearchConditionContext, bool isExpressionSearchCondition)
        => (inSearchCondition: inSearchConditionContext, isExpressionSearchCondition) switch
        {
            // A non-search condition expression in a search condition context - add equality to convert to search condition:
            // WHERE b.SomeBitColumn => WHERE b.SomeBitColumn = 1
            (true, false) => sqlExpression is SqlConstantExpression { Value: bool boolValue }
                ? sqlExpressionFactory.Equal(
                    boolValue
                        ? sqlExpressionFactory.Constant(1)
                        : sqlExpressionFactory.Constant(0),
                    sqlExpressionFactory.Constant(1))
                : sqlExpressionFactory.Equal(sqlExpression, sqlExpressionFactory.Constant(true)),

            // A search condition expression in non-search condition context - wrap in CASE/WHEN to convert to bit:
            // e.g. SELECT a LIKE b => SELECT CASE WHEN a LIKE b THEN 1 ELSE 0 END
            // TODO: NULL is not handled properly here, see #34001
            (false, true) => sqlExpressionFactory.Case(
                [
                    new CaseWhenClause(
                        SimplifyNegatedBinary(sqlExpression),
                        sqlExpressionFactory.ApplyDefaultTypeMapping(sqlExpressionFactory.Constant(true)))
                ],
                sqlExpressionFactory.Constant(false)),

            // All other cases (e.g. WHERE a LIKE b, SELECT b.SomebitColumn) - no need to do anything.
            _ => sqlExpression
        };

    private SqlExpression SimplifyNegatedBinary(SqlExpression sqlExpression)
    {
        if (sqlExpression is SqlUnaryExpression { OperatorType: ExpressionType.Not } sqlUnaryExpression
            && sqlUnaryExpression.Type == typeof(bool)
            && sqlUnaryExpression.Operand is SqlBinaryExpression
            {
                OperatorType: ExpressionType.Equal
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
                    return sqlExpressionFactory.MakeBinary(
                        ExpressionType.Equal,
                        sqlExpressionFactory.Constant(!(bool)constant.Value!, constant.TypeMapping),
                        sqlBinaryOperand.Right,
                        sqlBinaryOperand.TypeMapping)!;
                }

                return sqlExpressionFactory.MakeBinary(
                    ExpressionType.Equal,
                    sqlBinaryOperand.Left,
                    sqlExpressionFactory.Constant(!(bool)constant.Value!, constant.TypeMapping),
                    sqlBinaryOperand.TypeMapping)!;
            }

            return sqlExpressionFactory.MakeBinary(
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
    protected virtual Expression VisitCase(CaseExpression caseExpression, bool inSearchConditionContext)
    {
        var testIsCondition = caseExpression.Operand is null;
        var operand = (SqlExpression?)Visit(caseExpression.Operand);
        var whenClauses = new List<CaseWhenClause>();
        foreach (var whenClause in caseExpression.WhenClauses)
        {
            var test = (SqlExpression)Visit(whenClause.Test, testIsCondition);
            var result = (SqlExpression)Visit(whenClause.Result);
            whenClauses.Add(new CaseWhenClause(test, result));
        }

        var elseResult = (SqlExpression?)Visit(caseExpression.ElseResult);

        return ApplyConversion(
            sqlExpressionFactory.Case(operand, whenClauses, elseResult, caseExpression),
            inSearchConditionContext,
            isExpressionSearchCondition: false);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual Expression VisitPredicateJoin(PredicateJoinExpressionBase join)
        => join.Update(
            (TableExpressionBase)Visit(join.Table),
            (SqlExpression)Visit(join.JoinPredicate, inSearchConditionContext: true));

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual Expression VisitSelect(SelectExpression select)
    {
        var parentOptimizeForPredicateContext = _inLargerPredicateContext;
        _inLargerPredicateContext = false;

        var tables = this.VisitAndConvert(select.Tables);
        var predicate = (SqlExpression?)Visit(select.Predicate, inSearchConditionContext: true);
        var groupBy = this.VisitAndConvert(select.GroupBy);
        var havingExpression = (SqlExpression?)Visit(select.Having, inSearchConditionContext: true);
        var projections = this.VisitAndConvert(select.Projection);
        var orderings = this.VisitAndConvert(select.Orderings);
        var offset = (SqlExpression?)Visit(select.Offset);
        var limit = (SqlExpression?)Visit(select.Limit);

        _inLargerPredicateContext = parentOptimizeForPredicateContext;

        return select.Update(tables, predicate, groupBy, havingExpression, projections, orderings, offset, limit);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual Expression VisitSqlBinary(SqlBinaryExpression binary, bool inSearchConditionContext)
    {
        // Only logical operations need conditions on both sides
        var areOperandsInSearchConditionContext = binary.OperatorType is ExpressionType.AndAlso or ExpressionType.OrElse;

        var newLeft = (SqlExpression)Visit(binary.Left, areOperandsInSearchConditionContext);
        var newRight = (SqlExpression)Visit(binary.Right, areOperandsInSearchConditionContext);

        if (binary.OperatorType is ExpressionType.NotEqual or ExpressionType.Equal)
        {
            var leftType = newLeft.TypeMapping?.Converter?.ProviderClrType ?? newLeft.Type;
            var rightType = newRight.TypeMapping?.Converter?.ProviderClrType ?? newRight.Type;

            // Transform equality and inequality over bool/integer to bitwise operators:
            //     x != y => CAST(x ^ y AS BIT)
            //     x == y => CAST(~(x ^ y) AS BIT)
            // However, refrain from doing this if we're within a predicate - even if the immediate context is not a search condition -
            // since this might prevent index usage (e.g. we don't want this to happen within CASE THEN clauses, which aren't search
            // conditions but are seen through by SQL Server when in the WHERE clause - see #36291).
            if (!_inLargerPredicateContext
                && (leftType == typeof(bool) || leftType.IsInteger())
                && (rightType == typeof(bool) || rightType.IsInteger()))
            {
                // "lhs != rhs" is the same as "CAST(lhs ^ rhs AS BIT)", except that
                // the first is a boolean, the second is a BIT
                var result = sqlExpressionFactory.MakeBinary(ExpressionType.ExclusiveOr, newLeft, newRight, null)!;

                if (result.Type != typeof(bool))
                {
                    result = sqlExpressionFactory.Convert(result, typeof(bool), binary.TypeMapping);
                }

                // "lhs == rhs" is the same as "NOT(lhs != rhs)" aka "~(lhs ^ rhs)"
                if (binary.OperatorType is ExpressionType.Equal)
                {
                    result = sqlExpressionFactory.MakeUnary(ExpressionType.OnesComplement, result, result.Type, result.TypeMapping)!;
                }

                return result;
            }

            if (newLeft is SqlUnaryExpression { OperatorType: ExpressionType.OnesComplement } negatedLeft
                && newRight is SqlUnaryExpression { OperatorType: ExpressionType.OnesComplement } negatedRight)
            {
                newLeft = negatedLeft.Operand;
                newRight = negatedRight.Operand;
            }
        }

        binary = binary.Update(newLeft, newRight);

        var isExpressionSearchCondition = binary.OperatorType is ExpressionType.AndAlso
            or ExpressionType.OrElse
            or ExpressionType.Equal
            or ExpressionType.NotEqual
            or ExpressionType.GreaterThan
            or ExpressionType.GreaterThanOrEqual
            or ExpressionType.LessThan
            or ExpressionType.LessThanOrEqual;

        return ApplyConversion(binary, inSearchConditionContext, isExpressionSearchCondition);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual Expression VisitSqlUnary(SqlUnaryExpression sqlUnaryExpression, bool inSearchConditionContext)
    {
        bool isOperandInSearchConditionContext, isSearchConditionExpression;

        switch (sqlUnaryExpression.OperatorType)
        {
            case ExpressionType.Not
                when (sqlUnaryExpression.TypeMapping?.Converter?.ProviderClrType ?? sqlUnaryExpression.Type) == typeof(bool):
            {
                // when possible, avoid converting to/from predicate form
                if (!inSearchConditionContext && sqlUnaryExpression.Operand is not (ExistsExpression or InExpression or LikeExpression))
                {
                    var negatedOperand = (SqlExpression)Visit(sqlUnaryExpression.Operand);

                    if (negatedOperand is SqlUnaryExpression { OperatorType: ExpressionType.OnesComplement } unary)
                    {
                        return unary.Operand;
                    }

                    return sqlExpressionFactory.MakeUnary(
                        ExpressionType.OnesComplement, negatedOperand, negatedOperand.Type, negatedOperand.TypeMapping)!;
                }

                isOperandInSearchConditionContext = true;
                isSearchConditionExpression = true;
                break;
            }

            case ExpressionType.Not:
                isOperandInSearchConditionContext = false;
                isSearchConditionExpression = false;
                break;

            case ExpressionType.Convert:
            case ExpressionType.Negate:
                isOperandInSearchConditionContext = false;
                isSearchConditionExpression = false;
                break;

            case ExpressionType.Equal:
            case ExpressionType.NotEqual:
                isOperandInSearchConditionContext = false;
                isSearchConditionExpression = true;
                break;

            default:
                throw new InvalidOperationException(
                    RelationalStrings.UnsupportedOperatorForSqlExpression(
                        sqlUnaryExpression.OperatorType, typeof(SqlUnaryExpression)));
        }

        var operand = (SqlExpression)Visit(sqlUnaryExpression.Operand, isOperandInSearchConditionContext);

        return SimplifyNegatedBinary(
            ApplyConversion(
                sqlUnaryExpression.Update(operand),
                inSearchConditionContext,
                isSearchConditionExpression));
    }
}
