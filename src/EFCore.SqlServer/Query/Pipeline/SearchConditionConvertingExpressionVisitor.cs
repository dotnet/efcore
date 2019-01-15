// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Relational.Query.Pipeline;
using Microsoft.EntityFrameworkCore.Relational.Query.Pipeline.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.SqlServer.Query.Pipeline
{
    public class SearchConditionConvertingExpressionVisitor : SqlExpressionVisitor
    {
        private bool _isSearchCondition;
        private readonly ISqlExpressionFactory _sqlExpressionFactory;

        public SearchConditionConvertingExpressionVisitor(ISqlExpressionFactory sqlExpressionFactory)
        {
            _sqlExpressionFactory = sqlExpressionFactory;
        }

        private Expression ApplyConversion(SqlExpression sqlExpression, bool condition)
                => _isSearchCondition
                    ? ConvertToSearchCondition(sqlExpression, condition)
                    : ConvertToValue(sqlExpression, condition);

        private Expression ConvertToSearchCondition(SqlExpression sqlExpression, bool condition)
                => condition
                    ? sqlExpression
                    : BuildCompareToExpression(sqlExpression);

        private Expression ConvertToValue(SqlExpression sqlExpression, bool condition)
        {
            if (condition)
            {
                return _sqlExpressionFactory.Case(new[]
                {
                    new CaseWhenClause(
                        sqlExpression,
                        _sqlExpressionFactory.ApplyDefaultTypeMapping(_sqlExpressionFactory.Constant(true)))
                },
                _sqlExpressionFactory.Constant(false));
            }
            else
            {
                return sqlExpression;
            }
        }

        private SqlExpression BuildCompareToExpression(SqlExpression sqlExpression)
        {
            return _sqlExpressionFactory.Equal(sqlExpression, _sqlExpressionFactory.Constant(true));
        }

        protected override Expression VisitCase(CaseExpression caseExpression)
        {
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

        protected override Expression VisitColumn(ColumnExpression columnExpression)
        {
            return ApplyConversion(columnExpression, condition: false);
        }

        protected override Expression VisitExists(ExistsExpression existsExpression)
        {
            var parentSearchCondition = _isSearchCondition;
            _isSearchCondition = false;
            var subquery = (SelectExpression)Visit(existsExpression.Subquery);
            _isSearchCondition = parentSearchCondition;

            return ApplyConversion(existsExpression.Update(subquery), condition: true);
        }

        protected override Expression VisitIn(InExpression inExpression)
        {
            var parentSearchCondition = _isSearchCondition;

            _isSearchCondition = false;
            var item = (SqlExpression)Visit(inExpression.Item);
            var subquery = (SelectExpression)Visit(inExpression.Subquery);
            var values = (SqlExpression)Visit(inExpression.Values);
            _isSearchCondition = parentSearchCondition;

            return ApplyConversion(inExpression.Update(item, values, subquery), condition: true);
        }

        protected override Expression VisitLike(LikeExpression likeExpression)
        {
            var parentSearchCondition = _isSearchCondition;
            _isSearchCondition = false;
            var match = (SqlExpression)Visit(likeExpression.Match);
            var pattern = (SqlExpression)Visit(likeExpression.Pattern);
            var escapeChar = (SqlExpression)Visit(likeExpression.EscapeChar);
            _isSearchCondition = parentSearchCondition;

            return ApplyConversion(likeExpression.Update(match, pattern, escapeChar), condition: true);
        }

        protected override Expression VisitSelect(SelectExpression selectExpression)
        {
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

            if (changed)
            {
                return selectExpression.Update(
                    projections, tables, predicate, orderings, limit, offset, selectExpression.IsDistinct, selectExpression.Alias);
            }

            return selectExpression;
        }

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
            var parentSearchCondition = _isSearchCondition;
            bool resultCondition;
            switch (sqlUnaryExpression.OperatorType)
            {
                case ExpressionType.Not:
                    _isSearchCondition = true;
                    resultCondition = true;
                    break;

                case ExpressionType.Convert:
                    _isSearchCondition = false;
                    resultCondition = false;
                    break;

                default:
                    _isSearchCondition = false;
                    resultCondition = true;
                    break;
            }

            var operand = (SqlExpression)Visit(sqlUnaryExpression.Operand);
            _isSearchCondition = parentSearchCondition;

            return ApplyConversion(sqlUnaryExpression.Update(operand), condition: resultCondition);
        }

        protected override Expression VisitSqlConstant(SqlConstantExpression sqlConstantExpression)
        {
            return ApplyConversion(sqlConstantExpression, condition: false);
        }

        protected override Expression VisitSqlFragment(SqlFragmentExpression sqlFragmentExpression)
        {
            return sqlFragmentExpression;
        }

        protected override Expression VisitSqlFunction(SqlFunctionExpression sqlFunctionExpression)
        {
            var parentSearchCondition = _isSearchCondition;
            _isSearchCondition = false;
            var changed = false;
            var instance = (SqlExpression)Visit(sqlFunctionExpression.Instance);
            changed |= instance != sqlFunctionExpression.Instance;
            var arguments = new SqlExpression[sqlFunctionExpression.Arguments.Count];
            for (var i = 0; i < arguments.Length; i++)
            {
                arguments[i] = (SqlExpression)Visit(sqlFunctionExpression.Arguments[i]);
                changed |= arguments[i] != sqlFunctionExpression.Arguments[i];
            }

            _isSearchCondition = parentSearchCondition;
            SqlExpression newFunction;
            if (changed)
            {
                if (sqlFunctionExpression.Instance != null)
                {
                    if (sqlFunctionExpression.IsNiladic)
                    {
                        newFunction = _sqlExpressionFactory.Function(
                            instance,
                            sqlFunctionExpression.FunctionName,
                            sqlFunctionExpression.IsNiladic,
                            sqlFunctionExpression.Type,
                            sqlFunctionExpression.TypeMapping);
                    }
                    else
                    {
                        newFunction = _sqlExpressionFactory.Function(
                            instance,
                            sqlFunctionExpression.FunctionName,
                            arguments,
                            sqlFunctionExpression.Type,
                            sqlFunctionExpression.TypeMapping);
                    }
                }
                else
                {
                    if (sqlFunctionExpression.IsNiladic)
                    {
                        newFunction = _sqlExpressionFactory.Function(
                            sqlFunctionExpression.Schema,
                            sqlFunctionExpression.FunctionName,
                            sqlFunctionExpression.IsNiladic,
                            sqlFunctionExpression.Type,
                            sqlFunctionExpression.TypeMapping);
                    }
                    else
                    {
                        newFunction = _sqlExpressionFactory.Function(
                            sqlFunctionExpression.Schema,
                            sqlFunctionExpression.FunctionName,
                            arguments,
                            sqlFunctionExpression.Type,
                            sqlFunctionExpression.TypeMapping);
                    }
                }
            }
            else
            {
                newFunction = sqlFunctionExpression;
            }

            var condition = string.Equals(sqlFunctionExpression.FunctionName, "FREETEXT")
                || string.Equals(sqlFunctionExpression.FunctionName, "CONTAINS");

            return ApplyConversion(newFunction, condition);
        }

        protected override Expression VisitSqlParameter(SqlParameterExpression sqlParameterExpression)
        {
            return ApplyConversion(sqlParameterExpression, condition: false);
        }

        protected override Expression VisitTable(TableExpression tableExpression)
        {
            return tableExpression;
        }

        protected override Expression VisitProjection(ProjectionExpression projectionExpression)
        {
            var expression = (SqlExpression)Visit(projectionExpression.Expression);

            return projectionExpression.Update(expression);
        }

        protected override Expression VisitOrdering(OrderingExpression orderingExpression)
        {
            var expression = (SqlExpression)Visit(orderingExpression.Expression);

            return orderingExpression.Update(expression);
        }

        protected override Expression VisitCrossJoin(CrossJoinExpression crossJoinExpression)
        {
            var parentSearchCondition = _isSearchCondition;
            _isSearchCondition = false;
            var table = (TableExpressionBase)Visit(crossJoinExpression.Table);
            _isSearchCondition = parentSearchCondition;

            return crossJoinExpression.Update(table);
        }

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
    }
}
