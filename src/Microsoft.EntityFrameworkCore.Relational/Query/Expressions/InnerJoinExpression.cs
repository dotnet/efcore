// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.Sql;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query.Expressions
{
    public class InnerJoinExpression : JoinExpressionBase
    {
        public InnerJoinExpression([NotNull] TableExpressionBase tableExpression)
            : base(tableExpression)
        {
        }

        protected override Expression Accept(ExpressionVisitor visitor)
        {
            Check.NotNull(visitor, nameof(visitor));

            var specificVisitor = visitor as ISqlExpressionVisitor;

            return specificVisitor != null
                ? specificVisitor.VisitInnerJoin(this)
                : base.Accept(visitor);
        }

        public override string ToString()
            => "INNER JOIN (" + _tableExpression + ") ON " + Predicate;
    }
}
