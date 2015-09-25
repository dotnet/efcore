// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Query.ExpressionVisitors;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.Query.Internal
{
    public class QueryCompilationContextFactory : IQueryCompilationContextFactory
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly DbContext _context;

        public QueryCompilationContextFactory(
            [NotNull] ILoggerFactory loggerFactory,
            [NotNull] IEntityQueryModelVisitorFactory entityQueryModelVisitorFactory,
            [NotNull] IRequiresMaterializationExpressionVisitorFactory requiresMaterializationExpressionVisitorFactory,
            [NotNull] DbContext context)
        {
            Check.NotNull(loggerFactory, nameof(loggerFactory));
            Check.NotNull(entityQueryModelVisitorFactory, nameof(entityQueryModelVisitorFactory));
            Check.NotNull(requiresMaterializationExpressionVisitorFactory, nameof(requiresMaterializationExpressionVisitorFactory));
            Check.NotNull(context, nameof(context));

            _loggerFactory = loggerFactory;

            EntityQueryModelVisitorFactory = entityQueryModelVisitorFactory;
            RequiresMaterializationExpressionVisitorFactory = requiresMaterializationExpressionVisitorFactory;

            _context = context;
        }

        protected virtual IEntityQueryModelVisitorFactory EntityQueryModelVisitorFactory { get; }
        protected virtual IRequiresMaterializationExpressionVisitorFactory RequiresMaterializationExpressionVisitorFactory { get; }

        protected virtual Type ContextType => _context.GetType();

        public virtual QueryCompilationContext Create(bool async)
            => new QueryCompilationContext(
                CreateLogger(),
                EntityQueryModelVisitorFactory,
                RequiresMaterializationExpressionVisitorFactory,
                async ? (ILinqOperatorProvider)new AsyncLinqOperatorProvider() : new LinqOperatorProvider(),
                ContextType);

        protected virtual ILogger CreateLogger()
            => _loggerFactory.CreateLogger<QueryCompilationContext>();
    }
}
