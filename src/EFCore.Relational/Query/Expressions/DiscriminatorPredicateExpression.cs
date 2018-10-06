// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.Expressions.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using Remotion.Linq.Clauses;

namespace Microsoft.EntityFrameworkCore.Query.Expressions
{
    /// <summary>
    ///     Represents a discriminator predicate.
    /// </summary>
    public class DiscriminatorPredicateExpression : Expression, IPrintable
    {
        private readonly Expression _predicate;

        /// <summary>
        ///     Creates a new instance of a DiscriminatorPredicateExpression..
        /// </summary>
        /// <param name="predicate"> The predicate. </param>
        /// <param name="querySource"> The query source. </param>
        public DiscriminatorPredicateExpression(
            [NotNull] Expression predicate, [CanBeNull] IQuerySource querySource)
        {
            Check.NotNull(predicate, nameof(predicate));

            _predicate = predicate;

            QuerySource = querySource;
        }

        /// <summary>
        ///     Gets the query source.
        /// </summary>
        /// <value>
        ///     The query source.
        /// </value>
        public virtual IQuerySource QuerySource { get; }

        /// <summary>
        ///     Returns the node type of this <see cref="Expression" />. (Inherited from <see cref="Expression" />.)
        /// </summary>
        /// <returns>The <see cref="ExpressionType" /> that represents this expression.</returns>
        public override ExpressionType NodeType => ExpressionType.Extension;

        /// <summary>
        ///     Gets the static type of the expression that this <see cref="Expression" /> represents. (Inherited from <see cref="Expression" />.)
        /// </summary>
        /// <returns>The <see cref="Type" /> that represents the static type of the expression.</returns>
        public override Type Type => _predicate.Type;

        /// <summary>
        ///     Indicates that the node can be reduced to a simpler node. If this
        ///     returns true, Reduce() can be called to produce the reduced form.
        /// </summary>
        public override bool CanReduce => true;

        /// <summary>
        ///     Reduces this node to a simpler expression. If CanReduce returns
        ///     true, this should return a valid expression. This method is
        ///     allowed to return another node which itself must be reduced.
        /// </summary>
        /// <returns>The reduced expression.</returns>
        public override Expression Reduce() => _predicate;

        /// <summary>
        ///     Reduces the node and then calls the <see cref="ExpressionVisitor.Visit(Expression)" /> method passing the
        ///     reduced expression.
        ///     Throws an exception if the node isn't reducible.
        /// </summary>
        /// <param name="visitor"> An instance of <see cref="ExpressionVisitor" />. </param>
        /// <returns> The expression being visited, or an expression which should replace it in the tree. </returns>
        /// <remarks>
        ///     Override this method to provide logic to walk the node's children.
        ///     A typical implementation will call visitor.Visit on each of its
        ///     children, and if any of them change, should return a new copy of
        ///     itself with the modified children.
        /// </remarks>
        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var newPredicate = visitor.Visit(_predicate);

            return _predicate != newPredicate
                ? new DiscriminatorPredicateExpression(newPredicate, QuerySource)
                : this;
        }

        /// <summary>
        ///     Tests if this object is considered equal to another.
        /// </summary>
        /// <param name="obj"> The object to compare with the current object. </param>
        /// <returns>
        ///     true if the objects are considered equal, false if they are not.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (obj is null)
            {
                return false;
            }

            return ReferenceEquals(this, obj) ? true : obj.GetType() == GetType() && Equals((DiscriminatorPredicateExpression)obj);
        }

        private bool Equals(DiscriminatorPredicateExpression other)
        {
            return Equals(_predicate, other._predicate) && Equals(QuerySource, other.QuerySource);
        }

        /// <summary>
        ///     Returns a hash code for this object.
        /// </summary>
        /// <returns>
        ///     A hash code for this object.
        /// </returns>
        public override int GetHashCode()
        {
            unchecked
            {
                return (_predicate.GetHashCode() * 397) ^ (QuerySource?.GetHashCode() ?? 0);
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        void IPrintable.Print(ExpressionPrinter expressionPrinter)
            => expressionPrinter.Visit(_predicate);

        /// <summary>
        ///     Creates a <see cref="string" /> representation of the Expression.
        /// </summary>
        /// <returns>A <see cref="string" /> representation of the Expression.</returns>
        public override string ToString() => _predicate.ToString();
    }
}
