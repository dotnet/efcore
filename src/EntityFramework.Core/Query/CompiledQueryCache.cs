// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Query.ResultOperators;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;
using Remotion.Linq;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Clauses.StreamedData;
using Remotion.Linq.Parsing;
using Remotion.Linq.Parsing.ExpressionTreeVisitors.Transformation;
using Remotion.Linq.Parsing.ExpressionTreeVisitors.TreeEvaluation;
using Remotion.Linq.Parsing.Structure;
using Remotion.Linq.Parsing.Structure.ExpressionTreeProcessors;
using Remotion.Linq.Parsing.Structure.NodeTypeProviders;

namespace Microsoft.Data.Entity.Query
{
    public class CompiledQueryCache : ICompiledQueryCache
    {
        public const string CompiledQueryParameterPrefix = "__";

        private class CompiledQuery
        {
            public Expression Query;
            public Type ResultItemType;
            public Delegate Executor;
            public bool IsAsync;
            public IModel Model;
        }

        private readonly ICollection<CompiledQuery> _items = new List<CompiledQuery>();

        public virtual TResult Execute<TResult>(
            Expression query, DataStore dataStore, QueryContext queryContext)
        {
            Check.NotNull(query, "query");
            Check.NotNull(dataStore, "dataStore");
            Check.NotNull(queryContext, "queryContext");

            var compiledQuery
                = GetOrAdd(query, queryContext, dataStore.Model, isAsync: false, compiler: (q, m, a) =>
                    {
                        var queryModel = CreateQueryParser().GetParsedQuery(q);

                        var streamedSequenceInfo
                            = queryModel.GetOutputDataInfo() as StreamedSequenceInfo;

                        var resultItemType
                            = streamedSequenceInfo?.ResultItemType ?? typeof(TResult);

                        var executor
                            = CompileQuery(dataStore, DataStore.CompileQueryMethod, resultItemType, queryModel);

                        return new CompiledQuery
                            {
                                Query = q,
                                ResultItemType = resultItemType,
                                Executor = executor,
                                IsAsync = a,
                                Model = m
                            };
                    });

            return
                typeof(TResult) == compiledQuery.ResultItemType
                    ? ((Func<QueryContext, IEnumerable<TResult>>)compiledQuery.Executor)(queryContext).First()
                    : ((Func<QueryContext, TResult>)compiledQuery.Executor)(queryContext);
        }

        public virtual IAsyncEnumerable<TResult> ExecuteAsync<TResult>(
            Expression query, DataStore dataStore, QueryContext queryContext)
        {
            Check.NotNull(query, "query");
            Check.NotNull(dataStore, "dataStore");
            Check.NotNull(queryContext, "queryContext");

            var compiledQuery
                = GetOrAdd(query, queryContext, dataStore.Model, isAsync: true, compiler: (q, m, a) =>
                    {
                        var queryModel = CreateQueryParser().GetParsedQuery(query);

                        var executor
                            = CompileQuery(dataStore, DataStore.CompileAsyncQueryMethod, typeof(TResult), queryModel);

                        return new CompiledQuery
                            {
                                Query = query,
                                ResultItemType = typeof(TResult),
                                Executor = executor,
                                IsAsync = a,
                                Model = m
                            };
                    });

            return ((Func<QueryContext, IAsyncEnumerable<TResult>>)compiledQuery.Executor)(queryContext);
        }

        public virtual Task<TResult> ExecuteAsync<TResult>(
            Expression query, DataStore dataStore, QueryContext queryContext, CancellationToken cancellationToken)
        {
            Check.NotNull(query, "query");
            Check.NotNull(dataStore, "dataStore");
            Check.NotNull(queryContext, "queryContext");

            var compiledQuery
                = GetOrAdd(query, queryContext, dataStore.Model, isAsync: true, compiler: (q, m, a) =>
                    {
                        var queryModel = CreateQueryParser().GetParsedQuery(query);

                        var executor
                            = CompileQuery(dataStore, DataStore.CompileAsyncQueryMethod, typeof(TResult), queryModel);

                        return new CompiledQuery
                            {
                                Query = query,
                                ResultItemType = typeof(TResult),
                                Executor = executor,
                                IsAsync = a,
                                Model = m
                            };
                    });

            return ((Func<QueryContext, IAsyncEnumerable<TResult>>)compiledQuery.Executor)(queryContext)
                .First(cancellationToken);
        }

        private CompiledQuery GetOrAdd(
            Expression query,
            QueryContext queryContext,
            IModel model,
            bool isAsync,
            Func<Expression, IModel, bool, CompiledQuery> compiler)
        {
            var partiallyEvaluatedQuery
                = PartialEvaluatingExpressionTreeVisitor
                    .EvaluateIndependentSubtrees(query, queryContext);

            lock (_items)
            {
                var compiledQuery
                    = _items.Reverse()
                        .FirstOrDefault(cq =>
                            cq.Model == model
                            && cq.IsAsync == isAsync
                            && ExpressionComparer.AreEqual(cq.Query, partiallyEvaluatedQuery));

                if (compiledQuery == null)
                {
                    _items.Add(compiledQuery = compiler(partiallyEvaluatedQuery, model, isAsync));
                }

                return compiledQuery;
            }
        }

