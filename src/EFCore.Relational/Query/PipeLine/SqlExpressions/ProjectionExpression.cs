// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;

namespace Microsoft.EntityFrameworkCore.Relational.Query.Pipeline.SqlExpressions
{
    public class ProjectionExpression : Expression
    {
        public ProjectionExpression(SqlExpression sqlExpression, string alias)
        {
            SqlExpression = sqlExpression;
            Alias = alias;
        }

        public override Type Type => SqlExpression.Type;

        public override ExpressionType NodeType => ExpressionType.Extension;

        public string Alias { get; }

        public SqlExpression SqlExpression { get; }

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var sql = (SqlExpression)visitor.Visit(SqlExpression);

            return sql != SqlExpression
                ? new ProjectionExpression(sql, Alias)
                : this;
        }

        public override bool Equals(object obj)
            => obj != null
            && (ReferenceEquals(this, obj)
                || obj is ProjectionExpression projectionExpression
                    && Equals(projectionExpression));

        private bool Equals(ProjectionExpression projectionExpression)
            => string.Equals(Alias, projectionExpression.Alias)
            && SqlExpression.Equals(projectionExpression.SqlExpression);

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ Alias.GetHashCode();
                hashCode = (hashCode * 397) ^ SqlExpression.GetHashCode();

                return hashCode;
            }
        }
    }
}
