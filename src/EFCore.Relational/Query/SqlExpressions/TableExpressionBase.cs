// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query.SqlExpressions
{
    /// <summary>
    ///     <para>
    ///         An expression that represents a table source in a SQL tree.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    public abstract class TableExpressionBase : Expression, IPrintableExpression
    {
        /// <summary>
        ///     Creates a new instance of the <see cref="TableExpressionBase" /> class.
        /// </summary>
        /// <param name="alias"> A string alias for the table source. </param>
        protected TableExpressionBase([CanBeNull] string alias)
        {
            Check.NullButNotEmpty(alias, nameof(alias));

            Alias = alias;
        }

        /// <summary>
        ///     The alias assigned to this table source.
        /// </summary>
        public virtual string Alias { get; internal set; }

        /// <inheritdoc />
        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            Check.NotNull(visitor, nameof(visitor));

            return this;
        }

        /// <inheritdoc />
        public override Type Type
            => typeof(object);

        /// <inheritdoc />
        public sealed override ExpressionType NodeType
            => ExpressionType.Extension;

        /// <summary>
        ///     Creates a printable string representation of the given expression using <see cref="ExpressionPrinter" />.
        /// </summary>
        /// <param name="expressionPrinter"> The expression printer to use. </param>
        protected abstract void Print([NotNull] ExpressionPrinter expressionPrinter);

        /// <inheritdoc />
        void IPrintableExpression.Print(ExpressionPrinter expressionPrinter)
            => Print(expressionPrinter);

        /// <inheritdoc />
        public override bool Equals(object obj)
            => obj != null
                && (ReferenceEquals(this, obj)
                    || obj is TableExpressionBase tableExpressionBase
                    && Equals(tableExpressionBase));

        private bool Equals(TableExpressionBase tableExpressionBase)
            => string.Equals(Alias, tableExpressionBase.Alias);

        /// <inheritdoc />
        public override int GetHashCode()
            => HashCode.Combine(Alias);
    }
}
