// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Remotion.Linq.Parsing;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionVisitors
{
    [DebuggerStepThrough]
    public abstract class ExpressionVisitorBase : RelinqExpressionVisitor
    {
        public override Expression Visit([CanBeNull] Expression node)
            => node == null
               || node.NodeType == ExpressionType.Block
                ? node
                : base.Visit(node);
    }
}
