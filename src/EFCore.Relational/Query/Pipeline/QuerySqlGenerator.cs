// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Relational.Query.Pipeline.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;

namespace Microsoft.EntityFrameworkCore.Relational.Query.Pipeline
{
    public class QuerySqlGenerator : SqlExpressionVisitor
    {
        private readonly IRelationalCommandBuilderFactory _relationalCommandBuilderFactory;
        private readonly ISqlGenerationHelper _sqlGenerationHelper;
        private IRelationalCommandBuilder _relationalCommandBuilder;

        private static readonly Dictionary<ExpressionType, string> _operatorMap = new Dictionary<ExpressionType, string>
        {
            { ExpressionType.Equal, " = " },
            { ExpressionType.NotEqual, " <> " },
            { ExpressionType.GreaterThan, " > " },
            { ExpressionType.GreaterThanOrEqual, " >= " },
            { ExpressionType.LessThan, " < " },
            { ExpressionType.LessThanOrEqual, " <= " },
            { ExpressionType.AndAlso, " AND " },
            { ExpressionType.OrElse, " OR " },
            { ExpressionType.Add, " + " },
            { ExpressionType.Subtract, " - " },
            { ExpressionType.Multiply, " * " },
            { ExpressionType.Divide, " / " },
            { ExpressionType.Modulo, " % " },
            { ExpressionType.And, " & " },
            { ExpressionType.Or, " | " }
        };

        public QuerySqlGenerator(
            IRelationalCommandBuilderFactory relationalCommandBuilderFactory,
            ISqlGenerationHelper sqlGenerationHelper)
        {
            _relationalCommandBuilderFactory = relationalCommandBuilderFactory;
            _sqlGenerationHelper = sqlGenerationHelper;
        }

        public virtual IRelationalCommand GetCommand(SelectExpression selectExpression)
        {
            _relationalCommandBuilder = _relationalCommandBuilderFactory.Create();

            if (selectExpression.IsNonComposedFromSql())
            {
                GenerateFromSql((FromSqlExpression)selectExpression.Tables[0]);
            }
            else
            {
                VisitSelect(selectExpression);
            }

            return _relationalCommandBuilder.Build();
        }

        protected virtual IRelationalCommandBuilder Sql => _relationalCommandBuilder;

        protected override Expression VisitSqlFragment(SqlFragmentExpression sqlFragmentExpression)
        {
            _relationalCommandBuilder.Append(sqlFragmentExpression.Sql);

            return sqlFragmentExpression;
        }

        protected override Expression VisitSelect(SelectExpression selectExpression)
        {
            IDisposable subQueryIndent = null;

            if (selectExpression.Alias != null)
            {
                _relationalCommandBuilder.AppendLine("(");
                subQueryIndent = _relationalCommandBuilder.Indent();
            }

            if (selectExpression.IsSetOperation)
            {
                GenerateSetOperation(selectExpression);
            }
            else
            {
                GenerateSelect(selectExpression);
            }

            if (selectExpression.Alias != null)
            {
                subQueryIndent.Dispose();

                _relationalCommandBuilder.AppendLine()
                    .Append(") AS " + _sqlGenerationHelper.DelimitIdentifier(selectExpression.Alias));
            }

            return selectExpression;
        }

        protected virtual void GenerateSelect(SelectExpression selectExpression)
        {
            _relationalCommandBuilder.Append("SELECT ");

            if (selectExpression.IsDistinct)
            {
                _relationalCommandBuilder.Append("DISTINCT ");
            }

            GenerateTop(selectExpression);

            if (selectExpression.Projection.Any())
            {
                GenerateList(selectExpression.Projection, e => Visit(e));
            }
            else
            {
                _relationalCommandBuilder.Append("1");
            }

            if (selectExpression.Tables.Any())
            {
                _relationalCommandBuilder.AppendLine().Append("FROM ");

                GenerateList(selectExpression.Tables, e => Visit(e), sql => sql.AppendLine());
            }

            if (selectExpression.Predicate != null)
            {
                _relationalCommandBuilder.AppendLine().Append("WHERE ");

                Visit(selectExpression.Predicate);
            }

            GenerateOrderings(selectExpression);
            GenerateLimitOffset(selectExpression);
        }

