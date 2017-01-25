// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class NavigationRewritingQueryModelVisitorFactory : INavigationRewritingQueryModelVisitorFactory
    {
        private Lazy<IAsyncQueryProvider> _entityQueryProvider;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public NavigationRewritingQueryModelVisitorFactory([NotNull] IServiceProvider serviceProvider)
        {
            Check.NotNull(serviceProvider, nameof(serviceProvider));

            _entityQueryProvider = new Lazy<IAsyncQueryProvider>(() => 
                serviceProvider.GetRequiredService<IAsyncQueryProvider>());
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual NavigationRewritingQueryModelVisitor Create(EntityQueryModelVisitor queryModelVisitor)
        { 
            return new NavigationRewritingQueryModelVisitor(queryModelVisitor, _entityQueryProvider.Value);
        }
    }
}
