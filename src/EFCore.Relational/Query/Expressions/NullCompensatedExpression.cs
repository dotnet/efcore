// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query.Expressions
{
    /// <summary>
    ///     Reducible annotation expression indicating that the following expression fragment has been compensated for null semantics.
    ///     No additional null semantics related processing is needed for this fragment.
    /// </summary>
    public class NullCompensatedExpression : Expression
    {
        private readonly Expression _operand;

        /// <summary>
        ///     Creates an instance of NotNullableExpression.
        /// </summary>
        /// <param name="operand"> The operand. </param>
        public NullCompensatedExpression([NotNull] Expression operand)
        {
            Check.NotNull(operand, nameof(operand));

            _operand = operand;
            Type = _operand.Type.UnwrapNullableType();
        }

        /// <summary>
        ///     The operand.
        /// </summary>
        public virtual Expression Operand => _operand;

        /// <summary>
        ///     Type of the node.
        /// </summary>
        public override ExpressionType NodeType => ExpressionType.Extension;

        /// <summary>
        ///     The type.
        /// </summary>
        public override Type Type { get; }

        /// <summary>
        ///     Reduces the node and then calls the visitor delegate on the reduced expression. The method throws an exception if the node is not
        ///     reducible.
        /// </summary>
        /// <returns>The expression being visited, or an expression which should replace it in the tree.</returns>
        /// <param name="visitor">An instance of <see cref="T:System.Func`2" />.</param>
        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var newExpression = visitor.Visit(_operand);

            return newExpression != _operand
                ? new NullCompensatedExpression(newExpression)
                : this;
        }

        /// <summary>
        ///     Indicates that the node can be reduced to a simpler node. If this returns true, Reduce() can be called to produce the reduced
        ///     form.
        /// </summary>
        /// <returns>True if the node can be reduced, otherwise false.</returns>
        public override bool CanReduce => true;

        /// <summary>
        ///     Reduces this node to a simpler expression. If CanReduce returns true, this should return a valid expression. This method can
        ///     return another node which itself must be reduced.
        /// </summary>
        /// <returns>The reduced expression.</returns>
        public override Expression Reduce() => _operand;

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

            return ReferenceEquals(this, obj) ? true : obj.GetType() == GetType() && Equals((NullCompensatedExpression)obj);
        }

        private bool Equals([NotNull] NullCompensatedExpression other)
            => ExpressionEqualityComparer.Instance.Equals(_operand, other._operand);

        /// <summary>
        ///     Returns a hash code for this object.
        /// </summary>
        /// <returns>
        ///     A hash code for this object.
        /// </returns>
        public override int GetHashCode() => _operand.GetHashCode();
    }
}
