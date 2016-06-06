// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.Sql;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query.Expressions
{
    /// <summary>
    ///     Represents a SQL MIN aggregate expression.
    /// </summary>
    public class MinExpression : AggregateExpression
    {
        /// <summary>
        ///     Creates a new instance of MinExpression.
        /// </summary>
        /// <param name="expression"> The expression. </param>
        public MinExpression([NotNull] Expression expression)
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
                ? specificVisitor.VisitMin(this)
                : base.Accept(visitor);
        }
    }
}
