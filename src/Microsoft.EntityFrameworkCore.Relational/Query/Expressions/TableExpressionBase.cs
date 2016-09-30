// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;
using Remotion.Linq.Clauses;

namespace Microsoft.EntityFrameworkCore.Query.Expressions
{
    /// <summary>
    ///     A base class for SQL table expressions.
    /// </summary>
    public abstract class TableExpressionBase : Expression
    {
        private string _alias;
        private IQuerySource _querySource;

        /// <summary>
        ///     Initializes a new instance of the Microsoft.EntityFrameworkCore.Query.Expressions.TableExpressionBase class.
        /// </summary>
        /// <param name="querySource"> The query source. </param>
        /// <param name="alias"> The alias. </param>
        protected TableExpressionBase(
            [CanBeNull] IQuerySource querySource, [CanBeNull] string alias)
        {
            _querySource = querySource;
            _alias = alias;
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
        public override Type Type => typeof(object);

        /// <summary>
        ///     Gets the query source.
        /// </summary>
        /// <value>
        ///     The query source.
        /// </value>
        public virtual IQuerySource QuerySource
        {
            get { return _querySource; }
            [param: NotNull]
            set
            {
                Check.NotNull(value, nameof(value));

                _querySource = value;
            }
        }

        /// <summary>
        ///     Gets the alias.
        /// </summary>
        /// <value>
        ///     The alias.
        /// </value>
        public virtual string Alias
        {
            get { return _alias; }
            [param: NotNull]
            set
            {
                Check.NotNull(value, nameof(value));

                _alias = value;
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
        protected override Expression VisitChildren(ExpressionVisitor visitor) => this;
    }
}
