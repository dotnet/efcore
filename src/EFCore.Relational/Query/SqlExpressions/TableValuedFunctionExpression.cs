// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query.SqlExpressions
{
    /// <summary>
    ///     <para>
    ///         An expression that represents a table value function as a table source in a SQL tree.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    public class TableValuedFunctionExpression : TableExpressionBase
    {
        /// <summary>
        ///     Creates a new instance of the <see cref="TableValuedFunctionExpression" /> class.
        /// </summary>
        /// <param name="alias"> A string alias for the table source. </param>
        /// <param name="schema"> The schema in which the function is defined. </param>
        /// <param name="name"> The name of the function. </param>
        /// <param name="arguments"> The arguments of the function. </param>
        public TableValuedFunctionExpression(
            [NotNull] string alias, [CanBeNull] string schema, [NotNull] string name, [NotNull] IReadOnlyList<SqlExpression> arguments)
            : base(alias)
        {
            Check.NullButNotEmpty(schema, nameof(schema));
            Check.NotEmpty(name, nameof(name));
            Check.NotNull(arguments, nameof(arguments));

            Schema = schema;
            Name = name;
            Arguments = arguments;
        }

        /// <summary>
        ///     The name of the function.
        /// </summary>
        public virtual string Name { get; }
        /// <summary>
        ///     The schema in which the function is defined, if any.
        /// </summary>
        public virtual string Schema { get; }
        /// <summary>
        ///     The list of arguments of this function.
        /// </summary>
        public virtual IReadOnlyList<SqlExpression> Arguments { get; }

        /// <inheritdoc />
        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            Check.NotNull(visitor, nameof(visitor));

            var changed = false;
            var arguments = new SqlExpression[Arguments.Count];
            for (var i = 0; i < arguments.Length; i++)
            {
                arguments[i] = (SqlExpression)visitor.Visit(Arguments[i]);
                changed |= arguments[i] != Arguments[i];
            }

            return changed
                ? new TableValuedFunctionExpression(Alias,Schema, Name, arguments)
                : this;
        }

        /// <summary>
        ///     Creates a new expression that is like this one, but using the supplied children. If all of the children are the same, it will
        ///     return this expression.
        /// </summary>
        /// <param name="arguments"> The <see cref="Arguments"/> property of the result. </param>
        /// <returns> This expression if no children changed, or an expression with the updated children. </returns>
        public virtual TableValuedFunctionExpression Update([NotNull] IReadOnlyList<SqlExpression> arguments)
        {
            Check.NotNull(arguments, nameof(arguments));

            return !arguments.SequenceEqual(Arguments)
                ? new TableValuedFunctionExpression(Alias, Schema, Name, arguments)
                : this;
        }

        /// <inheritdoc />
        protected override void Print(ExpressionPrinter expressionPrinter)
        {
            if (!string.IsNullOrEmpty(Schema))
            {
                expressionPrinter.Append(Schema).Append(".");
            }

            expressionPrinter.Append(Name);
            expressionPrinter.Append("(");
            expressionPrinter.VisitCollection(Arguments);
            expressionPrinter.Append(") AS ");
            expressionPrinter.Append(Alias);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
            => obj != null
                && (ReferenceEquals(this, obj)
                    || obj is TableValuedFunctionExpression queryableExpression
                    && Equals(queryableExpression));

        private bool Equals(TableValuedFunctionExpression queryableExpression)
            => base.Equals(queryableExpression)
                && string.Equals(Name, queryableExpression.Name)
                && string.Equals(Schema, queryableExpression.Schema)
                && Arguments.SequenceEqual(queryableExpression.Arguments);

        /// <inheritdoc />
        public override int GetHashCode()
        {
            var hash = new HashCode();
            hash.Add(base.GetHashCode());
            hash.Add(Schema);
            hash.Add(Name);
            for (var i = 0; i < Arguments.Count; i++)
            {
                hash.Add(Arguments[i]);
            }

            return hash.ToHashCode();
        }
    }
}
