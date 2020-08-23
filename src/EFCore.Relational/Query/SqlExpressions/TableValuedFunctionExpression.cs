// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
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
        /// <param name="storeFunction"> The <see cref="IStoreFunction" /> associated this function. </param>
        /// <param name="arguments"> The arguments of the function. </param>
        public TableValuedFunctionExpression([NotNull] IStoreFunction storeFunction, [NotNull] IReadOnlyList<SqlExpression> arguments)
            : this(
                storeFunction.Name.Substring(0, 1).ToLower(),
                Check.NotNull(storeFunction, nameof(storeFunction)),
                Check.NotNull(arguments, nameof(arguments)))
        {
        }

        private TableValuedFunctionExpression(string alias, IStoreFunction storeFunction, IReadOnlyList<SqlExpression> arguments)
            : base(alias)
        {
            StoreFunction = storeFunction;
            Arguments = arguments;
        }

        /// <summary>
        ///     The store function.
        /// </summary>
        public virtual IStoreFunction StoreFunction { get; }

        /// <summary>
        ///     The list of arguments of this function.
        /// </summary>
        public virtual IReadOnlyList<SqlExpression> Arguments
        {
            get;
        }

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
                ? new TableValuedFunctionExpression(Alias, StoreFunction, arguments)
                : this;
        }

        /// <summary>
        ///     Creates a new expression that is like this one, but using the supplied children. If all of the children are the same, it will
        ///     return this expression.
        /// </summary>
        /// <param name="arguments"> The <see cref="Arguments" /> property of the result. </param>
        /// <returns> This expression if no children changed, or an expression with the updated children. </returns>
        public virtual TableValuedFunctionExpression Update([NotNull] IReadOnlyList<SqlExpression> arguments)
        {
            Check.NotNull(arguments, nameof(arguments));

            return !arguments.SequenceEqual(Arguments)
                ? new TableValuedFunctionExpression(Alias, StoreFunction, arguments)
                : this;
        }

        /// <inheritdoc />
        protected override void Print(ExpressionPrinter expressionPrinter)
        {
            if (!string.IsNullOrEmpty(StoreFunction.Schema))
            {
                expressionPrinter.Append(StoreFunction.Schema).Append(".");
            }

            expressionPrinter.Append(StoreFunction.Name);
            expressionPrinter.Append("(");
            expressionPrinter.VisitCollection(Arguments);
            expressionPrinter.Append(") AS ");
            expressionPrinter.Append(Alias);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
            => obj != null
                && (ReferenceEquals(this, obj)
                    || obj is TableValuedFunctionExpression tableValuedFunctionExpression
                    && Equals(tableValuedFunctionExpression));

        private bool Equals(TableValuedFunctionExpression tableValuedFunctionExpression)
            => base.Equals(tableValuedFunctionExpression)
                && StoreFunction == tableValuedFunctionExpression.StoreFunction
                && Arguments.SequenceEqual(tableValuedFunctionExpression.Arguments);

        /// <inheritdoc />
        public override int GetHashCode()
        {
            var hash = new HashCode();
            hash.Add(base.GetHashCode());
            hash.Add(StoreFunction);
            for (var i = 0; i < Arguments.Count; i++)
            {
                hash.Add(Arguments[i]);
            }

            return hash.ToHashCode();
        }
    }
}
