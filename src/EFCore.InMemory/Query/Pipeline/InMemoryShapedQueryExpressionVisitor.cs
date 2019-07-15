// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.InMemory.Query.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Query.Pipeline;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.InMemory.Query.Pipeline
{
    public class InMemoryShapedQueryCompilingExpressionVisitor : ShapedQueryCompilingExpressionVisitor
    {
        private readonly Type _contextType;
        private readonly IDiagnosticsLogger<DbLoggerCategory.Query> _logger;
        private static readonly ConstructorInfo _valueBufferConstructor
            = typeof(ValueBuffer).GetConstructors().Single(ci => ci.GetParameters().Length == 1);

        public InMemoryShapedQueryCompilingExpressionVisitor(
            QueryCompilationContext queryCompilationContext,
            IEntityMaterializerSource entityMaterializerSource)
            : base(queryCompilationContext, entityMaterializerSource)
        {
            _contextType = queryCompilationContext.ContextType;
            _logger = queryCompilationContext.Logger;
        }

        protected override Expression VisitExtension(Expression extensionExpression)
        {
            switch (extensionExpression)
            {
                case InMemoryQueryExpression inMemoryQueryExpression:
                    inMemoryQueryExpression.ApplyServerProjection();
                    return Visit(inMemoryQueryExpression.ServerQueryExpression);

                case InMemoryTableExpression inMemoryTableExpression:
                    return Expression.Call(
                        _tableMethodInfo,
                        QueryCompilationContext.QueryContextParameter,
                        Expression.Constant(inMemoryTableExpression.EntityType));
            }

            return base.VisitExtension(extensionExpression);
        }

        protected override Expression VisitShapedQueryExpression(ShapedQueryExpression shapedQueryExpression)
        {
            var shaperBody = InjectEntityMaterializers(shapedQueryExpression.ShaperExpression);

            var innerEnumerable = Visit(shapedQueryExpression.QueryExpression);

            var inMemoryQueryExpression = (InMemoryQueryExpression)shapedQueryExpression.QueryExpression;

            var newBody = new InMemoryProjectionBindingRemovingExpressionVisitor(inMemoryQueryExpression)
                .Visit(shaperBody);

            var shaperLambda = Expression.Lambda(
                newBody,
                QueryCompilationContext.QueryContextParameter,
                inMemoryQueryExpression.ValueBufferParameter);

            return Expression.New(
                (Async
                    ? typeof(AsyncQueryingEnumerable<>)
                    : typeof(QueryingEnumerable<>)).MakeGenericType(shaperLambda.ReturnType).GetConstructors()[0],
                QueryCompilationContext.QueryContextParameter,
                innerEnumerable,
                Expression.Constant(shaperLambda.Compile()),
                Expression.Constant(_contextType),
                Expression.Constant(_logger));
        }

        private static readonly MethodInfo _tableMethodInfo
            = typeof(InMemoryShapedQueryCompilingExpressionVisitor).GetTypeInfo()
                .GetDeclaredMethod(nameof(Table));

        private static IEnumerable<ValueBuffer> Table(
            QueryContext queryContext,
            IEntityType entityType)
        {
            return ((InMemoryQueryContext)queryContext).Store
                .GetTables(entityType)
                .SelectMany(t => t.Rows.Select(vs => new ValueBuffer(vs)));
        }

        private class QueryingEnumerable<T> : IEnumerable<T>
        {
            private readonly QueryContext _queryContext;
            private readonly IEnumerable<ValueBuffer> _innerEnumerable;
            private readonly Func<QueryContext, ValueBuffer, T> _shaper;
            private readonly Type _contextType;
            private readonly IDiagnosticsLogger<DbLoggerCategory.Query> _logger;

            public QueryingEnumerable(
                QueryContext queryContext,
                IEnumerable<ValueBuffer> innerEnumerable,
                Func<QueryContext, ValueBuffer, T> shaper,
                Type contextType,
                IDiagnosticsLogger<DbLoggerCategory.Query> logger)
            {
                _queryContext = queryContext;
                _innerEnumerable = innerEnumerable;
                _shaper = shaper;
                _contextType = contextType;
                _logger = logger;
            }

            public IEnumerator<T> GetEnumerator() => new Enumerator(this);

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            private sealed class Enumerator : IEnumerator<T>
            {
                private IEnumerator<ValueBuffer> _enumerator;
                private readonly QueryContext _queryContext;
                private readonly IEnumerable<ValueBuffer> _innerEnumerable;
                private readonly Func<QueryContext, ValueBuffer, T> _shaper;
                private readonly Type _contextType;
                private readonly IDiagnosticsLogger<DbLoggerCategory.Query> _logger;

                public Enumerator(QueryingEnumerable<T> queryingEnumerable)
                {
                    _queryContext = queryingEnumerable._queryContext;
                    _innerEnumerable = queryingEnumerable._innerEnumerable;
                    _shaper = queryingEnumerable._shaper;
                    _contextType = queryingEnumerable._contextType;
                    _logger = queryingEnumerable._logger;
                }

                public T Current { get; private set; }

                object IEnumerator.Current => Current;

                public void Dispose()
                {
                    _enumerator?.Dispose();
                    _enumerator = null;
                }

                public bool MoveNext()
                {
                    try
                    {
                        if (_enumerator == null)
                        {
                            _enumerator = _innerEnumerable.GetEnumerator();
                        }

                        var hasNext = _enumerator.MoveNext();

                        Current = hasNext
                            ? _shaper(_queryContext, _enumerator.Current)
                            : default;

                        return hasNext;
                    }
                    catch (Exception exception)
                    {
                        _logger.QueryIterationFailed(_contextType, exception);

                        throw;
                    }
                }

                public void Reset() => throw new NotImplementedException();
            }
        }

        private class AsyncQueryingEnumerable<T> : IAsyncEnumerable<T>
        {
            private readonly QueryContext _queryContext;
            private readonly IEnumerable<ValueBuffer> _innerEnumerable;
            private readonly Func<QueryContext, ValueBuffer, T> _shaper;
            private readonly Type _contextType;
            private readonly IDiagnosticsLogger<DbLoggerCategory.Query> _logger;

            public AsyncQueryingEnumerable(
                QueryContext queryContext,
                IEnumerable<ValueBuffer> innerEnumerable,
                Func<QueryContext, ValueBuffer, T> shaper,
                Type contextType,
                IDiagnosticsLogger<DbLoggerCategory.Query> logger)
            {
                _queryContext = queryContext;
                _innerEnumerable = innerEnumerable;
                _shaper = shaper;
                _contextType = contextType;
                _logger = logger;
            }

            public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
                => new AsyncEnumerator(this, cancellationToken);

            private sealed class AsyncEnumerator : IAsyncEnumerator<T>
            {
                private IEnumerator<ValueBuffer> _enumerator;
                private readonly QueryContext _queryContext;
                private readonly IEnumerable<ValueBuffer> _innerEnumerable;
                private readonly Func<QueryContext, ValueBuffer, T> _shaper;
                private readonly Type _contextType;
                private readonly IDiagnosticsLogger<DbLoggerCategory.Query> _logger;
                private readonly CancellationToken _cancellationToken;

                public AsyncEnumerator(
                    AsyncQueryingEnumerable<T> asyncQueryingEnumerable,
                    CancellationToken cancellationToken)
                {
                    _queryContext = asyncQueryingEnumerable._queryContext;
                    _innerEnumerable = asyncQueryingEnumerable._innerEnumerable;
                    _shaper = asyncQueryingEnumerable._shaper;
                    _contextType = asyncQueryingEnumerable._contextType;
                    _logger = asyncQueryingEnumerable._logger;
                    _cancellationToken = cancellationToken;
                }

                public T Current { get; private set; }

                public ValueTask<bool> MoveNextAsync()
                {
                    try
                    {
                        _cancellationToken.ThrowIfCancellationRequested();

                        if (_enumerator == null)
                        {
                            _enumerator = _innerEnumerable.GetEnumerator();
                        }

                        var hasNext = _enumerator.MoveNext();

                        Current = hasNext
                            ? _shaper(_queryContext, _enumerator.Current)
                            : default;

                        return new ValueTask<bool>(hasNext);
                    }
                    catch (Exception exception)
                    {
                        _logger.QueryIterationFailed(_contextType, exception);

                        throw;
                    }
                }

                public ValueTask DisposeAsync()
                {
                    var enumerator = _enumerator;
                    _enumerator = null;

                    return enumerator.DisposeAsyncIfAvailable();
                }
            }
        }

        private class InMemoryProjectionBindingRemovingExpressionVisitor : ExpressionVisitor
        {
            private readonly InMemoryQueryExpression _queryExpression;
            private readonly IDictionary<ParameterExpression, IDictionary<IProperty, int>> _materializationContextBindings
                = new Dictionary<ParameterExpression, IDictionary<IProperty, int>>();

            public InMemoryProjectionBindingRemovingExpressionVisitor(InMemoryQueryExpression queryExpression)
            {
                _queryExpression = queryExpression;
            }

            protected override Expression VisitBinary(BinaryExpression binaryExpression)
            {
                if (binaryExpression.NodeType == ExpressionType.Assign
                    && binaryExpression.Left is ParameterExpression parameterExpression
                    && parameterExpression.Type == typeof(MaterializationContext))
                {
                    var newExpression = (NewExpression)binaryExpression.Right;

                    var projectionBindingExpression = (ProjectionBindingExpression)newExpression.Arguments[0];

                    _materializationContextBindings[parameterExpression]
                        = (IDictionary<IProperty, int>)GetProjectionIndex(projectionBindingExpression);

                    var updatedExpression = Expression.New(newExpression.Constructor,
                        Expression.Constant(ValueBuffer.Empty),
                        newExpression.Arguments[1]);

                    return Expression.MakeBinary(ExpressionType.Assign, binaryExpression.Left, updatedExpression);
                }

                return base.VisitBinary(binaryExpression);
            }

            protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
            {
                if (methodCallExpression.Method.IsGenericMethod
                    && methodCallExpression.Method.GetGenericMethodDefinition() == EntityMaterializerSource.TryReadValueMethod)
                {
                    var property = (IProperty)((ConstantExpression)methodCallExpression.Arguments[2]).Value;
                    var indexMap = _materializationContextBindings[(ParameterExpression)((MethodCallExpression)methodCallExpression.Arguments[0]).Object];

                    return Expression.Call(
                        methodCallExpression.Method,
                        _queryExpression.ValueBufferParameter,
                        Expression.Constant(indexMap[property]),
                        methodCallExpression.Arguments[2]);
                }

                return base.VisitMethodCall(methodCallExpression);
            }

            protected override Expression VisitExtension(Expression extensionExpression)
            {
                if (extensionExpression is ProjectionBindingExpression projectionBindingExpression)
                {
                    var projectionIndex = (int)GetProjectionIndex(projectionBindingExpression);

                    return Expression.Call(
                        EntityMaterializerSource.TryReadValueMethod.MakeGenericMethod(projectionBindingExpression.Type),
                        _queryExpression.ValueBufferParameter,
                        Expression.Constant(projectionIndex),
                        Expression.Constant(InferPropertyFromInner(_queryExpression.Projection[projectionIndex]), typeof(IPropertyBase)));
                }

                return base.VisitExtension(extensionExpression);
            }

            private IPropertyBase InferPropertyFromInner(Expression expression)
            {
                if (expression is MethodCallExpression methodCallExpression
                    && methodCallExpression.Method.IsGenericMethod
                    && methodCallExpression.Method.GetGenericMethodDefinition() == EntityMaterializerSource.TryReadValueMethod)
                {
                    return (IPropertyBase)((ConstantExpression)methodCallExpression.Arguments[2]).Value;
                }

                return null;
            }

            private object GetProjectionIndex(ProjectionBindingExpression projectionBindingExpression)
            {
                return projectionBindingExpression.ProjectionMember != null
                    ? ((ConstantExpression)_queryExpression.GetMappedProjection(projectionBindingExpression.ProjectionMember)).Value
                    : (projectionBindingExpression.Index != null
                        ? (object)projectionBindingExpression.Index
                        : projectionBindingExpression.IndexMap);
            }
        }
    }
}
