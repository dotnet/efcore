// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Query.Sql;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query.Expressions
{
    /// <summary>
    ///     Represents a SQL string comparison expression.
    /// </summary>
    public class StringCompareExpression : Expression
    {
        /// <summary>
        ///     Initializes a new instance of the Microsoft.EntityFrameworkCore.Query.Expressions.StringCompareExpression class.
        /// </summary>
        /// <param name="op"> The comparison operation. </param>
        /// <param name="left"> The left operand. </param>
        /// <param name="right"> The right operand. </param>
        public StringCompareExpression(ExpressionType op, [NotNull] Expression left, [NotNull] Expression right)
        {
            Operator = op;
            Left = left;
            Right = right;
        }

        /// <summary>
        ///     Returns the node type of this <see cref="Expression" />. (Inherited from <see cref="Expression" />.)
        /// </summary>
        /// <returns>The <see cref="ExpressionType" /> that represents this expression.</returns>
        public override ExpressionType NodeType => ExpressionType.Extension;

        /// <summary>
        ///     Gets the static type of the expression that this <see cref="Expression" /> represents. (Inherited from <see cref="Expression" />.)
        /// </summary>
        /// <returns>The <see cref="Type" /> that represents the static type of the expression.</returns>
        public override Type Type => typeof(bool);

        /// <summary>
        ///     Gets the comparison operator.
        /// </summary>
        /// <value>
        ///     The comparison operator.
        /// </value>
        public virtual ExpressionType Operator { get; }

        /// <summary>
        ///     Gets the left operand.
        /// </summary>
        /// <value>
        ///     The left operand.
        /// </value>
        public virtual Expression Left { get; }

        /// <summary>
        ///     Gets the right operand.
        /// </summary>
        /// <value>
        ///     The right operand.
        /// </value>
        public virtual Expression Right { get; }

        /// <summary>
        ///     Dispatches to the specific visit method for this node type.
        /// </summary>
        protected override Expression Accept(ExpressionVisitor visitor)
        {
            Check.NotNull(visitor, nameof(visitor));

            return visitor is ISqlExpressionVisitor specificVisitor
                ? specificVisitor.VisitStringCompare(this)
                : base.Accept(visitor);
        }

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
            var newLeft = visitor.Visit(Left);
            var newRight = visitor.Visit(Right);

            return newLeft != Left || newRight != Right
                ? new StringCompareExpression(Operator, newLeft, newRight)
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

            return ReferenceEquals(this, obj) ? true : obj.GetType() == GetType() && Equals((StringCompareExpression)obj);
        }

        private bool Equals(StringCompareExpression other)
            => Operator == other.Operator
               && ExpressionEqualityComparer.Instance.Equals(Left, other.Left)
               && ExpressionEqualityComparer.Instance.Equals(Right, other.Right);

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
                var hashCode = (int)Operator;
                hashCode = (hashCode * 397) ^ Left.GetHashCode();
                hashCode = (hashCode * 397) ^ Right.GetHashCode();
                return hashCode;
            }
        }
    }
}
