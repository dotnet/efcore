// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Cosmos.Sql.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal;

namespace Microsoft.EntityFrameworkCore.Cosmos.Sql.Query.ExpressionVisitors.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class CosmosSqlEagerLoadingExpressionVisitor : EagerLoadingExpressionVisitor
    {
        public CosmosSqlEagerLoadingExpressionVisitor(
            QueryCompilationContext queryCompilationContext,
            IQuerySourceTracingExpressionVisitorFactory querySourceTracingExpressionVisitorFactory)
            : base(queryCompilationContext, querySourceTracingExpressionVisitorFactory)
        {
        }

        public override bool ShouldInclude(INavigation navigation)
            => base.ShouldInclude(navigation)
                && navigation.GetTargetType().IsDocumentRoot();
    }
}
