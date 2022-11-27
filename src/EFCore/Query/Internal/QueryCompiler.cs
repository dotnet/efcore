// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;

namespace Microsoft.EntityFrameworkCore.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class QueryCompiler : IQueryCompiler
{
    private readonly IQueryContextFactory _queryContextFactory;
    private readonly ICompiledQueryCache _compiledQueryCache;
    private readonly ICompiledQueryCacheKeyGenerator _compiledQueryCacheKeyGenerator;
    private readonly IDatabase _database;
    private readonly IDiagnosticsLogger<DbLoggerCategory.Query> _logger;

    private readonly DbContext _context;
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
        IQueryContextFactory queryContextFactory,
        ICompiledQueryCache compiledQueryCache,
        ICompiledQueryCacheKeyGenerator compiledQueryCacheKeyGenerator,
        IDatabase database,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger,
        ICurrentDbContext currentContext,
        IEvaluatableExpressionFilter evaluatableExpressionFilter,
        IModel model)
    {
        _queryContextFactory = queryContextFactory;
        _compiledQueryCache = compiledQueryCache;
        _compiledQueryCacheKeyGenerator = compiledQueryCacheKeyGenerator;
        _database = database;
        _logger = logger;
        _context = currentContext.Context;
        _contextType = _context.GetType();
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
        => ExecuteCore<TResult>(query, async: false, CancellationToken.None);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual TResult ExecuteAsync<TResult>(Expression query, CancellationToken cancellationToken = default)
        => ExecuteCore<TResult>(query, async: true, cancellationToken);

    private TResult ExecuteCore<TResult>(Expression query, bool async, CancellationToken cancellationToken)
    {
        var queryContext = _queryContextFactory.Create();

        queryContext.CancellationToken = cancellationToken;

        query = ExtractParameters(query, queryContext, _logger);
        var queryCacheKey = _compiledQueryCacheKeyGenerator.GenerateCacheKey(query, async);

        // TODO: Our EntityFrameworkEventSource event counters need to be updated for all this
        if (!_compiledQueryCache.TryGetQuery<TResult>(queryCacheKey, out var compiledQuery))
        {
            if (PrecompiledQueryFactoryRegistry.ArePrecompiledQueriesEnabled
                && PrecompiledQueryFactoryRegistry.TryGetPrecompiledQueryFactory(_context, queryCacheKey, out var precompiledQueryFactory))
            {
                var nonGenericCompiledQuery = precompiledQueryFactory(_context);
                if (nonGenericCompiledQuery is Func<QueryContext, TResult> compiledQuery2)
                {
                    _compiledQueryCache.AddQuery(queryCacheKey, compiledQuery2);
                    return compiledQuery2(queryContext);
                }
            }

            // A precompiled query wasn't found for the cache key, compile it now except if we're on NativeAOT
            // TODO: Possibly a separate feature switch to disable runtime query compilation; it may be desirable to turn it on even when
            // not doing NativeAOT (but NativeAOT would implicitly turn it on as well)
            if (!RuntimeFeature.IsDynamicCodeSupported)
            {
                throw new InvalidOperationException("Query wasn't precompiled and dynamic code isn't supported (NativeAOT)");
            }

            compiledQuery
                = _compiledQueryCache
                    .GetOrAddQuery(
                        queryCacheKey,
                        () => CompileQueryCore<TResult>(_database, query, _model, async));
        }

        return compiledQuery(queryContext);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
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
        query = ExtractParameters(query, _queryContextFactory.Create(), _logger, parameterize: false);

        return CompileQueryCore<TResult>(_database, query, _model, false);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Func<QueryContext, TResult> CreateCompiledAsyncQuery<TResult>(Expression query)
    {
        query = ExtractParameters(query, _queryContextFactory.Create(), _logger, parameterize: false);

        return CompileQueryCore<TResult>(_database, query, _model, true);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual (Expression, Expression<Func<QueryContext, TResult>>) CompileQueryToExpression<TResult>(Expression query, bool async)
    {
        Check.NotNull(query, nameof(query));

        query = ExtractParameters(query, _queryContextFactory.Create(), _logger);

        return (query, _database.CompileQueryExpression<TResult>(query, async));
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void CachePrecompiledQuery<TResult>(Expression query, Func<QueryContext, TResult> queryExecutor)
    {
        Check.NotNull(query, nameof(query));

        // TODO: Add nicer API to ICompiledQueryCache
        _compiledQueryCache.GetOrAddQuery(
            _compiledQueryCacheKeyGenerator.GenerateCacheKey(query, async: true),
            () => queryExecutor);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Expression ExtractParameters(
        Expression query,
        IParameterValues parameterValues,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger,
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
