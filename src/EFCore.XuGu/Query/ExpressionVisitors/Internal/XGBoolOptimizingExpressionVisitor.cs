// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.EntityFrameworkCore.XuGu.Storage.Internal;

namespace Microsoft.EntityFrameworkCore.XuGu.Query.ExpressionVisitors.Internal
{
    /// <summary>
    /// "WHERE `boolColumn`" doesn't use available indices, while "WHERE `boolColumn` = TRUE" does.
    /// See https://github.com/PomeloFoundation/Microsoft.EntityFrameworkCore.XuGu/issues/1104
    /// </summary>
    public class XGBoolOptimizingExpressionVisitor : SqlExpressionVisitor
    {
        private bool _optimize;
        private readonly ISqlExpressionFactory _sqlExpressionFactory;

        public XGBoolOptimizingExpressionVisitor(
            ISqlExpressionFactory sqlExpressionFactory)
        {
            _sqlExpressionFactory = sqlExpressionFactory;
        }

        private Expression ApplyConversion(SqlExpression sqlExpression, bool condition)
        {
            if (_optimize &&
                sqlExpression is ColumnExpression &&
                sqlExpression.TypeMapping is XGBoolTypeMapping &&
                sqlExpression.Type == typeof(bool)/* &&
                condition*/)
            {
                return _sqlExpressionFactory.Equal(sqlExpression, _sqlExpressionFactory.Constant(true));
            }

            return sqlExpression;
        }

        protected override Expression VisitAtTimeZone(AtTimeZoneExpression atTimeZoneExpression)
        {
            var parentOptimize = _optimize;
            _optimize = false;
            var operand = (SqlExpression)Visit(atTimeZoneExpression.Operand);
            var timeZone = (SqlExpression)Visit(atTimeZoneExpression.TimeZone);
            _optimize = parentOptimize;

            return atTimeZoneExpression.Update(operand, timeZone);
        }

        protected override Expression VisitCase(CaseExpression caseExpression)
        {
            Check.NotNull(caseExpression, nameof(caseExpression));

            var parentOptimize = _optimize;

            var testIsCondition = caseExpression.Operand == null;
            _optimize = false;
            var operand = (SqlExpression)Visit(caseExpression.Operand);
            var whenClauses = new List<CaseWhenClause>();
            foreach (var whenClause in caseExpression.WhenClauses)
            {
                _optimize = testIsCondition;
                var test = (SqlExpression)Visit(whenClause.Test);
                _optimize = false;
                var result = (SqlExpression)Visit(whenClause.Result);
                whenClauses.Add(new CaseWhenClause(test, result));
            }

            _optimize = false;
            var elseResult = (SqlExpression)Visit(caseExpression.ElseResult);

            _optimize = parentOptimize;

            return ApplyConversion(caseExpression.Update(operand, whenClauses, elseResult), condition: false);
        }

        protected override Expression VisitCollate(CollateExpression collateExpression)
        {
            Check.NotNull(collateExpression, nameof(collateExpression));

            var parentOptimize = _optimize;
            _optimize = false;
            var operand = (SqlExpression)Visit(collateExpression.Operand);
            _optimize = parentOptimize;

            return ApplyConversion(collateExpression.Update(operand), condition: false);
        }

        protected override Expression VisitColumn(ColumnExpression columnExpression)
        {
            Check.NotNull(columnExpression, nameof(columnExpression));

            return ApplyConversion(columnExpression, condition: false);
        }

        protected override Expression VisitDelete(DeleteExpression deleteExpression)
            => deleteExpression.Update(deleteExpression.Table, (SelectExpression)Visit(deleteExpression.SelectExpression));

        protected override Expression VisitDistinct(DistinctExpression distinctExpression)
        {
            Check.NotNull(distinctExpression, nameof(distinctExpression));

            var parentOptimize = _optimize;
            _optimize = false;
            var operand = (SqlExpression)Visit(distinctExpression.Operand);
            _optimize = parentOptimize;

            return ApplyConversion(distinctExpression.Update(operand), condition: false);
        }

        protected override Expression VisitExists(ExistsExpression existsExpression)
        {
            Check.NotNull(existsExpression, nameof(existsExpression));

            var parentOptimize = _optimize;
            _optimize = false;
            var subquery = (SelectExpression)Visit(existsExpression.Subquery);
            _optimize = parentOptimize;

            return ApplyConversion(existsExpression.Update(subquery), condition: true);
        }