        protected virtual void GenerateSetOperation(SelectExpression setOperationExpression)
        {
            Debug.Assert(setOperationExpression.Tables.Count == 2,
                $"{nameof(SelectExpression)} with {setOperationExpression.Tables.Count} tables, must be 2");

            GenerateSetOperationOperand(setOperationExpression, (SelectExpression)setOperationExpression.Tables[0]);

            _relationalCommandBuilder
                .AppendLine()
                .AppendLine(GenerateSetOperationType(setOperationExpression.SetOperationType));

            GenerateSetOperationOperand(setOperationExpression, (SelectExpression)setOperationExpression.Tables[1]);

            GenerateOrderings(setOperationExpression);
            GenerateLimitOffset(setOperationExpression);
        }

        private static string GenerateSetOperationType(SetOperationType setOperationType)
            => setOperationType switch {
                SetOperationType.Union     => "UNION",
                SetOperationType.UnionAll  => "UNION ALL",
                SetOperationType.Intersect => "INTERSECT",
                SetOperationType.Except    => "EXCEPT",
                _ => throw new NotSupportedException($"Invalid {nameof(SetOperationType)}: {setOperationType}")
                };

        protected virtual void GenerateSetOperationOperand(
            SelectExpression setOperationExpression,
            SelectExpression operandExpression)
        {
            // INTERSECT has higher precedence over UNION and EXCEPT, but otherwise evaluation is left-to-right.
            // To preserve meaning, add parentheses whenever a set operation is nested within a different set operation.
            if (operandExpression.IsSetOperation
                && operandExpression.SetOperationType != setOperationExpression.SetOperationType)
            {
                _relationalCommandBuilder.AppendLine("(");
                using (_relationalCommandBuilder.Indent())
                {
                    Visit(operandExpression);
                }
                _relationalCommandBuilder.AppendLine().Append(")");
                return;
            }

            Visit(operandExpression);
        }

        protected override Expression VisitProjection(ProjectionExpression projectionExpression)
        {
            Visit(projectionExpression.Expression);

            if (!string.Equals(string.Empty, projectionExpression.Alias)
                && !(projectionExpression.Expression is ColumnExpression column
                     && string.Equals(column.Name, projectionExpression.Alias)))
            {
                _relationalCommandBuilder.Append(" AS " + _sqlGenerationHelper.DelimitIdentifier(projectionExpression.Alias));
            }

            return projectionExpression;
        }

        protected override Expression VisitSqlFunction(SqlFunctionExpression sqlFunctionExpression)
        {
            if (sqlFunctionExpression.IsBuiltIn)
            {
                if (sqlFunctionExpression.Instance != null)
                {
                    Visit(sqlFunctionExpression.Instance);
                    _relationalCommandBuilder.Append(".");
                }

                _relationalCommandBuilder.Append(sqlFunctionExpression.FunctionName);
            }
            else
            {
                if (!string.IsNullOrEmpty(sqlFunctionExpression.Schema))
                {
                    _relationalCommandBuilder
                        .Append(_sqlGenerationHelper.DelimitIdentifier(sqlFunctionExpression.Schema))
                        .Append(".");
                }

                _relationalCommandBuilder
                    .Append(_sqlGenerationHelper.DelimitIdentifier(sqlFunctionExpression.FunctionName));
            }

            if (!sqlFunctionExpression.IsNiladic)
            {
                _relationalCommandBuilder.Append("(");
                GenerateList(sqlFunctionExpression.Arguments, e => Visit(e));
                _relationalCommandBuilder.Append(")");
            }

            return sqlFunctionExpression;
        }

        protected override Expression VisitColumn(ColumnExpression columnExpression)
        {
            _relationalCommandBuilder
                .Append(_sqlGenerationHelper.DelimitIdentifier(columnExpression.Table.Alias))
                .Append(".")
                .Append(_sqlGenerationHelper.DelimitIdentifier(columnExpression.Name));

            return columnExpression;
        }

        protected override Expression VisitTable(TableExpression tableExpression)
        {
            _relationalCommandBuilder
                .Append(_sqlGenerationHelper.DelimitIdentifier(tableExpression.Table, tableExpression.Schema))
                .Append(" AS ")
                .Append(_sqlGenerationHelper.DelimitIdentifier(tableExpression.Alias));

            return tableExpression;
        }

