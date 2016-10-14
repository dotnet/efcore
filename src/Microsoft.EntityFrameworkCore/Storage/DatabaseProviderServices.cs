// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Storage
{
    /// <summary>
    ///     <para>
    ///         The base set of services required by EF for a database provider to function.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    public abstract class DatabaseProviderServices : IDatabaseProviderServices
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="DatabaseProviderServices" /> class.
        /// </summary>
        /// <param name="services"> The service provider to resolve services from. </param>
        protected DatabaseProviderServices([NotNull] IServiceProvider services)
        {
            Check.NotNull(services, nameof(services));

            Services = services;
        }

        /// <summary>
        ///     The unique name used to identify the database provider. This should be the same as the NuGet package name
        ///     for the providers runtime.
        /// </summary>
        public abstract string InvariantName { get; }

        /// <summary>
        ///     Gets the service provider to resolve services from.
        /// </summary>
        protected virtual IServiceProvider Services { get; }

        /// <summary>
        ///     Resolves a required service from <see cref="Services" />.
        /// </summary>
        /// <typeparam name="TService"> The service to be resolved. </typeparam>
        /// <returns> The resolved service. </returns>
        protected virtual TService GetService<TService>() => Services.GetRequiredService<TService>();

        /// <summary>
        ///     The convention set builder for the database provider. By default this returns null, meaning the
        ///     default <see cref="CoreConventionSetBuilder" /> will be used.
        /// </summary>
        public virtual IConventionSetBuilder ConventionSetBuilder => null;

        /// <summary>
        ///     Gets the <see cref="IValueGeneratorSelector" /> for the database provider. By default, EF will register a default implementation
        ///     (<see cref="Microsoft.EntityFrameworkCore.ValueGeneration.ValueGeneratorSelector" />) which provides basic functionality but can be
        ///     overridden if needed.
        /// </summary>
        public virtual IValueGeneratorSelector ValueGeneratorSelector => GetService<ValueGeneratorSelector>();

        /// <summary>
        ///     Gets the <see cref="IModelValidator" /> for the database provider. By default, EF will register a default implementation
        ///     which does no validation.
        /// </summary>
        public virtual IModelValidator ModelValidator => GetService<NoopModelValidator>();

        /// <summary>
        ///     Gets the <see cref="ICompiledQueryCacheKeyGenerator" /> for the database provider. By default, EF will register a default
        ///     implementation
        ///     (<see cref="Query.CompiledQueryCacheKeyGenerator" />) which provides basic functionality but can be
        ///     overridden if needed.
        /// </summary>
        public virtual ICompiledQueryCacheKeyGenerator CompiledQueryCacheKeyGenerator => GetService<CompiledQueryCacheKeyGenerator>();

        /// <summary>
        ///     Gets the <see cref="IExpressionPrinter" /> for the database provider. By default, EF will register a default implementation
        ///     (<see cref="Query.Internal.ExpressionPrinter" />) which provides basic functionality but can be
        ///     overridden if needed.
        /// </summary>
        public virtual IExpressionPrinter ExpressionPrinter => GetService<ExpressionPrinter>();

        /// <summary>
        ///     Gets the <see cref="IResultOperatorHandler" /> for the database provider. By default, EF will register a default implementation
        ///     (<see cref="Query.ResultOperatorHandler" />) which provides basic functionality but can be
        ///     overridden if needed.
        /// </summary>
        public virtual IResultOperatorHandler ResultOperatorHandler => GetService<ResultOperatorHandler>();

        /// <summary>
        ///     Gets the <see cref="IQueryCompilationContextFactory" /> for the database provider. By default, EF will register a default
        ///     implementation
        ///     (<see cref="ValueGeneration.ValueGeneratorSelector" />) which provides basic functionality but can be
        ///     overridden if needed.
        /// </summary>
        public virtual IQueryCompilationContextFactory QueryCompilationContextFactory => GetService<QueryCompilationContextFactory>();

        /// <summary>
        ///     Gets the <see cref="IProjectionExpressionVisitorFactory" /> for the database provider. By default, EF will register a default
        ///     implementation
        ///     (<see cref="Query.ExpressionVisitors.Internal.ProjectionExpressionVisitorFactory" />) which provides basic functionality but can be
        ///     overridden if needed.
        /// </summary>
        public virtual IProjectionExpressionVisitorFactory ProjectionExpressionVisitorFactory => GetService<ProjectionExpressionVisitorFactory>();

        /// <summary>
        ///     Gets the <see cref="IDatabase" /> for the database provider.
        /// </summary>
        public abstract IDatabase Database { get; }

        /// <summary>
        ///     Gets the <see cref="IDbContextTransactionManager" /> for the database provider.
        /// </summary>
        public abstract IDbContextTransactionManager TransactionManager { get; }

        /// <summary>
        ///     Gets the <see cref="IDatabaseCreator" /> for the database provider.
        /// </summary>
        public abstract IDatabaseCreator Creator { get; }

        /// <summary>
        ///     Gets the <see cref="IModelSource" /> for the database provider.
        /// </summary>
        public abstract IModelSource ModelSource { get; }

        /// <summary>
        ///     Gets the <see cref="IValueGeneratorCache" /> for the database provider.
        /// </summary>
        public abstract IValueGeneratorCache ValueGeneratorCache { get; }

        /// <summary>
        ///     Gets the <see cref="IQueryContextFactory" /> for the database provider.
        /// </summary>
        public abstract IQueryContextFactory QueryContextFactory { get; }

        /// <summary>
        ///     Gets the <see cref="IEntityQueryableExpressionVisitorFactory" /> for the database provider.
        /// </summary>
        public abstract IEntityQueryableExpressionVisitorFactory EntityQueryableExpressionVisitorFactory { get; }

        /// <summary>
        ///     Gets the <see cref="IEntityQueryModelVisitorFactory" /> for the database provider.
        /// </summary>
        public abstract IEntityQueryModelVisitorFactory EntityQueryModelVisitorFactory { get; }

        /// <summary>
        ///     Gets the <see cref="IExecutionStrategyFactory" /> for the database provider.
        /// </summary>
        public virtual IExecutionStrategyFactory ExecutionStrategyFactory => GetService<ExecutionStrategyFactory>();
    }
}
