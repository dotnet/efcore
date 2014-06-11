// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using JetBrains.Annotations;
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

        private int _aliasCount;

        private Expression _binaryExpression;

        public virtual string GenerateSql(SelectExpression expression)
        {
            Check.NotNull(expression, "expression");

            _sql = new StringBuilder();
            _parameters = new List<CommandParameter>();
            _aliasCount = 0;

            expression.Accept(this);

            return _sql.ToString();
        }

        protected virtual StringBuilder Sql
        {
            get { return _sql; }
        }

        public virtual Expression VisitSelectExpression(SelectExpression expression)
        {
            Check.NotNull(expression, "expression");

            _sql.Append("SELECT ");

            if (expression.IsDistinct)
            {
                _sql.Append("DISTINCT ");
            }

            GenerateTop(expression);

            if (expression.IsStar)
            {
                _sql.Append("*");
            }
            else
            {
                _sql.AppendJoin(
                    expression.Projection.Any()
                        ? expression.Projection.Select(p => p.StorageName)
                        : new[] { "1" });
            }

            _sql.AppendLine()
                .Append("FROM ");

            var subSelectExpression = expression.TableSource as SelectExpression;

            if (subSelectExpression != null)
            {
                _sql.Append("(")
                    .Append(VisitExpression(subSelectExpression))
                    .Append(") AS ")
                    .Append("t")
                    .Append(_aliasCount++.ToString(CultureInfo.InvariantCulture));
            }
            else
            {
                _sql.Append(expression.TableSource);
            }

            if (expression.Predicate != null)
            {
                _sql.AppendLine()
                    .Append("WHERE ");

                VisitExpression(expression.Predicate);
            }

            if (expression.OrderBy.Any())
            {
                _sql.AppendLine()
                    .Append("ORDER BY ")
                    .AppendJoin(
                        expression.OrderBy
                            .Select(o => o.Item2 == OrderingDirection.Asc
                                ? o.Item1.StorageName
                                : o.Item1.StorageName + " DESC"));
            }

            GenerateLimitOffset(expression);

            return expression;
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

        public virtual Expression VisitPropertyAccessExpression(PropertyAccessExpression expression)
        {
            Check.NotNull(expression, "expression");

            _sql.Append(expression.Property.StorageName);

            return expression;
        }

        public virtual Expression VisitIsNullExpression(IsNullExpression expression)
        {
            Check.NotNull(expression, "expression");

            VisitExpression(expression.Operand);

            _sql.Append(" IS NULL");

            return expression;
        }

        public virtual Expression VisitIsNotNullExpression(IsNotNullExpression expression)
        {
            Check.NotNull(expression, "expression");

            VisitExpression(expression.Operand);

            _sql.Append(" IS NOT NULL");

            return expression;
        }

        public virtual Expression VisitLikeExpression(LikeExpression expression)
        {
            Check.NotNull(expression, "expression");

            VisitExpression(expression.Match);

            _sql.Append(" LIKE ");

            VisitExpression(expression.Pattern);

            return expression;
        }

        public virtual Expression VisitLiteralExpression(LiteralExpression expression)
        {
            Check.NotNull(expression, "expression");

            _sql.Append("'")
                .Append(expression.Literal)
                .Append("'");

            return expression;
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
