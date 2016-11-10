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
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal;
using Microsoft.EntityFrameworkCore.Query.ResultOperators.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.Logging;
using Remotion.Linq.Clauses.StreamedData;
using Remotion.Linq.Parsing.ExpressionVisitors.Transformation;
using Remotion.Linq.Parsing.ExpressionVisitors.TreeEvaluation;
using Remotion.Linq.Parsing.Structure;
using Remotion.Linq.Parsing.Structure.ExpressionTreeProcessors;
using Remotion.Linq.Parsing.Structure.NodeTypeProviders;

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

        private static readonly IEvaluatableExpressionFilter _evaluatableExpressionFilter
            = new EvaluatableExpressionFilter();

        private readonly IQueryContextFactory _queryContextFactory;
        private readonly ICompiledQueryCache _compiledQueryCache;
        private readonly ICompiledQueryCacheKeyGenerator _compiledQueryCacheKeyGenerator;
        private readonly IDatabase _database;
        private readonly ISensitiveDataLogger _logger;
        private readonly MethodInfoBasedNodeTypeRegistry _methodInfoBasedNodeTypeRegistry;
        private readonly Type _contextType;

        private INodeTypeProvider _nodeTypeProvider;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public QueryCompiler(
            [NotNull] IQueryContextFactory queryContextFactory,
            [NotNull] ICompiledQueryCache compiledQueryCache,
            [NotNull] ICompiledQueryCacheKeyGenerator compiledQueryCacheKeyGenerator,
            [NotNull] IDatabase database,
            [NotNull] ISensitiveDataLogger<QueryCompiler> logger,
            [NotNull] MethodInfoBasedNodeTypeRegistry methodInfoBasedNodeTypeRegistry,
            [NotNull] ICurrentDbContext currentContext)
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
            _methodInfoBasedNodeTypeRegistry = methodInfoBasedNodeTypeRegistry;
            _contextType = currentContext.Context.GetType();
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

            query = ExtractParameters(query, queryContext);

            var compiledQuery
                = _compiledQueryCache
                    .GetOrAddQuery(_compiledQueryCacheKeyGenerator.GenerateCacheKey(query, async: false),
                        () => CompileQueryCore<TResult>(query, NodeTypeProvider, _database, _logger, _contextType));

            return compiledQuery(queryContext);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Func<QueryContext, TResult> CreateCompiledQuery<TResult>(Expression query)
        {
            Check.NotNull(query, nameof(query));

            query = ExtractParameters(query, _queryContextFactory.Create(), parameterize: false);

            return CompileQueryCore<TResult>(query, NodeTypeProvider, _database, _logger, _contextType);
        }

        private static Func<QueryContext, TResult> CompileQueryCore<TResult>(
            Expression query, INodeTypeProvider nodeTypeProvider, IDatabase database, ILogger logger, Type contextType)
        {
            var queryModel
                = CreateQueryParser(nodeTypeProvider)
                    .GetParsedQuery(query);

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
                            logger
                                .LogError(
                                    CoreEventId.DatabaseError,
                                    () => new DatabaseErrorLogState(contextType),
                                    exception,
                                    e => CoreStrings.LogExceptionDuringQueryIteration(Environment.NewLine, e));

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

            query = ExtractParameters(query, queryContext);

            return CompileAsyncQuery<TResult>(query)(queryContext);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Func<QueryContext, IAsyncEnumerable<TResult>> CreateCompiledAsyncEnumerableQuery<TResult>(Expression query)
        {
            Check.NotNull(query, nameof(query));

            query = ExtractParameters(query, _queryContextFactory.Create(), parameterize: false);

            return CompileAsyncQueryCore<TResult>(query, NodeTypeProvider, _database);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Func<QueryContext, Task<TResult>> CreateCompiledAsyncTaskQuery<TResult>(Expression query)
        {
            Check.NotNull(query, nameof(query));

            query = ExtractParameters(query, _queryContextFactory.Create(), parameterize: false);

            var compiledQuery = CompileAsyncQueryCore<TResult>(query, NodeTypeProvider, _database);

            return CreateCompiledSingletonAsyncQuery(compiledQuery, _logger, _contextType);
        }

        private static Func<QueryContext, Task<TResult>> CreateCompiledSingletonAsyncQuery<TResult>(
                Func<QueryContext, IAsyncEnumerable<TResult>> compiledQuery, ILogger logger, Type contextType)
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

            query = ExtractParameters(query, queryContext);

            var compiledQuery = CompileAsyncQuery<TResult>(query);

            return ExecuteSingletonAsyncQuery(queryContext, compiledQuery, _logger, _contextType);
        }

        private static async Task<TResult> ExecuteSingletonAsyncQuery<TResult>(
            QueryContext queryContext,
            Func<QueryContext, IAsyncEnumerable<TResult>> compiledQuery,
            ILogger logger,
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
                logger
                    .LogError(
                        CoreEventId.DatabaseError,
                        () => new DatabaseErrorLogState(contextType),
                        exception,
                        e => CoreStrings.LogExceptionDuringQueryIteration(Environment.NewLine, e));

                throw;
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual Func<QueryContext, IAsyncEnumerable<TResult>> CompileAsyncQuery<TResult>([NotNull] Expression query)
        {
            Check.NotNull(query, nameof(query));

            return _compiledQueryCache
                .GetOrAddAsyncQuery(_compiledQueryCacheKeyGenerator.GenerateCacheKey(query, async: true),
                    () => CompileAsyncQueryCore<TResult>(query, NodeTypeProvider, _database));
        }

        private static Func<QueryContext, IAsyncEnumerable<TResult>> CompileAsyncQueryCore<TResult>(
            Expression query, INodeTypeProvider nodeTypeProvider, IDatabase database)
        {
            var queryModel
                = CreateQueryParser(nodeTypeProvider)
                    .GetParsedQuery(query);

            return database.CompileAsyncQuery<TResult>(queryModel);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual Expression ExtractParameters(
            [NotNull] Expression query,
            [NotNull] QueryContext queryContext,
            bool parameterize = true)
        {
            Check.NotNull(query, nameof(query));
            Check.NotNull(queryContext, nameof(queryContext));

            return ParameterExtractingExpressionVisitor
                .ExtractParameters(
                    query,
                    queryContext,
                    _evaluatableExpressionFilter,
                    _logger,
                    parameterize);
        }

        private static QueryParser CreateQueryParser(INodeTypeProvider nodeTypeProvider)
            => new QueryParser(
                new ExpressionTreeParser(
                    nodeTypeProvider,
                    new CompoundExpressionTreeProcessor(new IExpressionTreeProcessor[]
                    {
                        new PartialEvaluatingExpressionTreeProcessor(_evaluatableExpressionFilter),
                        new TransformingExpressionTreeProcessor(ExpressionTransformerRegistry.CreateDefault())
                    })));

        private class EvaluatableExpressionFilter : EvaluatableExpressionFilterBase
        {
            private static readonly PropertyInfo _dateTimeNow
                = typeof(DateTime).GetTypeInfo().GetDeclaredProperty(nameof(DateTime.Now));

            private static readonly PropertyInfo _dateTimeUtcNow
                = typeof(DateTime).GetTypeInfo().GetDeclaredProperty(nameof(DateTime.UtcNow));

            private static readonly MethodInfo _guidNewGuid
                = typeof(Guid).GetTypeInfo().GetDeclaredMethod(nameof(Guid.NewGuid));

            private static readonly List<MethodInfo> _randomNext
                = typeof(Random).GetTypeInfo().GetDeclaredMethods(nameof(Random.Next)).ToList();

            public override bool IsEvaluatableMethodCall(MethodCallExpression methodCallExpression)
            {
                if (methodCallExpression.Method == _guidNewGuid
                    || _randomNext.Contains(methodCallExpression.Method))
                {
                    return false;
                }

                return base.IsEvaluatableMethodCall(methodCallExpression);
            }

            public override bool IsEvaluatableMember(MemberExpression memberExpression)
                => memberExpression.Member != _dateTimeNow && memberExpression.Member != _dateTimeUtcNow;
        }

        private INodeTypeProvider NodeTypeProvider
            => _nodeTypeProvider
               ?? (_nodeTypeProvider
                   = CreateNodeTypeProvider(_methodInfoBasedNodeTypeRegistry));

        private static INodeTypeProvider CreateNodeTypeProvider(
            MethodInfoBasedNodeTypeRegistry methodInfoBasedNodeTypeRegistry)
        {
            methodInfoBasedNodeTypeRegistry
                .Register(TrackingExpressionNode.SupportedMethods, typeof(TrackingExpressionNode));

            methodInfoBasedNodeTypeRegistry
                .Register(IncludeExpressionNode.SupportedMethods, typeof(IncludeExpressionNode));

            methodInfoBasedNodeTypeRegistry
                .Register(StringIncludeExpressionNode.SupportedMethods, typeof(StringIncludeExpressionNode));

            methodInfoBasedNodeTypeRegistry
                .Register(ThenIncludeExpressionNode.SupportedMethods, typeof(ThenIncludeExpressionNode));

            var innerProviders
                = new INodeTypeProvider[]
                {
                    methodInfoBasedNodeTypeRegistry,
                    MethodNameBasedNodeTypeRegistry.CreateFromRelinqAssembly()
                };

            return new CompoundNodeTypeProvider(innerProviders);
        }
    }
}
