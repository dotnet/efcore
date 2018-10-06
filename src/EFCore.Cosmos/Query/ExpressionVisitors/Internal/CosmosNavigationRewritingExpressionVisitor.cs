// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Cosmos.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal;

namespace Microsoft.EntityFrameworkCore.Cosmos.Query.ExpressionVisitors.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class CosmosNavigationRewritingExpressionVisitor : NavigationRewritingExpressionVisitor
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public CosmosNavigationRewritingExpressionVisitor(EntityQueryModelVisitor queryModelVisitor)
            : base(queryModelVisitor)
        {
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public CosmosNavigationRewritingExpressionVisitor(EntityQueryModelVisitor queryModelVisitor, bool navigationExpansionSubquery)
            : base(queryModelVisitor, navigationExpansionSubquery)
        {
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override bool ShouldRewrite(INavigation navigation)
            => navigation.GetTargetType().IsDocumentRoot();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override NavigationRewritingExpressionVisitor CreateVisitorForSubQuery(bool navigationExpansionSubquery)
            => new CosmosNavigationRewritingExpressionVisitor(QueryModelVisitor, navigationExpansionSubquery: true);
    }
}
