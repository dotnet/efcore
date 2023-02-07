// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

/// <summary>
///     <para>
///         Service dependencies parameter class for <see cref="QueryCompilationContext" />
///     </para>
///     <para>
///         This type is typically used by database providers (and other extensions). It is generally
///         not used in application code.
///     </para>
/// </summary>
/// <remarks>
///     <para>
///         Do not construct instances of this class directly from either provider or application code as the
///         constructor signature may change as new dependencies are added. Instead, use this type in
///         your constructor so that an instance will be created and injected automatically by the
///         dependency injection container. To create an instance with some dependent services replaced,
///         first resolve the object from the dependency injection container, then replace selected
///         services using the C# 'with' operator. Do not call the constructor at any point in this process.
///     </para>
///     <para>
///         The service lifetime is <see cref="ServiceLifetime.Scoped" />. This means that each
///         <see cref="DbContext" /> instance will use its own instance of this service.
///         The implementation may depend on other services registered with any lifetime.
///         The implementation does not need to be thread-safe.
///     </para>
/// </remarks>
public sealed record QueryCompilationContextDependencies
{
    private readonly ICurrentDbContext _currentContext;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    /// <remarks>
    ///     Do not call this constructor directly from either provider or application code as it may change
    ///     as new dependencies are added. Instead, use this type in your constructor so that an instance
    ///     will be created and injected automatically by the dependency injection container. To create
    ///     an instance with some dependent services replaced, first resolve the object from the dependency
    ///     injection container, then replace selected services using the C# 'with' operator. Do not call
    ///     the constructor at any point in this process.
    /// </remarks>
    [EntityFrameworkInternal]
    public QueryCompilationContextDependencies(
        IModel model,
        IQueryTranslationPreprocessorFactory queryTranslationPreprocessorFactory,
        IQueryableMethodTranslatingExpressionVisitorFactory queryableMethodTranslatingExpressionVisitorFactory,
        IQueryTranslationPostprocessorFactory queryTranslationPostprocessorFactory,
        IShapedQueryCompilingExpressionVisitorFactory shapedQueryCompilingExpressionVisitorFactory,
        IExecutionStrategy executionStrategy,
        ICurrentDbContext currentContext,
        IDbContextOptions contextOptions,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger,
        IInterceptors interceptors)
    {
        _currentContext = currentContext;
        Model = model;
        QueryTranslationPreprocessorFactory = queryTranslationPreprocessorFactory;
        QueryableMethodTranslatingExpressionVisitorFactory = queryableMethodTranslatingExpressionVisitorFactory;
        QueryTranslationPostprocessorFactory = queryTranslationPostprocessorFactory;
        ShapedQueryCompilingExpressionVisitorFactory = shapedQueryCompilingExpressionVisitorFactory;
        IsRetryingExecutionStrategy = executionStrategy.RetriesOnFailure;
        ContextOptions = contextOptions;
        Logger = logger;
        Interceptors = interceptors;
    }

    /// <summary>
    ///     The current context.
    /// </summary>
    public DbContext Context
        => _currentContext.Context;

    /// <summary>
    ///     The CLR type of DbContext.
    /// </summary>
    public Type ContextType
        => _currentContext.Context.GetType();

    /// <summary>
    ///     The default query tracking behavior.
    /// </summary>
    public QueryTrackingBehavior QueryTrackingBehavior
        => _currentContext.Context.ChangeTracker.QueryTrackingBehavior;

    /// <summary>
    ///     The model.
    /// </summary>
    public IModel Model { get; init; }

    /// <summary>
    ///     The query optimizer factory.
    /// </summary>
    public IQueryTranslationPreprocessorFactory QueryTranslationPreprocessorFactory { get; init; }

    /// <summary>
    ///     The queryable method-translating expression visitor factory.
    /// </summary>
    public IQueryableMethodTranslatingExpressionVisitorFactory QueryableMethodTranslatingExpressionVisitorFactory { get; init; }

    /// <summary>
    ///     The shaped-query optimizer factory
    /// </summary>
    public IQueryTranslationPostprocessorFactory QueryTranslationPostprocessorFactory { get; init; }

    /// <summary>
    ///     The shaped-query compiling expression visitor factory.
    /// </summary>
    public IShapedQueryCompilingExpressionVisitorFactory ShapedQueryCompilingExpressionVisitorFactory { get; init; }

    /// <summary>
    ///     Whether the configured execution strategy can retry.
    /// </summary>
    public bool IsRetryingExecutionStrategy { get; init; }

    /// <summary>
    ///     The context options.
    /// </summary>
    public IDbContextOptions ContextOptions { get; init; }

    /// <summary>
    ///     The logger.
    /// </summary>
    public IDiagnosticsLogger<DbLoggerCategory.Query> Logger { get; init; }

    /// <summary>
    ///     Registered interceptors.
    /// </summary>
    public IInterceptors Interceptors { get; }
}
