// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Query.Expressions;
using Microsoft.Data.Entity.Relational.Utilities;
using Remotion.Linq.Clauses;
using Remotion.Linq.Parsing;

namespace Microsoft.Data.Entity.Relational.Query.Sql
{
    public class DefaultSqlQueryGenerator : ThrowingExpressionTreeVisitor, ISqlExpressionVisitor, ISqlQueryGenerator
    {
        private StringBuilder _sql;
        private List<CommandParameter> _parameters;
        private Expression _binaryExpression;

        public virtual string GenerateSql(SelectExpression expression)
        {
            Check.NotNull(expression, "expression");

            _sql = new StringBuilder();
            _parameters = new List<CommandParameter>();

            expression.Accept(this);

            return _sql.ToString();
        }

        protected virtual StringBuilder Sql
        {
            get { return _sql; }
        }

        public virtual Expression VisitSelectExpression(SelectExpression selectExpression)
        {
            Check.NotNull(selectExpression, "selectExpression");

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
            else
            {
                _sql.Append("1");
            }

            _sql.AppendLine()
                .Append("FROM ");

            VisitJoin(selectExpression.Tables);

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
                        VisitExpression(t.Expression);

                        if (t.OrderingDirection == OrderingDirection.Desc)
                        {
                            _sql.Append(" DESC");
                        }
                    });
            }

            GenerateLimitOffset(selectExpression);

            return selectExpression;
        }

        private void VisitJoin(IReadOnlyList<Expression> expressions)
        {
            VisitJoin(expressions, e => VisitExpression(e));
        }

        private void VisitJoin<T>(IReadOnlyList<T> items, Action<T> itemAction)
        {
            for (var i = 0; i < items.Count; i++)
            {
                if (i > 0)
                {
                    _sql.Append(", ");
                }

                itemAction(items[i]);
            }
        }

        public virtual Expression VisitTableExpression(TableExpression tableExpression)
        {
            _sql.Append(DelimitIdentifier(tableExpression.Table))
                .Append(" AS ")
                .Append(tableExpression.Alias);

            return tableExpression;
        }

        protected virtual void GenerateTop([NotNull] SelectExpression expression)
        {
            Check.NotNull(expression, "expression");

            if (expression.Limit != null)
            {
                _sql.Append("TOP ")
                    .Append(expression.Limit)
                    .Append(" ");
            }
        }

        protected virtual void GenerateLimitOffset([NotNull] SelectExpression expression)
        {
        }

        protected override Expression VisitBinaryExpression([NotNull] BinaryExpression expression)
        {
            Check.NotNull(expression, "expression");

            _binaryExpression = expression;

            if (expression.IsLogicalOperation())
            {
                _sql.Append("(");
            }

            VisitExpression(expression.Left);

            string op;

            switch (expression.NodeType)
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

            VisitExpression(expression.Right);

            if (expression.IsLogicalOperation())
            {
                _sql.Append(")");
            }

            _binaryExpression = null;

            return expression;
        }

        protected virtual string ConcatOperator
        {
            get { return "+"; }
        }

        protected virtual string DelimitIdentifier(string identifier)
        {
            return "\"" + identifier + "\"";
        }

        protected virtual string GenerateLiteral(string literal)
        {
            return "'" + literal.Replace("'", "''") + "'";
        }

        public virtual Expression VisitColumnExpression(ColumnExpression columnExpression)
        {
            Check.NotNull(columnExpression, "columnExpression");

            _sql.Append(columnExpression.Alias)
                .Append(".")
                .Append(DelimitIdentifier(columnExpression.Property.ColumnName()));

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

        protected override Expression VisitConstantExpression(ConstantExpression expression)
        {
            Check.NotNull(expression, "expression");

            var maybeBool = expression.Value as bool?;

            if (maybeBool != null
                && (_binaryExpression == null
                    || _binaryExpression.IsLogicalOperation()))
            {
                _sql.Append(maybeBool.Value ? "1 = 1" : "1 = 0");
            }
            else
            {
                _sql.Append(CreateParameter(expression.Value));
            }

            return expression;
        }

        private string CreateParameter(object value)
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

        public virtual IEnumerable<CommandParameter> Parameters
        {
            get { return _parameters; }
        }

        protected override Exception CreateUnhandledItemException<T>(T unhandledItem, string visitMethod)
        {
            return new NotImplementedException(visitMethod);
        }
    }
}
