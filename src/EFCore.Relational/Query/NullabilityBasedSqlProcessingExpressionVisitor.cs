// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class NullabilityBasedSqlProcessingExpressionVisitor : SqlExpressionVisitor
    {
        protected virtual bool UseRelationalNulls { get; }
        protected virtual ISqlExpressionFactory SqlExpressionFactory { get; }
        protected virtual IReadOnlyDictionary<string, object> ParameterValues { get; }
        protected virtual List<ColumnExpression> NonNullableColumns { get; } = new List<ColumnExpression>();

        protected virtual bool CanCache { get; set; }

        private bool _nullable;
        private bool _allowOptimizedExpansion;

        public NullabilityBasedSqlProcessingExpressionVisitor(
            [NotNull] ISqlExpressionFactory sqlExpressionFactory,
            [NotNull] IReadOnlyDictionary<string, object> parameterValues,
            bool useRelationalNulls)
        {
            SqlExpressionFactory = sqlExpressionFactory;
            ParameterValues = parameterValues;
            UseRelationalNulls = useRelationalNulls;
            CanCache = true;

            _allowOptimizedExpansion = false;
        }

        private void RestoreNonNullableColumnsList(int counter)
        {
            if (counter < NonNullableColumns.Count)
            {
                NonNullableColumns.RemoveRange(counter, NonNullableColumns.Count - counter);
            }
        }

        public virtual (SelectExpression selectExpression, bool canCache) Process([NotNull] SelectExpression selectExpression)
        {
            Check.NotNull(selectExpression, nameof(selectExpression));

            return (selectExpression: VisitInternal<SelectExpression>(selectExpression).ResultExpression, canCache: CanCache);
        }

        /// <summary>
        ///     Method that handles visitation of SqlExpression nodes. All provider specific nodes should be handled by this method in the provider specific implementation of <see cref="NullabilityBasedSqlProcessingExpressionVisitor"/>.
        ///     Depending on the settings, the method sets up state for the actual visitation, cleans up after the visitation is complete
        ///     and returns resulting expression along with it's nullability.
        /// </summary>
        /// <typeparam name="TResult">Type of the resulting expression. </typeparam>
        /// <param name="expression">Expression that is to be visited. </param>
        /// <param name="allowOptimizedExpansion">True if null semantics inside the expression can be expanded in the optimized way (i.e. 'null' and 'false' are interchangable), false otherwise. </param>
        /// <param name="restoreNonNullableColumnInformation">True if method should reset the number of non-nullable columns after the visitation is complete, false otherwise. </param>
        /// <returns> Tuple representing visited expression and it's nullability. </returns>
        protected virtual (TResult ResultExpression, bool Nullable) VisitInternal<TResult>(
            [CanBeNull] Expression expression,
            bool allowOptimizedExpansion = false,
            bool restoreNonNullableColumnInformation = true)
            where TResult : Expression
        {
            if (expression == null)
            {
                return (null, false);
            }

            _nullable = false;
            var currentNonNullableColumnsCount = NonNullableColumns.Count;
            var previousAllowOptimizedExpansion = _allowOptimizedExpansion;
            _allowOptimizedExpansion = allowOptimizedExpansion;
            var resultExpression = (TResult)Visit(expression);
            _allowOptimizedExpansion = previousAllowOptimizedExpansion;
            if (restoreNonNullableColumnInformation)
            {
                RestoreNonNullableColumnsList(currentNonNullableColumnsCount);
            }

            return (resultExpression, _nullable);
        }

        protected override Expression VisitCase(CaseExpression caseExpression)
        {
            Check.NotNull(caseExpression, nameof(caseExpression));

            // if there is no 'else' there is a possibility of null, when none of the conditions are met
            // otherwise the result is nullable if any of the WhenClause results OR ElseResult is nullable
            var nullable = caseExpression.ElseResult == null;
            var currentNonNullableColumnsCount = NonNullableColumns.Count;

            var operand = VisitInternal<SqlExpression>(caseExpression.Operand).ResultExpression;
            var whenClauses = new List<CaseWhenClause>();
            var testIsCondition = caseExpression.Operand == null;

            var testEvaluatesToTrue = false;
            foreach (var whenClause in caseExpression.WhenClauses)
            {
                // we can use non-nullable column information we got from visiting Test, in the Result
                var test = VisitInternal<SqlExpression>(whenClause.Test, allowOptimizedExpansion: testIsCondition, restoreNonNullableColumnInformation: false).ResultExpression;

                if (test is SqlConstantExpression testConstant
                    && testConstant.Value is bool testConstantBool)
                {
                    if (testConstantBool)
                    {
                        testEvaluatesToTrue = true;
                    }
                    else
                    {
                        // if test evaluates to 'false' we can remove the WhenClause
                        RestoreNonNullableColumnsList(currentNonNullableColumnsCount);

                        continue;
                    }
                }

                var (newResult, resultNullable) = VisitInternal<SqlExpression>(whenClause.Result);

                nullable |= resultNullable;
                whenClauses.Add(new CaseWhenClause(test, newResult));
                RestoreNonNullableColumnsList(currentNonNullableColumnsCount);

                // if test evaluates to 'true' we can remove every condition that comes after, including ElseResult
                if (testEvaluatesToTrue)
                {
                    break;
                }
            }

            SqlExpression elseResult = null;
            if (!testEvaluatesToTrue)
            {
                bool elseResultNullable;
                (elseResult, elseResultNullable) = VisitInternal<SqlExpression>(caseExpression.ElseResult);
                _nullable = nullable || elseResultNullable;
            }

            // if there are no whenClauses left (e.g. their tests evaluated to false):
            // - if there is Else block, return it
            // - if there is no Else block, return null 
            if (whenClauses.Count == 0)
            {
                return elseResult == null
                    ? SqlExpressionFactory.Constant(null, caseExpression.TypeMapping)
                    : elseResult;
            }

            // if there is only one When clause and it's test evaluates to 'true' AND there is no else block, simply return the result
            if (elseResult == null
                && whenClauses.Count == 1
                && whenClauses[0].Test is SqlConstantExpression singleTestConstant
                && singleTestConstant.Value is bool boolConstant
                && boolConstant)
            {
                return whenClauses[0].Result;
            }

            return caseExpression.Update(operand, whenClauses, elseResult);
        }

        protected override Expression VisitColumn(ColumnExpression columnExpression)
        {
            Check.NotNull(columnExpression, nameof(columnExpression));

            _nullable = columnExpression.IsNullable && !NonNullableColumns.Contains(columnExpression);

            return columnExpression;
        }

        protected override Expression VisitCrossApply(CrossApplyExpression crossApplyExpression)
        {
            Check.NotNull(crossApplyExpression, nameof(crossApplyExpression));

            return crossApplyExpression.Update(
                VisitInternal<TableExpressionBase>(crossApplyExpression.Table).ResultExpression);
        }

        protected override Expression VisitCrossJoin(CrossJoinExpression crossJoinExpression)
        {
            Check.NotNull(crossJoinExpression, nameof(crossJoinExpression));

            return crossJoinExpression.Update(
                VisitInternal<TableExpressionBase>(crossJoinExpression.Table).ResultExpression);
        }

        protected override Expression VisitExcept(ExceptExpression exceptExpression)
        {
            Check.NotNull(exceptExpression, nameof(exceptExpression));

            var source1 = VisitInternal<SelectExpression>(exceptExpression.Source1).ResultExpression;
            var source2 = VisitInternal<SelectExpression>(exceptExpression.Source2).ResultExpression;

            return exceptExpression.Update(source1, source2);
        }

        protected override Expression VisitExists(ExistsExpression existsExpression)
        {
            Check.NotNull(existsExpression, nameof(existsExpression));

            return existsExpression.Update(
                VisitInternal<SelectExpression>(existsExpression.Subquery).ResultExpression);
        }

        protected override Expression VisitFromSql(FromSqlExpression fromSqlExpression)
            => Check.NotNull(fromSqlExpression, nameof(fromSqlExpression));

        protected override Expression VisitIn(InExpression inExpression)
        {
            Check.NotNull(inExpression, nameof(inExpression));

            var (item, itemNullable) = VisitInternal<SqlExpression>(inExpression.Item);

            if (inExpression.Subquery != null)
            {
                var (subquery, subqueryNullable) = VisitInternal<SelectExpression>(inExpression.Subquery);
                _nullable = itemNullable || subqueryNullable;

                return inExpression.Update(item, values: null, subquery);
            }

            // for relational null semantics just leave as is
            // same for values we don't know how to properly handle (i.e. other than constant or parameter)
            if (UseRelationalNulls
                || !(inExpression.Values is SqlConstantExpression || inExpression.Values is SqlParameterExpression))
            {
                var (values, valuesNullable) = VisitInternal<SqlExpression>(inExpression.Values);
                _nullable = itemNullable || valuesNullable;

                return inExpression.Update(item, values, subquery: null);
            }

            // for c# null semantics we need to remove nulls from Values and add IsNull/IsNotNull when necessary
            var (inValuesExpression, inValuesList, hasNullValue) = ProcessInExpressionValues(inExpression.Values);

            // either values array is empty or only contains null
            if (inValuesList.Count == 0)
            {
                _nullable = false;

                // a IN () -> false
                // non_nullable IN (NULL) -> false
                // a NOT IN () -> true
                // non_nullable NOT IN (NULL) -> true
                // nullable IN (NULL) -> nullable IS NULL
                // nullable NOT IN (NULL) -> nullable IS NOT NULL
                return !hasNullValue || !itemNullable
                    ? (SqlExpression)SqlExpressionFactory.Constant(
                        inExpression.IsNegated,
                        inExpression.TypeMapping)
                    : inExpression.IsNegated
                        ? SqlExpressionFactory.IsNotNull(item)
                        : SqlExpressionFactory.IsNull(item);
            }

            if (!itemNullable
                || (_allowOptimizedExpansion && !inExpression.IsNegated && !hasNullValue))
            {
                _nullable = itemNullable;

                // non_nullable IN (1, 2) -> non_nullable IN (1, 2)
                // non_nullable IN (1, 2, NULL) -> non_nullable IN (1, 2)
                // non_nullable NOT IN (1, 2) -> non_nullable NOT IN (1, 2)
                // non_nullable NOT IN (1, 2, NULL) -> non_nullable NOT IN (1, 2)
                // nullable IN (1, 2) -> nullable IN (1, 2) (optimized)
                return inExpression.Update(item, inValuesExpression, subquery: null);
            }

            // adding null comparison term to remove nulls completely from the resulting expression
            _nullable = false;

            // nullable IN (1, 2) -> nullable IN (1, 2) AND nullable IS NOT NULL (full)
            // nullable IN (1, 2, NULL) -> nullable IN (1, 2) OR nullable IS NULL (full)
            // nullable NOT IN (1, 2) -> nullable NOT IN (1, 2) OR nullable IS NULL (full)
            // nullable NOT IN (1, 2, NULL) -> nullable NOT IN (1, 2) AND nullable IS NOT NULL (full)
            return inExpression.IsNegated == hasNullValue
                ? SqlExpressionFactory.AndAlso(
                    inExpression.Update(item, inValuesExpression, subquery: null),
                    SqlExpressionFactory.IsNotNull(item))
                : SqlExpressionFactory.OrElse(
                    inExpression.Update(item, inValuesExpression, subquery: null),
                    SqlExpressionFactory.IsNull(item));

            (SqlConstantExpression ProcessedValuesExpression, List<object> ProcessedValuesList, bool HasNullValue) ProcessInExpressionValues(SqlExpression valuesExpression)
            {
                var inValues = new List<object>();
                var hasNullValue = false;
                RelationalTypeMapping typeMapping = null;

                IEnumerable values = null;
                if (valuesExpression is SqlConstantExpression sqlConstant)
                {
                    typeMapping = sqlConstant.TypeMapping;
                    values = (IEnumerable)sqlConstant.Value;
                }
                else if (valuesExpression is SqlParameterExpression sqlParameter)
                {
                    CanCache = false;
                    typeMapping = sqlParameter.TypeMapping;
                    values = (IEnumerable)ParameterValues[sqlParameter.Name];
                }

                foreach (var value in values)
                {
                    if (value == null)
                    {
                        hasNullValue = true;
                        continue;
                    }

                    inValues.Add(value);
                }

                var processedValuesExpression = SqlExpressionFactory.Constant(inValues, typeMapping);

                return (processedValuesExpression, inValues, hasNullValue);
            }
        }

        protected override Expression VisitInnerJoin(InnerJoinExpression innerJoinExpression)
        {
            Check.NotNull(innerJoinExpression, nameof(innerJoinExpression));

            var newTable = VisitInternal<TableExpressionBase>(innerJoinExpression.Table).ResultExpression;
            var newJoinPredicate = VisitJoinPredicate((SqlBinaryExpression)innerJoinExpression.JoinPredicate);

            return newJoinPredicate is SqlConstantExpression constantJoinPredicate
                && constantJoinPredicate.Value is bool boolPredicate
                && boolPredicate
                ? (Expression)new CrossJoinExpression(newTable)
                : innerJoinExpression.Update(newTable, newJoinPredicate);
        }

        protected override Expression VisitIntersect(IntersectExpression intersectExpression)
        {
            Check.NotNull(intersectExpression, nameof(intersectExpression));

            var source1 = VisitInternal<SelectExpression>(intersectExpression.Source1).ResultExpression;
            var source2 = VisitInternal<SelectExpression>(intersectExpression.Source2).ResultExpression;

            return intersectExpression.Update(source1, source2);
        }

        protected override Expression VisitLeftJoin(LeftJoinExpression leftJoinExpression)
        {
            Check.NotNull(leftJoinExpression, nameof(leftJoinExpression));

            var newTable = VisitInternal<TableExpressionBase>(leftJoinExpression.Table).ResultExpression;
            var newJoinPredicate = VisitJoinPredicate((SqlBinaryExpression)leftJoinExpression.JoinPredicate);

            return leftJoinExpression.Update(newTable, newJoinPredicate);
        }

        private SqlExpression VisitJoinPredicate(SqlBinaryExpression predicate)
        {
            switch (predicate.OperatorType)
            {
                case ExpressionType.Equal:
                {
                    var (left, leftNullable) = VisitInternal<SqlExpression>(predicate.Left, allowOptimizedExpansion: true);
                    var (right, rightNullable) = VisitInternal<SqlExpression>(predicate.Right, allowOptimizedExpansion: true);

                    var result = OptimizeComparison(
                        predicate.Update(left, right),
                        left,
                        right,
                        leftNullable,
                        rightNullable);

                    return result;
                }

                case ExpressionType.AndAlso:
                    return VisitInternal<SqlExpression>(predicate, allowOptimizedExpansion: true).ResultExpression;

                default:
                    throw new InvalidOperationException("Unexpected join predicate shape: " + predicate);
            }
        }

        protected override Expression VisitLike(LikeExpression likeExpression)
        {
            var (match, matchNullable) = VisitInternal<SqlExpression>(likeExpression.Match);
            var (pattern, patternNullable) = VisitInternal<SqlExpression>(likeExpression.Pattern);
            var (escapeChar, escapeCharNullable) = VisitInternal<SqlExpression>(likeExpression.EscapeChar);
            _nullable = matchNullable || patternNullable || escapeCharNullable;

            return likeExpression.Update(match, pattern, escapeChar);
        }

        protected override Expression VisitOrdering(OrderingExpression orderingExpression)
        {
            Check.NotNull(orderingExpression, nameof(orderingExpression));

            return orderingExpression.Update(
                VisitInternal<SqlExpression>(orderingExpression.Expression).ResultExpression);
        }

        protected override Expression VisitOuterApply(OuterApplyExpression outerApplyExpression)
        {
            Check.NotNull(outerApplyExpression, nameof(outerApplyExpression));

            return outerApplyExpression.Update(
                VisitInternal<TableExpressionBase>(outerApplyExpression.Table).ResultExpression);
        }

        protected override Expression VisitProjection(ProjectionExpression projectionExpression)
        {
            Check.NotNull(projectionExpression, nameof(projectionExpression));

            return projectionExpression.Update(
                VisitInternal<SqlExpression>(projectionExpression.Expression).ResultExpression);
        }

        protected override Expression VisitRowNumber(RowNumberExpression rowNumberExpression)
        {
            Check.NotNull(rowNumberExpression, nameof(rowNumberExpression));

            var changed = false;
            var partitions = new List<SqlExpression>();
            foreach (var partition in rowNumberExpression.Partitions)
            {
                var newPartition = VisitInternal<SqlExpression>(partition).ResultExpression;
                changed |= newPartition != partition;
                partitions.Add(newPartition);
            }

            var orderings = new List<OrderingExpression>();
            foreach (var ordering in rowNumberExpression.Orderings)
            {
                var newOrdering = VisitInternal<OrderingExpression>(ordering).ResultExpression;
                changed |= newOrdering != ordering;
                orderings.Add(newOrdering);
            }

            return rowNumberExpression.Update(partitions, orderings);
        }

        protected override Expression VisitScalarSubquery(ScalarSubqueryExpression scalarSubqueryExpression)
        {
            Check.NotNull(scalarSubqueryExpression, nameof(scalarSubqueryExpression));

            return scalarSubqueryExpression.Update(
                VisitInternal<SelectExpression>(scalarSubqueryExpression.Subquery).ResultExpression);
        }

        protected override Expression VisitSelect(SelectExpression selectExpression)
        {
            Check.NotNull(selectExpression, nameof(selectExpression));

            var changed = false;
            var projections = new List<ProjectionExpression>();
            foreach (var item in selectExpression.Projection)
            {
                var updatedProjection = VisitInternal<ProjectionExpression>(item).ResultExpression;
                projections.Add(updatedProjection);
                changed |= updatedProjection != item;
            }

            var tables = new List<TableExpressionBase>();
            foreach (var table in selectExpression.Tables)
            {
                var newTable = VisitInternal<TableExpressionBase>(table).ResultExpression;
                changed |= newTable != table;
                tables.Add(newTable);
            }

            var predicate = VisitInternal<SqlExpression>(selectExpression.Predicate, allowOptimizedExpansion: true).ResultExpression;
            changed |= predicate != selectExpression.Predicate;

            if (predicate is SqlConstantExpression predicateConstantExpression
                && predicateConstantExpression.Value is bool predicateBoolValue
                && predicateBoolValue)
            {
                predicate = null;
                changed = true;
            }

            var groupBy = new List<SqlExpression>();
            foreach (var groupingKey in selectExpression.GroupBy)
            {
                var newGroupingKey = VisitInternal<SqlExpression>(groupingKey).ResultExpression;
                changed |= newGroupingKey != groupingKey;
                groupBy.Add(newGroupingKey);
            }

            var having = VisitInternal<SqlExpression>(selectExpression.Having, allowOptimizedExpansion: true).ResultExpression;
            changed |= having != selectExpression.Having;

            if (having is SqlConstantExpression havingConstantExpression
                && havingConstantExpression.Value is bool havingBoolValue
                && havingBoolValue)
            {
                having = null;
                changed = true;
            }

            var orderings = new List<OrderingExpression>();
            foreach (var ordering in selectExpression.Orderings)
            {
                var orderingExpression = VisitInternal<SqlExpression>(ordering.Expression).ResultExpression;
                changed |= orderingExpression != ordering.Expression;
                orderings.Add(ordering.Update(orderingExpression));
            }

            var offset = VisitInternal<SqlExpression>(selectExpression.Offset).ResultExpression;
            changed |= offset != selectExpression.Offset;

            var limit = VisitInternal<SqlExpression>(selectExpression.Limit).ResultExpression;
            changed |= limit != selectExpression.Limit;

            // SelectExpression can always yield null
            // (e.g. projecting non-nullable column but with predicate that filters out all rows)
            _nullable = true;

            return changed
                ? selectExpression.Update(
                    projections, tables, predicate, groupBy, having, orderings, limit, offset, selectExpression.IsDistinct,
                    selectExpression.Alias)
                : selectExpression;
        }

        protected override Expression VisitSqlBinary(SqlBinaryExpression sqlBinaryExpression)
        {
            Check.NotNull(sqlBinaryExpression, nameof(sqlBinaryExpression));

            _nullable = false;
            var optimize = _allowOptimizedExpansion;

            _allowOptimizedExpansion = _allowOptimizedExpansion
                && (sqlBinaryExpression.OperatorType == ExpressionType.AndAlso
                    || sqlBinaryExpression.OperatorType == ExpressionType.OrElse);

            var currentNonNullableColumnsCount = NonNullableColumns.Count;

            var (left, leftNullable) = VisitInternal<SqlExpression>(
                sqlBinaryExpression.Left,
                allowOptimizedExpansion: _allowOptimizedExpansion,
                restoreNonNullableColumnInformation: false);

            var leftNonNullableColumns = NonNullableColumns.Skip(currentNonNullableColumnsCount).ToList();
            if (sqlBinaryExpression.OperatorType != ExpressionType.AndAlso)
            {
                RestoreNonNullableColumnsList(currentNonNullableColumnsCount);
            }

            var (right, rightNullable) = VisitInternal<SqlExpression>(
                sqlBinaryExpression.Right,
                allowOptimizedExpansion: _allowOptimizedExpansion,
                restoreNonNullableColumnInformation: false);

            if (sqlBinaryExpression.OperatorType == ExpressionType.OrElse)
            {
                var intersect = leftNonNullableColumns.Intersect(NonNullableColumns.Skip(currentNonNullableColumnsCount)).ToList();
                RestoreNonNullableColumnsList(currentNonNullableColumnsCount);
                NonNullableColumns.AddRange(intersect);
            }
            else if (sqlBinaryExpression.OperatorType != ExpressionType.AndAlso)
            {
                // in case of AndAlso we already have what we need as the column information propagates from left to right
                RestoreNonNullableColumnsList(currentNonNullableColumnsCount);
            }

            // nullableStringColumn + NULL -> COALESCE(nullableStringColumn, "") + ""
            if (sqlBinaryExpression.OperatorType == ExpressionType.Add
                && sqlBinaryExpression.Type == typeof(string))
            {
                if (leftNullable)
                {
                    left = AddNullConcatenationProtection(left, sqlBinaryExpression.TypeMapping);
                }

                if (rightNullable)
                {
                    right = AddNullConcatenationProtection(right, sqlBinaryExpression.TypeMapping);
                }

                return sqlBinaryExpression.Update(left, right);
            }

            if (sqlBinaryExpression.OperatorType == ExpressionType.Equal
                || sqlBinaryExpression.OperatorType == ExpressionType.NotEqual)
            {
                var updated = sqlBinaryExpression.Update(left, right);

                var optimized = OptimizeComparison(
                    updated,
                    left,
                    right,
                    leftNullable,
                    rightNullable);

                if (optimized is SqlUnaryExpression optimizedUnary
                    && optimizedUnary.OperatorType == ExpressionType.NotEqual
                    && optimizedUnary.Operand is ColumnExpression optimizedUnaryColumnOperand)
                {
                    NonNullableColumns.Add(optimizedUnaryColumnOperand);
                }

                // we assume that NullSemantics rewrite is only needed (on the current level)
                // if the optimization didn't make any changes.
                // Reason is that optimization can/will change the nullability of the resulting expression
                // and that inforation is not tracked/stored anywhere
                // so we can no longer rely on nullabilities that we computed earlier (leftNullable, rightNullable)
                // when performing null semantics rewrite.
                // It should be fine because current optimizations *radically* change the expression
                // (e.g. binary -> unary, or binary -> constant)
                // but we need to pay attention in the future if we introduce more subtle transformations here
                if (optimized.Equals(updated)
                    && (leftNullable || rightNullable)
                    && !UseRelationalNulls)
                {
                    var rewriteNullSemanticsResult = RewriteNullSemantics(
                        updated,
                        updated.Left,
                        updated.Right,
                        leftNullable,
                        rightNullable,
                        optimize);

                    _allowOptimizedExpansion = optimize;

                    return rewriteNullSemanticsResult;
                }

                _allowOptimizedExpansion = optimize;

                return optimized;
            }

            _nullable = leftNullable || rightNullable;
            _allowOptimizedExpansion = optimize;

            var result = sqlBinaryExpression.Update(left, right);

            return result is SqlBinaryExpression sqlBinaryResult
                && (sqlBinaryExpression.OperatorType == ExpressionType.AndAlso
                    || sqlBinaryExpression.OperatorType == ExpressionType.OrElse)
                ? SimplifyLogicalSqlBinaryExpression(sqlBinaryResult)
                : result;

            SqlExpression AddNullConcatenationProtection(SqlExpression argument, RelationalTypeMapping typeMapping)
                => argument is SqlConstantExpression || argument is SqlParameterExpression
                ? (SqlExpression)SqlExpressionFactory.Constant(string.Empty, typeMapping)
                : SqlExpressionFactory.Coalesce(argument, SqlExpressionFactory.Constant(string.Empty, typeMapping));
        }

        private SqlExpression OptimizeComparison(
            SqlBinaryExpression sqlBinaryExpression,
            SqlExpression left,
            SqlExpression right,
            bool leftNullable,
            bool rightNullable)
        {
            var leftNullValue = leftNullable && (left is SqlConstantExpression || left is SqlParameterExpression);
            var rightNullValue = rightNullable && (right is SqlConstantExpression || right is SqlParameterExpression);

            // a == null -> a IS NULL
            // a != null -> a IS NOT NULL
            if (rightNullValue)
            {
                var result = sqlBinaryExpression.OperatorType == ExpressionType.Equal
                    ? ProcessNullNotNull(SqlExpressionFactory.IsNull(left), leftNullable)
                    : ProcessNullNotNull(SqlExpressionFactory.IsNotNull(left), leftNullable);

                _nullable = false;

                return result;
            }

            // null == a -> a IS NULL
            // null != a -> a IS NOT NULL
            if (leftNullValue)
            {
                var result = sqlBinaryExpression.OperatorType == ExpressionType.Equal
                    ? ProcessNullNotNull(SqlExpressionFactory.IsNull(right), rightNullable)
                    : ProcessNullNotNull(SqlExpressionFactory.IsNotNull(right), rightNullable);

                _nullable = false;

                return result;
            }

            if (IsTrueOrFalse(right) is bool rightTrueFalseValue
                && !leftNullable)
            {
                _nullable = leftNullable;

                // only correct in 2-value logic
                // a == true -> a
                // a == false -> !a
                // a != true -> !a
                // a != false -> a
                return sqlBinaryExpression.OperatorType == ExpressionType.Equal ^ rightTrueFalseValue
                    ? OptimizeNonNullableNotExpression(SqlExpressionFactory.Not(left))
                    : left;
            }

            if (IsTrueOrFalse(left) is bool leftTrueFalseValue
                && !rightNullable)
            {
                _nullable = rightNullable;

                // only correct in 2-value logic
                // true == a -> a
                // false == a -> !a
                // true != a -> !a
                // false != a -> a
                return sqlBinaryExpression.OperatorType == ExpressionType.Equal ^ leftTrueFalseValue
                    ? SqlExpressionFactory.Not(right)
                    : right;
            }

            // only correct in 2-value logic
            // a == a -> true
            // a != a -> false
            if (!leftNullable
                && left.Equals(right))
            {
                _nullable = false;

                return SqlExpressionFactory.Constant(
                    sqlBinaryExpression.OperatorType == ExpressionType.Equal,
                    sqlBinaryExpression.TypeMapping);
            }

            if (!leftNullable
                && !rightNullable
                && (sqlBinaryExpression.OperatorType == ExpressionType.Equal || sqlBinaryExpression.OperatorType == ExpressionType.NotEqual))
            {
                var leftUnary = left as SqlUnaryExpression;
                var rightUnary = right as SqlUnaryExpression;

                var leftNegated = leftUnary?.IsLogicalNot() == true;
                var rightNegated = rightUnary?.IsLogicalNot() == true;

                if (leftNegated)
                {
                    left = leftUnary.Operand;
                }

                if (rightNegated)
                {
                    right = rightUnary.Operand;
                }

                // a == b <=> !a == !b -> a == b
                // !a == b <=> a == !b -> a != b
                // a != b <=> !a != !b -> a != b
                // !a != b <=> a != !b -> a == b
                return sqlBinaryExpression.OperatorType == ExpressionType.Equal ^ leftNegated == rightNegated
                    ? SqlExpressionFactory.NotEqual(left, right)
                    : SqlExpressionFactory.Equal(left, right);
            }

            return sqlBinaryExpression.Update(left, right);

            bool? IsTrueOrFalse(SqlExpression sqlExpression)
            {
                if (sqlExpression is SqlConstantExpression sqlConstantExpression && sqlConstantExpression.Value is bool boolConstant)
                {
                    return boolConstant;
                }

                return null;
            }
        }

        private SqlExpression RewriteNullSemantics(
            SqlBinaryExpression sqlBinaryExpression,
            SqlExpression left,
            SqlExpression right,
            bool leftNullable,
            bool rightNullable,
            bool optimize)
        {
            var leftUnary = left as SqlUnaryExpression;
            var rightUnary = right as SqlUnaryExpression;

            var leftNegated = leftUnary?.IsLogicalNot() == true;
            var rightNegated = rightUnary?.IsLogicalNot() == true;

            if (leftNegated)
            {
                left = leftUnary.Operand;
            }

            if (rightNegated)
            {
                right = rightUnary.Operand;
            }

            var leftIsNull = ProcessNullNotNull(SqlExpressionFactory.IsNull(left), leftNullable);
            var leftIsNotNull = OptimizeNonNullableNotExpression(SqlExpressionFactory.Not(leftIsNull));

            var rightIsNull = ProcessNullNotNull(SqlExpressionFactory.IsNull(right), rightNullable);
            var rightIsNotNull = OptimizeNonNullableNotExpression(SqlExpressionFactory.Not(rightIsNull));

            // optimized expansion which doesn't distinguish between null and false
            if (optimize
                && sqlBinaryExpression.OperatorType == ExpressionType.Equal
                && !leftNegated
                && !rightNegated)
            {
                // when we use optimized form, the result can still be nullable
                if (leftNullable && rightNullable)
                {
                    _nullable = true;

                    return SimplifyLogicalSqlBinaryExpression(
                        SqlExpressionFactory.OrElse(
                            SqlExpressionFactory.Equal(left, right),
                            SimplifyLogicalSqlBinaryExpression(
                                SqlExpressionFactory.AndAlso(leftIsNull, rightIsNull))));
                }

                if ((leftNullable && !rightNullable)
                    || (!leftNullable && rightNullable))
                {
                    _nullable = true;

                    return SqlExpressionFactory.Equal(left, right);
                }
            }

            // doing a full null semantics rewrite - removing all nulls from truth table
            _nullable = false;

            if (sqlBinaryExpression.OperatorType == ExpressionType.Equal)
            {
                if (leftNullable && rightNullable)
                {
                    // ?a == ?b <=> !(?a) == !(?b) -> [(a == b) && (a != null && b != null)] || (a == null && b == null))
                    // !(?a) == ?b <=> ?a == !(?b) -> [(a != b) && (a != null && b != null)] || (a == null && b == null)
                    return leftNegated == rightNegated
                        ? ExpandNullableEqualNullable(left, right, leftIsNull, leftIsNotNull, rightIsNull, rightIsNotNull)
                        : ExpandNegatedNullableEqualNullable(left, right, leftIsNull, leftIsNotNull, rightIsNull, rightIsNotNull);
                }

                if (leftNullable && !rightNullable)
                {
                    // ?a == b <=> !(?a) == !b -> (a == b) && (a != null)
                    // !(?a) == b <=> ?a == !b -> (a != b) && (a != null)
                    return leftNegated == rightNegated
                        ? ExpandNullableEqualNonNullable(left, right, leftIsNotNull)
                        : ExpandNegatedNullableEqualNonNullable(left, right, leftIsNotNull);
                }

                if (rightNullable && !leftNullable)
                {
                    // a == ?b <=> !a == !(?b) -> (a == b) && (b != null)
                    // !a == ?b <=> a == !(?b) -> (a != b) && (b != null)
                    return leftNegated == rightNegated
                        ? ExpandNullableEqualNonNullable(left, right, rightIsNotNull)
                        : ExpandNegatedNullableEqualNonNullable(left, right, rightIsNotNull);
                }
            }

            if (sqlBinaryExpression.OperatorType == ExpressionType.NotEqual)
            {
                if (leftNullable && rightNullable)
                {
                    // ?a != ?b <=> !(?a) != !(?b) -> [(a != b) || (a == null || b == null)] && (a != null || b != null)
                    // !(?a) != ?b <=> ?a != !(?b) -> [(a == b) || (a == null || b == null)] && (a != null || b != null)
                    return leftNegated == rightNegated
                        ? ExpandNullableNotEqualNullable(left, right, leftIsNull, leftIsNotNull, rightIsNull, rightIsNotNull)
                        : ExpandNegatedNullableNotEqualNullable(left, right, leftIsNull, leftIsNotNull, rightIsNull, rightIsNotNull);
                }

                if (leftNullable && !rightNullable)
                {
                    // ?a != b <=> !(?a) != !b -> (a != b) || (a == null)
                    // !(?a) != b <=> ?a != !b -> (a == b) || (a == null)
                    return leftNegated == rightNegated
                        ? ExpandNullableNotEqualNonNullable(left, right, leftIsNull)
                        : ExpandNegatedNullableNotEqualNonNullable(left, right, leftIsNull);
                }

                if (rightNullable && !leftNullable)
                {
                    // a != ?b <=> !a != !(?b) -> (a != b) || (b == null)
                    // !a != ?b <=> a != !(?b) -> (a == b) || (b == null)
                    return leftNegated == rightNegated
                        ? ExpandNullableNotEqualNonNullable(left, right, rightIsNull)
                        : ExpandNegatedNullableNotEqualNonNullable(left, right, rightIsNull);
                }
            }

            return sqlBinaryExpression.Update(left, right);
        }

        private SqlExpression SimplifyLogicalSqlBinaryExpression(SqlBinaryExpression sqlBinaryExpression)
        {
            var leftUnary = sqlBinaryExpression.Left as SqlUnaryExpression;
            var rightUnary = sqlBinaryExpression.Right as SqlUnaryExpression;
            if (leftUnary != null
                && rightUnary != null
                && (leftUnary.OperatorType == ExpressionType.Equal || leftUnary.OperatorType == ExpressionType.NotEqual)
                && (rightUnary.OperatorType == ExpressionType.Equal || rightUnary.OperatorType == ExpressionType.NotEqual)
                && leftUnary.Operand.Equals(rightUnary.Operand))
            {
                // a is null || a is null -> a is null
                // a is not null || a is not null -> a is not null
                // a is null && a is null -> a is null
                // a is not null && a is not null -> a is not null
                // a is null || a is not null -> true
                // a is null && a is not null -> false
                return leftUnary.OperatorType == rightUnary.OperatorType
                    ? (SqlExpression)leftUnary
                    : SqlExpressionFactory.Constant(sqlBinaryExpression.OperatorType == ExpressionType.OrElse, sqlBinaryExpression.TypeMapping);
            }

            // true && a -> a
            // true || a -> true
            // false && a -> false
            // false || a -> a
            if (sqlBinaryExpression.Left is SqlConstantExpression newLeftConstant)
            {
                return sqlBinaryExpression.OperatorType == ExpressionType.AndAlso
                    ? (bool)newLeftConstant.Value
                        ? sqlBinaryExpression.Right
                        : newLeftConstant
                    : (bool)newLeftConstant.Value
                        ? newLeftConstant
                        : sqlBinaryExpression.Right;
            }
            else if (sqlBinaryExpression.Right is SqlConstantExpression newRightConstant)
            {
                // a && true -> a
                // a || true -> true
                // a && false -> false
                // a || false -> a
                return sqlBinaryExpression.OperatorType == ExpressionType.AndAlso
                    ? (bool)newRightConstant.Value
                        ? sqlBinaryExpression.Left
                        : newRightConstant
                    : (bool)newRightConstant.Value
                        ? newRightConstant
                        : sqlBinaryExpression.Left;
            }

            return sqlBinaryExpression;
        }

        protected override Expression VisitSqlConstant(SqlConstantExpression sqlConstantExpression)
        {
            Check.NotNull(sqlConstantExpression, nameof(sqlConstantExpression));

            _nullable = sqlConstantExpression.Value == null;

            return sqlConstantExpression;
        }

        protected override Expression VisitSqlFragment(SqlFragmentExpression sqlFragmentExpression)
        {
            Check.NotNull(sqlFragmentExpression, nameof(sqlFragmentExpression));

            return sqlFragmentExpression;
        }

        protected override Expression VisitSqlFunction(SqlFunctionExpression sqlFunctionExpression)
        {
            Check.NotNull(sqlFunctionExpression, nameof(sqlFunctionExpression));

            if (sqlFunctionExpression.IsBuiltIn
                && string.Equals(sqlFunctionExpression.Name, "COALESCE", StringComparison.OrdinalIgnoreCase))
            {
                var (left, leftNullable) = VisitInternal<SqlExpression>(sqlFunctionExpression.Arguments[0]);
                var (right, rightNullable) = VisitInternal<SqlExpression>(sqlFunctionExpression.Arguments[1]);
                _nullable = leftNullable && rightNullable;

                return sqlFunctionExpression.Update(sqlFunctionExpression.Instance, new[] { left, right });
            }

            var (instance, _) = VisitInternal<SqlExpression>(sqlFunctionExpression.Instance);

            if (sqlFunctionExpression.IsNiladic)
            {
                // TODO: #18555
                _nullable = true;

                return sqlFunctionExpression.Update(instance, sqlFunctionExpression.Arguments);
            }

            var arguments = new SqlExpression[sqlFunctionExpression.Arguments.Count];
            for (var i = 0; i < arguments.Length; i++)
            {
                (arguments[i], _) = VisitInternal<SqlExpression>(sqlFunctionExpression.Arguments[i]);
            }

            // TODO: #18555
            _nullable = true;

            return sqlFunctionExpression.Update(instance, arguments);
        }

        protected override Expression VisitSqlParameter(SqlParameterExpression sqlParameterExpression)
        {
            Check.NotNull(sqlParameterExpression, nameof(sqlParameterExpression));

            _nullable = ParameterValues[sqlParameterExpression.Name] == null;

            return _nullable
                ? SqlExpressionFactory.Constant(null, sqlParameterExpression.TypeMapping)
                : (SqlExpression)sqlParameterExpression;
        }

        protected override Expression VisitSqlUnary(SqlUnaryExpression sqlUnaryExpression)
        {
            Check.NotNull(sqlUnaryExpression, nameof(sqlUnaryExpression));

            var (operand, operandNullable) = VisitInternal<SqlExpression>(sqlUnaryExpression.Operand);
            var updated = sqlUnaryExpression.Update(operand);

            if (sqlUnaryExpression.OperatorType == ExpressionType.Equal
                || sqlUnaryExpression.OperatorType == ExpressionType.NotEqual)
            {
                var result = ProcessNullNotNull(updated, operandNullable);

                // result of IsNull/IsNotNull can never be null
                _nullable = false;

                if (result is SqlUnaryExpression resultUnary
                    && resultUnary.OperatorType == ExpressionType.NotEqual
                    && resultUnary.Operand is ColumnExpression resultColumnOperand)
                {
                    NonNullableColumns.Add(resultColumnOperand);
                }

                return result;
            }

            return !_nullable && sqlUnaryExpression.OperatorType == ExpressionType.Not
                ? OptimizeNonNullableNotExpression(updated)
                : updated;
        }

        private SqlExpression OptimizeNonNullableNotExpression(SqlUnaryExpression sqlUnaryExpression)
        {
            if (sqlUnaryExpression.OperatorType != ExpressionType.Not)
            {
                return sqlUnaryExpression;
            }

            switch (sqlUnaryExpression.Operand)
            {
                // !(true) -> false
                // !(false) -> true
                case SqlConstantExpression constantOperand
                    when constantOperand.Value is bool value:
                {
                    return SqlExpressionFactory.Constant(!value, sqlUnaryExpression.TypeMapping);
                }

                case InExpression inOperand:
                    return inOperand.Negate();

                case SqlUnaryExpression sqlUnaryOperand:
                {
                    switch (sqlUnaryOperand.OperatorType)
                    {
                        // !(!a) -> a
                        case ExpressionType.Not:
                            return sqlUnaryOperand.Operand;

                        //!(a IS NULL) -> a IS NOT NULL
                        case ExpressionType.Equal:
                            return SqlExpressionFactory.IsNotNull(sqlUnaryOperand.Operand);

                        //!(a IS NOT NULL) -> a IS NULL
                        case ExpressionType.NotEqual:
                            return SqlExpressionFactory.IsNull(sqlUnaryOperand.Operand);
                    }
                    break;
                }

                case SqlBinaryExpression sqlBinaryOperand:
                {
                    // optimizations below are only correct in 2-value logic
                    // De Morgan's
                    if (sqlBinaryOperand.OperatorType == ExpressionType.AndAlso
                        || sqlBinaryOperand.OperatorType == ExpressionType.OrElse)
                    {
                        // since entire AndAlso/OrElse expression is non-nullable, both sides of it (left and right) must also be non-nullable
                        // so it's safe to perform recursive optimization here
                        var left = OptimizeNonNullableNotExpression(SqlExpressionFactory.Not(sqlBinaryOperand.Left));
                        var right = OptimizeNonNullableNotExpression(SqlExpressionFactory.Not(sqlBinaryOperand.Right));

                        return SimplifyLogicalSqlBinaryExpression(
                            SqlExpressionFactory.MakeBinary(
                                sqlBinaryOperand.OperatorType == ExpressionType.AndAlso
                                    ? ExpressionType.OrElse
                                    : ExpressionType.AndAlso,
                                left,
                                right,
                                sqlBinaryOperand.TypeMapping));
                    }

                    // !(a == b) -> a != b
                    // !(a != b) -> a == b
                    // !(a > b) -> a <= b
                    // !(a >= b) -> a < b
                    // !(a < b) -> a >= b
                    // !(a <= b) -> a > b
                    if (TryNegate(sqlBinaryOperand.OperatorType, out var negated))
                    {
                        return SqlExpressionFactory.MakeBinary(
                            negated,
                            sqlBinaryOperand.Left,
                            sqlBinaryOperand.Right,
                            sqlBinaryOperand.TypeMapping);
                    }
                }
                break;
            }

            return sqlUnaryExpression;

            static bool TryNegate(ExpressionType expressionType, out ExpressionType result)
            {
                var negated = expressionType switch
                {
                    ExpressionType.Equal => ExpressionType.NotEqual,
                    ExpressionType.NotEqual => ExpressionType.Equal,
                    ExpressionType.GreaterThan => ExpressionType.LessThanOrEqual,
                    ExpressionType.GreaterThanOrEqual => ExpressionType.LessThan,
                    ExpressionType.LessThan => ExpressionType.GreaterThanOrEqual,
                    ExpressionType.LessThanOrEqual => ExpressionType.GreaterThan,
                    _ => (ExpressionType?)null
                };

                result = negated ?? default;

                return negated.HasValue;
            }
        }

        protected virtual SqlExpression ProcessNullNotNull(
            [NotNull] SqlUnaryExpression sqlUnaryExpression,
            bool? operandNullable)
        {
            Check.NotNull(sqlUnaryExpression, nameof(sqlUnaryExpression));

            if (operandNullable == false)
            {
                // when we know that operand is non-nullable:
                // not_null_operand is null-> false
                // not_null_operand is not null -> true
                return SqlExpressionFactory.Constant(
                    sqlUnaryExpression.OperatorType == ExpressionType.NotEqual,
                    sqlUnaryExpression.TypeMapping);
            }

            switch (sqlUnaryExpression.Operand)
            {
                case SqlConstantExpression sqlConstantOperand:
                    // null_value_constant is null -> true
                    // null_value_constant is not null -> false
                    // not_null_value_constant is null -> false
                    // not_null_value_constant is not null -> true
                    return SqlExpressionFactory.Constant(
                        sqlConstantOperand.Value == null ^ sqlUnaryExpression.OperatorType == ExpressionType.NotEqual,
                        sqlUnaryExpression.TypeMapping);

                case SqlParameterExpression sqlParameterOperand:
                    // null_value_parameter is null -> true
                    // null_value_parameter is not null -> false
                    // not_null_value_parameter is null -> false
                    // not_null_value_parameter is not null -> true
                    return SqlExpressionFactory.Constant(
                        ParameterValues[sqlParameterOperand.Name] == null ^ sqlUnaryExpression.OperatorType == ExpressionType.NotEqual,
                        sqlUnaryExpression.TypeMapping);

                case ColumnExpression columnOperand
                    when !columnOperand.IsNullable || NonNullableColumns.Contains(columnOperand):
                {
                    // IsNull(non_nullable_column) -> false
                    // IsNotNull(non_nullable_column) -> true
                    return SqlExpressionFactory.Constant(
                        sqlUnaryExpression.OperatorType == ExpressionType.NotEqual,
                        sqlUnaryExpression.TypeMapping);
                }

                case SqlUnaryExpression sqlUnaryOperand:
                    switch (sqlUnaryOperand.OperatorType)
                    {
                        case ExpressionType.Convert:
                        case ExpressionType.Not:
                        case ExpressionType.Negate:
                            // op(a) is null -> a is null
                            // op(a) is not null -> a is not null
                            return ProcessNullNotNull(
                                sqlUnaryExpression.Update(sqlUnaryOperand.Operand),
                                operandNullable);

                        case ExpressionType.Equal:
                        case ExpressionType.NotEqual:
                            // (a is null) is null -> false
                            // (a is not null) is null -> false
                            // (a is null) is not null -> true
                            // (a is not null) is not null -> true
                            return SqlExpressionFactory.Constant(
                                sqlUnaryOperand.OperatorType == ExpressionType.NotEqual,
                                sqlUnaryOperand.TypeMapping);
                    }
                    break;

                case SqlBinaryExpression sqlBinaryOperand
                    when sqlBinaryOperand.OperatorType != ExpressionType.AndAlso
                        && sqlBinaryOperand.OperatorType != ExpressionType.OrElse:
                {
                    // in general:
                    // binaryOp(a, b) == null -> a == null || b == null
                    // binaryOp(a, b) != null -> a != null && b != null
                    // for AndAlso, OrElse we can't do this optimization
                    // we could do something like this, but it seems too complicated:
                    // (a && b) == null -> a == null && b != 0 || a != 0 && b == null
                    // NOTE: we don't preserve nullabilities of left/right individually so we are using nullability binary expression as a whole
                    // this may lead to missing some optimizations, where one of the operands (left or right) is not nullable and the other one is
                    var left = ProcessNullNotNull(
                        SqlExpressionFactory.MakeUnary(
                            sqlUnaryExpression.OperatorType,
                            sqlBinaryOperand.Left,
                            typeof(bool),
                            sqlUnaryExpression.TypeMapping),
                        operandNullable: null);

                    var right = ProcessNullNotNull(
                        SqlExpressionFactory.MakeUnary(
                            sqlUnaryExpression.OperatorType,
                            sqlBinaryOperand.Right,
                            typeof(bool),
                            sqlUnaryExpression.TypeMapping),
                        operandNullable: null);

                    return SimplifyLogicalSqlBinaryExpression(
                        SqlExpressionFactory.MakeBinary(
                            sqlUnaryExpression.OperatorType == ExpressionType.Equal
                                ? ExpressionType.OrElse
                                : ExpressionType.AndAlso,
                            left,
                            right,
                            sqlUnaryExpression.TypeMapping));
                }

                case SqlFunctionExpression sqlFunctionExpression:
                {
                    if (sqlFunctionExpression.IsBuiltIn && string.Equals("COALESCE", sqlFunctionExpression.Name, StringComparison.OrdinalIgnoreCase))
                    {
                        // for coalesce:
                        // (a ?? b) == null -> a == null && b == null
                        // (a ?? b) != null -> a != null || b != null
                        var left = ProcessNullNotNull(
                            SqlExpressionFactory.MakeUnary(
                                sqlUnaryExpression.OperatorType,
                                sqlFunctionExpression.Arguments[0],
                                typeof(bool),
                                sqlUnaryExpression.TypeMapping),
                            operandNullable: null);

                        var right = ProcessNullNotNull(
                            SqlExpressionFactory.MakeUnary(
                                sqlUnaryExpression.OperatorType,
                                sqlFunctionExpression.Arguments[1],
                                typeof(bool),
                                sqlUnaryExpression.TypeMapping),
                            operandNullable: null);

                        return SimplifyLogicalSqlBinaryExpression(
                            SqlExpressionFactory.MakeBinary(
                                sqlUnaryExpression.OperatorType == ExpressionType.Equal
                                    ? ExpressionType.AndAlso
                                    : ExpressionType.OrElse,
                                left,
                                right,
                                sqlUnaryExpression.TypeMapping));
                    }

                    if (!sqlFunctionExpression.NullResultAllowed)
                    {
                        // when we know that function can't be nullable:
                        // non_nullable_function() is null-> false
                        // non_nullable_function() is not null -> true
                        return SqlExpressionFactory.Constant(
                            sqlUnaryExpression.OperatorType == ExpressionType.NotEqual,
                            sqlUnaryExpression.TypeMapping);
                    }

                    // see if we can derive function nullability from it's instance and/or arguments
                    // rather than evaluating nullability of the entire function
                    var nullabilityPropagationElements = new List<SqlExpression>();
                    if (sqlFunctionExpression.Instance != null
                        && sqlFunctionExpression.InstancPropagatesNullability == true)
                    {
                        nullabilityPropagationElements.Add(sqlFunctionExpression.Instance);
                    }

                    for (var i = 0; i < sqlFunctionExpression.Arguments.Count; i++)
                    {
                        if (sqlFunctionExpression.ArgumentsPropagateNullability[i])
                        {
                            nullabilityPropagationElements.Add(sqlFunctionExpression.Arguments[i]);
                        }
                    }

                    if (nullabilityPropagationElements.Count > 0)
                    {
                        var result = ProcessNullNotNull(
                            SqlExpressionFactory.MakeUnary(
                                sqlUnaryExpression.OperatorType,
                                nullabilityPropagationElements[0],
                                sqlUnaryExpression.Type,
                                sqlUnaryExpression.TypeMapping),
                            operandNullable: null);

                        foreach (var element in nullabilityPropagationElements.Skip(1))
                        {
                            result = SimplifyLogicalSqlBinaryExpression(
                                sqlUnaryExpression.OperatorType == ExpressionType.Equal
                                    ? SqlExpressionFactory.OrElse(
                                        result,
                                        ProcessNullNotNull(
                                            SqlExpressionFactory.IsNull(element),
                                            operandNullable: null))
                                    : SqlExpressionFactory.AndAlso(
                                        result,
                                        ProcessNullNotNull(
                                            SqlExpressionFactory.IsNotNull(element),
                                            operandNullable: null)));
                        }

                        return result;
                    }
                }
                break;
            }

            return sqlUnaryExpression;
        }

        protected override Expression VisitTable(TableExpression tableExpression)
        {
            Check.NotNull(tableExpression, nameof(tableExpression));

            return tableExpression;
        }

        protected override Expression VisitUnion(UnionExpression unionExpression)
        {
            Check.NotNull(unionExpression, nameof(unionExpression));

            var source1 = VisitInternal<SelectExpression>(unionExpression.Source1).ResultExpression;
            var source2 = VisitInternal<SelectExpression>(unionExpression.Source2).ResultExpression;

            return unionExpression.Update(source1, source2);
        }

        // ?a == ?b -> [(a == b) && (a != null && b != null)] || (a == null && b == null))
        //
        // a | b | F1 = a == b | F2 = (a != null && b != null) | F3 = F1 && F2 |
        //   |   |             |                               |               |
        // 0 | 0 | 1           | 1                             | 1             |
        // 0 | 1 | 0           | 1                             | 0             |
        // 0 | N | N           | 0                             | 0             |
        // 1 | 0 | 0           | 1                             | 0             |
        // 1 | 1 | 1           | 1                             | 1             |
        // 1 | N | N           | 0                             | 0             |
        // N | 0 | N           | 0                             | 0             |
        // N | 1 | N           | 0                             | 0             |
        // N | N | N           | 0                             | 0             |
        //
        // a | b | F4 = (a == null && b == null) | Final = F3 OR F4 |
        //   |   |                               |                  |
        // 0 | 0 | 0                             | 1 OR 0 = 1       |
        // 0 | 1 | 0                             | 0 OR 0 = 0       |
        // 0 | N | 0                             | 0 OR 0 = 0       |
        // 1 | 0 | 0                             | 0 OR 0 = 0       |
        // 1 | 1 | 0                             | 1 OR 0 = 1       |
        // 1 | N | 0                             | 0 OR 0 = 0       |
        // N | 0 | 0                             | 0 OR 0 = 0       |
        // N | 1 | 0                             | 0 OR 0 = 0       |
        // N | N | 1                             | 0 OR 1 = 1       |
        private SqlExpression ExpandNullableEqualNullable(
            SqlExpression left,
            SqlExpression right,
            SqlExpression leftIsNull,
            SqlExpression leftIsNotNull,
            SqlExpression rightIsNull,
            SqlExpression rightIsNotNull)
            => SimplifyLogicalSqlBinaryExpression(
                SqlExpressionFactory.OrElse(
                    SimplifyLogicalSqlBinaryExpression(
                        SqlExpressionFactory.AndAlso(
                            SqlExpressionFactory.Equal(left, right),
                            SimplifyLogicalSqlBinaryExpression(
                                SqlExpressionFactory.AndAlso(leftIsNotNull, rightIsNotNull)))),
                    SimplifyLogicalSqlBinaryExpression(
                        SqlExpressionFactory.AndAlso(leftIsNull, rightIsNull))));

        // !(?a) == ?b -> [(a != b) && (a != null && b != null)] || (a == null && b == null)
        //
        // a | b | F1 = a != b | F2 = (a != null && b != null) | F3 = F1 && F2 |
        //   |   |             |                               |               |
        // 0 | 0 | 0           | 1                             | 0             |
        // 0 | 1 | 1           | 1                             | 1             |
        // 0 | N | N           | 0                             | 0             |
        // 1 | 0 | 1           | 1                             | 1             |
        // 1 | 1 | 0           | 1                             | 0             |
        // 1 | N | N           | 0                             | 0             |
        // N | 0 | N           | 0                             | 0             |
        // N | 1 | N           | 0                             | 0             |
        // N | N | N           | 0                             | 0             |
        //
        // a | b | F4 = (a == null && b == null) | Final = F3 OR F4 |
        //   |   |                               |                  |
        // 0 | 0 | 0                             | 0 OR 0 = 0       |
        // 0 | 1 | 0                             | 1 OR 0 = 1       |
        // 0 | N | 0                             | 0 OR 0 = 0       |
        // 1 | 0 | 0                             | 1 OR 0 = 1       |
        // 1 | 1 | 0                             | 0 OR 0 = 0       |
        // 1 | N | 0                             | 0 OR 0 = 0       |
        // N | 0 | 0                             | 0 OR 0 = 0       |
        // N | 1 | 0                             | 0 OR 0 = 0       |
        // N | N | 1                             | 0 OR 1 = 1       |
        private SqlExpression ExpandNegatedNullableEqualNullable(
            SqlExpression left,
            SqlExpression right,
            SqlExpression leftIsNull,
            SqlExpression leftIsNotNull,
            SqlExpression rightIsNull,
            SqlExpression rightIsNotNull)
            => SimplifyLogicalSqlBinaryExpression(
                SqlExpressionFactory.OrElse(
                    SimplifyLogicalSqlBinaryExpression(
                        SqlExpressionFactory.AndAlso(
                            SqlExpressionFactory.NotEqual(left, right),
                        SimplifyLogicalSqlBinaryExpression(
                            SqlExpressionFactory.AndAlso(leftIsNotNull, rightIsNotNull)))),
                    SimplifyLogicalSqlBinaryExpression(
                        SqlExpressionFactory.AndAlso(leftIsNull, rightIsNull))));

        // ?a == b -> (a == b) && (a != null)
        //
        // a | b | F1 = a == b | F2 = (a != null) | Final = F1 && F2 |
        //   |   |             |                  |                  |
        // 0 | 0 | 1           | 1                | 1                |
        // 0 | 1 | 0           | 1                | 0                |
        // 1 | 0 | 0           | 1                | 0                |
        // 1 | 1 | 1           | 1                | 1                |
        // N | 0 | N           | 0                | 0                |
        // N | 1 | N           | 0                | 0                |
        private SqlExpression ExpandNullableEqualNonNullable(
            SqlExpression left, SqlExpression right, SqlExpression leftIsNotNull)
            => SimplifyLogicalSqlBinaryExpression(
                SqlExpressionFactory.AndAlso(
                    SqlExpressionFactory.Equal(left, right),
                    leftIsNotNull));

        // !(?a) == b -> (a != b) && (a != null)
        //
        // a | b | F1 = a != b | F2 = (a != null) | Final = F1 && F2 |
        //   |   |             |                  |                  |
        // 0 | 0 | 0           | 1                | 0                |
        // 0 | 1 | 1           | 1                | 1                |
        // 1 | 0 | 1           | 1                | 1                |
        // 1 | 1 | 0           | 1                | 0                |
        // N | 0 | N           | 0                | 0                |
        // N | 1 | N           | 0                | 0                |
        private SqlExpression ExpandNegatedNullableEqualNonNullable(
            SqlExpression left, SqlExpression right, SqlExpression leftIsNotNull)
            => SimplifyLogicalSqlBinaryExpression(
                SqlExpressionFactory.AndAlso(
                    SqlExpressionFactory.NotEqual(left, right),
                    leftIsNotNull));

        // ?a != ?b -> [(a != b) || (a == null || b == null)] && (a != null || b != null)
        //
        // a | b | F1 = a != b | F2 = (a == null || b == null) | F3 = F1 || F2 |
        //   |   |             |                               |               |
        // 0 | 0 | 0           | 0                             | 0             |
        // 0 | 1 | 1           | 0                             | 1             |
        // 0 | N | N           | 1                             | 1             |
        // 1 | 0 | 1           | 0                             | 1             |
        // 1 | 1 | 0           | 0                             | 0             |
        // 1 | N | N           | 1                             | 1             |
        // N | 0 | N           | 1                             | 1             |
        // N | 1 | N           | 1                             | 1             |
        // N | N | N           | 1                             | 1             |
        //
        // a | b | F4 = (a != null || b != null) | Final = F3 && F4 |
        //   |   |                               |                  |
        // 0 | 0 | 1                             | 0 && 1 = 0       |
        // 0 | 1 | 1                             | 1 && 1 = 1       |
        // 0 | N | 1                             | 1 && 1 = 1       |
        // 1 | 0 | 1                             | 1 && 1 = 1       |
        // 1 | 1 | 1                             | 0 && 1 = 0       |
        // 1 | N | 1                             | 1 && 1 = 1       |
        // N | 0 | 1                             | 1 && 1 = 1       |
        // N | 1 | 1                             | 1 && 1 = 1       |
        // N | N | 0                             | 1 && 0 = 0       |
        private SqlExpression ExpandNullableNotEqualNullable(
            SqlExpression left,
            SqlExpression right,
            SqlExpression leftIsNull,
            SqlExpression leftIsNotNull,
            SqlExpression rightIsNull,
            SqlExpression rightIsNotNull)
            => SimplifyLogicalSqlBinaryExpression(
                SqlExpressionFactory.AndAlso(
                    SimplifyLogicalSqlBinaryExpression(
                        SqlExpressionFactory.OrElse(
                            SqlExpressionFactory.NotEqual(left, right),
                            SimplifyLogicalSqlBinaryExpression(
                                SqlExpressionFactory.OrElse(leftIsNull, rightIsNull)))),
                    SimplifyLogicalSqlBinaryExpression(
                        SqlExpressionFactory.OrElse(leftIsNotNull, rightIsNotNull))));

        // !(?a) != ?b -> [(a == b) || (a == null || b == null)] && (a != null || b != null)
        //
        // a | b | F1 = a == b | F2 = (a == null || b == null) | F3 = F1 || F2 |
        //   |   |             |                               |               |
        // 0 | 0 | 1           | 0                             | 1             |
        // 0 | 1 | 0           | 0                             | 0             |
        // 0 | N | N           | 1                             | 1             |
        // 1 | 0 | 0           | 0                             | 0             |
        // 1 | 1 | 1           | 0                             | 1             |
        // 1 | N | N           | 1                             | 1             |
        // N | 0 | N           | 1                             | 1             |
        // N | 1 | N           | 1                             | 1             |
        // N | N | N           | 1                             | 1             |
        //
        // a | b | F4 = (a != null || b != null) | Final = F3 && F4 |
        //   |   |                               |                  |
        // 0 | 0 | 1                             | 1 && 1 = 1       |
        // 0 | 1 | 1                             | 0 && 1 = 0       |
        // 0 | N | 1                             | 1 && 1 = 1       |
        // 1 | 0 | 1                             | 0 && 1 = 0       |
        // 1 | 1 | 1                             | 1 && 1 = 1       |
        // 1 | N | 1                             | 1 && 1 = 1       |
        // N | 0 | 1                             | 1 && 1 = 1       |
        // N | 1 | 1                             | 1 && 1 = 1       |
        // N | N | 0                             | 1 && 0 = 0       |
        private SqlExpression ExpandNegatedNullableNotEqualNullable(
            SqlExpression left,
            SqlExpression right,
            SqlExpression leftIsNull,
            SqlExpression leftIsNotNull,
            SqlExpression rightIsNull,
            SqlExpression rightIsNotNull)
            => SimplifyLogicalSqlBinaryExpression(
                SqlExpressionFactory.AndAlso(
                    SimplifyLogicalSqlBinaryExpression(
                        SqlExpressionFactory.OrElse(
                            SqlExpressionFactory.Equal(left, right),
                            SimplifyLogicalSqlBinaryExpression(
                                SqlExpressionFactory.OrElse(leftIsNull, rightIsNull)))),
                    SimplifyLogicalSqlBinaryExpression(
                        SqlExpressionFactory.OrElse(leftIsNotNull, rightIsNotNull))));

        // ?a != b -> (a != b) || (a == null)
        //
        // a | b | F1 = a != b | F2 = (a == null) | Final = F1 OR F2 |
        //   |   |             |                  |                  |
        // 0 | 0 | 0           | 0                | 0                |
        // 0 | 1 | 1           | 0                | 1                |
        // 1 | 0 | 1           | 0                | 1                |
        // 1 | 1 | 0           | 0                | 0                |
        // N | 0 | N           | 1                | 1                |
        // N | 1 | N           | 1                | 1                |
        private SqlExpression ExpandNullableNotEqualNonNullable(
            SqlExpression left, SqlExpression right, SqlExpression leftIsNull)
            => SimplifyLogicalSqlBinaryExpression(
                SqlExpressionFactory.OrElse(
                    SqlExpressionFactory.NotEqual(left, right),
                    leftIsNull));

        // !(?a) != b -> (a == b) || (a == null)
        //
        // a | b | F1 = a == b | F2 = (a == null) | F3 = F1 OR F2 |
        //   |   |             |                  |               |
        // 0 | 0 | 1           | 0                | 1             |
        // 0 | 1 | 0           | 0                | 0             |
        // 1 | 0 | 0           | 0                | 0             |
        // 1 | 1 | 1           | 0                | 1             |
        // N | 0 | N           | 1                | 1             |
        // N | 1 | N           | 1                | 1             |
        private SqlExpression ExpandNegatedNullableNotEqualNonNullable(
            SqlExpression left, SqlExpression right, SqlExpression leftIsNull)
            => SimplifyLogicalSqlBinaryExpression(
                SqlExpressionFactory.OrElse(
                    SqlExpressionFactory.Equal(left, right),
                    leftIsNull));
    }
}