        private void GenerateFromSql(FromSqlExpression fromSqlExpression)
        {
            var sql = fromSqlExpression.Sql;
            string[] substitutions = null;

            switch (fromSqlExpression.Arguments)
            {
                case ConstantExpression constantExpression
                when constantExpression.Value is CompositeRelationalParameter compositeRelationalParameter:
                    {
                        var subParameters = compositeRelationalParameter.RelationalParameters;
                        substitutions = new string[subParameters.Count];
                        for (var i = 0; i < subParameters.Count; i++)
                        {
                            substitutions[i] = _sqlGenerationHelper.GenerateParameterNamePlaceholder(subParameters[i].InvariantName);
                        }

                        _relationalCommandBuilder.AddParameter(compositeRelationalParameter);

                        break;
                    }
            }

            if (substitutions != null)
            {
                // ReSharper disable once CoVariantArrayConversion
                // InvariantCulture not needed since substitutions are all strings
                sql = string.Format(sql, substitutions);
            }


            _relationalCommandBuilder.AppendLines(sql);
        }

        protected override Expression VisitFromSql(FromSqlExpression fromSqlExpression)
        {
            _relationalCommandBuilder.AppendLine("(");

            using (_relationalCommandBuilder.Indent())
            {
                GenerateFromSql(fromSqlExpression);
            }

            _relationalCommandBuilder.Append(") AS ")
                .Append(_sqlGenerationHelper.DelimitIdentifier(fromSqlExpression.Alias));

            return fromSqlExpression;
        }

        protected override Expression VisitSqlBinary(SqlBinaryExpression sqlBinaryExpression)
        {
            if (sqlBinaryExpression.OperatorType == ExpressionType.Coalesce)
            {
                _relationalCommandBuilder.Append("COALESCE(");
                Visit(sqlBinaryExpression.Left);
                _relationalCommandBuilder.Append(", ");
                Visit(sqlBinaryExpression.Right);
                _relationalCommandBuilder.Append(")");
            }
            else
            {
                var requiresBrackets = RequiresBrackets(sqlBinaryExpression.Left);

                if (requiresBrackets)
                {
                    _relationalCommandBuilder.Append("(");
                }

                Visit(sqlBinaryExpression.Left);

                if (requiresBrackets)
                {
                    _relationalCommandBuilder.Append(")");
                }

                _relationalCommandBuilder.Append(GenerateOperator(sqlBinaryExpression));

                requiresBrackets = RequiresBrackets(sqlBinaryExpression.Right);

                if (requiresBrackets)
                {
                    _relationalCommandBuilder.Append("(");
                }

                Visit(sqlBinaryExpression.Right);

                if (requiresBrackets)
                {
                    _relationalCommandBuilder.Append(")");
                }
            }

            return sqlBinaryExpression;
        }

        private bool RequiresBrackets(SqlExpression expression)
        {
            return expression is SqlBinaryExpression sqlBinary
                && sqlBinary.OperatorType != ExpressionType.Coalesce
                || expression is LikeExpression;
        }

        protected override Expression VisitSqlConstant(SqlConstantExpression sqlConstantExpression)
        {
            _relationalCommandBuilder
                .Append(sqlConstantExpression.TypeMapping.GenerateSqlLiteral(sqlConstantExpression.Value));

            return sqlConstantExpression;
        }

        protected override Expression VisitSqlParameter(SqlParameterExpression sqlParameterExpression)
        {
            var parameterNameInCommand = _sqlGenerationHelper.GenerateParameterName(sqlParameterExpression.Name);

            if (_relationalCommandBuilder.Parameters
                .All(p => p.InvariantName != sqlParameterExpression.Name))
            {
                _relationalCommandBuilder.AddParameter(
                    sqlParameterExpression.Name,
                    parameterNameInCommand,
                    sqlParameterExpression.TypeMapping,
                    sqlParameterExpression.Type.IsNullableType());
            }

            _relationalCommandBuilder
                .Append(_sqlGenerationHelper.GenerateParameterNamePlaceholder(sqlParameterExpression.Name));

            return sqlParameterExpression;
        }

