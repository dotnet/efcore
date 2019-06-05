// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query.Expressions.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace Microsoft.EntityFrameworkCore.Relational.Query.Pipeline.SqlExpressions
{
    public class OrderingExpression : Expression, IPrintable
    {
        public OrderingExpression(SqlExpression expression, bool ascending)
        {
            Expression = expression;
            Ascending = ascending;
        }

        public SqlExpression Expression { get; }
        public bool Ascending { get; }

        public override ExpressionType NodeType => ExpressionType.Extension;
        public override Type Type => Expression.Type;
        protected override Expression VisitChildren(ExpressionVisitor visitor)
            => Update((SqlExpression)visitor.Visit(Expression));

        public OrderingExpression Update(SqlExpression expression)
            => expression != Expression
                ? new OrderingExpression(expression, Ascending)
                : this;

        public void Print(ExpressionPrinter expressionPrinter)
        {
            expressionPrinter.Visit(Expression);

            expressionPrinter.StringBuilder.Append(Ascending ? "ASC" : "DESC");
        }

        public override bool Equals(object obj)
            => obj != null
            && (ReferenceEquals(this, obj)
                || obj is OrderingExpression orderingExpression
                    && Equals(orderingExpression));

        private bool Equals(OrderingExpression orderingExpression)
            => Expression.Equals(orderingExpression.Expression)
            && Ascending == orderingExpression.Ascending;

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Expression.GetHashCode();
                hashCode = (hashCode * 397) ^ Ascending.GetHashCode();

                return hashCode;
            }
        }
    }
}
