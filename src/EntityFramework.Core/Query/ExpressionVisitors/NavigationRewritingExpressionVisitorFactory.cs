// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Query.ExpressionVisitors
{
    public class NavigationRewritingExpressionVisitorFactory : INavigationRewritingExpressionVisitorFactory
    {
        public virtual NavigationRewritingExpressionVisitor Create([NotNull] EntityQueryModelVisitor queryModelVisitor)
            => new NavigationRewritingExpressionVisitor(
                Check.NotNull(queryModelVisitor, nameof(queryModelVisitor)));
    }
}
