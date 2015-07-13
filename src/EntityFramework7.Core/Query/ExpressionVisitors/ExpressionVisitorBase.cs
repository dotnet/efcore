// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using JetBrains.Annotations;
using Remotion.Linq.Parsing;

namespace Microsoft.Data.Entity.Query.ExpressionVisitors
{
    public abstract class ExpressionVisitorBase : RelinqExpressionVisitor
    {
        public override Expression Visit([CanBeNull] Expression expression)
            => expression == null
               || expression.NodeType == ExpressionType.Block
                ? expression
                : base.Visit(expression);
    }
}