        protected override Expression VisitOrdering(OrderingExpression orderingExpression)
        {
            if (orderingExpression.Expression is SqlConstantExpression
                || orderingExpression.Expression is SqlParameterExpression)
            {
                _relationalCommandBuilder.Append("(SELECT 1)");
            }
            else
            {
                Visit(orderingExpression.Expression);
            }

            if (!orderingExpression.Ascending)
            {
                _relationalCommandBuilder.Append(" DESC");
            }

            return orderingExpression;
        }

        protected override Expression VisitLike(LikeExpression likeExpression)
        {
            Visit(likeExpression.Match);
            _relationalCommandBuilder.Append(" LIKE ");
            Visit(likeExpression.Pattern);

            if (likeExpression.EscapeChar != null)
            {
                _relationalCommandBuilder.Append(" ESCAPE ");
                Visit(likeExpression.EscapeChar);
            }

            return likeExpression;
        }

        protected override Expression VisitCase(CaseExpression caseExpression)
        {
            _relationalCommandBuilder.Append("CASE");

            if (caseExpression.Operand != null)
            {
                _relationalCommandBuilder.Append(" ");
                Visit(caseExpression.Operand);
            }

            using (_relationalCommandBuilder.Indent())
            {
                foreach (var whenClause in caseExpression.WhenClauses)
                {
                    _relationalCommandBuilder
                        .AppendLine()
                        .Append("WHEN ");
                    Visit(whenClause.Test);
                    _relationalCommandBuilder.Append(" THEN ");
                    Visit(whenClause.Result);
                }

                if (caseExpression.ElseResult != null)
                {
                    _relationalCommandBuilder
                        .AppendLine()
                        .Append("ELSE ");
                    Visit(caseExpression.ElseResult);
                }
            }

            _relationalCommandBuilder
                .AppendLine()
                .Append("END");

            return caseExpression;
        }

        protected override Expression VisitSqlUnary(SqlUnaryExpression sqlUnaryExpression)
        {
            switch (sqlUnaryExpression.OperatorType)
            {
                case ExpressionType.Convert:
                    {
                        _relationalCommandBuilder.Append("CAST(");
                        var requiresBrackets = RequiresBrackets(sqlUnaryExpression.Operand);
                        if (requiresBrackets)
                        {
                            _relationalCommandBuilder.Append("(");
                        }
                        Visit(sqlUnaryExpression.Operand);
                        if (requiresBrackets)
                        {
                            _relationalCommandBuilder.Append(")");
                        }
                        _relationalCommandBuilder.Append(" AS ");
                        _relationalCommandBuilder.Append(sqlUnaryExpression.TypeMapping.StoreType);
                        _relationalCommandBuilder.Append(")");
                    }

                    break;

                case ExpressionType.Not:
                    {
                        _relationalCommandBuilder.Append("NOT (");
                        Visit(sqlUnaryExpression.Operand);
                        _relationalCommandBuilder.Append(")");
                    }

                    break;

                case ExpressionType.Equal:
                    {
                        Visit(sqlUnaryExpression.Operand);
                        _relationalCommandBuilder.Append(" IS NULL");
                    }

                    break;

                case ExpressionType.NotEqual:
                    {
                        Visit(sqlUnaryExpression.Operand);
                        _relationalCommandBuilder.Append(" IS NOT NULL");
                    }

                    break;

                case ExpressionType.Negate:
                    {
                        _relationalCommandBuilder.Append("-");
                        Visit(sqlUnaryExpression.Operand);
                    }

                    break;
            }

            return sqlUnaryExpression;
        }

        protected override Expression VisitExists(ExistsExpression existsExpression)
        {
            if (existsExpression.Negated)
            {
                _relationalCommandBuilder.Append("NOT ");
            }

            _relationalCommandBuilder.AppendLine("EXISTS (");

            using (_relationalCommandBuilder.Indent())
            {
                Visit(existsExpression.Subquery);
            }

            _relationalCommandBuilder.Append(")");

            return existsExpression;
        }

