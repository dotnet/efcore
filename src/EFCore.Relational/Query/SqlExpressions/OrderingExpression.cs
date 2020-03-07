// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;

namespace Microsoft.EntityFrameworkCore.Query.SqlExpressions
{
    public class OrderingExpression : Expression, IPrintableExpression
    {
        public OrderingExpression(SqlExpression expression, bool ascending)
        {
            Expression = expression;
            IsAscending = ascending;
        }

        public virtual SqlExpression Expression { get; }
        public virtual bool IsAscending { get; }

        public sealed override ExpressionType NodeType => ExpressionType.Extension;
        public override Type Type => Expression.Type;

        protected override Expression VisitChildren(ExpressionVisitor visitor)
            => Update((SqlExpression)visitor.Visit(Expression));

        public virtual OrderingExpression Update(SqlExpression expression)
            => expression != Expression
                ? new OrderingExpression(expression, IsAscending)
                : this;

        public virtual void Print(ExpressionPrinter expressionPrinter)
        {
            expressionPrinter.Visit(Expression);

            expressionPrinter.Append(IsAscending ? " ASC" : " DESC");
        }

        public override bool Equals(object obj)
            => obj != null
                && (ReferenceEquals(this, obj)
                    || obj is OrderingExpression orderingExpression
                    && Equals(orderingExpression));

        private bool Equals(OrderingExpression orderingExpression)
            => Expression.Equals(orderingExpression.Expression)
                && IsAscending == orderingExpression.IsAscending;

        public override int GetHashCode() => HashCode.Combine(Expression, IsAscending);
    }
}
