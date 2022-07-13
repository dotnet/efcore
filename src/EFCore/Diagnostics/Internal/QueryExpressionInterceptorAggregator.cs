// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Diagnostics.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class QueryExpressionInterceptorAggregator : InterceptorAggregator<IQueryExpressionInterceptor>
{
    /// <inheritdoc />
    protected override IQueryExpressionInterceptor CreateChain(IEnumerable<IQueryExpressionInterceptor> interceptors)
        => new CompositeQueryExpressionInterceptor(interceptors);

    private sealed class CompositeQueryExpressionInterceptor : IQueryExpressionInterceptor
    {
        private readonly IQueryExpressionInterceptor[] _interceptors;

        public CompositeQueryExpressionInterceptor(IEnumerable<IQueryExpressionInterceptor> interceptors)
        {
            _interceptors = interceptors.ToArray();
        }

        public Expression ProcessingQuery(
            Expression queryExpression,
            QueryExpressionEventData eventData)
        {
            for (var i = 0; i < _interceptors.Length; i++)
            {
                queryExpression = _interceptors[i].ProcessingQuery(queryExpression, eventData);
            }

            return queryExpression;
        }
            
        public Expression<Func<QueryContext, TResult>> CompilingQuery<TResult>(
            Expression queryExpression,
            Expression<Func<QueryContext, TResult>> queryExecutorExpression,
            QueryExpressionEventData eventData)
        {
            for (var i = 0; i < _interceptors.Length; i++)
            {
                queryExecutorExpression = _interceptors[i].CompilingQuery(queryExpression, queryExecutorExpression, eventData);
            }

            return queryExecutorExpression;
        }

        public Func<QueryContext, TResult> CompiledQuery<TResult>(
            Expression queryExpression,
            QueryExpressionEventData eventData,
            Func<QueryContext, TResult> queryExecutor)
        {
            for (var i = 0; i < _interceptors.Length; i++)
            {
                queryExecutor = _interceptors[i].CompiledQuery(queryExpression, eventData, queryExecutor);
            }

            return queryExecutor;
        }
    }
}
