// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
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
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Query.ExpressionTreeVisitors;
using Microsoft.Data.Entity.Query.ResultOperators;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.Cache.Memory;
using Remotion.Linq;
using Remotion.Linq.Clauses.StreamedData;
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
            public Type ResultItemType;
            public Delegate Executor;
        }

        private readonly IMemoryCache _memoryCache;

        public CompiledQueryCache([NotNull] IMemoryCache memoryCache)
        {
            Check.NotNull(memoryCache, nameof(memoryCache));

            _memoryCache = memoryCache;
        }

        public virtual TResult Execute<TResult>(
            Expression query, DataStore dataStore, QueryContext queryContext)
        {
            Check.NotNull(query, nameof(query));
            Check.NotNull(dataStore, nameof(dataStore));
            Check.NotNull(queryContext, nameof(queryContext));

            var compiledQuery
                = GetOrAdd(query, queryContext, dataStore, isAsync: false, compiler: (q, ds) =>
                    {
                        var queryModel = CreateQueryParser().GetParsedQuery(q);

                        var streamedSequenceInfo
                            = queryModel.GetOutputDataInfo() as StreamedSequenceInfo;

                        var resultItemType
                            = streamedSequenceInfo?.ResultItemType ?? typeof(TResult);

                        var executor
                            = CompileQuery(ds, DataStore.CompileQueryMethod, resultItemType, queryModel);

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
            Expression query, DataStore dataStore, QueryContext queryContext)
        {
            Check.NotNull(query, nameof(query));
            Check.NotNull(dataStore, nameof(dataStore));
            Check.NotNull(queryContext, nameof(queryContext));

            var compiledQuery
                = GetOrAdd(query, queryContext, dataStore, isAsync: true, compiler: (q, ds) =>
                    {
                        var queryModel = CreateQueryParser().GetParsedQuery(q);

                        var executor
                            = CompileQuery(ds, DataStore.CompileAsyncQueryMethod, typeof(TResult), queryModel);

                        return new CompiledQuery
                            {
                                ResultItemType = typeof(TResult),
                                Executor = executor
                            };
                    });

            return ((Func<QueryContext, IAsyncEnumerable<TResult>>)compiledQuery.Executor)(queryContext);
        }

        public virtual Task<TResult> ExecuteAsync<TResult>(
            Expression query, DataStore dataStore, QueryContext queryContext, CancellationToken cancellationToken)
        {
            Check.NotNull(query, nameof(query));
            Check.NotNull(dataStore, nameof(dataStore));
            Check.NotNull(queryContext, nameof(queryContext));

            var compiledQuery
                = GetOrAdd(query, queryContext, dataStore, isAsync: true, compiler: (q, ds) =>
                    {
                        var queryModel = CreateQueryParser().GetParsedQuery(q);

                        var executor
                            = CompileQuery(ds, DataStore.CompileAsyncQueryMethod, typeof(TResult), queryModel);

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
            DataStore dataStore,
            bool isAsync,
            Func<Expression, DataStore, CompiledQuery> compiler)
        {
            var parameterizedQuery
                = ParameterExtractingExpressionTreeVisitor
                    .ExtractParameters(query, queryContext);

            var cacheKey
                = dataStore.Model.GetHashCode().ToString()
                  + isAsync
                  + new ExpressionStringBuilder()
                      .Build(query);

            var compiledQuery
                = _memoryCache.GetOrSet(
                    cacheKey,
                    Tuple.Create(parameterizedQuery, dataStore),
                    c =>
                        {
                            var tuple = (Tuple<Expression, DataStore>)c.State;

                            return compiler(tuple.Item1, tuple.Item2);
                        });

            return compiledQuery;
        }

        private class ParameterExtractingExpressionTreeVisitor : ExpressionTreeVisitorBase
        {
            public static Expression ExtractParameters(Expression expressionTree, QueryContext queryContext)
            {
                var partialEvaluationInfo = EvaluatableTreeFindingExpressionTreeVisitor.Analyze(expressionTree);
                var visitor = new ParameterExtractingExpressionTreeVisitor(partialEvaluationInfo, queryContext);

                return visitor.VisitExpression(expressionTree);
            }

            private readonly PartialEvaluationInfo _partialEvaluationInfo;
            private readonly QueryContext _queryContext;

            private ParameterExtractingExpressionTreeVisitor(
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

                var e = expression;

                if (expression.NodeType == ExpressionType.Convert)
                {
                    var unaryExpression = (UnaryExpression)expression;

                    if (unaryExpression.Type.IsNullableType()
                        && !unaryExpression.Operand.Type.IsNullableType())
                    {
                        e = unaryExpression.Operand;
                    }
                }

                if (e.NodeType != ExpressionType.Constant
                    && !typeof(IQueryable).GetTypeInfo().IsAssignableFrom(e.Type.GetTypeInfo()))
                {
                    try
                    {
                        string parameterName;

                        var parameterValue = Evaluate(e, out parameterName);

                        parameterName
                            = string.Format("{0}{1}_{2}",
                                CompiledQueryParameterPrefix,
                                parameterName,
                                _queryContext.ParameterValues.Count);

                        _queryContext.ParameterValues.Add(parameterName, parameterValue);

                        return Expression.Parameter(expression.Type, parameterName);
                    }
                    catch (Exception exception)
                    {
                        throw new InvalidOperationException(
                            Strings.ExpressionParameterizationException(expression),
                            exception);
                    }
                }

                return expression;
            }

            private static object Evaluate(Expression expression, out string parameterName)
            {
                parameterName = null;

                if (expression == null)
                {
                    return null;
                }

                switch (expression.NodeType)
                {
                    case ExpressionType.MemberAccess:
                    {
                        var memberExpression = (MemberExpression)expression;
                        var @object = Evaluate(memberExpression.Expression, out parameterName);

                        var fieldInfo = memberExpression.Member as FieldInfo;

                        if (fieldInfo != null)
                        {
                            parameterName = parameterName != null
                                ? parameterName + "_" + fieldInfo.Name
                                : fieldInfo.Name;

                            try
                            {
                                return fieldInfo.GetValue(@object);
                            }
                            catch
                            {
                                // Try again when we compile the delegate
                            }
                        }

                        var propertyInfo = memberExpression.Member as PropertyInfo;

                        if (propertyInfo != null)
                        {
                            parameterName = parameterName != null
                                ? parameterName + "_" + propertyInfo.Name
                                : propertyInfo.Name;

                            try
                            {
                                return propertyInfo.GetValue(@object);
                            }
                            catch
                            {
                                // Try again when we compile the delegate
                            }
                        }

                        break;
                    }
                    case ExpressionType.Constant:
                    {
                        return ((ConstantExpression)expression).Value;
                    }
                    case ExpressionType.Call:
                    {
                        parameterName = ((MethodCallExpression)expression).Method.Name;

                        break;
                    }
                }

                if (parameterName == null)
                {
                    parameterName = "p";
                }

                return
                    Expression.Lambda<Func<object>>(
                        Expression.Convert(expression, typeof(object)))
                        .Compile()
                        .Invoke();
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
                ExceptionDispatchInfo.Capture(e.InnerException).Throw();

                throw;
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
