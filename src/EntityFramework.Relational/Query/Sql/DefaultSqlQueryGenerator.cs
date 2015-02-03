// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Relational.Query.Expressions;
using Microsoft.Data.Entity.Utilities;
using Remotion.Linq.Clauses;
using Remotion.Linq.Parsing;

namespace Microsoft.Data.Entity.Relational.Query.Sql
{
    public class DefaultSqlQueryGenerator : ThrowingExpressionTreeVisitor, ISqlExpressionVisitor, ISqlQueryGenerator
    {
        private IndentedStringBuilder _sql;
        private List<CommandParameter> _parameters;
        private Expression _binaryExpression;

        public virtual string GenerateSql(SelectExpression selectExpression)
        {
            Check.NotNull(selectExpression, "selectExpression");

            _sql = new IndentedStringBuilder();
            _parameters = new List<CommandParameter>();

            selectExpression.Accept(this);

            return _sql.ToString();
        }

        protected virtual IndentedStringBuilder Sql => _sql;

        public virtual Expression VisitSelectExpression(SelectExpression selectExpression)
        {
            Check.NotNull(selectExpression, "selectExpression");

            IDisposable subQueryIndent = null;

            if (selectExpression.Alias != null)
            {
                _sql.AppendLine("(");

                subQueryIndent = _sql.Indent();
            }

            _sql.Append("SELECT ");

            if (selectExpression.IsDistinct)
            {
                _sql.Append("DISTINCT ");
            }

            GenerateTop(selectExpression);

            if (selectExpression.Projection.Any())
            {
                VisitJoin(selectExpression.Projection);
            }
            else if (selectExpression.ProjectionExpression != null)
            {
                VisitExpression(selectExpression.ProjectionExpression);
            }
            else if (selectExpression.IsProjectStar)
            {
                _sql.Append(DelimitIdentifier(selectExpression.Tables.Single().Alias))
                    .Append(".*");
            }
            else
            {
                _sql.Append("1");
            }

            if (selectExpression.Tables.Any())
            {
                _sql.AppendLine()
                    .Append("FROM ");

                VisitJoin(selectExpression.Tables, sql => sql.AppendLine());
            }

            if (selectExpression.Predicate != null)
            {
                _sql.AppendLine()
                    .Append("WHERE ");

                VisitExpression(selectExpression.Predicate);
            }

            if (selectExpression.OrderBy.Any())
            {
                _sql.AppendLine()
                    .Append("ORDER BY ");

                VisitJoin(selectExpression.OrderBy, t =>
                    {
                        var columnExpression = t.Expression as ColumnExpression;
                        if (columnExpression != null)
                        {
                            // strip column expression from alias - it should not be generated for ORDER BY clause
                            VisitExpression(new ColumnExpression(columnExpression.Name, columnExpression.Property, columnExpression.Table));
                        }
                        else
                        {
                            VisitExpression(t.Expression);
                        }

                        if (t.OrderingDirection == OrderingDirection.Desc)
                        {
                            _sql.Append(" DESC");
                        }
                    });
            }

            GenerateLimitOffset(selectExpression);

            if (subQueryIndent != null)
            {
                subQueryIndent.Dispose();

                _sql.AppendLine()
                    .Append(") AS ")
                    .Append(DelimitIdentifier(selectExpression.Alias));
            }

            return selectExpression;
        }

        private void VisitJoin(
            IReadOnlyList<Expression> expressions, Action<IndentedStringBuilder> joinAction = null)
        {
            VisitJoin(expressions, e => VisitExpression(e), joinAction);
        }

        private void VisitJoin<T>(
            IReadOnlyList<T> items, Action<T> itemAction, Action<IndentedStringBuilder> joinAction = null)
        {
            joinAction = joinAction ?? (isb => isb.Append(", "));

            for (var i = 0; i < items.Count; i++)
            {
                if (i > 0)
                {
                    joinAction(_sql);
                }

                itemAction(items[i]);
            }
        }

        public virtual Expression VisitTableExpression(TableExpression tableExpression)
        {
            Check.NotNull(tableExpression, "tableExpression");

            if (tableExpression.Schema != null)
            {
                _sql.Append(DelimitIdentifier(tableExpression.Schema))
                    .Append(".");
            }

            _sql.Append(DelimitIdentifier(tableExpression.Table))
                .Append(" AS ")
                .Append(DelimitIdentifier(tableExpression.Alias));

            return tableExpression;
        }

