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
    ///         An expression that represents a subquery table source with user-provided custom SQL.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    // Class is sealed because there are no public/protected constructors. Can be unsealed if this is changed.
    public sealed class FromSqlExpression : TableExpressionBase
    {
        internal FromSqlExpression([NotNull] string sql, [NotNull] Expression arguments, [NotNull] string alias)
            : base(alias)
        {
            Check.NotEmpty(sql, nameof(sql));
            Check.NotNull(arguments, nameof(arguments));

            Sql = sql;
            Arguments = arguments;
        }

        /// <summary>
        ///     The user-provided custom SQL for the table source.
        /// </summary>
        public string Sql { get; }
        /// <summary>
        ///     The user-provided parameters passed to the custom SQL.
        /// </summary>
        public Expression Arguments { get; }

        /// <summary>
        ///     Creates a new expression that is like this one, but using the supplied children. If all of the children are the same, it will
        ///     return this expression.
        /// </summary>
        /// <param name="arguments"> The <see cref="Arguments"/> property of the result. </param>
        /// <returns> This expression if no children changed, or an expression with the updated children. </returns>
        public FromSqlExpression Update([NotNull] Expression arguments)
        {
            Check.NotNull(arguments, nameof(arguments));

            return arguments != Arguments
                ? new FromSqlExpression(Sql, arguments, Alias)
                : this;
        }

        /// <inheritdoc />
        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            Check.NotNull(visitor, nameof(visitor));

            return this;
        }

        /// <inheritdoc />
        public override void Print(ExpressionPrinter expressionPrinter)
        {
            Check.NotNull(expressionPrinter, nameof(expressionPrinter));

            expressionPrinter.Append(Sql);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
            => obj != null
                && (ReferenceEquals(this, obj)
                    || obj is FromSqlExpression fromSqlExpression
                    && Equals(fromSqlExpression));

        private bool Equals(FromSqlExpression fromSqlExpression)
            => base.Equals(fromSqlExpression)
                && string.Equals(Sql, fromSqlExpression.Sql);

        /// <inheritdoc />
        public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), Sql);
    }
}
