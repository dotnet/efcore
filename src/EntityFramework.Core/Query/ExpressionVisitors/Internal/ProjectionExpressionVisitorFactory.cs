// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using Remotion.Linq.Clauses;

namespace Microsoft.Data.Entity.Query.ExpressionVisitors.Internal
{
    public class ProjectionExpressionVisitorFactory : IProjectionExpressionVisitorFactory
    {
        public virtual ExpressionVisitor Create(
            EntityQueryModelVisitor entityQueryModelVisitor,
            IQuerySource querySource)
            => new ProjectionExpressionVisitor(entityQueryModelVisitor);
    }
}
