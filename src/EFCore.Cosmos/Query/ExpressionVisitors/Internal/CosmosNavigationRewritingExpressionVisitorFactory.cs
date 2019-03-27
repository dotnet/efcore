// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal;

namespace Microsoft.EntityFrameworkCore.Cosmos.Query.ExpressionVisitors.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class CosmosNavigationRewritingExpressionVisitorFactory : INavigationRewritingExpressionVisitorFactory
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public NavigationRewritingExpressionVisitor Create(EntityQueryModelVisitor queryModelVisitor)
            => new CosmosNavigationRewritingExpressionVisitor(queryModelVisitor, navigationExpansionSubquery: false);
    }
}
