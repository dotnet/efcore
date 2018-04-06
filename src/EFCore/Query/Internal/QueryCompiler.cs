// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;
using Remotion.Linq.Clauses.StreamedData;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class QueryCompiler : IQueryCompiler
    {
        private static MethodInfo CompileQueryMethod { get; }
            = typeof(IDatabase).GetTypeInfo()
                .GetDeclaredMethod(nameof(IDatabase.CompileQuery));

        private readonly IQueryContextFactory _queryContextFactory;
        private readonly ICompiledQueryCache _compiledQueryCache;
        private readonly ICompiledQueryCacheKeyGenerator _compiledQueryCacheKeyGenerator;
        private readonly IDatabase _database;
        private readonly IDiagnosticsLogger<DbLoggerCategory.Query> _logger;
        private readonly IQueryModelGenerator _queryModelGenerator;

        private readonly Type _contextType;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public QueryCompiler(
            [NotNull] IQueryContextFactory queryContextFactory,
            [NotNull] ICompiledQueryCache compiledQueryCache,
            [NotNull] ICompiledQueryCacheKeyGenerator compiledQueryCacheKeyGenerator,
            [NotNull] IDatabase database,
            [NotNull] IDiagnosticsLogger<DbLoggerCategory.Query> logger,
            [NotNull] ICurrentDbContext currentContext,
            [NotNull] IQueryModelGenerator queryModelGenerator)
        {
            Check.NotNull(queryContextFactory, nameof(queryContextFactory));
            Check.NotNull(compiledQueryCache, nameof(compiledQueryCache));
            Check.NotNull(compiledQueryCacheKeyGenerator, nameof(compiledQueryCacheKeyGenerator));
            Check.NotNull(database, nameof(database));
            Check.NotNull(logger, nameof(logger));
            Check.NotNull(currentContext, nameof(currentContext));

            _queryContextFactory = queryContextFactory;
            _compiledQueryCache = compiledQueryCache;
            _compiledQueryCacheKeyGenerator = compiledQueryCacheKeyGenerator;
            _database = database;
            _logger = logger;
            _contextType = currentContext.Context.GetType();
            _queryModelGenerator = queryModelGenerator;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual IDatabase Database => _database;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual TResult Execute<TResult>(Expression query)
        {
            Check.NotNull(query, nameof(query));

            var queryContext = _queryContextFactory.Create();

            query = _queryModelGenerator.ExtractParameters(_logger, query, queryContext);

            var compiledQuery
                = _compiledQueryCache
                    .GetOrAddQuery(
                        _compiledQueryCacheKeyGenerator.GenerateCacheKey(query, async: false),
                        () => CompileQueryCore<TResult>(query, _queryModelGenerator, _database, _logger, _contextType));

            return compiledQuery(queryContext);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Func<QueryContext, TResult> CreateCompiledQuery<TResult>(Expression query)
        {
            Check.NotNull(query, nameof(query));

            query = _queryModelGenerator.ExtractParameters(
                _logger, query, _queryContextFactory.Create(),  parameterize: false);

            return CompileQueryCore<TResult>(query, _queryModelGenerator, _database, _logger, _contextType);
        }

        private static Func<QueryContext, TResult> CompileQueryCore<TResult>(
            Expression query,
            IQueryModelGenerator queryModelGenerator,
            IDatabase database,
            IDiagnosticsLogger<DbLoggerCategory.Query> logger,
            Type contextType)
        {
            var queryModel = queryModelGenerator.ParseQuery(query);

            var resultItemType
                = (queryModel.GetOutputDataInfo()
                      as StreamedSequenceInfo)?.ResultItemType
                  ?? typeof(TResult);

            if (resultItemType == typeof(TResult))
            {
                var compiledQuery = database.CompileQuery<TResult>(queryModel);

                return qc =>
                {
                    try
                    {
                        return compiledQuery(qc).First();
                    }
                    catch (Exception exception)
                    {
                        logger.QueryIterationFailed(contextType, exception);

                        throw;
                    }
                };
            }

            try
            {
                return (Func<QueryContext, TResult>)CompileQueryMethod
                    .MakeGenericMethod(resultItemType)
                    .Invoke(database, new object[] { queryModel });
            }
            catch (TargetInvocationException e)
            {
                ExceptionDispatchInfo.Capture(e.InnerException).Throw();

                throw;
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IAsyncEnumerable<TResult> ExecuteAsync<TResult>(Expression query)
        {
            Check.NotNull(query, nameof(query));

            var queryContext = _queryContextFactory.Create();

            query = _queryModelGenerator.ExtractParameters(_logger, query, queryContext);

            return CompileAsyncQuery<TResult>(query)(queryContext);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Func<QueryContext, IAsyncEnumerable<TResult>> CreateCompiledAsyncEnumerableQuery<TResult>(
            Expression query)
        {
            Check.NotNull(query, nameof(query));

            query = _queryModelGenerator.ExtractParameters(
                _logger, query, _queryContextFactory.Create(), parameterize: false);

            return CompileAsyncQueryCore<TResult>(query, _queryModelGenerator, _database);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Func<QueryContext, Task<TResult>> CreateCompiledAsyncTaskQuery<TResult>(Expression query)
        {
            Check.NotNull(query, nameof(query));

            query = _queryModelGenerator.ExtractParameters(
                _logger, query, _queryContextFactory.Create(), parameterize: false);

            var compiledQuery = CompileAsyncQueryCore<TResult>(query, _queryModelGenerator, _database);

            return CreateCompiledSingletonAsyncQuery(compiledQuery, _logger, _contextType);
        }

        private static Func<QueryContext, Task<TResult>> CreateCompiledSingletonAsyncQuery<TResult>(
            Func<QueryContext, IAsyncEnumerable<TResult>> compiledQuery,
            IDiagnosticsLogger<DbLoggerCategory.Query> logger, Type contextType)
            => qc => ExecuteSingletonAsyncQuery(qc, compiledQuery, logger, contextType);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Task<TResult> ExecuteAsync<TResult>(Expression query, CancellationToken cancellationToken)
        {
            Check.NotNull(query, nameof(query));

            var queryContext = _queryContextFactory.Create();

            queryContext.CancellationToken = cancellationToken;

            query = _queryModelGenerator.ExtractParameters(_logger, query, queryContext);

            var compiledQuery = CompileAsyncQuery<TResult>(query);

            return ExecuteSingletonAsyncQuery(queryContext, compiledQuery, _logger, _contextType);
        }

        private static async Task<TResult> ExecuteSingletonAsyncQuery<TResult>(
            QueryContext queryContext,
            Func<QueryContext, IAsyncEnumerable<TResult>> compiledQuery,
            IDiagnosticsLogger<DbLoggerCategory.Query> logger,
            Type contextType)
        {
            try
            {
                var asyncEnumerable = compiledQuery(queryContext);

                using (var asyncEnumerator = asyncEnumerable.GetEnumerator())
                {
                    await asyncEnumerator.MoveNext(queryContext.CancellationToken);

                    return asyncEnumerator.Current;
                }
            }
            catch (Exception exception)
            {
                logger.QueryIterationFailed(contextType, exception);

                throw;
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual Func<QueryContext, IAsyncEnumerable<TResult>> CompileAsyncQuery<TResult>(
            [NotNull] Expression query)
        {
            Check.NotNull(query, nameof(query));

            return _compiledQueryCache
                .GetOrAddAsyncQuery(
                    _compiledQueryCacheKeyGenerator.GenerateCacheKey(query, async: true),
                    () => CompileAsyncQueryCore<TResult>(query, _queryModelGenerator, _database));
        }

        private static Func<QueryContext, IAsyncEnumerable<TResult>> CompileAsyncQueryCore<TResult>(
            Expression query,
            IQueryModelGenerator queryModelGenerator,
            IDatabase database)
        {
            var queryModel = queryModelGenerator.ParseQuery(query);

            return database.CompileAsyncQuery<TResult>(queryModel);
        }
    }
}
