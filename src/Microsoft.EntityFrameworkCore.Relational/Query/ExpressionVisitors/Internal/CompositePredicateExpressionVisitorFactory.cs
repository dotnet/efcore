// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal
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
