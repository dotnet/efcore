// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Cosmos.Query.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class FromSqlExpression : RootReferenceExpression, ICloneable, IPrintableExpression
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public FromSqlExpression(IEntityType entityType, string alias, string sql, Expression arguments) : base(entityType, alias)
        {
            Check.NotEmpty(sql, nameof(sql));
            Check.NotNull(arguments, nameof(arguments));

            Sql = sql;
            Arguments = arguments;
        }

        /// <summary>
        ///     The alias assigned to this table source.
        /// </summary>
        [NotNull]
        public override string? Alias => base.Alias!;

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
        public virtual FromSqlExpression Update(Expression arguments)
        {
            Check.NotNull(arguments, nameof(arguments));

            return arguments != Arguments
                ? new FromSqlExpression(EntityType, Alias, Sql, arguments)
                : this;
        }

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
        public virtual object Clone() => new FromSqlExpression(EntityType, Alias, Sql, Arguments);

        /// <inheritdoc />
        void IPrintableExpression.Print(ExpressionPrinter expressionPrinter)
        {
            Check.NotNull(expressionPrinter, nameof(expressionPrinter));

            expressionPrinter.Append(Sql);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
            => obj != null
                && (ReferenceEquals(this, obj)
                    || obj is FromSqlExpression fromSqlExpression
                    && Equals(fromSqlExpression));

        private bool Equals(FromSqlExpression fromSqlExpression)
            => base.Equals(fromSqlExpression)
                && Sql == fromSqlExpression.Sql
                && ExpressionEqualityComparer.Instance.Equals(Arguments, fromSqlExpression.Arguments);

        /// <inheritdoc />
        public override int GetHashCode()
            => HashCode.Combine(base.GetHashCode(), Sql);
    }
}