        public virtual Expression VisitCrossJoinExpression(CrossJoinExpression crossJoinExpression)
        {
            Check.NotNull(crossJoinExpression, "crossJoinExpression");

            _sql.Append("CROSS JOIN ");

            VisitExpression(crossJoinExpression.TableExpression);

            return crossJoinExpression;
        }

        public virtual Expression VisitCountExpression(CountExpression countExpression)
        {
            Check.NotNull(countExpression, "countExpression");

            _sql.Append("COUNT(*)");

            return countExpression;
        }

        public virtual Expression VisitSumExpression(SumExpression sumExpression)
        {
            Check.NotNull(sumExpression, "sumExpression");

            _sql.Append("SUM(");

            VisitExpression(sumExpression.ColumnExpression);

            _sql.Append(")");

            return sumExpression;
        }

        public virtual Expression VisitMinExpression(MinExpression minExpression)
        {
            Check.NotNull(minExpression, "minExpression");

            _sql.Append("MIN(");

            VisitExpression(minExpression.ColumnExpression);

            _sql.Append(")");

            return minExpression;
        }

        public virtual Expression VisitMaxExpression(MaxExpression maxExpression)
        {
            Check.NotNull(maxExpression, "maxExpression");

            _sql.Append("MAX(");

            VisitExpression(maxExpression.ColumnExpression);

            _sql.Append(")");

            return maxExpression;
        }

        public virtual Expression VisitInnerJoinExpression(InnerJoinExpression innerJoinExpression)
        {
            Check.NotNull(innerJoinExpression, "innerJoinExpression");

            _sql.Append("INNER JOIN ");

            VisitExpression(innerJoinExpression.TableExpression);

            _sql.Append(" ON ");

            VisitExpression(innerJoinExpression.Predicate);

            return innerJoinExpression;
        }

        public virtual Expression VisitOuterJoinExpression(LeftOuterJoinExpression leftOuterJoinExpression)
        {
            Check.NotNull(leftOuterJoinExpression, "leftOuterJoinExpression");

            _sql.Append("LEFT JOIN ");

            VisitExpression(leftOuterJoinExpression.TableExpression);

            _sql.Append(" ON ");

            VisitExpression(leftOuterJoinExpression.Predicate);

            return leftOuterJoinExpression;
        }

        protected virtual void GenerateTop([NotNull] SelectExpression selectExpression)
        {
            Check.NotNull(selectExpression, "selectExpression");

            if (selectExpression.Limit != null
                && selectExpression.Offset == null)
            {
                _sql.Append("TOP(")
                    .Append(CreateParameter(selectExpression.Limit))
                    .Append(") ");
            }
        }

        protected virtual void GenerateLimitOffset([NotNull] SelectExpression selectExpression)
        {
            Check.NotNull(selectExpression, "selectExpression");

            if (selectExpression.Offset != null)
            {
                if (!selectExpression.OrderBy.Any())
                {
                    throw new InvalidOperationException(Strings.SkipNeedsOrderBy);
                }

                _sql.Append(" OFFSET ")
                    .Append(CreateParameter(selectExpression.Offset))
                    .Append(" ROWS");

                if (selectExpression.Limit != null)
                {
                    _sql.Append(" FETCH NEXT ")
                        .Append(CreateParameter(selectExpression.Limit))
                        .Append(" ROWS ONLY");
                }
            }
        }

        public virtual Expression VisitCaseExpression(CaseExpression caseExpression)
        {
            Check.NotNull(caseExpression, "caseExpression");

            _sql.AppendLine("CASE WHEN (");

            using (_sql.Indent())
            {
                VisitExpression(caseExpression.When);
            }

            _sql.Append(") THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END");

            return caseExpression;
        }

        public virtual Expression VisitExistsExpression(ExistsExpression existsExpression)
        {
            Check.NotNull(existsExpression, "existsExpression");

            _sql.AppendLine("EXISTS (");

            using (_sql.Indent())
            {
                VisitExpression(existsExpression.Expression);
            }

            _sql.AppendLine()
                .AppendLine(")");

            return existsExpression;
        }

