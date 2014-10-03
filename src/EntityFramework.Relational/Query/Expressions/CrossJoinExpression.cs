// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Relational.Query.Sql;
using Microsoft.Data.Entity.Relational.Utilities;
using Remotion.Linq.Parsing;

namespace Microsoft.Data.Entity.Relational.Query.Expressions
{
    public class CrossJoinExpression : TableExpressionBase
    {
        private readonly TableExpressionBase _tableExpression;

        public CrossJoinExpression([NotNull] TableExpressionBase tableExpression)
            : base(
                Check.NotNull(tableExpression, "tableExpression").QuerySource, 
                tableExpression.Alias)
        {
            _tableExpression = tableExpression;
        }

        public virtual TableExpressionBase TableExpression
        {
            get { return _tableExpression; }
        }

        public override Expression Accept([NotNull] ExpressionTreeVisitor visitor)
        {
            Check.NotNull(visitor, "visitor");

            var specificVisitor = visitor as ISqlExpressionVisitor;

            if (specificVisitor != null)
            {
                return specificVisitor.VisitCrossJoinExpression(this);
            }

            return base.Accept(visitor);
        }

        public override string ToString()
        {
            return "CROSS JOIN " + _tableExpression;
        }
    }
}
