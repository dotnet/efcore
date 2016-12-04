// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.ValueGeneration;

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
    public interface IDatabaseProviderServices
    {
        /// <summary>
        ///     The unique name used to identify the database provider. This should be the same as the NuGet package name
        ///     for the providers runtime.
        /// </summary>
        string InvariantName { get; }

        /// <summary>
        ///     Gets the <see cref="IDatabase" /> for the database provider.
        /// </summary>
        IDatabase Database { get; }

        /// <summary>
        ///     Gets the <see cref="IDbContextTransactionManager" /> for the database provider.
        /// </summary>
        IDbContextTransactionManager TransactionManager { get; }

        /// <summary>
        ///     Gets the <see cref="IDatabaseCreator" /> for the database provider.
        /// </summary>
        IDatabaseCreator Creator { get; }

        /// <summary>
        ///     Gets the <see cref="IValueGeneratorSelector" /> for the database provider.
        /// </summary>
        IValueGeneratorSelector ValueGeneratorSelector { get; }

        /// <summary>
        ///     Gets the <see cref="IConventionSetBuilder" /> for the database provider.
        /// </summary>
        IConventionSetBuilder ConventionSetBuilder { get; }

        /// <summary>
        ///     Gets the <see cref="IModelSource" /> for the database provider.
        /// </summary>
        IModelSource ModelSource { get; }

        /// <summary>
        ///     Gets the <see cref="IModelValidator" /> for the database provider.
        /// </summary>
        IModelValidator ModelValidator { get; }

        /// <summary>
        ///     Gets the <see cref="IValueGeneratorCache" /> for the database provider.
        /// </summary>
        IValueGeneratorCache ValueGeneratorCache { get; }

        /// <summary>
        ///     Gets the <see cref="IQueryContextFactory" /> for the database provider.
        /// </summary>
        IQueryContextFactory QueryContextFactory { get; }

        /// <summary>
        ///     Gets the <see cref="IQueryCompilationContextFactory" /> for the database provider.
        /// </summary>
        IQueryCompilationContextFactory QueryCompilationContextFactory { get; }

        /// <summary>
        ///     Gets the <see cref="IEntityQueryModelVisitorFactory" /> for the database provider.
        /// </summary>
        IEntityQueryModelVisitorFactory EntityQueryModelVisitorFactory { get; }

        /// <summary>
        ///     Gets the <see cref="ICompiledQueryCacheKeyGenerator" /> for the database provider.
        /// </summary>
        ICompiledQueryCacheKeyGenerator CompiledQueryCacheKeyGenerator { get; }

        /// <summary>
        ///     Gets the <see cref="IExpressionPrinter" /> for the database provider.
        /// </summary>
        IExpressionPrinter ExpressionPrinter { get; }

        /// <summary>
        ///     Gets the <see cref="IResultOperatorHandler" /> for the database provider.
        /// </summary>
        IResultOperatorHandler ResultOperatorHandler { get; }

        /// <summary>
        ///     Gets the <see cref="IEntityQueryableExpressionVisitorFactory" /> for the database provider.
        /// </summary>
        IEntityQueryableExpressionVisitorFactory EntityQueryableExpressionVisitorFactory { get; }

        /// <summary>
        ///     Gets the <see cref="IProjectionExpressionVisitorFactory" /> for the database provider.
        /// </summary>
        IProjectionExpressionVisitorFactory ProjectionExpressionVisitorFactory { get; }

        /// <summary>
        ///     Gets the <see cref="IExecutionStrategyFactory" /> for the database provider.
        /// </summary>
        IExecutionStrategyFactory ExecutionStrategyFactory { get; }

        /// <summary>
        ///     Reset any stateful database provider services. Used when context pooling is enabled to allow
        ///     re-use of provider services.
        /// </summary>
        void Reset();
    }
}
