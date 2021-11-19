// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Linq.Expressions;

namespace Microsoft.EntityFrameworkCore.Query.SqlExpressions
{
    /// <summary>
    ///     An expression that represents a projection in <see cref="SelectExpression" />.
    /// </summary>
    /// <remarks>
    ///     This is a simple wrapper around a <see cref="SqlExpression" /> and an alias.
    ///     Instances of this type cannot be constructed by application or database provider code. If this is a problem for your
    ///     application or provider, then please file an issue at
    ///     <see href="https://github.com/dotnet/efcore">github.com/dotnet/efcore</see>.
    /// </remarks>
    public sealed class ProjectionExpression : Expression, IPrintableExpression
    {
        internal ProjectionExpression(SqlExpression expression, string alias)
        {
            Expression = expression;
            Alias = alias;
        }

        /// <summary>
        ///     The alias assigned to this projection, if any.
        /// </summary>
        public string Alias { get; }

        /// <summary>
        ///     The SQL value which is being projected.
        /// </summary>
        public SqlExpression Expression { get; }

        /// <inheritdoc />
        public override Type Type
            => Expression.Type;

        /// <inheritdoc />
        public override ExpressionType NodeType
            => ExpressionType.Extension;

        /// <inheritdoc />
        protected override Expression VisitChildren(ExpressionVisitor visitor)
            => Update((SqlExpression)visitor.Visit(Expression));

        /// <summary>
        ///     Creates a new expression that is like this one, but using the supplied children. If all of the children are the same, it will
        ///     return this expression.
        /// </summary>
        /// <param name="expression">The <see cref="Expression" /> property of the result.</param>
        /// <returns>This expression if no children changed, or an expression with the updated children.</returns>
        public ProjectionExpression Update(SqlExpression expression)
            => expression != Expression
                ? new ProjectionExpression(expression, Alias)
                : this;

        /// <inheritdoc />
        void IPrintableExpression.Print(ExpressionPrinter expressionPrinter)
        {
            expressionPrinter.Visit(Expression);
            if (Alias != string.Empty
                && !(Expression is ColumnExpression column
                    && column.Name == Alias))
            {
                expressionPrinter.Append(" AS " + Alias);
            }
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
            => obj != null
                && (ReferenceEquals(this, obj)
                    || obj is ProjectionExpression projectionExpression
                    && Equals(projectionExpression));

        private bool Equals(ProjectionExpression projectionExpression)
            => Alias == projectionExpression.Alias && Expression.Equals(projectionExpression.Expression);

        /// <inheritdoc />
        public override int GetHashCode()
            => HashCode.Combine(base.GetHashCode(), Alias, Expression);
    }
}
