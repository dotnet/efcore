// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.SqlServer.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.SqlServer.Query.Internal
{
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
            [NotNull] ISqlExpressionFactory sqlExpressionFactory)
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
        {
            return condition
                ? _sqlExpressionFactory.Case(
                    new[]
                    {
                        new CaseWhenClause(
                            SimplifyNegatedBinary(sqlExpression),
                            _sqlExpressionFactory.ApplyDefaultTypeMapping(_sqlExpressionFactory.Constant(true)))
                    },
                    _sqlExpressionFactory.Constant(false))
                : sqlExpression;
        }

        private SqlExpression BuildCompareToExpression(SqlExpression sqlExpression)
            => sqlExpression is SqlConstantExpression sqlConstantExpression
                && sqlConstantExpression.Value is bool boolValue
                    ? _sqlExpressionFactory.Equal(
                        boolValue
                            ? _sqlExpressionFactory.Constant(1)
                            : _sqlExpressionFactory.Constant(0),
                        _sqlExpressionFactory.Constant(1))
                    : _sqlExpressionFactory.Equal(
                        sqlExpression,
                        _sqlExpressionFactory.Constant(true));

        // !(a == b) -> (a != b)
        // !(a != b) -> (a == b)
        private SqlExpression SimplifyNegatedBinary(SqlExpression sqlExpression)
            => sqlExpression is SqlUnaryExpression sqlUnaryExpression
                && sqlUnaryExpression.OperatorType == ExpressionType.Not
                && sqlUnaryExpression.Type == typeof(bool)
                && sqlUnaryExpression.Operand is SqlBinaryExpression sqlBinaryOperand
                && (sqlBinaryOperand.OperatorType == ExpressionType.Equal || sqlBinaryOperand.OperatorType == ExpressionType.NotEqual)
                    ? _sqlExpressionFactory.MakeBinary(
                        sqlBinaryOperand.OperatorType == ExpressionType.Equal
                            ? ExpressionType.NotEqual
                            : ExpressionType.Equal,
                        sqlBinaryOperand.Left,
                        sqlBinaryOperand.Right,
                        sqlBinaryOperand.TypeMapping)
                    : sqlExpression;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override Expression VisitCase(CaseExpression caseExpression)
        {
            Check.NotNull(caseExpression, nameof(caseExpression));

            var parentSearchCondition = _isSearchCondition;

            var testIsCondition = caseExpression.Operand == null;
            _isSearchCondition = false;
            var operand = (SqlExpression)Visit(caseExpression.Operand);
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
            var elseResult = (SqlExpression)Visit(caseExpression.ElseResult);

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
            Check.NotNull(collateExpression, nameof(collateExpression));

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
        {
            Check.NotNull(columnExpression, nameof(columnExpression));

            return ApplyConversion(columnExpression, condition: false);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override Expression VisitDistinct(DistinctExpression distinctExpression)
        {
            Check.NotNull(distinctExpression, nameof(distinctExpression));

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
            Check.NotNull(existsExpression, nameof(existsExpression));

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
        {
            Check.NotNull(fromSqlExpression, nameof(fromSqlExpression));

            return fromSqlExpression;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override Expression VisitIn(InExpression inExpression)
        {
            Check.NotNull(inExpression, nameof(inExpression));

            var parentSearchCondition = _isSearchCondition;

            _isSearchCondition = false;
            var item = (SqlExpression)Visit(inExpression.Item);
            var subquery = (SelectExpression)Visit(inExpression.Subquery);
            var values = (SqlExpression)Visit(inExpression.Values);
            _isSearchCondition = parentSearchCondition;

            return ApplyConversion(inExpression.Update(item, values, subquery), condition: true);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override Expression VisitLike(LikeExpression likeExpression)
        {
            Check.NotNull(likeExpression, nameof(likeExpression));

            var parentSearchCondition = _isSearchCondition;
            _isSearchCondition = false;
            var match = (SqlExpression)Visit(likeExpression.Match);
            var pattern = (SqlExpression)Visit(likeExpression.Pattern);
            var escapeChar = (SqlExpression)Visit(likeExpression.EscapeChar);
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
            Check.NotNull(selectExpression, nameof(selectExpression));

            var changed = false;
            var parentSearchCondition = _isSearchCondition;

            var projections = new List<ProjectionExpression>();
            _isSearchCondition = false;
            foreach (var item in selectExpression.Projection)
            {
                var updatedProjection = (ProjectionExpression)Visit(item);
                projections.Add(updatedProjection);
                changed |= updatedProjection != item;
            }

            var tables = new List<TableExpressionBase>();
            foreach (var table in selectExpression.Tables)
            {
                var newTable = (TableExpressionBase)Visit(table);
                changed |= newTable != table;
                tables.Add(newTable);
            }

            _isSearchCondition = true;
            var predicate = (SqlExpression)Visit(selectExpression.Predicate);
            changed |= predicate != selectExpression.Predicate;

            var groupBy = new List<SqlExpression>();
            _isSearchCondition = false;
            foreach (var groupingKey in selectExpression.GroupBy)
            {
                var newGroupingKey = (SqlExpression)Visit(groupingKey);
                changed |= newGroupingKey != groupingKey;
                groupBy.Add(newGroupingKey);
            }

            _isSearchCondition = true;
            var havingExpression = (SqlExpression)Visit(selectExpression.Having);
            changed |= havingExpression != selectExpression.Having;

            var orderings = new List<OrderingExpression>();
            _isSearchCondition = false;
            foreach (var ordering in selectExpression.Orderings)
            {
                var orderingExpression = (SqlExpression)Visit(ordering.Expression);
                changed |= orderingExpression != ordering.Expression;
                orderings.Add(ordering.Update(orderingExpression));
            }

            var offset = (SqlExpression)Visit(selectExpression.Offset);
            changed |= offset != selectExpression.Offset;

            var limit = (SqlExpression)Visit(selectExpression.Limit);
            changed |= limit != selectExpression.Limit;

            _isSearchCondition = parentSearchCondition;

            return changed
                ? selectExpression.Update(
                    projections, tables, predicate, groupBy, havingExpression, orderings, limit, offset)
                : selectExpression;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override Expression VisitSqlBinary(SqlBinaryExpression sqlBinaryExpression)
        {
            Check.NotNull(sqlBinaryExpression, nameof(sqlBinaryExpression));

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
            var condition = sqlBinaryExpression.OperatorType == ExpressionType.AndAlso
                || sqlBinaryExpression.OperatorType == ExpressionType.OrElse
                || sqlBinaryExpression.OperatorType == ExpressionType.Equal
                || sqlBinaryExpression.OperatorType == ExpressionType.NotEqual
                || sqlBinaryExpression.OperatorType == ExpressionType.GreaterThan
                || sqlBinaryExpression.OperatorType == ExpressionType.GreaterThanOrEqual
                || sqlBinaryExpression.OperatorType == ExpressionType.LessThan
                || sqlBinaryExpression.OperatorType == ExpressionType.LessThanOrEqual;

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
            Check.NotNull(sqlUnaryExpression, nameof(sqlUnaryExpression));

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
                    throw new InvalidOperationException(RelationalStrings.UnsupportedOperatorForSqlExpression(
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
        {
            Check.NotNull(sqlConstantExpression, nameof(sqlConstantExpression));

            return ApplyConversion(sqlConstantExpression, condition: false);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override Expression VisitSqlFragment(SqlFragmentExpression sqlFragmentExpression)
        {
            Check.NotNull(sqlFragmentExpression, nameof(sqlFragmentExpression));

            return sqlFragmentExpression;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override Expression VisitSqlFunction(SqlFunctionExpression sqlFunctionExpression)
        {
            Check.NotNull(sqlFunctionExpression, nameof(sqlFunctionExpression));

            var parentSearchCondition = _isSearchCondition;
            _isSearchCondition = false;
            var instance = (SqlExpression)Visit(sqlFunctionExpression.Instance);
            SqlExpression[] arguments = default;
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

            var condition = string.Equals(sqlFunctionExpression.Name, "FREETEXT")
                || string.Equals(sqlFunctionExpression.Name, "CONTAINS");

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
            Check.NotNull(tableValuedFunctionExpression, nameof(tableValuedFunctionExpression));

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
        {
            Check.NotNull(sqlParameterExpression, nameof(sqlParameterExpression));

            return ApplyConversion(sqlParameterExpression, condition: false);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override Expression VisitTable(TableExpression tableExpression)
        {
            Check.NotNull(tableExpression, nameof(tableExpression));

            return tableExpression;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override Expression VisitProjection(ProjectionExpression projectionExpression)
        {
            Check.NotNull(projectionExpression, nameof(projectionExpression));

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
            Check.NotNull(orderingExpression, nameof(orderingExpression));

            var expression = (SqlExpression)Visit(orderingExpression.Expression);

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
            Check.NotNull(crossJoinExpression, nameof(crossJoinExpression));

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
            Check.NotNull(crossApplyExpression, nameof(crossApplyExpression));

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
            Check.NotNull(outerApplyExpression, nameof(outerApplyExpression));

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
            Check.NotNull(innerJoinExpression, nameof(innerJoinExpression));

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
            Check.NotNull(leftJoinExpression, nameof(leftJoinExpression));

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
            Check.NotNull(scalarSubqueryExpression, nameof(scalarSubqueryExpression));

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
            Check.NotNull(rowNumberExpression, nameof(rowNumberExpression));

            var parentSearchCondition = _isSearchCondition;
            _isSearchCondition = false;
            var changed = false;
            var partitions = new List<SqlExpression>();
            foreach (var partition in rowNumberExpression.Partitions)
            {
                var newPartition = (SqlExpression)Visit(partition);
                changed |= newPartition != partition;
                partitions.Add(newPartition);
            }

            var orderings = new List<OrderingExpression>();
            foreach (var ordering in rowNumberExpression.Orderings)
            {
                var newOrdering = (OrderingExpression)Visit(ordering);
                changed |= newOrdering != ordering;
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
        protected override Expression VisitExcept(ExceptExpression exceptExpression)
        {
            Check.NotNull(exceptExpression, nameof(exceptExpression));

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
            Check.NotNull(intersectExpression, nameof(intersectExpression));

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
            Check.NotNull(unionExpression, nameof(unionExpression));

            var parentSearchCondition = _isSearchCondition;
            _isSearchCondition = false;
            var source1 = (SelectExpression)Visit(unionExpression.Source1);
            var source2 = (SelectExpression)Visit(unionExpression.Source2);
            _isSearchCondition = parentSearchCondition;

            return unionExpression.Update(source1, source2);
        }
    }
}