        protected override Expression VisitFromSql(FromSqlExpression fromSqlExpression)
        {
            Check.NotNull(fromSqlExpression, nameof(fromSqlExpression));

            return fromSqlExpression;
        }

        protected override Expression VisitIn(InExpression inExpression)
        {
            var parentOptimize = _optimize;

            _optimize = false;
            var item = (SqlExpression)Visit(inExpression.Item);
            var subquery = (SelectExpression)Visit(inExpression.Subquery);

            var values = inExpression.Values;
            SqlExpression[] newValues = null;
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

            var valuesParameter = (SqlParameterExpression)Visit(inExpression.ValuesParameter);
            _optimize = parentOptimize;

            return ApplyConversion(inExpression.Update(item, subquery, newValues ?? values, valuesParameter), condition: true);
        }

        protected override Expression VisitLike(LikeExpression likeExpression)
        {
            Check.NotNull(likeExpression, nameof(likeExpression));

            var parentOptimize = _optimize;
            _optimize = false;
            var match = (SqlExpression)Visit(likeExpression.Match);
            var pattern = (SqlExpression)Visit(likeExpression.Pattern);
            var escapeChar = (SqlExpression)Visit(likeExpression.EscapeChar);
            _optimize = parentOptimize;

            return ApplyConversion(likeExpression.Update(match, pattern, escapeChar), condition: true);
        }

        protected override Expression VisitSelect(SelectExpression selectExpression)
        {
            Check.NotNull(selectExpression, nameof(selectExpression));

            var changed = false;
            var parentOptimize = _optimize;

            var projections = new List<ProjectionExpression>();
            _optimize = false;
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

            _optimize = true;
            var predicate = (SqlExpression)Visit(selectExpression.Predicate);
            changed |= predicate != selectExpression.Predicate;

            var groupBy = new List<SqlExpression>();
            _optimize = false;
            foreach (var groupingKey in selectExpression.GroupBy)
            {
                var newGroupingKey = (SqlExpression)Visit(groupingKey);
                changed |= newGroupingKey != groupingKey;
                groupBy.Add(newGroupingKey);
            }

            _optimize = true;
            var havingExpression = (SqlExpression)Visit(selectExpression.Having);
            changed |= havingExpression != selectExpression.Having;

            var orderings = new List<OrderingExpression>();
            _optimize = false;
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

            _optimize = parentOptimize;

            return changed
                ? selectExpression.Update(
                    tables, predicate, groupBy, havingExpression, projections, orderings, offset, limit)
                : selectExpression;
        }

