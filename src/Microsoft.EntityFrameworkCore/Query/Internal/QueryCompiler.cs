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

        protected virtual IDatabase Database => _database;

        public virtual TResult Execute<TResult>(Expression query)
        {
            Check.NotNull(query, nameof(query));

            var queryContext = _queryContextFactory.Create();

            query = ExtractParameters(query, queryContext);

            return CompileQuery<TResult>(query)(queryContext);
        }

        public virtual IAsyncEnumerable<TResult> ExecuteAsync<TResult>(Expression query)
        {
            Check.NotNull(query, nameof(query));

            var queryContext = _queryContextFactory.Create();

            query = ExtractParameters(query, queryContext);

            return CompileAsyncQuery<TResult>(query)(queryContext);
        }

        public virtual Task<TResult> ExecuteAsync<TResult>(Expression query, CancellationToken cancellationToken)
        {
            Check.NotNull(query, nameof(query));

            var queryContext = _queryContextFactory.Create();

            queryContext.CancellationToken = cancellationToken;

            query = ExtractParameters(query, queryContext);

            try
            {
                return CompileAsyncQuery<TResult>(query)(queryContext).First(cancellationToken);
            }
            catch (Exception exception)
            {
                _logger
                    .LogError(
                        CoreLoggingEventId.DatabaseError,
                        () => new DatabaseErrorLogState(_contextType),
                        exception,
                        e => CoreStrings.LogExceptionDuringQueryIteration(Environment.NewLine, e));

                throw;
            }
        }

        protected virtual Expression ExtractParameters([NotNull] Expression query, [NotNull] QueryContext queryContext)
        {
            Check.NotNull(query, nameof(query));
            Check.NotNull(queryContext, nameof(queryContext));

            return ParameterExtractingExpressionVisitor
                .ExtractParameters(query, queryContext, _evaluatableExpressionFilter, _logger);
        }

        protected virtual Func<QueryContext, TResult> CompileQuery<TResult>([NotNull] Expression query)
        {
            Check.NotNull(query, nameof(query));

            return _compiledQueryCache
                .GetOrAddQuery(_compiledQueryCacheKeyGenerator.GenerateCacheKey(query, async: false),
                    () =>
                        {
                            var queryModel
                                = CreateQueryParser(NodeTypeProvider)
                                    .GetParsedQuery(query);

                            var resultItemType
                                = (queryModel.GetOutputDataInfo()
                                    as StreamedSequenceInfo)?.ResultItemType
                                  ?? typeof(TResult);

                            if (resultItemType == typeof(TResult))
                            {
                                var compiledQuery = _database.CompileQuery<TResult>(queryModel);

                                return qc =>
                                    {
                                        try
                                        {
                                            return compiledQuery(qc).First();
                                        }
                                        catch (Exception exception)
                                        {
                                            _logger
                                                .LogError(
                                                    CoreLoggingEventId.DatabaseError,
                                                    () => new DatabaseErrorLogState(_contextType),
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
                                    .Invoke(_database, new object[] { queryModel });
                            }
                            catch (TargetInvocationException e)
                            {
                                ExceptionDispatchInfo.Capture(e.InnerException).Throw();

                                throw;
                            }
                        });
        }

        protected virtual Func<QueryContext, IAsyncEnumerable<TResult>> CompileAsyncQuery<TResult>([NotNull] Expression query)
        {
            Check.NotNull(query, nameof(query));

            return _compiledQueryCache
                .GetOrAddAsyncQuery(_compiledQueryCacheKeyGenerator.GenerateCacheKey(query, async: true),
                    () =>
                        {
                            var queryModel
                                = CreateQueryParser(NodeTypeProvider)
                                    .GetParsedQuery(query);

                            return _database.CompileAsyncQuery<TResult>(queryModel);
                        });
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
                if ((methodCallExpression.Method == _guidNewGuid)
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
