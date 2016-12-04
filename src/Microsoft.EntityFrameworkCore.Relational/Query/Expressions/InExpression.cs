// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.Sql;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query.Expressions
{
    /// <summary>
    ///     Represents a SQL IN expression.
    /// </summary>
    public class InExpression : Expression
    {
        /// <summary>
        ///     Creates a new instance of InExpression.
        /// </summary>
        /// <param name="operand"> The operand. </param>
        /// <param name="values"> The values. </param>
        public InExpression(
            [NotNull] Expression operand,
            [NotNull] IReadOnlyList<Expression> values)
        {
            Check.NotNull(operand, nameof(operand));
            Check.NotNull(values, nameof(values));

            Operand = operand;
            Values = values;
        }

        /// <summary>
        ///     Creates a new instance of InExpression.
        /// </summary>
        /// <param name="operand"> The operand. </param>
        /// <param name="subQuery"> The sub query. </param>
        public InExpression(
            [NotNull] Expression operand,
            [NotNull] SelectExpression subQuery)
        {
            Check.NotNull(operand, nameof(operand));
            Check.NotNull(subQuery, nameof(subQuery));

            Operand = operand;
            SubQuery = subQuery;
        }

        /// <summary>
        ///     Gets the operand.
        /// </summary>
        /// <value>
        ///     The operand.
        /// </value>
        public virtual Expression Operand { get; }

        /// <summary>
        ///     Gets the values.
        /// </summary>
        /// <value>
        ///     The values.
        /// </value>
        public virtual IReadOnlyList<Expression> Values { get; }

        /// <summary>
        ///     Gets the sub query.
        /// </summary>
        /// <value>
        ///     The sub query.
        /// </value>
        public virtual SelectExpression SubQuery { get; }

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
        ///     Dispatches to the specific visit method for this node type.
        /// </summary>
        protected override Expression Accept(ExpressionVisitor visitor)
        {
            Check.NotNull(visitor, nameof(visitor));

            var specificVisitor = visitor as ISqlExpressionVisitor;

            return specificVisitor != null
                ? specificVisitor.VisitIn(this)
                : base.Accept(visitor);
        }

        /// <summary>
        ///     Reduces the node and then calls the <see cref="ExpressionVisitor.Visit(System.Linq.Expressions.Expression)" /> method passing the
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
        protected override Expression VisitChildren(ExpressionVisitor visitor) => this;

        /// <summary>
        ///     Creates a <see cref="String" /> representation of the Expression.
        /// </summary>
        /// <returns>A <see cref="String" /> representation of the Expression.</returns>
        public override string ToString()
            => Operand + " IN (" + string.Join(", ", Values) + ")";
    }
}