        protected override Expression VisitSqlBinary(SqlBinaryExpression sqlBinaryExpression)
        {
            Check.NotNull(sqlBinaryExpression, nameof(sqlBinaryExpression));

            var parentOptimize = _optimize;
            var columnExpression = sqlBinaryExpression.Left as ColumnExpression ?? sqlBinaryExpression.Right as ColumnExpression;
            var sqlConstantExpression = sqlBinaryExpression.Left as SqlConstantExpression ?? sqlBinaryExpression.Right as SqlConstantExpression;

            // TODO: Simplify for .NET 5, due to the already existing bool expression optimizations performed by `SqlNullabilityProcessor`.
            //       This custom logic can probably be removed completely.
            //       See `GearsOfWarQueryXGTest`.

            // Optimize translation of the following expressions:
            //     context.Table.Where(t => t.BoolColumn == true)
            //         translate to: `boolColumn` = TRUE
            //         instead of:   (`boolColumn` = TRUE) = TRUE
            //     context.Table.Where(t => t.BoolColumn == false)
            //         translate to: `boolColumn` = FALSE
            //         instead of:   (`boolColumn` = TRUE) = FALSE
            //     context.Table.Where(t => t.BoolColumn != true)
            //         translate to: `boolColumn` <> TRUE
            //         instead of:   (`boolColumn` = TRUE) <> TRUE
            //     context.Table.Where(t => t.BoolColumn != false)
            //         translate to: `boolColumn` <> FALSE
            //         instead of:   (`boolColumn` = TRUE) <> FALSE
            if (_optimize &&
                (sqlBinaryExpression.OperatorType == ExpressionType.Equal || sqlBinaryExpression.OperatorType == ExpressionType.NotEqual) &&
                columnExpression != null &&
                sqlConstantExpression != null &&
                columnExpression.TypeMapping is XGBoolTypeMapping &&
                columnExpression.Type == typeof(bool) &&
                sqlConstantExpression.TypeMapping is XGBoolTypeMapping &&
                sqlConstantExpression.Type == typeof(bool))
            {
                _optimize = false;
            }
            else
            {
                switch (sqlBinaryExpression.OperatorType)
                {
                    // Only logical operations need conditions on both sides
                    case ExpressionType.AndAlso:
                    case ExpressionType.OrElse:
                        _optimize = true;
                        break;
                    default:
                        _optimize = false;
                        break;
                }
            }

            var newLeft = (SqlExpression)Visit(sqlBinaryExpression.Left);
            var newRight = (SqlExpression)Visit(sqlBinaryExpression.Right);

            _optimize = parentOptimize;

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

        protected override Expression VisitSqlUnary(SqlUnaryExpression sqlUnaryExpression)
        {
            Check.NotNull(sqlUnaryExpression, nameof(sqlUnaryExpression));

            var parentOptimize = _optimize;
            bool resultCondition;
            switch (sqlUnaryExpression.OperatorType)
            {
                case ExpressionType.Not
                    when sqlUnaryExpression.Type == typeof(bool):
                    _optimize = true;
                    resultCondition = true;
                    break;

                case ExpressionType.Not:
                    _optimize = false;
                    resultCondition = false;
                    break;

                case ExpressionType.Convert:
                case ExpressionType.Negate:
                    _optimize = false;
                    resultCondition = false;
                    break;

                case ExpressionType.Equal:
                case ExpressionType.NotEqual:
                    _optimize = false;
                    resultCondition = true;
                    break;

                default:
                    throw new InvalidOperationException("Unknown operator type encountered in SqlUnaryExpression.");
            }

            SqlExpression expression;

            // TODO: Simplify for .NET 5, due to the already existing bool expression optimizations performed by `SqlNullabilityProcessor`.
            //       This custom logic can probably be removed completely.
            //       See `GearsOfWarQueryXGTest`.

            // Optimize translation of the following expressions:
            //     context.Table.Where(t => !t.BoolColumn)
            //         translate to: `boolColumn` = FALSE
            //         instead of:   NOT(`boolColumn` = TRUE)
            // Translating to "NOT(`boolColumn`)" would not use indices in MySQL 5.7.
            if (sqlUnaryExpression.OperatorType == ExpressionType.Not &&
                sqlUnaryExpression.Operand is ColumnExpression columnExpression &&
                columnExpression.TypeMapping is XGBoolTypeMapping &&
                columnExpression.Type == typeof(bool))
            {
                _optimize = false;

                expression = _sqlExpressionFactory.MakeBinary(
                    ExpressionType.Equal,
                    (SqlExpression)Visit(sqlUnaryExpression.Operand),
                    _sqlExpressionFactory.Constant(false),
                    sqlUnaryExpression.TypeMapping);
            }
            else
            {
                expression = sqlUnaryExpression.Update((SqlExpression)Visit(sqlUnaryExpression.Operand));
            }

            _optimize = parentOptimize;

            return ApplyConversion(expression, condition: resultCondition);
        }

        protected override Expression VisitSqlConstant(SqlConstantExpression sqlConstantExpression)
        {
            Check.NotNull(sqlConstantExpression, nameof(sqlConstantExpression));

            return ApplyConversion(sqlConstantExpression, condition: false);
        }

        protected override Expression VisitSqlFragment(SqlFragmentExpression sqlFragmentExpression)
        {
            Check.NotNull(sqlFragmentExpression, nameof(sqlFragmentExpression));

            return sqlFragmentExpression;
        }

        protected override Expression VisitSqlFunction(SqlFunctionExpression sqlFunctionExpression)
        {
            Check.NotNull(sqlFunctionExpression, nameof(sqlFunctionExpression));

            var parentOptimize = _optimize;
            _optimize = false;
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

            _optimize = parentOptimize;
            var newFunction = sqlFunctionExpression.Update(instance, arguments);

            var condition = string.Equals(sqlFunctionExpression.Name, "FREETEXT", StringComparison.OrdinalIgnoreCase)
                || string.Equals(sqlFunctionExpression.Name, "CONTAINS", StringComparison.OrdinalIgnoreCase);

            return ApplyConversion(newFunction, condition);
        }

        protected override Expression VisitTableValuedFunction(TableValuedFunctionExpression tableValuedFunctionExpression)
        {
            Check.NotNull(tableValuedFunctionExpression, nameof(tableValuedFunctionExpression));

            var parentOptimize = _optimize;
            _optimize = false;

            var arguments = new SqlExpression[tableValuedFunctionExpression.Arguments.Count];
            for (var i = 0; i < arguments.Length; i++)
            {
                arguments[i] = (SqlExpression)Visit(tableValuedFunctionExpression.Arguments[i]);
            }

            _optimize = parentOptimize;
            return tableValuedFunctionExpression.Update(arguments);
        }

        protected override Expression VisitSqlParameter(SqlParameterExpression sqlParameterExpression)
        {
            Check.NotNull(sqlParameterExpression, nameof(sqlParameterExpression));

            return ApplyConversion(sqlParameterExpression, condition: false);
        }

        protected override Expression VisitTable(TableExpression tableExpression)
        {
            Check.NotNull(tableExpression, nameof(tableExpression));

            return tableExpression;
        }

        protected override Expression VisitProjection(ProjectionExpression projectionExpression)
        {
            Check.NotNull(projectionExpression, nameof(projectionExpression));

            var expression = (SqlExpression)Visit(projectionExpression.Expression);

            return projectionExpression.Update(expression);
        }

        protected override Expression VisitOrdering(OrderingExpression orderingExpression)
        {
            Check.NotNull(orderingExpression, nameof(orderingExpression));

            var expression = (SqlExpression)Visit(orderingExpression.Expression);

            return orderingExpression.Update(expression);
        }

        protected override Expression VisitCrossJoin(CrossJoinExpression crossJoinExpression)
        {
            Check.NotNull(crossJoinExpression, nameof(crossJoinExpression));

            var parentOptimize = _optimize;
            _optimize = false;
            var table = (TableExpressionBase)Visit(crossJoinExpression.Table);
            _optimize = parentOptimize;

            return crossJoinExpression.Update(table);
        }

        protected override Expression VisitCrossApply(CrossApplyExpression crossApplyExpression)
        {
            Check.NotNull(crossApplyExpression, nameof(crossApplyExpression));

            var parentOptimize = _optimize;
            _optimize = false;
            var table = (TableExpressionBase)Visit(crossApplyExpression.Table);
            _optimize = parentOptimize;

            return crossApplyExpression.Update(table);
        }

        protected override Expression VisitOuterApply(OuterApplyExpression outerApplyExpression)
        {
            Check.NotNull(outerApplyExpression, nameof(outerApplyExpression));

            var parentOptimize = _optimize;
            _optimize = false;
            var table = (TableExpressionBase)Visit(outerApplyExpression.Table);
            _optimize = parentOptimize;

            return outerApplyExpression.Update(table);
        }

        protected override Expression VisitInnerJoin(InnerJoinExpression innerJoinExpression)
        {
            Check.NotNull(innerJoinExpression, nameof(innerJoinExpression));

            var parentOptimize = _optimize;
            _optimize = false;
            var table = (TableExpressionBase)Visit(innerJoinExpression.Table);
            _optimize = true;
            var joinPredicate = (SqlExpression)Visit(innerJoinExpression.JoinPredicate);
            _optimize = parentOptimize;

            return innerJoinExpression.Update(table, joinPredicate);
        }

        protected override Expression VisitLeftJoin(LeftJoinExpression leftJoinExpression)
        {
            Check.NotNull(leftJoinExpression, nameof(leftJoinExpression));

            var parentOptimize = _optimize;
            _optimize = false;
            var table = (TableExpressionBase)Visit(leftJoinExpression.Table);
            _optimize = true;
            var joinPredicate = (SqlExpression)Visit(leftJoinExpression.JoinPredicate);
            _optimize = parentOptimize;

            return leftJoinExpression.Update(table, joinPredicate);
        }

        protected override Expression VisitRowValue(RowValueExpression rowValueExpression)
        {
            var parentOptimize = _optimize;
            _optimize = false;

            var values = new SqlExpression[rowValueExpression.Values.Count];
            for (var i = 0; i < values.Length; i++)
            {
                values[i] = (SqlExpression)Visit(rowValueExpression.Values[i]);
            }

            _optimize = parentOptimize;
            return rowValueExpression.Update(values);
        }

        protected override Expression VisitScalarSubquery(ScalarSubqueryExpression scalarSubqueryExpression)
        {
            Check.NotNull(scalarSubqueryExpression, nameof(scalarSubqueryExpression));

            var parentOptimize = _optimize;
            var subquery = (SelectExpression)Visit(scalarSubqueryExpression.Subquery);
            _optimize = parentOptimize;

            return ApplyConversion(scalarSubqueryExpression.Update(subquery), condition: false);
        }

        protected override Expression VisitRowNumber(RowNumberExpression rowNumberExpression)
        {
            Check.NotNull(rowNumberExpression, nameof(rowNumberExpression));

            var parentOptimize = _optimize;
            _optimize = false;
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

            _optimize = parentOptimize;

            return ApplyConversion(rowNumberExpression.Update(partitions, orderings), condition: false);
        }

        protected override Expression VisitExcept(ExceptExpression exceptExpression)
        {
            Check.NotNull(exceptExpression, nameof(exceptExpression));

            var parentOptimize = _optimize;
            _optimize = false;
            var source1 = (SelectExpression)Visit(exceptExpression.Source1);
            var source2 = (SelectExpression)Visit(exceptExpression.Source2);
            _optimize = parentOptimize;

            return exceptExpression.Update(source1, source2);
        }

        protected override Expression VisitIntersect(IntersectExpression intersectExpression)
        {
            Check.NotNull(intersectExpression, nameof(intersectExpression));

            var parentOptimize = _optimize;
            _optimize = false;
            var source1 = (SelectExpression)Visit(intersectExpression.Source1);
            var source2 = (SelectExpression)Visit(intersectExpression.Source2);
            _optimize = parentOptimize;

            return intersectExpression.Update(source1, source2);
        }

        protected override Expression VisitUnion(UnionExpression unionExpression)
        {
            Check.NotNull(unionExpression, nameof(unionExpression));

            var parentOptimize = _optimize;
            _optimize = false;
            var source1 = (SelectExpression)Visit(unionExpression.Source1);
            var source2 = (SelectExpression)Visit(unionExpression.Source2);
            _optimize = parentOptimize;

            return unionExpression.Update(source1, source2);
        }

        protected override Expression VisitUpdate(UpdateExpression updateExpression)
        {
            var selectExpression = (SelectExpression)Visit(updateExpression.SelectExpression);
            var parentOptimize = _optimize;
            _optimize = false;
            List<ColumnValueSetter> columnValueSetters = null;
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
                    columnValueSetters = new List<ColumnValueSetter>();
                    for (var j = 0; j < i; j++)
                    {
                        columnValueSetters.Add(updateExpression.ColumnValueSetters[j]);
                    }

                    columnValueSetters.Add(new ColumnValueSetter(columnValueSetter.Column, newValue));
                }
            }

            _optimize = parentOptimize;
            return updateExpression.Update(selectExpression, columnValueSetters ?? updateExpression.ColumnValueSetters);
        }

        protected override Expression VisitJsonScalar(JsonScalarExpression jsonScalarExpression)
            => ApplyConversion(jsonScalarExpression, condition: false);

        protected override Expression VisitValues(ValuesExpression valuesExpression)
        {
            var parentOptimize = _optimize;
            _optimize = false;

            switch (valuesExpression)
            {
                case { RowValues: not null }:
                    var rowValues = new RowValueExpression[valuesExpression.RowValues!.Count];
                    for (var i = 0; i < rowValues.Length; i++)
                    {
                        rowValues[i] = (RowValueExpression)Visit(valuesExpression.RowValues[i]);
                    }
                    _optimize = parentOptimize;
                    return valuesExpression.Update(rowValues);

                case { ValuesParameter: not null }:
                    var valuesParameter = (SqlParameterExpression)Visit(valuesExpression.ValuesParameter);
                    _optimize = parentOptimize;
                    return valuesExpression.Update(valuesParameter);

                default:
                    throw new UnreachableException();
            }
        }
    }
}
