// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query.SqlExpressions
{
    public class OrderingExpression : Expression, IPrintableExpression
    {
        public OrderingExpression([NotNull] SqlExpression expression, bool ascending)
        {
            Check.NotNull(expression, nameof(expression));

            Expression = expression;
            IsAscending = ascending;
        }

        public virtual SqlExpression Expression { get; }
        public virtual bool IsAscending { get; }

        public sealed override ExpressionType NodeType => ExpressionType.Extension;
        public override Type Type => Expression.Type;

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            Check.NotNull(visitor, nameof(visitor));

            return Update((SqlExpression)visitor.Visit(Expression));
        }

        public virtual OrderingExpression Update([NotNull] SqlExpression expression)
        {
            Check.NotNull(expression, nameof(expression));

            return expression != Expression
                ? new OrderingExpression(expression, IsAscending)
                : this;
        }

        public virtual void Print(ExpressionPrinter expressionPrinter)
        {
            Check.NotNull(expressionPrinter, nameof(expressionPrinter));

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
