// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Query.ExpressionVisitors;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Extensions.Logging;

namespace Microsoft.Data.Entity.Query.Internal
{
    public class QueryCompilationContextFactory : IQueryCompilationContextFactory
    {
        private readonly DbContext _context;

        public QueryCompilationContextFactory(
            [NotNull] ILogger<QueryCompilationContextFactory> logger,
            [NotNull] IEntityQueryModelVisitorFactory entityQueryModelVisitorFactory,
            [NotNull] IRequiresMaterializationExpressionVisitorFactory requiresMaterializationExpressionVisitorFactory,
            [NotNull] DbContext context)
        {
            Check.NotNull(logger, nameof(logger));
            Check.NotNull(entityQueryModelVisitorFactory, nameof(entityQueryModelVisitorFactory));
            Check.NotNull(requiresMaterializationExpressionVisitorFactory, nameof(requiresMaterializationExpressionVisitorFactory));
            Check.NotNull(context, nameof(context));

            Logger = logger;

            EntityQueryModelVisitorFactory = entityQueryModelVisitorFactory;
            RequiresMaterializationExpressionVisitorFactory = requiresMaterializationExpressionVisitorFactory;

            _context = context;
        }

        protected virtual ILogger Logger { get; }
        protected virtual IEntityQueryModelVisitorFactory EntityQueryModelVisitorFactory { get; }
        protected virtual IRequiresMaterializationExpressionVisitorFactory RequiresMaterializationExpressionVisitorFactory { get; }

        protected virtual Type ContextType => _context.GetType();

        protected virtual bool TrackQueryResults 
            => _context.ChangeTracker.QueryTrackingBehavior == QueryTrackingBehavior.TrackAll;

        public virtual QueryCompilationContext Create(bool async)
            => new QueryCompilationContext(
                Logger,
                EntityQueryModelVisitorFactory,
                RequiresMaterializationExpressionVisitorFactory,
                async ? (ILinqOperatorProvider)new AsyncLinqOperatorProvider() : new LinqOperatorProvider(),
                ContextType,
                TrackQueryResults);
    }
}
