// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.InMemory.Query.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.Pipeline;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.InMemory.Query.Pipeline
{
    public class InMemoryShapedQueryCompilingExpressionVisitor : ShapedQueryCompilingExpressionVisitor
    {
        public InMemoryShapedQueryCompilingExpressionVisitor(
            IEntityMaterializerSource entityMaterializerSource, bool trackQueryResults, bool async)
            : base(entityMaterializerSource, trackQueryResults, async)
        {
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
                        QueryCompilationContext2.QueryContextParameter,
                        Expression.Constant(inMemoryTableExpression.EntityType));
            }

            return base.VisitExtension(extensionExpression);
        }


        protected override Expression VisitShapedQueryExpression(ShapedQueryExpression shapedQueryExpression)
        {
            var shaperBody = InjectEntityMaterializer(shapedQueryExpression.ShaperExpression);

            var innerEnumerable = Visit(shapedQueryExpression.QueryExpression);

            var enumeratorParameter = Expression.Parameter(typeof(IEnumerator<ValueBuffer>), "enumerator");

            var newBody = new InMemoryProjectionBindingRemovingExpressionVisitor(
                (InMemoryQueryExpression)shapedQueryExpression.QueryExpression)
                .Visit(shaperBody);

            newBody = ReplacingExpressionVisitor.Replace(
                InMemoryQueryExpression.ValueBufferParameter,
                Expression.MakeMemberAccess(enumeratorParameter, _enumeratorCurrent),
                newBody);

            var shaperLambda = Expression.Lambda(
                newBody,
                QueryCompilationContext2.QueryContextParameter,
                enumeratorParameter);

            return Expression.Call(
                Async
                    ? _shapeAsyncMethodInfo.MakeGenericMethod(shaperLambda.ReturnType.GetGenericArguments().Single())
                    : _shapeMethodInfo.MakeGenericMethod(shaperLambda.ReturnType),
                innerEnumerable,
                QueryCompilationContext2.QueryContextParameter,
                Expression.Constant(shaperLambda.Compile()));
        }

        private readonly MemberInfo _enumeratorCurrent = typeof(IEnumerator<ValueBuffer>)
            .GetProperty(nameof(IEnumerator<ValueBuffer>.Current));


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

        private static readonly MethodInfo _shapeMethodInfo
            = typeof(InMemoryShapedQueryCompilingExpressionVisitor).GetTypeInfo().GetDeclaredMethod(nameof(_Shape));

        private static IEnumerable<TResult> _Shape<TResult>(
            IEnumerable<ValueBuffer> innerEnumerable,
            QueryContext queryContext,
            Func<QueryContext, IEnumerator<ValueBuffer>, TResult> shaper)
        {
            var enumerator = innerEnumerable.GetEnumerator();
            while (enumerator.MoveNext())
            {
                yield return shaper(queryContext, enumerator);
            }
        }

        private static readonly MethodInfo _shapeAsyncMethodInfo
            = typeof(InMemoryShapedQueryCompilingExpressionVisitor).GetTypeInfo().GetDeclaredMethod(nameof(_ShapeAsync));

        private static IAsyncEnumerable<TResult> _ShapeAsync<TResult>(
            IEnumerable<ValueBuffer> innerEnumerable,
            QueryContext queryContext,
            Func<QueryContext, IEnumerator<ValueBuffer>, Task<TResult>> shaper)
        {
            return new AsyncQueryingEnumerable<TResult>(queryContext, innerEnumerable, shaper);
        }

        private class AsyncQueryingEnumerable<T> : IAsyncEnumerable<T>
        {
            private readonly QueryContext _queryContext;
            private readonly IEnumerable<ValueBuffer> _innerEnumerable;
            private readonly Func<QueryContext, IEnumerator<ValueBuffer>, Task<T>> _shaper;

            public AsyncQueryingEnumerable(
                QueryContext queryContext,
                IEnumerable<ValueBuffer> innerEnumerable,
                Func<QueryContext, IEnumerator<ValueBuffer>, Task<T>> shaper)
            {
                _queryContext = queryContext;
                _innerEnumerable = innerEnumerable;
                _shaper = shaper;
            }

            public IAsyncEnumerator<T> GetEnumerator()
            {
                return new AsyncEnumerator(this);
            }

            private sealed class AsyncEnumerator : IAsyncEnumerator<T>
            {
                private IEnumerator<ValueBuffer> _enumerator;
                private readonly QueryContext _queryContext;
                private readonly IEnumerable<ValueBuffer> _innerEnumerable;
                private readonly Func<QueryContext, IEnumerator<ValueBuffer>, Task<T>> _shaper;

                public AsyncEnumerator(AsyncQueryingEnumerable<T> asyncQueryingEnumerable)
                {
                    _queryContext = asyncQueryingEnumerable._queryContext;
                    _innerEnumerable = asyncQueryingEnumerable._innerEnumerable;
                    _shaper = asyncQueryingEnumerable._shaper;
                }

                public T Current { get; private set; }

                public void Dispose()
                {
                    _enumerator?.Dispose();
                }

                public async Task<bool> MoveNext(CancellationToken cancellationToken)
                {
                    if (_enumerator == null)
                    {
                        _enumerator = _innerEnumerable.GetEnumerator();
                    }

                    var hasNext = _enumerator.MoveNext();

                    Current = hasNext
                        ? await _shaper(_queryContext, _enumerator)
                        : default;

                    return hasNext;
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
                        ? ((EntityProjectionExpression)_queryExpression.GetProjectionExpression(projectionBindingExpression.ProjectionMember)).StartIndex
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
                    return _queryExpression.GetProjectionExpression(projectionBindingExpression.ProjectionMember);
                }

                return base.VisitExtension(extensionExpression);
            }
        }
    }
}
