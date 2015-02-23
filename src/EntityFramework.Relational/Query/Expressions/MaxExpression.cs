// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Relational.Query.Sql;
using Microsoft.Data.Entity.Utilities;
using Remotion.Linq.Parsing;

namespace Microsoft.Data.Entity.Relational.Query.Expressions
{
    public class MaxExpression : ColumnAggregateExpression
    {
        public MaxExpression([NotNull] ColumnExpression columnExpression)
            : base(Check.NotNull(columnExpression, nameof(columnExpression)))
        {
        }

        public override Expression Accept([NotNull] ExpressionTreeVisitor visitor)
        {
            Check.NotNull(visitor, nameof(visitor));

            var specificVisitor = visitor as ISqlExpressionVisitor;

            if (specificVisitor != null)
            {
                return specificVisitor.VisitMaxExpression(this);
            }

            return base.Accept(visitor);
        }
    }
}
