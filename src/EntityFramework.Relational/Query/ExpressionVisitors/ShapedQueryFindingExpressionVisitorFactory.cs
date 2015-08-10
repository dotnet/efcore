// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Query.ExpressionVisitors
{
    public class ShapedQueryFindingExpressionVisitorFactory : IShapedQueryFindingExpressionVisitorFactory
    {
        public virtual ShapedQueryFindingExpressionVisitor Create(
            [NotNull] RelationalQueryCompilationContext relationalQueryCompilationContext)
            => new ShapedQueryFindingExpressionVisitor(
                Check.NotNull(relationalQueryCompilationContext, nameof(relationalQueryCompilationContext)));
    }
}
