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
                        _queryMethodInfo,
                        QueryCompilationContext.QueryContextParameter,
                        Expression.Constant(inMemoryTableExpression.EntityType));
            }

            return base.VisitExtension(extensionExpression);
        }


        protected override Expression VisitShapedQueryExpression(ShapedQueryExpression shapedQueryExpression)
        {
            var shaperBody = InjectEntityMaterializer(shapedQueryExpression.ShaperExpression);

            var innerEnumerable = Visit(shapedQueryExpression.QueryExpression);

            var newBody = new InMemoryProjectionBindingRemovingExpressionVisitor(
                (InMemoryQueryExpression)shapedQueryExpression.QueryExpression)
                .Visit(shaperBody);

            var shaperLambda = Expression.Lambda(
                newBody,
                QueryCompilationContext.QueryContextParameter,
                InMemoryQueryExpression.ValueBufferParameter);

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

        private static readonly MethodInfo _queryMethodInfo
            = typeof(InMemoryShapedQueryCompilingExpressionVisitor).GetTypeInfo()
                .GetDeclaredMethod(nameof(Query));

        private static IEnumerable<ValueBuffer> Query(
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
            private readonly IDictionary<ParameterExpression, int> _materializationContextBindings
                = new Dictionary<ParameterExpression, int>();

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

                    var innerExpression = Visit(newExpression.Arguments[0]);

                    var entityStartIndex = ((EntityProjectionExpression)innerExpression).StartIndex;
                    _materializationContextBindings[parameterExpression] = entityStartIndex;

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
                    var originalIndex = (int)((ConstantExpression)methodCallExpression.Arguments[1]).Value;
                    var indexOffset = methodCallExpression.Arguments[0] is ProjectionBindingExpression projectionBindingExpression
                        ? ((EntityProjectionExpression)_queryExpression.GetMappedProjection(projectionBindingExpression.ProjectionMember)).StartIndex
                        : _materializationContextBindings[(ParameterExpression)((MethodCallExpression)methodCallExpression.Arguments[0]).Object];

                    return Expression.Call(
                        methodCallExpression.Method,
                        InMemoryQueryExpression.ValueBufferParameter,
                        Expression.Constant(indexOffset + originalIndex),
                        methodCallExpression.Arguments[2]);
                }

                return base.VisitMethodCall(methodCallExpression);
            }

            protected override Expression VisitExtension(Expression extensionExpression)
            {
                if (extensionExpression is ProjectionBindingExpression projectionBindingExpression)
                {
                    return _queryExpression.GetMappedProjection(projectionBindingExpression.ProjectionMember);
                }

                return base.VisitExtension(extensionExpression);
            }
        }
    }
}
