// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Query
{
    /// <summary>
    ///     <para>
    ///         Service dependencies parameter class for <see cref="QueryCompilationContext" />
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    ///     <para>
    ///         Do not construct instances of this class directly from either provider or application code as the
    ///         constructor signature may change as new dependencies are added. Instead, use this type in
    ///         your constructor so that an instance will be created and injected automatically by the
    ///         dependency injection container. To create an instance with some dependent services replaced,
    ///         first resolve the object from the dependency injection container, then replace selected
    ///         services using the 'With...' methods. Do not call the constructor at any point in this process.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Scoped" />. This means that each
    ///         <see cref="DbContext" /> instance will use its own instance of this service.
    ///         The implementation may depend on other services registered with any lifetime.
    ///         The implementation does not need to be thread-safe.
    ///     </para>
    /// </summary>
    public sealed record QueryCompilationContextDependencies
    {
        private readonly ICurrentDbContext _currentContext;

        /// <summary>
        ///     <para>
        ///         Creates the service dependencies parameter object for a <see cref="QueryCompilationContext" />.
        ///     </para>
        ///     <para>
        ///         Do not call this constructor directly from either provider or application code as it may change
        ///         as new dependencies are added. Instead, use this type in your constructor so that an instance
        ///         will be created and injected automatically by the dependency injection container. To create
        ///         an instance with some dependent services replaced, first resolve the object from the dependency
        ///         injection container, then replace selected services using the 'With...' methods. Do not call
        ///         the constructor at any point in this process.
        ///     </para>
        ///     <para>
        ///         This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///         the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///         any release. You should only use it directly in your code with extreme caution and knowing that
        ///         doing so can result in application failures when updating to a new Entity Framework Core release.
        ///     </para>
        /// </summary>
        [EntityFrameworkInternal]
        public QueryCompilationContextDependencies(
            IModel model,
            IQueryTranslationPreprocessorFactory queryTranslationPreprocessorFactory,
            IQueryableMethodTranslatingExpressionVisitorFactory queryableMethodTranslatingExpressionVisitorFactory,
            IQueryTranslationPostprocessorFactory queryTranslationPostprocessorFactory,
            IShapedQueryCompilingExpressionVisitorFactory shapedQueryCompilingExpressionVisitorFactory,
            IExecutionStrategyFactory executionStrategyFactory,
            ICurrentDbContext currentContext,
            IDbContextOptions contextOptions,
            IDiagnosticsLogger<DbLoggerCategory.Query> logger)
        {
            Check.NotNull(model, nameof(model));
            Check.NotNull(queryTranslationPreprocessorFactory, nameof(queryTranslationPreprocessorFactory));
            Check.NotNull(queryableMethodTranslatingExpressionVisitorFactory, nameof(queryableMethodTranslatingExpressionVisitorFactory));
            Check.NotNull(queryTranslationPostprocessorFactory, nameof(queryTranslationPostprocessorFactory));
            Check.NotNull(shapedQueryCompilingExpressionVisitorFactory, nameof(shapedQueryCompilingExpressionVisitorFactory));
            Check.NotNull(executionStrategyFactory, nameof(executionStrategyFactory));
            Check.NotNull(currentContext, nameof(currentContext));
            Check.NotNull(contextOptions, nameof(contextOptions));
            Check.NotNull(logger, nameof(logger));

            _currentContext = currentContext;
            Model = model;
            QueryTranslationPreprocessorFactory = queryTranslationPreprocessorFactory;
            QueryableMethodTranslatingExpressionVisitorFactory = queryableMethodTranslatingExpressionVisitorFactory;
            QueryTranslationPostprocessorFactory = queryTranslationPostprocessorFactory;
            ShapedQueryCompilingExpressionVisitorFactory = shapedQueryCompilingExpressionVisitorFactory;
            IsRetryingExecutionStrategy = executionStrategyFactory.Create().RetriesOnFailure;
            ContextOptions = contextOptions;
            Logger = logger;
        }

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
        public IQueryableMethodTranslatingExpressionVisitorFactory QueryableMethodTranslatingExpressionVisitorFactory
        {
            get;
            init;
        }

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
    }
}
