// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Relational.Query.Sql;
using Microsoft.Data.Entity.Utilities;
using Remotion.Linq.Parsing;

namespace Microsoft.Data.Entity.Relational.Query.Expressions
{
    public class LeftOuterJoinExpression : JoinExpressionBase
    {
        public LeftOuterJoinExpression([NotNull] TableExpressionBase tableExpression)
            : base(Check.NotNull(tableExpression, nameof(tableExpression)))
        {
        }

        public override Expression Accept([NotNull] ExpressionTreeVisitor visitor)
        {
            Check.NotNull(visitor, nameof(visitor));

            var specificVisitor = visitor as ISqlExpressionVisitor;

            if (specificVisitor != null)
            {
                return specificVisitor.VisitOuterJoinExpression(this);
            }

            return base.Accept(visitor);
        }

        public override string ToString()
        {
            return "OUTER JOIN (" + _tableExpression + ") ON " + Predicate;
        }
    }
}
