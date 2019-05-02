// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;

namespace Microsoft.EntityFrameworkCore.Relational.Query.Pipeline.SqlExpressions
{
    public class ProjectionExpression : Expression
    {
        #region Fields & Constructors
        public ProjectionExpression(SqlExpression expression, string alias)
        {
            Expression = expression;
            Alias = alias;
        }
        #endregion

        #region Public Properties
        public string Alias { get; }
        public SqlExpression Expression { get; }
        #endregion

        #region Expression-based methods/properties
        public override Type Type => Expression.Type;
        public override ExpressionType NodeType => ExpressionType.Extension;

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var expression = (SqlExpression)visitor.Visit(Expression);

            return Update(expression);
        }

        public ProjectionExpression Update(SqlExpression expression)
        {
            return expression != Expression
                ? new ProjectionExpression(expression, Alias)
                : this;
        }
        #endregion

        #region Equality & HashCode
        public override bool Equals(object obj)
            => obj != null
            && (ReferenceEquals(this, obj)
                || obj is ProjectionExpression projectionExpression
                    && Equals(projectionExpression));

        private bool Equals(ProjectionExpression projectionExpression)
            => string.Equals(Alias, projectionExpression.Alias)
            && Expression.Equals(projectionExpression.Expression);

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ Alias.GetHashCode();
                hashCode = (hashCode * 397) ^ Expression.GetHashCode();

                return hashCode;
            }
        }
        #endregion
    }
}
