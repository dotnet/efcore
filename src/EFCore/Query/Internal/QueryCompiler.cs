// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using System.Threading;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    /// <summary>
    ///     <para>
    ///         This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///         the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///         any release. You should only use it directly in your code with extreme caution and knowing that
    ///         doing so can result in application failures when updating to a new Entity Framework Core release.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Scoped"/>. This means that each
    ///         <see cref="DbContext"/> instance will use its own instance of this service.
    ///         The implementation may depend on other services registered with any lifetime.
    ///         The implementation does not need to be thread-safe.
    ///     </para>
    /// </summary>
    public class QueryCompiler : IQueryCompiler
    {
        private readonly IQueryContextFactory _queryContextFactory;
        private readonly ICompiledQueryCache _compiledQueryCache;
        private readonly ICompiledQueryCacheKeyGenerator _compiledQueryCacheKeyGenerator;
        private readonly IDatabase _database;
        private readonly IDiagnosticsLogger<DbLoggerCategory.Query> _logger;

        private readonly Type _contextType;
        private readonly IEvaluatableExpressionFilter _evaluatableExpressionFilter;
        private readonly IModel _model;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public QueryCompiler(
            [NotNull] IQueryContextFactory queryContextFactory,
            [NotNull] ICompiledQueryCache compiledQueryCache,
            [NotNull] ICompiledQueryCacheKeyGenerator compiledQueryCacheKeyGenerator,
            [NotNull] IDatabase database,
            [NotNull] IDiagnosticsLogger<DbLoggerCategory.Query> logger,
            [NotNull] ICurrentDbContext currentContext,
            [NotNull] IEvaluatableExpressionFilter evaluatableExpressionFilter,
            [NotNull] IModel model)
        {
            Check.NotNull(queryContextFactory, nameof(queryContextFactory));
            Check.NotNull(compiledQueryCache, nameof(compiledQueryCache));
            Check.NotNull(compiledQueryCacheKeyGenerator, nameof(compiledQueryCacheKeyGenerator));
            Check.NotNull(database, nameof(database));
            Check.NotNull(logger, nameof(logger));
            Check.NotNull(currentContext, nameof(currentContext));
            Check.NotNull(evaluatableExpressionFilter, nameof(evaluatableExpressionFilter));
            Check.NotNull(model, nameof(model));

            _queryContextFactory = queryContextFactory;
            _compiledQueryCache = compiledQueryCache;
            _compiledQueryCacheKeyGenerator = compiledQueryCacheKeyGenerator;
            _database = database;
            _logger = logger;
            _contextType = currentContext.Context.GetType();
            _evaluatableExpressionFilter = evaluatableExpressionFilter;
            _model = model;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual TResult Execute<TResult>(Expression query)
        {
            Check.NotNull(query, nameof(query));

            var queryContext = _queryContextFactory.Create();

            query = ExtractParameters(query, queryContext, _logger);

            var compiledQuery
                = _compiledQueryCache
                    .GetOrAddQuery(
                        _compiledQueryCacheKeyGenerator.GenerateCacheKey(query, async: false),
                        () => CompileQueryCore<TResult>(_database, query, _model, false));

            return compiledQuery(queryContext);
        }

        public virtual Func<QueryContext, TResult> CompileQueryCore<TResult>(
            IDatabase database,
            Expression query,
            IModel model,
            bool async)
            => database.CompileQuery<TResult>(query, async);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Func<QueryContext, TResult> CreateCompiledQuery<TResult>(Expression query)
        {
            Check.NotNull(query, nameof(query));

            query = ExtractParameters(query, _queryContextFactory.Create(), _logger, parameterize: false);

            return CompileQueryCore<TResult>(_database, query, _model, false);
        }

        public virtual TResult ExecuteAsync<TResult>(Expression query, CancellationToken cancellationToken = default)
        {
            Check.NotNull(query, nameof(query));

            var queryContext = _queryContextFactory.Create();

            queryContext.CancellationToken = cancellationToken;

            query = ExtractParameters(query, queryContext, _logger);

            var compiledQuery
                = _compiledQueryCache
                    .GetOrAddQuery(
                        _compiledQueryCacheKeyGenerator.GenerateCacheKey(query, async: true),
                        () => CompileQueryCore<TResult>(_database, query, _model, true));

            return compiledQuery(queryContext);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Func<QueryContext, TResult> CreateCompiledAsyncQuery<TResult>(Expression query)
        {
            Check.NotNull(query, nameof(query));

            query = ExtractParameters(query, _queryContextFactory.Create(), _logger, parameterize: false);

            return CompileQueryCore<TResult>(_database, query, _model, true);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Expression ExtractParameters(
            [NotNull] Expression query,
            [NotNull] IParameterValues parameterValues,
            [NotNull] IDiagnosticsLogger<DbLoggerCategory.Query> logger,
            bool parameterize = true,
            bool generateContextAccessors = false)
        {
            var visitor = new ParameterExtractingExpressionVisitor(
                _evaluatableExpressionFilter,
                parameterValues,
                _contextType,
                _model,
                logger,
                parameterize,
                generateContextAccessors);

            return visitor.ExtractParameters(query);
        }
    }
}
