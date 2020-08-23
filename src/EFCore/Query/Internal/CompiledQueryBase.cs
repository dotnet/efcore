// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public abstract class CompiledQueryBase<TContext, TResult>
        where TContext : DbContext
    {
        private readonly LambdaExpression _queryExpression;

        private Func<QueryContext, TResult> _executor;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected CompiledQueryBase([NotNull] LambdaExpression queryExpression)
        {
            _queryExpression = queryExpression;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected virtual TResult ExecuteCore(
            [NotNull] TContext context,
            [NotNull] params object[] parameters)
            => ExecuteCore(context, default, parameters);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
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
                    QueryCompilationContext.QueryParameterPrefix + _queryExpression.Parameters[i + 1].Name,
                    parameters[i]);
            }

            return executor(queryContext);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected abstract Func<QueryContext, TResult> CreateCompiledQuery(
            [NotNull] IQueryCompiler queryCompiler,
            [NotNull] Expression expression);

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
                TContext context,
                IReadOnlyCollection<ParameterExpression> parameters)
            {
                _context = context;
                _parameters = parameters;
            }

            protected override Expression VisitParameter(ParameterExpression parameterExpression)
            {
                Check.NotNull(parameterExpression, nameof(parameterExpression));

                if (typeof(TContext).IsAssignableFrom(parameterExpression.Type))
                {
                    return Expression.Constant(_context);
                }

                return _parameters.Contains(parameterExpression)
                    ? Expression.Parameter(
                        parameterExpression.Type,
                        QueryCompilationContext.QueryParameterPrefix + parameterExpression.Name)
                    : parameterExpression;
            }
        }
    }
}
