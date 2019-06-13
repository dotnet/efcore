// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query.Expressions.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace Microsoft.EntityFrameworkCore.Relational.Query.Pipeline.SqlExpressions
{
    public class ProjectionExpression : Expression, IPrintable
    {
        public ProjectionExpression(SqlExpression expression, string alias)
        {
            Expression = expression;
            Alias = alias;
        }

        public string Alias { get; }
        public SqlExpression Expression { get; }

        public override Type Type => Expression.Type;
        public override ExpressionType NodeType => ExpressionType.Extension;

        protected override Expression VisitChildren(ExpressionVisitor visitor)
            => Update((SqlExpression)visitor.Visit(Expression));

        public ProjectionExpression Update(SqlExpression expression)
            => expression != Expression
                ? new ProjectionExpression(expression, Alias)
                : this;

        public void Print(ExpressionPrinter expressionPrinter)
        {
            expressionPrinter.Visit(Expression);
            if (!string.Equals(string.Empty, Alias)
                && !(Expression is ColumnExpression column
                && string.Equals(column.Name, Alias)))
            {
                expressionPrinter.StringBuilder.Append(" AS " + Alias);
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
