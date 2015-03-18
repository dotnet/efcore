// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Relational.Query.Sql;
using Microsoft.Data.Entity.Utilities;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Parsing;

namespace Microsoft.Data.Entity.Relational.Query.Expressions
{
    public class LongCountExpression : ExtensionExpression
    {
        public LongCountExpression()
            : base(typeof(long))
        {
        }

        public override Expression Accept([NotNull] ExpressionTreeVisitor visitor)
        {
            Check.NotNull(visitor, nameof(visitor));

            var specificVisitor = visitor as ISqlExpressionVisitor;

            if (specificVisitor != null)
            {
                return specificVisitor.VisitLongCountExpression(this);
            }

            return base.Accept(visitor);
        }

        protected override Expression VisitChildren(ExpressionTreeVisitor visitor)
        {
            return this;
        }
    }
}
