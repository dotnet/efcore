// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query.SqlExpressions
{
    // Class is sealed because there are no public/protected constructors. Can be unsealed if this is changed.
    public sealed class ProjectionExpression : Expression, IPrintableExpression
    {
        internal ProjectionExpression([NotNull] SqlExpression expression, [NotNull] string alias)
        {
            Check.NotNull(expression, nameof(expression));
            Check.NotNull(alias, nameof(alias));

            Expression = expression;
            Alias = alias;
        }

        public string Alias { get; }
        public SqlExpression Expression { get; }

        public override Type Type => Expression.Type;
        public sealed override ExpressionType NodeType => ExpressionType.Extension;

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            Check.NotNull(visitor, nameof(visitor));

            return Update((SqlExpression)visitor.Visit(Expression));
        }

        public ProjectionExpression Update([NotNull] SqlExpression expression)
        {
            Check.NotNull(expression, nameof(expression));

            return expression != Expression
                ? new ProjectionExpression(expression, Alias)
                : this;
        }

        public void Print(ExpressionPrinter expressionPrinter)
        {
            Check.NotNull(expressionPrinter, nameof(expressionPrinter));

            expressionPrinter.Visit(Expression);
            if (!string.Equals(string.Empty, Alias)
                && !(Expression is ColumnExpression column
                    && string.Equals(column.Name, Alias)))
            {
                expressionPrinter.Append(" AS " + Alias);
            }
        }

        public override bool Equals(object obj)
            => obj != null
                && (ReferenceEquals(this, obj)
                    || obj is ProjectionExpression projectionExpression
                    && Equals(projectionExpression));

        private bool Equals(ProjectionExpression projectionExpression)
            => string.Equals(Alias, projectionExpression.Alias)
                && Expression.Equals(projectionExpression.Expression);

        public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), Alias, Expression);
    }
}
