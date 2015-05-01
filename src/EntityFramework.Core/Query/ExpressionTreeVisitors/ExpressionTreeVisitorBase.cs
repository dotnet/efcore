// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using JetBrains.Annotations;
using Remotion.Linq.Parsing;

namespace Microsoft.Data.Entity.Query.ExpressionTreeVisitors
{
    public abstract class ExpressionTreeVisitorBase : ExpressionTreeVisitor
    {
        public override Expression VisitExpression([CanBeNull] Expression expression)
            => expression == null
               || expression.NodeType == ExpressionType.Block
                ? expression
                : base.VisitExpression(expression);
    }
}