        protected override Expression VisitIn(InExpression inExpression)
        {
            if (inExpression.Values != null)
            {
                Visit(inExpression.Item);
                _relationalCommandBuilder.Append(inExpression.Negated ? " NOT IN " : " IN ");
                _relationalCommandBuilder.Append("(");
                var valuesConstant = (SqlConstantExpression)inExpression.Values;
                var valuesList = ((IEnumerable<object>)valuesConstant.Value)
                    .Select(v => new SqlConstantExpression(Expression.Constant(v), valuesConstant.TypeMapping)).ToList();
                GenerateList(valuesList, e => Visit(e));
                _relationalCommandBuilder.Append(")");
            }
            else
            {
                Visit(inExpression.Item);
                _relationalCommandBuilder.Append(inExpression.Negated ? " NOT IN " : " IN ");
                _relationalCommandBuilder.AppendLine("(");

                using (_relationalCommandBuilder.Indent())
                {
                    Visit(inExpression.Subquery);
                }

                _relationalCommandBuilder.AppendLine().AppendLine(")");
            }

            return inExpression;
        }

        protected virtual string GenerateOperator(SqlBinaryExpression binaryExpression)
        {
            return _operatorMap[binaryExpression.OperatorType];
        }

        protected virtual void GenerateTop(SelectExpression selectExpression)
        {
        }

        protected virtual void GenerateOrderings(SelectExpression selectExpression)
        {
            if (selectExpression.Orderings.Any())
            {
                var orderings = selectExpression.Orderings.ToList();

                if (selectExpression.Limit == null
                    && selectExpression.Offset == null)
                {
                    orderings.RemoveAll(oe => oe.Expression is SqlConstantExpression || oe.Expression is SqlParameterExpression);
                }

                if (orderings.Count > 0)
                {
                    _relationalCommandBuilder.AppendLine()
                        .Append("ORDER BY ");

                    GenerateList(orderings, e => Visit(e));
                }
            }
            else if (selectExpression.Offset != null)
            {
                _relationalCommandBuilder.AppendLine().Append("ORDER BY (SELECT 1)");
            }
        }

        protected virtual void GenerateLimitOffset(SelectExpression selectExpression)
        {
            // The below implements ISO SQL:2008

            if (selectExpression.Offset != null)
            {
                _relationalCommandBuilder.AppendLine()
                    .Append("OFFSET ");

                Visit(selectExpression.Offset);

                _relationalCommandBuilder.Append(" ROWS");

                if (selectExpression.Limit != null)
                {
                    _relationalCommandBuilder.Append(" FETCH NEXT ");

                    Visit(selectExpression.Limit);

                    _relationalCommandBuilder.Append(" ROWS ONLY");
                }
            }
            else if (selectExpression.Limit != null)
            {
                _relationalCommandBuilder.AppendLine()
                    .Append("FETCH FIRST ");

                Visit(selectExpression.Limit);

                _relationalCommandBuilder.Append(" ROWS ONLY");
            }
        }

        private void GenerateList<T>(
            IReadOnlyList<T> items,
            Action<T> generationAction,
            Action<IRelationalCommandBuilder> joinAction = null)
        {
            joinAction ??= (isb => isb.Append(", "));

            for (var i = 0; i < items.Count; i++)
            {
                if (i > 0)
                {
                    joinAction(_relationalCommandBuilder);
                }

                generationAction(items[i]);
            }
        }

        protected override Expression VisitCrossJoin(CrossJoinExpression crossJoinExpression)
        {
            _relationalCommandBuilder.Append("CROSS JOIN ");
            Visit(crossJoinExpression.Table);

            return crossJoinExpression;
        }

        protected override Expression VisitInnerJoin(InnerJoinExpression innerJoinExpression)
        {
            _relationalCommandBuilder.Append("INNER JOIN ");
            Visit(innerJoinExpression.Table);
            _relationalCommandBuilder.Append(" ON ");
            Visit(innerJoinExpression.JoinPredicate);

            return innerJoinExpression;
        }

        protected override Expression VisitLeftJoin(LeftJoinExpression leftJoinExpression)
        {
            _relationalCommandBuilder.Append("LEFT JOIN ");
            Visit(leftJoinExpression.Table);
            _relationalCommandBuilder.Append(" ON ");
            Visit(leftJoinExpression.JoinPredicate);

            return leftJoinExpression;
        }

        protected override Expression VisitSubSelect(SubSelectExpression subSelectExpression)
        {
            _relationalCommandBuilder.AppendLine("(");
            using (_relationalCommandBuilder.Indent())
            {
                Visit(subSelectExpression.Subquery);
            }

            _relationalCommandBuilder.Append(")");

            return subSelectExpression;
        }
    }
}
