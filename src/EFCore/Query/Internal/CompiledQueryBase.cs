// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Internal;

namespace Microsoft.EntityFrameworkCore.Query.Internal;

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

    private Func<QueryContext, TResult>? _executor;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected CompiledQueryBase(LambdaExpression queryExpression)
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
        TContext context,
        params object?[] parameters)
        => ExecuteCore(context, default, parameters);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual TResult ExecuteCore(
        TContext context,
        CancellationToken cancellationToken,
        params object?[] parameters)
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
        IQueryCompiler queryCompiler,
        Expression expression);

    private Func<QueryContext, TResult> EnsureExecutor(TContext context)
        => NonCapturingLazyInitializer.EnsureInitialized(
            ref _executor,
            this,
            context,
            _queryExpression,
            static (t, c, q) =>
            {
                var queryCompiler = c.GetService<IQueryCompiler>();
                var expression = new QueryExpressionRewriter(c, q.Parameters).Visit(q.Body);

                return t.CreateCompiledQuery(queryCompiler, expression);
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
