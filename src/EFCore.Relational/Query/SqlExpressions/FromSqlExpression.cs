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
    public class FromSqlExpression : TableExpressionBase
    {
        /// <summary>
        ///     Creates a new instance of the <see cref="FromSqlExpression" /> class.
        /// </summary>
        /// <param name="sql"> A user-provided custom SQL for the table source. </param>
        /// <param name="arguments"> A user-provided parameters to pass to the custom SQL. </param>
        /// <param name="alias"> A string alias for the table source. </param>
        [Obsolete("Use the constructor which takes alias as first argument.")]
        public FromSqlExpression([NotNull] string sql, [NotNull] Expression arguments, [NotNull] string alias)
            : base(alias)
        {
            Check.NotEmpty(sql, nameof(sql));
            Check.NotNull(arguments, nameof(arguments));

            Sql = sql;
            Arguments = arguments;
        }

        /// <summary>
        ///     Creates a new instance of the <see cref="FromSqlExpression" /> class.
        /// </summary>
        /// <param name="alias"> A string alias for the table source. </param>
        /// <param name="sql"> A user-provided custom SQL for the table source. </param>
        /// <param name="arguments"> A user-provided parameters to pass to the custom SQL. </param>
        public FromSqlExpression([NotNull] string alias, [NotNull] string sql, [NotNull] Expression arguments)
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
        public virtual string Sql { get; }

        /// <summary>
        ///     The user-provided parameters passed to the custom SQL.
        /// </summary>
        public virtual Expression Arguments { get; }

        /// <summary>
        ///     Creates a new expression that is like this one, but using the supplied children. If all of the children are the same, it will
        ///     return this expression.
        /// </summary>
        /// <param name="arguments"> The <see cref="Arguments" /> property of the result. </param>
        /// <returns> This expression if no children changed, or an expression with the updated children. </returns>
        public virtual FromSqlExpression Update([NotNull] Expression arguments)
        {
            Check.NotNull(arguments, nameof(arguments));

            return arguments != Arguments
                ? new FromSqlExpression(Alias, Sql, arguments)
                : this;
        }

        /// <inheritdoc />
        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            Check.NotNull(visitor, nameof(visitor));

            return this;
        }

        /// <inheritdoc />
        protected override void Print(ExpressionPrinter expressionPrinter)
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
                && string.Equals(Sql, fromSqlExpression.Sql)
                && ExpressionEqualityComparer.Instance.Equals(Arguments, fromSqlExpression.Arguments);

        /// <inheritdoc />
        public override int GetHashCode()
            => HashCode.Combine(base.GetHashCode(), Sql);
    }
}
