// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.Sql;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query.Expressions
{
    /// <summary>
    ///     Represents a SQL SUM expression.
    /// </summary>
    public class SumExpression : AggregateExpression
    {
        /// <summary>
        ///     Initializes a new instance of the Microsoft.EntityFrameworkCore.Query.Expressions.SumExpression class.
        /// </summary>
        /// <param name="expression"> The expression. </param>
        public SumExpression([NotNull] Expression expression)
            : base(Check.NotNull(expression, nameof(expression)))
        {
        }

        /// <summary>
        /// Dispatches to the specific visit method for this node type.
        /// </summary>
        protected override Expression Accept(ExpressionVisitor visitor)
        {
            Check.NotNull(visitor, nameof(visitor));

            var specificVisitor = visitor as ISqlExpressionVisitor;

            return specificVisitor != null
                ? specificVisitor.VisitSum(this)
                : base.Accept(visitor);
        }
    }
}
