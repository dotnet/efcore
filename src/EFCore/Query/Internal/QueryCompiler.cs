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
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal;
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

        private static MethodInfo CompileAsyncQueryMethod { get; }
            = typeof(IDatabase).GetTypeInfo()
                .GetDeclaredMethod(nameof(IDatabase.CompileAsyncQuery));

        private readonly IQueryContextFactory _queryContextFactory;
        private readonly ICompiledQueryCache _compiledQueryCache;
        private readonly ICompiledQueryCacheKeyGenerator _compiledQueryCacheKeyGenerator;
        private readonly IDatabase _database;
        private readonly IDiagnosticsLogger<DbLoggerCategory.Query> _logger;
        private readonly IQueryModelGenerator _queryModelGenerator;

        private readonly Type _contextType;
        private readonly IEvaluatableExpressionFilter _evaluatableExpressionFilter;

        private readonly IModel _model;

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
            [NotNull] IQueryModelGenerator queryModelGenerator,
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
            _queryModelGenerator = queryModelGenerator;
            _evaluatableExpressionFilter = evaluatableExpressionFilter;
            _model = model;
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

            query = ExtractParameters(query, queryContext, _logger);

            var navigationExpandingExpressionVisitor = new NavigationExpandingExpressionVisitor(_model);
            var newQuery = navigationExpandingExpressionVisitor.ExpandNavigations(query);

            var compiledQuery
                = _compiledQueryCache
                    .GetOrAddQuery(
                        _compiledQueryCacheKeyGenerator.GenerateCacheKey(query, async: false),
                        () => CompileQueryCore<TResult>(newQuery, _queryModelGenerator, _database, _logger, _contextType));

            return compiledQuery(queryContext);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Func<QueryContext, TResult> CreateCompiledQuery<TResult>(Expression query)
        {
            Check.NotNull(query, nameof(query));

            query = ExtractParameters(query, _queryContextFactory.Create(), _logger, parameterize: false);

            return CompileQueryCore<TResult>(query, _queryModelGenerator, _database, _logger, _contextType);
        }

        private class TransparentIdentifierRemovingExpressionVisitor : ExpressionVisitorBase
        {
            private static Expression ExtractFromTransparentIdentifier(MemberExpression memberExpression, Stack<string> extractionPath)
            {
                if (memberExpression.Member.Name == "Outer"
                    || memberExpression.Member.Name == "Inner")
                {
                    extractionPath.Push(memberExpression.Member.Name);

                    if (memberExpression.Expression is MemberExpression innerMember)
                    {
                        return ExtractFromTransparentIdentifier(innerMember, extractionPath);
                    }
                    else
                    {
                        var result = memberExpression.Expression;
                        while (extractionPath.Count > 0)
                        {
                            var extractionPathElement = extractionPath.Pop();
                            var newExpression = (NewExpression)result;

                            if (extractionPathElement == "Outer")
                            {
                                result = newExpression.Arguments[0];
                            }
                            else
                            {
                                result = newExpression.Arguments[1];
                            }
                        }

                        return result;
                    }
                }

                return memberExpression;
            }


            protected override Expression VisitMember(MemberExpression memberExpression)
            {
                if (memberExpression.Member.Name == "Outer"
                    || memberExpression.Member.Name == "Inner")
                {
                    return ExtractFromTransparentIdentifier(memberExpression, new Stack<string>());
                }
                else
                {
                    return base.VisitMember(memberExpression);
                }
            }
        }

        private static Func<QueryContext, TResult> CompileQueryCore<TResult>(
            Expression query,
            IQueryModelGenerator queryModelGenerator,
            IDatabase database,
            IDiagnosticsLogger<DbLoggerCategory.Query> logger,
            Type contextType)
        {
            var queryModel = queryModelGenerator.ParseQuery(query);

            // this is temporary, until relinq is removed
            var tirev = new TransparentIdentifierRemovingExpressionVisitor();
            queryModel.TransformExpressions(tirev.Visit);

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
#pragma warning disable RCS1047 // Non-asynchronous method name should not end with 'Async'.
        public virtual TResult ExecuteAsync<TResult>(Expression query)
#pragma warning restore RCS1047 // Non-asynchronous method name should not end with 'Async'.
        {
            Check.NotNull(query, nameof(query));

            var queryContext = _queryContextFactory.Create();

            query = ExtractParameters(query, queryContext, _logger);

            var navigationExpandingExpressionVisitor = new NavigationExpandingExpressionVisitor(_model);
            var newQuery = navigationExpandingExpressionVisitor.ExpandNavigations(query);

            var compiledQuery
                = _compiledQueryCache
                    .GetOrAddAsyncQuery(
                        _compiledQueryCacheKeyGenerator.GenerateCacheKey(query, async: true),
                        () => CompileAsyncQueryCore<TResult>(newQuery, _queryModelGenerator, _database));

            return compiledQuery(queryContext);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Func<QueryContext, TResult> CreateCompiledAsyncQuery<TResult>(Expression query)
        {
            Check.NotNull(query, nameof(query));

            query = ExtractParameters(query, _queryContextFactory.Create(), _logger, parameterize: false);

            return CompileAsyncQueryCore<TResult>(query, _queryModelGenerator, _database);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Task<TResult> ExecuteAsync<TResult>(Expression query, CancellationToken cancellationToken)
        {
            Check.NotNull(query, nameof(query));

            var queryContext = _queryContextFactory.Create();

            queryContext.CancellationToken = cancellationToken;

            query = ExtractParameters(query, queryContext, _logger);

            var compiledQuery
                = _compiledQueryCache
                    .GetOrAddAsyncQuery(
                        _compiledQueryCacheKeyGenerator.GenerateCacheKey(query, async: true),
                        () => CompileAsyncQueryCore<IAsyncEnumerable<TResult>>(query, _queryModelGenerator, _database));

            return ExecuteSingletonAsyncQuery(queryContext, compiledQuery, _logger, _contextType);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Func<QueryContext, Task<TResult>> CreateCompiledAsyncSingletonQuery<TResult>(Expression query)
        {
            Check.NotNull(query, nameof(query));

            query = ExtractParameters(query, _queryContextFactory.Create(), _logger, parameterize: false);

            var compiledQuery = CompileAsyncQueryCore<IAsyncEnumerable<TResult>>(query, _queryModelGenerator, _database);

            return qc => ExecuteSingletonAsyncQuery(qc, compiledQuery, _logger, _contextType);
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

        private static Func<QueryContext, TResult> CompileAsyncQueryCore<TResult>(
            Expression query,
            IQueryModelGenerator queryModelGenerator,
            IDatabase database)
        {
            var queryModel = queryModelGenerator.ParseQuery(query);

            var resultItemType
                = (queryModel.GetOutputDataInfo()
                      as StreamedSequenceInfo)?.ResultItemType
                  ?? typeof(TResult).TryGetSequenceType();

            try
            {
                return (Func<QueryContext, TResult>)CompileAsyncQueryMethod
                   .MakeGenericMethod(resultItemType)
                   .Invoke(database, new object[] { queryModel });
            }
            catch (TargetInvocationException e)
            {
                ExceptionDispatchInfo.Capture(e.InnerException).Throw();

                throw;
            }
        }

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
                logger,
                parameterize,
                generateContextAccessors);

            return visitor.ExtractParameters(query);
        }
    }
}
