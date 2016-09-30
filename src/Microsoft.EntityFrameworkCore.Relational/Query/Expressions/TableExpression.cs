// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.Sql;
using Microsoft.EntityFrameworkCore.Utilities;
using Remotion.Linq.Clauses;

namespace Microsoft.EntityFrameworkCore.Query.Expressions
{
    /// <summary>
    ///     Represents a SQL table expression.
    /// </summary>
    public class TableExpression : TableExpressionBase
    {
        /// <summary>
        ///     Initializes a new instance of the Microsoft.EntityFrameworkCore.Query.Expressions.TableExpression class.
        /// </summary>
        /// <param name="table"> The table name. </param>
        /// <param name="schema"> The schema name. </param>
        /// <param name="alias"> The alias. </param>
        /// <param name="querySource"> The query source. </param>
        public TableExpression(
            [NotNull] string table,
            [CanBeNull] string schema,
            [NotNull] string alias,
            [NotNull] IQuerySource querySource)
            : base(
                Check.NotNull(querySource, nameof(querySource)),
                Check.NotEmpty(alias, nameof(alias)))
        {
            Check.NotEmpty(table, nameof(table));

            Table = table;
            Schema = schema;
        }

        /// <summary>
        ///     Gets the table name.
        /// </summary>
        /// <value>
        ///     The table name.
        /// </value>
        public virtual string Table { get; }

        /// <summary>
        ///     Gets the schema name.
        /// </summary>
        /// <value>
        ///     The schema name.
        /// </value>
        public virtual string Schema { get; }

        /// <summary>
        ///     Dispatches to the specific visit method for this node type.
        /// </summary>
        protected override Expression Accept(ExpressionVisitor visitor)
        {
            Check.NotNull(visitor, nameof(visitor));

            var specificVisitor = visitor as ISqlExpressionVisitor;

            return specificVisitor != null
                ? specificVisitor.VisitTable(this)
                : base.Accept(visitor);
        }

        /// <summary>
        ///     Creates a <see cref="string" /> representation of the Expression.
        /// </summary>
        /// <returns>A <see cref="string" /> representation of the Expression.</returns>
        public override string ToString() => Table + " " + Alias;
    }
}
