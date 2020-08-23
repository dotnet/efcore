// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
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
    public sealed class QueryCompilationContextDependencies
    {
        private readonly IExecutionStrategyFactory _executionStrategyFactory;
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
            [NotNull] IModel model,
            [NotNull] IQueryTranslationPreprocessorFactory queryTranslationPreprocessorFactory,
            [NotNull] IQueryableMethodTranslatingExpressionVisitorFactory queryableMethodTranslatingExpressionVisitorFactory,
            [NotNull] IQueryTranslationPostprocessorFactory queryTranslationPostprocessorFactory,
            [NotNull] IShapedQueryCompilingExpressionVisitorFactory shapedQueryCompilingExpressionVisitorFactory,
            [NotNull] IExecutionStrategyFactory executionStrategyFactory,
            [NotNull] ICurrentDbContext currentContext,
            [NotNull] IDbContextOptions contextOptions,
            [NotNull] IDiagnosticsLogger<DbLoggerCategory.Query> logger)
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
            _executionStrategyFactory = executionStrategyFactory;
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
        public IModel Model { get; }

        /// <summary>
        ///     The query optimizer factory.
        /// </summary>
        public IQueryTranslationPreprocessorFactory QueryTranslationPreprocessorFactory { get; }

        /// <summary>
        ///     The queryable method-translating expression visitor factory.
        /// </summary>
        public IQueryableMethodTranslatingExpressionVisitorFactory QueryableMethodTranslatingExpressionVisitorFactory { get; }

        /// <summary>
        ///     The shaped-query optimizer factory
        /// </summary>
        public IQueryTranslationPostprocessorFactory QueryTranslationPostprocessorFactory { get; }

        /// <summary>
        ///     The shaped-query compiling expression visitor factory.
        /// </summary>
        public IShapedQueryCompilingExpressionVisitorFactory ShapedQueryCompilingExpressionVisitorFactory { get; }

        /// <summary>
        ///     Whether the configured execution strategy can retry.
        /// </summary>
        public bool IsRetryingExecutionStrategy { get; }

        /// <summary>
        ///     The context options.
        /// </summary>
        public IDbContextOptions ContextOptions { get; }

        /// <summary>
        ///     The logger.
        /// </summary>
        public IDiagnosticsLogger<DbLoggerCategory.Query> Logger { get; }

        /// <summary>
        ///     Clones this dependency parameter object with one service replaced.
        /// </summary>
        /// <param name="model"> A replacement for the current dependency of this type. </param>
        /// <returns> A new parameter object with the given service replaced. </returns>
        public QueryCompilationContextDependencies With([NotNull] IModel model)
            => new QueryCompilationContextDependencies(
                model,
                QueryTranslationPreprocessorFactory,
                QueryableMethodTranslatingExpressionVisitorFactory,
                QueryTranslationPostprocessorFactory,
                ShapedQueryCompilingExpressionVisitorFactory,
                _executionStrategyFactory,
                _currentContext,
                ContextOptions,
                Logger);

        /// <summary>
        ///     Clones this dependency parameter object with one service replaced.
        /// </summary>
        /// <param name="queryTranslationPreprocessorFactory"> A replacement for the current dependency of this type. </param>
        /// <returns> A new parameter object with the given service replaced. </returns>
        public QueryCompilationContextDependencies With([NotNull] IQueryTranslationPreprocessorFactory queryTranslationPreprocessorFactory)
            => new QueryCompilationContextDependencies(
                Model,
                queryTranslationPreprocessorFactory,
                QueryableMethodTranslatingExpressionVisitorFactory,
                QueryTranslationPostprocessorFactory,
                ShapedQueryCompilingExpressionVisitorFactory,
                _executionStrategyFactory,
                _currentContext,
                ContextOptions,
                Logger);

        /// <summary>
        ///     Clones this dependency parameter object with one service replaced.
        /// </summary>
        /// <param name="queryableMethodTranslatingExpressionVisitorFactory"> A replacement for the current dependency of this type. </param>
        /// <returns> A new parameter object with the given service replaced. </returns>
        public QueryCompilationContextDependencies With(
            [NotNull] IQueryableMethodTranslatingExpressionVisitorFactory queryableMethodTranslatingExpressionVisitorFactory)
            => new QueryCompilationContextDependencies(
                Model,
                QueryTranslationPreprocessorFactory,
                queryableMethodTranslatingExpressionVisitorFactory,
                QueryTranslationPostprocessorFactory,
                ShapedQueryCompilingExpressionVisitorFactory,
                _executionStrategyFactory,
                _currentContext,
                ContextOptions,
                Logger);

        /// <summary>
        ///     Clones this dependency parameter object with one service replaced.
        /// </summary>
        /// <param name="queryTranslationPostprocessorFactory"> A replacement for the current dependency of this type. </param>
        /// <returns> A new parameter object with the given service replaced. </returns>
        public QueryCompilationContextDependencies With(
            [NotNull] IQueryTranslationPostprocessorFactory queryTranslationPostprocessorFactory)
            => new QueryCompilationContextDependencies(
                Model,
                QueryTranslationPreprocessorFactory,
                QueryableMethodTranslatingExpressionVisitorFactory,
                queryTranslationPostprocessorFactory,
                ShapedQueryCompilingExpressionVisitorFactory,
                _executionStrategyFactory,
                _currentContext,
                ContextOptions,
                Logger);

        /// <summary>
        ///     Clones this dependency parameter object with one service replaced.
        /// </summary>
        /// <param name="shapedQueryCompilingExpressionVisitorFactory"> A replacement for the current dependency of this type. </param>
        /// <returns> A new parameter object with the given service replaced. </returns>
        public QueryCompilationContextDependencies With(
            [NotNull] IShapedQueryCompilingExpressionVisitorFactory shapedQueryCompilingExpressionVisitorFactory)
            => new QueryCompilationContextDependencies(
                Model,
                QueryTranslationPreprocessorFactory,
                QueryableMethodTranslatingExpressionVisitorFactory,
                QueryTranslationPostprocessorFactory,
                shapedQueryCompilingExpressionVisitorFactory,
                _executionStrategyFactory,
                _currentContext,
                ContextOptions,
                Logger);

        /// <summary>
        ///     Clones this dependency parameter object with one service replaced.
        /// </summary>
        /// <param name="executionStrategyFactory"> A replacement for the current dependency of this type. </param>
        /// <returns> A new parameter object with the given service replaced. </returns>
        public QueryCompilationContextDependencies With([NotNull] IExecutionStrategyFactory executionStrategyFactory)
            => new QueryCompilationContextDependencies(
                Model,
                QueryTranslationPreprocessorFactory,
                QueryableMethodTranslatingExpressionVisitorFactory,
                QueryTranslationPostprocessorFactory,
                ShapedQueryCompilingExpressionVisitorFactory,
                executionStrategyFactory,
                _currentContext,
                ContextOptions,
                Logger);

        /// <summary>
        ///     Clones this dependency parameter object with one service replaced.
        /// </summary>
        /// <param name="currentContext"> A replacement for the current dependency of this type. </param>
        /// <returns> A new parameter object with the given service replaced. </returns>
        public QueryCompilationContextDependencies With([NotNull] ICurrentDbContext currentContext)
            => new QueryCompilationContextDependencies(
                Model,
                QueryTranslationPreprocessorFactory,
                QueryableMethodTranslatingExpressionVisitorFactory,
                QueryTranslationPostprocessorFactory,
                ShapedQueryCompilingExpressionVisitorFactory,
                _executionStrategyFactory,
                currentContext,
                ContextOptions,
                Logger);

        /// <summary>
        ///     Clones this dependency parameter object with one service replaced.
        /// </summary>
        /// <param name="contextOptions"> A replacement for the current dependency of this type. </param>
        /// <returns> A new parameter object with the given service replaced. </returns>
        public QueryCompilationContextDependencies With([NotNull] IDbContextOptions contextOptions)
            => new QueryCompilationContextDependencies(
                Model,
                QueryTranslationPreprocessorFactory,
                QueryableMethodTranslatingExpressionVisitorFactory,
                QueryTranslationPostprocessorFactory,
                ShapedQueryCompilingExpressionVisitorFactory,
                _executionStrategyFactory,
                _currentContext,
                contextOptions,
                Logger);

        /// <summary>
        ///     Clones this dependency parameter object with one service replaced.
        /// </summary>
        /// <param name="logger"> A replacement for the current dependency of this type. </param>
        /// <returns> A new parameter object with the given service replaced. </returns>
        public QueryCompilationContextDependencies With([NotNull] IDiagnosticsLogger<DbLoggerCategory.Query> logger)
            => new QueryCompilationContextDependencies(
                Model,
                QueryTranslationPreprocessorFactory,
                QueryableMethodTranslatingExpressionVisitorFactory,
                QueryTranslationPostprocessorFactory,
                ShapedQueryCompilingExpressionVisitorFactory,
                _executionStrategyFactory,
                _currentContext,
                ContextOptions,
                logger);
    }
}
