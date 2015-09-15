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
using Microsoft.Data.Entity.Query.ExpressionVisitors.Internal;
using Microsoft.Data.Entity.Query.Internal;
using Microsoft.Data.Entity.Query.ResultOperators.Internal;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;
using Remotion.Linq.Clauses.StreamedData;
using Remotion.Linq.Parsing.ExpressionVisitors.Transformation;
using Remotion.Linq.Parsing.ExpressionVisitors.TreeEvaluation;
using Remotion.Linq.Parsing.Structure;
using Remotion.Linq.Parsing.Structure.ExpressionTreeProcessors;
using Remotion.Linq.Parsing.Structure.NodeTypeProviders;

namespace Microsoft.Data.Entity.Query
{
    public class QueryCompiler : IQueryCompiler
    {
        private static MethodInfo CompileQueryMethod { get; }
            = typeof(IDatabase).GetTypeInfo().GetDeclaredMethod(nameof(IDatabase.CompileQuery));

        private static readonly INodeTypeProvider _nodeTypeProvider = CreateNodeTypeProvider();
        private static readonly IEvaluatableExpressionFilter _evaluatableExpressionFilter = new EvaluatableExpressionFilter();

        private readonly IQueryContextFactory _contextFactory;
        private readonly ICompiledQueryCache _cache;
        private readonly ICompiledQueryCacheKeyGenerator _cacheKeyGenerator;
        private readonly IDatabase _database;

        public QueryCompiler(
            [NotNull] IQueryContextFactory contextFactory,
            [NotNull] ICompiledQueryCache cache,
            [NotNull] ICompiledQueryCacheKeyGenerator cacheKeyGenerator,
            [NotNull] IDatabase database)
        {
            Check.NotNull(contextFactory, nameof(contextFactory));
            Check.NotNull(cache, nameof(cache));
            Check.NotNull(cacheKeyGenerator, nameof(cacheKeyGenerator));
            Check.NotNull(database, nameof(database));

            _contextFactory = contextFactory;
            _cache = cache;
            _cacheKeyGenerator = cacheKeyGenerator;
            _database = database;
        }

        protected virtual IDatabase Database => _database;

        public virtual TResult Execute<TResult>(Expression query)
        {
            Check.NotNull(query, nameof(query));

            var queryContext = _contextFactory.Create();

            query = Preprocess(query, queryContext);

            return CompileQuery<TResult>(query)(queryContext);
        }

        public virtual IAsyncEnumerable<TResult> ExecuteAsync<TResult>(Expression query)
        {
            Check.NotNull(query, nameof(query));

            var queryContext = _contextFactory.Create();

            query = Preprocess(query, queryContext);

            return CompileAsyncQuery<TResult>(query)(queryContext);
        }

        public virtual Task<TResult> ExecuteAsync<TResult>(Expression query, CancellationToken cancellationToken)
        {
            Check.NotNull(query, nameof(query));

            var queryContext = _contextFactory.Create();

            queryContext.CancellationToken = cancellationToken;

            query = Preprocess(query, queryContext);

            return CompileAsyncQuery<TResult>(query)(queryContext)
                .First(cancellationToken);
        }

        protected virtual Expression Preprocess([NotNull] Expression query, [NotNull] QueryContext queryContext)
        {
            Check.NotNull(query, nameof(query));
            Check.NotNull(queryContext, nameof(queryContext));

            query = new QueryAnnotatingExpressionVisitor().Visit(query);

            return ParameterExtractingExpressionVisitor
                .ExtractParameters(query, queryContext, _evaluatableExpressionFilter);
        }

        protected virtual Func<QueryContext, TResult> CompileQuery<TResult>([NotNull] Expression query)
        {
            Check.NotNull(query, nameof(query));

            return _cache.GetOrAddQuery(_cacheKeyGenerator.GenerateCacheKey(query, async: false), () =>
                {
                    var queryModel = CreateQueryParser().GetParsedQuery(query);

                    var resultItemType
                        = (queryModel.GetOutputDataInfo() as StreamedSequenceInfo)?.ResultItemType ?? typeof(TResult);

                    if (resultItemType == typeof(TResult))
                    {
                        var compiledQuery = _database.CompileQuery<TResult>(queryModel);

                        return qc => compiledQuery(qc).First();
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

            return _cache.GetOrAddAsyncQuery(_cacheKeyGenerator.GenerateCacheKey(query, async: true), () =>
                {
                    var queryModel = CreateQueryParser().GetParsedQuery(query);

                    return _database.CompileAsyncQuery<TResult>(queryModel);
                });
        }

        private static QueryParser CreateQueryParser()
            => new QueryParser(
                new ExpressionTreeParser(
                    _nodeTypeProvider,
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

            public override bool IsEvaluatableMethodCall(MethodCallExpression methodCallExpression)
                => typeof(IQueryable).IsAssignableFrom(methodCallExpression.Type);

            public override bool IsEvaluatableMember(MemberExpression memberExpression)
                => memberExpression.Member != _dateTimeNow && memberExpression.Member != _dateTimeUtcNow;
        }

        private static INodeTypeProvider CreateNodeTypeProvider()
        {
            var methodInfoBasedNodeTypeRegistry = MethodInfoBasedNodeTypeRegistry.CreateFromRelinqAssembly();

            methodInfoBasedNodeTypeRegistry
                .Register(QueryAnnotationExpressionNode.SupportedMethods, typeof(QueryAnnotationExpressionNode));

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
