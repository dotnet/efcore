// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using Remotion.Linq.Clauses;

namespace Microsoft.Data.Entity.Query.ExpressionVisitors.Internal
{
    public class MemberAccessBindingExpressionVisitorFactory : IMemberAccessBindingExpressionVisitorFactory
    {
        public virtual ExpressionVisitor Create(
            QuerySourceMapping querySourceMapping,
            EntityQueryModelVisitor queryModelVisitor,
            bool inProjection)
            => new MemberAccessBindingExpressionVisitor(querySourceMapping, queryModelVisitor, inProjection);
    }
}