        protected override Expression VisitBinaryExpression([NotNull] BinaryExpression binaryExpression)
        {
            Check.NotNull(binaryExpression, "binaryExpression");

            _binaryExpression = binaryExpression;

            if (binaryExpression.IsLogicalOperation())
            {
                _sql.Append("(");
            }

            VisitExpression(binaryExpression.Left);

            string op;

            switch (binaryExpression.NodeType)
            {
                case ExpressionType.Equal:
                    op = " = ";
                    break;
                case ExpressionType.NotEqual:
                    op = " <> ";
                    break;
                case ExpressionType.GreaterThan:
                    op = " > ";
                    break;
                case ExpressionType.GreaterThanOrEqual:
                    op = " >= ";
                    break;
                case ExpressionType.LessThan:
                    op = " < ";
                    break;
                case ExpressionType.LessThanOrEqual:
                    op = " <= ";
                    break;
                case ExpressionType.AndAlso:
                    op = " AND ";
                    break;
                case ExpressionType.OrElse:
                    op = " OR ";
                    break;
                case ExpressionType.Add:
                    op = " " + ConcatOperator + " ";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            _sql.Append(op);

            VisitExpression(binaryExpression.Right);

            if (binaryExpression.IsLogicalOperation())
            {
                _sql.Append(")");
            }

            _binaryExpression = null;

            return binaryExpression;
        }

        protected virtual string ConcatOperator => "+";

        protected virtual string DelimitIdentifier([NotNull] string identifier)
        {
            Check.NotEmpty(identifier, "identifier");

            return "\"" + identifier + "\"";
        }

        protected virtual string GenerateLiteral([NotNull] string literal)
        {
            Check.NotEmpty(literal, "literal");

            return "'" + literal.Replace("'", "''") + "'";
        }

        public virtual Expression VisitColumnExpression(ColumnExpression columnExpression)
        {
            Check.NotNull(columnExpression, "columnExpression");

            _sql.Append(DelimitIdentifier(columnExpression.TableAlias))
                .Append(".")
                .Append(DelimitIdentifier(columnExpression.Name));

            if (columnExpression.Alias != null)
            {
                _sql.Append(" AS ")
                    .Append(DelimitIdentifier(columnExpression.Alias));
            }

            return columnExpression;
        }

        public virtual Expression VisitIsNullExpression(IsNullExpression isNullExpression)
        {
            Check.NotNull(isNullExpression, "isNullExpression");

            VisitExpression(isNullExpression.Operand);

            _sql.Append(" IS NULL");

            return isNullExpression;
        }

        public virtual Expression VisitIsNotNullExpression(IsNotNullExpression isNotNullExpression)
        {
            Check.NotNull(isNotNullExpression, "isNotNullExpression");

            VisitExpression(isNotNullExpression.Operand);

            _sql.Append(" IS NOT NULL");

            return isNotNullExpression;
        }

        public virtual Expression VisitLikeExpression(LikeExpression likeExpression)
        {
            Check.NotNull(likeExpression, "likeExpression");

            VisitExpression(likeExpression.Match);

            _sql.Append(" LIKE ");

            VisitExpression(likeExpression.Pattern);

            return likeExpression;
        }

        public virtual Expression VisitLiteralExpression(LiteralExpression literalExpression)
        {
            Check.NotNull(literalExpression, "literalExpression");

            _sql.Append(GenerateLiteral(literalExpression.Literal));

            return literalExpression;
        }

        protected override Expression VisitUnaryExpression(UnaryExpression unaryExpression)
        {
            Check.NotNull(unaryExpression, "unaryExpression");

            if (unaryExpression.NodeType == ExpressionType.Not)
            {
                _sql.Append("NOT ");

                return VisitExpression(unaryExpression.Operand);
            }

            if (unaryExpression.NodeType == ExpressionType.Convert
                && unaryExpression.Type.IsNullableType())
            {
                return VisitExpression(unaryExpression.Operand);
            }

            return base.VisitUnaryExpression(unaryExpression);
        }

        protected override Expression VisitConstantExpression(ConstantExpression constantExpression)
        {
            Check.NotNull(constantExpression, "constantExpression");

            var maybeBool = constantExpression.Value as bool?;

            if (maybeBool != null
                && (_binaryExpression == null
                    || _binaryExpression.IsLogicalOperation()))
            {
                _sql.Append(maybeBool.Value ? "1 = 1" : "1 = 0");
            }
            else
            {
                _sql.Append(CreateParameter(constantExpression.Value));
            }

            return constantExpression;
        }

        protected virtual string CreateParameter([NotNull] object value)
        {
            Check.NotNull(value, "value");

            var parameter
                = _parameters.SingleOrDefault(kv => Equals(kv.Value, value));

            if (parameter == null)
            {
                _parameters.Add(parameter = new CommandParameter("@p" + _parameters.Count, value));
            }

            return parameter.Name;
        }

        public virtual IEnumerable<CommandParameter> Parameters => _parameters;

        protected override Exception CreateUnhandledItemException<T>(T unhandledItem, string visitMethod)
        {
            return new NotImplementedException(visitMethod);
        }
    }
}
