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
using Microsoft.Data.Entity.Query.Expressions;
using Microsoft.Data.Entity.Query.ExpressionVisitors;
using Microsoft.Data.Entity.Query.ResultOperators;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.Caching.Memory;
using Remotion.Linq;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Clauses.StreamedData;
using Remotion.Linq.Parsing.ExpressionVisitors.Transformation;
using Remotion.Linq.Parsing.Structure;
using Remotion.Linq.Parsing.Structure.ExpressionTreeProcessors;
using Remotion.Linq.Parsing.Structure.NodeTypeProviders;

namespace Microsoft.Data.Entity.Query
{
    public class CompiledQueryCache : ICompiledQueryCache
    {
        public const string CompiledQueryParameterPrefix = "__";

        private static readonly object _compiledQueryLockObject = new object();

        private class CompiledQuery
        {
            public Type ResultItemType;
            public Delegate Executor;
        }

        private class ReadonlyNodeTypeProvider : INodeTypeProvider
        {
            private readonly INodeTypeProvider _nodeTypeProvider;

            public ReadonlyNodeTypeProvider(INodeTypeProvider nodeTypeProvider)
            {
                _nodeTypeProvider = nodeTypeProvider;
            }

            public bool IsRegistered(MethodInfo method) => _nodeTypeProvider.IsRegistered(method);

            public Type GetNodeType(MethodInfo method) => _nodeTypeProvider.GetNodeType(method);
        }

        private static readonly Lazy<ReadonlyNodeTypeProvider> _cachedNodeTypeProvider = new Lazy<ReadonlyNodeTypeProvider>(CreateNodeTypeProvider);

        private readonly IMemoryCache _memoryCache;

        public CompiledQueryCache([NotNull] IMemoryCache memoryCache)
        {
            Check.NotNull(memoryCache, nameof(memoryCache));

            _memoryCache = memoryCache;
        }

        public virtual TResult Execute<TResult>(
            Expression query, IDatabase database, QueryContext queryContext)
        {
            Check.NotNull(query, nameof(query));
            Check.NotNull(database, nameof(database));
            Check.NotNull(queryContext, nameof(queryContext));

            var compiledQuery
                = GetOrAdd(query, queryContext, database, isAsync: false, compiler: (q, ds) =>
                    {
                        var queryModel = CreateQueryParser().GetParsedQuery(q);

                        var streamedSequenceInfo
                            = queryModel.GetOutputDataInfo() as StreamedSequenceInfo;

                        var resultItemType
                            = streamedSequenceInfo?.ResultItemType ?? typeof(TResult);

                        var executor
                            = CompileQuery(ds, Database.CompileQueryMethod, resultItemType, queryModel);

                        return new CompiledQuery
                        {
                            ResultItemType = resultItemType,
                            Executor = executor
                        };
                    });

            return
                typeof(TResult) == compiledQuery.ResultItemType
                    ? ((Func<QueryContext, IEnumerable<TResult>>)compiledQuery.Executor)(queryContext).First()
                    : ((Func<QueryContext, TResult>)compiledQuery.Executor)(queryContext);
        }

        public virtual IAsyncEnumerable<TResult> ExecuteAsync<TResult>(
            Expression query, IDatabase database, QueryContext queryContext)
        {
            Check.NotNull(query, nameof(query));
            Check.NotNull(database, nameof(database));
            Check.NotNull(queryContext, nameof(queryContext));

            var compiledQuery
                = GetOrAdd(query, queryContext, database, isAsync: true, compiler: (q, ds) =>
                    {
                        var queryModel = CreateQueryParser().GetParsedQuery(q);

                        var executor
                            = CompileQuery(ds, Database.CompileAsyncQueryMethod, typeof(TResult), queryModel);

                        return new CompiledQuery
                        {
                            ResultItemType = typeof(TResult),
                            Executor = executor
                        };
                    });

            return ((Func<QueryContext, IAsyncEnumerable<TResult>>)compiledQuery.Executor)(queryContext);
        }

