// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Query.Expressions
{
    /// <summary>
    ///     Reducible annotation expression used to affect null expansion logic.
    /// </summary>
    public class NotNullableExpression : Expression
    {
        private readonly Expression _operand;

        /// <summary>
        ///     Creates an instance of NotNullableExpression.
        /// </summary>
        /// <param name="operand"> The operand. </param>
        public NotNullableExpression([NotNull] Expression operand)
        {
            _operand = operand;
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
        public override Type Type => typeof(bool);

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
                ? new NotNullableExpression(newExpression)
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
    }
}
