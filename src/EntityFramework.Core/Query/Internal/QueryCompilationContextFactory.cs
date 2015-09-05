// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Query.ExpressionVisitors;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.Query.Internal
{
    public class QueryCompilationContextFactory : IQueryCompilationContextFactory
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly IEntityQueryModelVisitorFactory _entityQueryModelVisitorFactory;
        private readonly IRequiresMaterializationExpressionVisitorFactory _requiresMaterializationExpressionVisitorFactory;

        public QueryCompilationContextFactory(
            [NotNull] ILoggerFactory loggerFactory,
            [NotNull] IEntityQueryModelVisitorFactory entityQueryModelVisitorFactory,
            [NotNull] IRequiresMaterializationExpressionVisitorFactory requiresMaterializationExpressionVisitorFactory)
        {
            _loggerFactory = loggerFactory;
            _entityQueryModelVisitorFactory = entityQueryModelVisitorFactory;
            _requiresMaterializationExpressionVisitorFactory = requiresMaterializationExpressionVisitorFactory;
        }

        public virtual QueryCompilationContext Create([NotNull] IDatabase database, bool async)
            => async
                ? new QueryCompilationContext(
                    _loggerFactory,
                    _entityQueryModelVisitorFactory,
                    _requiresMaterializationExpressionVisitorFactory,
                    Check.NotNull(database, nameof(database)),
                    new AsyncLinqOperatorProvider())
                : new QueryCompilationContext(
                    _loggerFactory,
                    _entityQueryModelVisitorFactory,
                    _requiresMaterializationExpressionVisitorFactory,
                    Check.NotNull(database, nameof(database)),
                    new LinqOperatorProvider());
    }
}