        private class PartialEvaluatingExpressionTreeVisitor : ExpressionTreeVisitor
        {
            public static Expression EvaluateIndependentSubtrees(Expression expressionTree, QueryContext queryContext)
            {
                var partialEvaluationInfo = EvaluatableTreeFindingExpressionTreeVisitor.Analyze(expressionTree);

                var visitor = new PartialEvaluatingExpressionTreeVisitor(partialEvaluationInfo, queryContext);

                return visitor.VisitExpression(expressionTree);
            }

            private readonly PartialEvaluationInfo _partialEvaluationInfo;
            private readonly QueryContext _queryContext;

            private PartialEvaluatingExpressionTreeVisitor(
                PartialEvaluationInfo partialEvaluationInfo, QueryContext queryContext)
            {
                _partialEvaluationInfo = partialEvaluationInfo;
                _queryContext = queryContext;
            }

            protected override Expression VisitMethodCallExpression(MethodCallExpression methodCallExpression)
            {
                if (methodCallExpression.Method.IsGenericMethod
                    && ReferenceEquals(
                        methodCallExpression.Method.GetGenericMethodDefinition(),
                        QueryExtensions.PropertyMethodInfo))
                {
                    return methodCallExpression;
                }

                return base.VisitMethodCallExpression(methodCallExpression);
            }

            public override Expression VisitExpression(Expression expression)
            {
                if (expression == null)
                {
                    return null;
                }

                if (expression.NodeType == ExpressionType.Lambda
                    || !_partialEvaluationInfo.IsEvaluatableExpression(expression))
                {
                    return base.VisitExpression(expression);
                }

                Expression evaluatedExpression;

                try
                {
                    evaluatedExpression = EvaluateSubtree(expression);
                }
                catch (Exception ex)
                {
                    var baseVisitedExpression = base.VisitExpression(expression);

                    return new PartialEvaluationExceptionExpression(ex, baseVisitedExpression);
                }

                return evaluatedExpression != expression
                    ? EvaluateIndependentSubtrees(evaluatedExpression, _queryContext)
                    : evaluatedExpression;
            }

            private Expression EvaluateSubtree(Expression subtree)
            {
                if (subtree.NodeType == ExpressionType.MemberAccess)
                {
                    var memberExpression = (MemberExpression)subtree;

                    if (!typeof(IQueryable).GetTypeInfo()
                        .IsAssignableFrom(memberExpression.Type.GetTypeInfo()))
                    {
                        var parameterExpression
                            = Expression.Parameter(
                                memberExpression.Type,
                                string.Format("{0}{1}_{2}",
                                    CompiledQueryParameterPrefix,
                                    memberExpression.Member.Name,
                                    _queryContext.ParameterValues.Count));

                        var constantExpression = memberExpression.Expression as ConstantExpression;

                        if (constantExpression != null)
                        {
                            var fieldInfo = memberExpression.Member as FieldInfo;

                            if (fieldInfo != null)
                            {
                                _queryContext.ParameterValues
                                    .Add(parameterExpression.Name, fieldInfo.GetValue(constantExpression.Value));

                                return parameterExpression;
                            }

                            var propertyInfo = memberExpression.Member as PropertyInfo;

                            if (propertyInfo != null)
                            {
                                _queryContext.ParameterValues
                                    .Add(parameterExpression.Name, propertyInfo.GetValue(constantExpression.Value));

                                return parameterExpression;
                            }
                        }
                    }
                }

                return subtree;
            }
        }

        private static Delegate CompileQuery(
            DataStore dataStore, MethodInfo compileMethodInfo, Type resultItemType, QueryModel queryModel)
        {
            try
            {
                return (Delegate)compileMethodInfo
                    .MakeGenericMethod(resultItemType)
                    .Invoke(dataStore, new object[] { queryModel });
            }
            catch (TargetInvocationException e)
            {
                throw e.InnerException;
            }
        }

        private static QueryParser CreateQueryParser()
        {
            return new QueryParser(
                new ExpressionTreeParser(
                    CreateNodeTypeProvider(),
                    new CompoundExpressionTreeProcessor(new IExpressionTreeProcessor[]
                        {
                            new PartialEvaluatingExpressionTreeProcessor(),
                            new TransformingExpressionTreeProcessor(ExpressionTransformerRegistry.CreateDefault())
                        })));
        }

        private static CompoundNodeTypeProvider CreateNodeTypeProvider()
        {
            var searchedTypes
                = typeof(MethodInfoBasedNodeTypeRegistry)
                    .GetTypeInfo()
                    .Assembly
                    .DefinedTypes
                    .Select(ti => ti.AsType())
                    .ToList();

            var methodInfoBasedNodeTypeRegistry
                = MethodInfoBasedNodeTypeRegistry.CreateFromTypes(searchedTypes);

            methodInfoBasedNodeTypeRegistry
                .Register(AsNoTrackingExpressionNode.SupportedMethods, typeof(AsNoTrackingExpressionNode));

            methodInfoBasedNodeTypeRegistry
                .Register(IncludeExpressionNode.SupportedMethods, typeof(IncludeExpressionNode));

            methodInfoBasedNodeTypeRegistry
                .Register(ThenIncludeExpressionNode.SupportedMethods, typeof(ThenIncludeExpressionNode));

            var innerProviders
                = new INodeTypeProvider[]
                    {
                        methodInfoBasedNodeTypeRegistry,
                        MethodNameBasedNodeTypeRegistry.CreateFromTypes(searchedTypes)
                    };

            return new CompoundNodeTypeProvider(innerProviders);
        }
    }
}