        public virtual Task<TResult> ExecuteAsync<TResult>(
            Expression query, IDatabase database, QueryContext queryContext, CancellationToken cancellationToken)
        {
            Check.NotNull(query, nameof(query));
            Check.NotNull(database, nameof(database));
            Check.NotNull(queryContext, nameof(queryContext));

            var compiledQuery
                = GetOrAdd(query, queryContext, database, isAsync: true, compiler: (q, ds) =>
                    {
                        var queryModel = CreateQueryParser().GetParsedQuery(q);

                        var executor
                            = CompileQuery(ds, Database.CompileAsyncQueryMethod, typeof(TResult), queryModel);

                        return new CompiledQuery
                        {
                            ResultItemType = typeof(TResult),
                            Executor = executor
                        };
                    });

            return ((Func<QueryContext, IAsyncEnumerable<TResult>>)compiledQuery.Executor)(queryContext)
                .First(cancellationToken);
        }

        private CompiledQuery GetOrAdd(
            Expression query,
            QueryContext queryContext,
            IDatabase database,
            bool isAsync,
            Func<Expression, IDatabase, CompiledQuery> compiler)
        {
            query = new QueryAnnotatingExpressionVisitor()
                .Visit(query);

            var parameterizedQuery
                = ParameterExtractingExpressionVisitor
                    .ExtractParameters(query, queryContext);

            var cacheKey
                = database.Model.GetHashCode().ToString()
                  + isAsync
                  + new ExpressionStringBuilder()
                      .Build(query);

            CompiledQuery compiledQuery;
            lock (_compiledQueryLockObject)
            {
                if (!_memoryCache.TryGetValue(cacheKey, out compiledQuery))
                {
                    compiledQuery = compiler(parameterizedQuery, database);
                    _memoryCache.Set(cacheKey, compiledQuery);
                }
            }

            return compiledQuery;
        }

        private static Delegate CompileQuery(
            IDatabase database, MethodInfo compileMethodInfo, Type resultItemType, QueryModel queryModel)
        {
            try
            {
                return (Delegate)compileMethodInfo
                    .MakeGenericMethod(resultItemType)
                    .Invoke(database, new object[] { queryModel });
            }
            catch (TargetInvocationException e)
            {
                ExceptionDispatchInfo.Capture(e.InnerException).Throw();

                throw;
            }
        }

        private static QueryParser CreateQueryParser()
            => new QueryParser(
                new ExpressionTreeParser(
                    _cachedNodeTypeProvider.Value,
                    new CompoundExpressionTreeProcessor(new IExpressionTreeProcessor[]
                    {
                        new PartialEvaluatingExpressionTreeProcessor(),
                        new FunctionEvaluationEnablingProcessor(),
                        new TransformingExpressionTreeProcessor(ExpressionTransformerRegistry.CreateDefault())
                    })));

        private class FunctionEvaluationEnablingProcessor : IExpressionTreeProcessor
        {
            public Expression Process(Expression expressionTree)
            {
                return new FunctionEvaluationEnablingVisitor().Visit(expressionTree);
            }
        }

        private class FunctionEvaluationEnablingVisitor : ExpressionVisitorBase
        {
            protected override Expression VisitExtension(Expression expression)
            {
                var methodCallWrapper = expression as MethodCallEvaluationPreventingExpression;
                if (methodCallWrapper != null)
                {
                    return Visit(methodCallWrapper.MethodCall);
                }

                var propertyWrapper = expression as PropertyEvaluationPreventingExpression;
                if (propertyWrapper != null)
                {
                    return Visit(propertyWrapper.MemberExpression);
                }

                return base.VisitExtension(expression);
            }

            protected override Expression VisitSubQuery(SubQueryExpression expression)
            {
                var clonedModel = expression.QueryModel.Clone();
                clonedModel.TransformExpressions(Visit);

                return new SubQueryExpression(clonedModel);
            }
        }

        private static ReadonlyNodeTypeProvider CreateNodeTypeProvider()
        {
            var methodInfoBasedNodeTypeRegistry = MethodInfoBasedNodeTypeRegistry.CreateFromRemotionLinqAssembly();

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
                    MethodNameBasedNodeTypeRegistry.CreateFromRemotionLinqAssembly()
                };

            return new ReadonlyNodeTypeProvider(new CompoundNodeTypeProvider(innerProviders));
        }
    }
}
