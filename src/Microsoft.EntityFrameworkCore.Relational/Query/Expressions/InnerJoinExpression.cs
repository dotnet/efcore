// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.Sql;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query.Expressions
{
    /// <summary>
    ///     Represents a SQL INNER JOIN expression.
    /// </summary>
    public class InnerJoinExpression : JoinExpressionBase
    {
        /// <summary>
        ///     Creates a new instance of InnerJoinExpression.
        /// </summary>
        /// <param name="tableExpression"> The table expression. </param>
        public InnerJoinExpression([NotNull] TableExpressionBase tableExpression)
            : base(tableExpression)
        {
        }

        /// <summary>
        ///     Dispatches to the specific visit method for this node type.
        /// </summary>
        protected override Expression Accept(ExpressionVisitor visitor)
        {
            Check.NotNull(visitor, nameof(visitor));

            var specificVisitor = visitor as ISqlExpressionVisitor;

            return specificVisitor != null
                ? specificVisitor.VisitInnerJoin(this)
                : base.Accept(visitor);
        }

        /// <summary>
        ///     Creates a <see cref="string" /> representation of the Expression.
        /// </summary>
        /// <returns>A <see cref="string" /> representation of the Expression.</returns>
        public override string ToString()
            => "INNER JOIN (" + TableExpression + ") ON " + Predicate;
    }
}
