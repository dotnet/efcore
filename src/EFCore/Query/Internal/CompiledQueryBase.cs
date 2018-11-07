// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public abstract class CompiledQueryBase<TContext, TResult>
        where TContext : DbContext
    {
        private readonly LambdaExpression _queryExpression;

        private Func<QueryContext, TResult> _executor;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected CompiledQueryBase([NotNull] LambdaExpression queryExpression)
        {
            _queryExpression = queryExpression;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual TResult ExecuteCore(
            [NotNull] TContext context,
            [NotNull] params object[] parameters)
            => ExecuteCore(context, default, parameters);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual TResult ExecuteCore(
            [NotNull] TContext context,
            CancellationToken cancellationToken,
            [NotNull] params object[] parameters)
        {
            var executor = EnsureExecutor(context);
            var queryContextFactory = context.GetService<IQueryContextFactory>();
            var queryContext = queryContextFactory.Create();

            queryContext.CancellationToken = cancellationToken;

            for (var i = 0; i < parameters.Length; i++)
            {
                queryContext.AddParameter(
                    CompiledQueryCache.CompiledQueryParameterPrefix + _queryExpression.Parameters[i + 1].Name,
                    parameters[i]);
            }

            return executor(queryContext);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected abstract Func<QueryContext, TResult> CreateCompiledQuery(
            [NotNull] IQueryCompiler queryCompiler, [NotNull] Expression expression);

        private Func<QueryContext, TResult> EnsureExecutor(TContext context)
            => NonCapturingLazyInitializer.EnsureInitialized(
                ref _executor,
                context,
                _queryExpression,
                (c, q) =>
                {
                    var queryCompiler = context.GetService<IQueryCompiler>();
                    var expression = new QueryExpressionRewriter(c, q.Parameters).Visit(q.Body);

                    return CreateCompiledQuery(queryCompiler, expression);
                });

        private sealed class QueryExpressionRewriter : ExpressionVisitor
        {
            private readonly TContext _context;
            private readonly IReadOnlyCollection<ParameterExpression> _parameters;

            public QueryExpressionRewriter(
                TContext context, IReadOnlyCollection<ParameterExpression> parameters)
            {
                _context = context;
                _parameters = parameters;
            }

            protected override Expression VisitParameter(ParameterExpression parameterExpression)
            {
                if (typeof(TContext).GetTypeInfo().IsAssignableFrom(parameterExpression.Type.GetTypeInfo()))
                {
                    return Expression.Constant(_context);
                }

                return _parameters.Contains(parameterExpression)
                    ? Expression.Parameter(
                        parameterExpression.Type,
                        CompiledQueryCache.CompiledQueryParameterPrefix + parameterExpression.Name)
                    : parameterExpression;
            }
        }
    }
}
