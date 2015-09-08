// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Query
{
    public class EntityQueryProvider : IEntityQueryProvider
    {
        private static readonly MethodInfo _genericCreateQueryMethod
            = typeof(EntityQueryProvider).GetRuntimeMethods()
                .Single(m => m.Name == "CreateQuery" && m.IsGenericMethod);

        private readonly DbContext _context;
        private readonly IDatabase _database;
        private readonly ICompiledQueryCache _compiledQueryCache;
        private readonly IQueryContextFactory _queryContextFactory;

        public EntityQueryProvider(
            [NotNull] DbContext context,
            [NotNull] IDatabase database,
            [NotNull] ICompiledQueryCache compiledQueryCache,
            [NotNull] IQueryContextFactory queryContextFactory)
        {
            Check.NotNull(context, nameof(context));
            Check.NotNull(database, nameof(database));
            Check.NotNull(compiledQueryCache, nameof(compiledQueryCache));
            Check.NotNull(queryContextFactory, nameof(queryContextFactory));

            _context = context;
            _database = database;
            _compiledQueryCache = compiledQueryCache;
            _queryContextFactory = queryContextFactory;
        }

        public virtual IQueryable<TElement> CreateQuery<TElement>([NotNull] Expression expression)
        {
            Check.NotNull(expression, nameof(expression));

            return new EntityQueryable<TElement>(this, expression);
        }

        public virtual IQueryable CreateQuery([NotNull] Expression expression)
        {
            Check.NotNull(expression, nameof(expression));

            var sequenceType = expression.Type.GetSequenceType();

            return (IQueryable)_genericCreateQueryMethod
                .MakeGenericMethod(sequenceType)
                .Invoke(this, new object[] { expression });
        }

        public virtual TResult Execute<TResult>([NotNull] Expression expression)
        {
            Check.NotNull(expression, nameof(expression));

            var queryContext = _queryContextFactory.Create();

            queryContext.ContextType = _context.GetType();

            return _compiledQueryCache.Execute<TResult>(expression, _database, queryContext);
        }

        public virtual object Execute([NotNull] Expression expression)
        {
            Check.NotNull(expression, nameof(expression));

            return Execute<object>(expression);
        }

        public virtual IAsyncEnumerable<TResult> ExecuteAsync<TResult>(Expression expression)
        {
            Check.NotNull(expression, nameof(expression));

            var queryContext = _queryContextFactory.Create();

            queryContext.ContextType = _context.GetType();

            return _compiledQueryCache.ExecuteAsync<TResult>(expression, _database, queryContext);
        }

        public virtual Task<TResult> ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken)
        {
            Check.NotNull(expression, nameof(expression));

            var queryContext = _queryContextFactory.Create();

            queryContext.CancellationToken = cancellationToken;
            queryContext.ContextType = _context.GetType();

            return _compiledQueryCache
                .ExecuteAsync<TResult>(expression, _database, queryContext, cancellationToken);
        }
    }
}
