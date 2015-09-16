// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Query.ExpressionVisitors
{
    public class CompositePredicateExpressionVisitorFactory : ICompositePredicateExpressionVisitorFactory
    {
        private readonly IDbContextOptions _contextOptions;

        public CompositePredicateExpressionVisitorFactory([NotNull] IDbContextOptions contextOptions)
        {
            Check.NotNull(contextOptions, nameof(contextOptions));

            _contextOptions = contextOptions;
        }

        public virtual ExpressionVisitor Create()
            => new CompositePredicateExpressionVisitor(
                RelationalOptionsExtension.Extract(_contextOptions).UseRelationalNulls);
    }
}
