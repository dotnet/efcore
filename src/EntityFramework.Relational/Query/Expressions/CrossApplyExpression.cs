// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Query.Sql;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Query.Expressions
{
    public class CrossApplyExpression : TableExpressionBase
    {
        private readonly TableExpressionBase _tableExpression;

        public CrossApplyExpression([NotNull] TableExpressionBase tableExpression)
            : base(
                Check.NotNull(tableExpression, nameof(tableExpression)).QuerySource,
                tableExpression.Alias)
        {
            _tableExpression = tableExpression;
        }

        public virtual TableExpressionBase TableExpression => _tableExpression;

        protected override Expression Accept(ExpressionVisitor visitor)
        {
            Check.NotNull(visitor, nameof(visitor));

            var specificVisitor = visitor as ISqlExpressionVisitor;

            return specificVisitor != null
                ? specificVisitor.VisitCrossApply(this)
                : base.Accept(visitor);
        }

        public override string ToString() => "CROSS APPLY " + _tableExpression;


        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            visitor.Visit(_tableExpression);

            return this;
        }
    }
}
