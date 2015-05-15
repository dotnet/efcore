// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Relational.Query.Sql;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Relational.Query.Expressions
{
    public class InnerJoinExpression : JoinExpressionBase
    {
        public InnerJoinExpression([NotNull] TableExpressionBase tableExpression)
            : base(tableExpression)
        {
        }

        protected override Expression Accept([NotNull] ExpressionVisitor visitor)
        {
            Check.NotNull(visitor, nameof(visitor));

            var specificVisitor = visitor as ISqlExpressionVisitor;

            return specificVisitor != null
                ? specificVisitor.VisitInnerJoinExpression(this)
                : base.Accept(visitor);
        }

        public override string ToString() 
            => "INNER JOIN (" + _tableExpression + ") ON " + Predicate;
    }
}
