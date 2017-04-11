// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query.Expressions
{
    /// <summary>
    ///     Represents the result of translating an Expression into a set of 
    ///     various relational expressions, such as <see cref="Microsoft.EntityFrameworkCore.Query.Expressions.AliasExpression"/>
    ///     and <see cref="Microsoft.EntityFrameworkCore.Query.Expressions.SqlFunctionExpression"/>.
    /// </summary>
    public class CompositeExpression : Expression
    {
        /// <summary>
        ///     Creates a new instance of CompositeExpression.
        /// </summary>
        /// <param name="expressions"> The expressions. </param>
        public CompositeExpression([NotNull] IEnumerable<Expression> expressions)
        {
            Check.NotNull(expressions, nameof(expressions));

            Expressions = expressions.ToArray();
        }

        /// <summary>
        ///     Gets the expressions.
        /// </summary>
        /// <value>
        ///     The expressions.
        /// </value>
        public virtual IReadOnlyList<Expression> Expressions { get; }

        /// <summary>
        ///     Returns the node type of this <see cref="Expression" />. (Inherited from <see cref="Expression" />.)
        /// </summary>
        /// <returns>The <see cref="ExpressionType" /> that represents this expression.</returns>
        public override ExpressionType NodeType => ExpressionType.Extension;

        /// <summary>
        ///     Gets the static type of the expression that this <see cref="Expression" /> represents. (Inherited from <see cref="Expression" />.)
        /// </summary>
        /// <returns>The <see cref="Type" /> that represents the static type of the expression.</returns>
        public override Type Type => typeof(object);

        /// <summary>
        ///     Produces a sequence of <see cref="Expression" /> instances within this
        ///     expression, flattening nested <see cref="CompositeExpression" /> into the sequence.
        /// </summary>
        /// <returns> A sequence of flattened expressions. </returns>
        public virtual IEnumerable<Expression> Flatten()
        {
            foreach (var expression in Expressions)
            {
                if (expression is CompositeExpression compositeExpression)
                {
                    foreach (var subExpression in compositeExpression.Flatten())
                    {
                        yield return subExpression;
                    }
                }
                else
                {
                    yield return expression;
                }
            }
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
        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var expressions = Expressions.Select(visitor.Visit).ToArray();

            return !expressions.SequenceEqual(Expressions)
                ? new CompositeExpression(expressions)
                : this;
        }

        /// <summary>
        ///     Creates a <see cref="string" /> representation of the Expression.
        /// </summary>
        /// <returns>A <see cref="string" /> representation of the Expression.</returns>
        public override string ToString() => "{ " + string.Join(", ", Expressions.Select(e => e.ToString())) + " }";
    }
}