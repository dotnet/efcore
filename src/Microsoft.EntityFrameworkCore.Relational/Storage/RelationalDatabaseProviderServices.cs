// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Query.Sql;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.EntityFrameworkCore.Update.Internal;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace Microsoft.EntityFrameworkCore.Storage
{
    /// <summary>
    ///     <para>
    ///         The primary services needed to interact with a relational database.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    public abstract class RelationalDatabaseProviderServices : DatabaseProviderServices, IRelationalDatabaseProviderServices
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="RelationalDatabaseProviderServices" /> class.
        /// </summary>
        /// <param name="services"> The service provider to resolve services from. </param>
        protected RelationalDatabaseProviderServices([NotNull] IServiceProvider services)
            : base(services)
        {
        }

        /// <summary>
        ///     Gets the <see cref="IDatabase" /> for the database provider.
        /// </summary>
        public override IDatabase Database => GetService<RelationalDatabase>();

        /// <summary>
        ///     Gets the <see cref="IDbContextTransactionManager" /> for the database provider.
        /// </summary>
        public override IDbContextTransactionManager TransactionManager => GetService<IRelationalConnection>();

        /// <summary>
        ///     Gets the <see cref="IModelValidator" /> for the database provider.
        /// </summary>
        public override IModelValidator ModelValidator => GetService<RelationalModelValidator>();

        /// <summary>
        ///     Gets the <see cref="ICompiledQueryCacheKeyGenerator" /> for the database provider.
        /// </summary>
        public override ICompiledQueryCacheKeyGenerator CompiledQueryCacheKeyGenerator => GetService<RelationalCompiledQueryCacheKeyGenerator>();

        /// <summary>
        ///     Gets the <see cref="IValueGeneratorSelector" /> for the database provider.
        /// </summary>
        public override IValueGeneratorSelector ValueGeneratorSelector => GetService<RelationalValueGeneratorSelector>();

        /// <summary>
        ///     Gets the <see cref="IExpressionPrinter" /> for the database provider.
        /// </summary>
        public override IExpressionPrinter ExpressionPrinter => GetService<RelationalExpressionPrinter>();

        /// <summary>
        ///     Gets the <see cref="IResultOperatorHandler" /> for the database provider.
        /// </summary>
        public override IResultOperatorHandler ResultOperatorHandler => GetService<RelationalResultOperatorHandler>();

        /// <summary>
        ///     Gets the <see cref="IQueryContextFactory" /> for the database provider.
        /// </summary>
        public override IQueryContextFactory QueryContextFactory => GetService<RelationalQueryContextFactory>();

        /// <summary>
        ///     Gets the <see cref="IQueryCompilationContextFactory" /> for the database provider.
        /// </summary>
        public override IQueryCompilationContextFactory QueryCompilationContextFactory => GetService<RelationalQueryCompilationContextFactory>();

        /// <summary>
        ///     Gets the <see cref="IEntityQueryableExpressionVisitorFactory" /> for the database provider.
        /// </summary>
        public override IEntityQueryableExpressionVisitorFactory EntityQueryableExpressionVisitorFactory => GetService<RelationalEntityQueryableExpressionVisitorFactory>();

        /// <summary>
        ///     Gets the <see cref="IEntityQueryModelVisitorFactory" /> for the database provider.
        /// </summary>
        public override IEntityQueryModelVisitorFactory EntityQueryModelVisitorFactory => GetService<RelationalQueryModelVisitorFactory>();

        /// <summary>
        ///     Gets the <see cref="IProjectionExpressionVisitorFactory" /> for the database provider.
        /// </summary>
        public override IProjectionExpressionVisitorFactory ProjectionExpressionVisitorFactory => GetService<RelationalProjectionExpressionVisitorFactory>();

        /// <summary>
        ///     Gets the <see cref="IRelationalTypeMapper" /> for the database provider.
        /// </summary>
        public virtual IRelationalTypeMapper TypeMapper => GetService<RelationalTypeMapper>();

        /// <summary>
        ///     Gets the <see cref="IMigrationsAnnotationProvider" /> for the database provider.
        /// </summary>
        public virtual IMigrationsAnnotationProvider MigrationsAnnotationProvider => GetService<MigrationsAnnotationProvider>();

        /// <summary>
        ///     Gets the <see cref="IBatchExecutor" /> for the database provider.
        /// </summary>
        public virtual IBatchExecutor BatchExecutor => GetService<BatchExecutor>();

        /// <summary>
        ///     Gets the <see cref="IRelationalValueBufferFactoryFactory" /> for the database provider.
        /// </summary>
        public virtual IRelationalValueBufferFactoryFactory ValueBufferFactoryFactory => GetService<TypedRelationalValueBufferFactoryFactory>();

        /// <summary>
        ///     Gets the <see cref="ICommandBatchPreparer" /> for the database provider.
        /// </summary>
        public virtual ICommandBatchPreparer CommandBatchPreparer => GetService<CommandBatchPreparer>();

        /// <summary>
        ///     Gets the <see cref="IParameterNameGeneratorFactory" /> for the database provider.
        /// </summary>
        public virtual IParameterNameGeneratorFactory ParameterNameGeneratorFactory => GetService<ParameterNameGeneratorFactory>();

        /// <summary>
        ///     Gets the <see cref="IMigrationsSqlGenerator" /> for the database provider.
        /// </summary>
        public virtual IMigrationsSqlGenerator MigrationsSqlGenerator => GetService<MigrationsSqlGenerator>();

        /// <summary>
        ///     Gets the <see cref="IExpressionFragmentTranslator" /> for the database provider.
        /// </summary>
        public virtual IExpressionFragmentTranslator CompositeExpressionFragmentTranslator => GetService<RelationalCompositeExpressionFragmentTranslator>();

        /// <summary>
        ///     Gets the <see cref="IDatabaseCreator" /> for the database provider.
        /// </summary>
        public override IDatabaseCreator Creator => RelationalDatabaseCreator;

        /// <summary>
        ///     Gets the <see cref="IMethodCallTranslator" /> for the database provider.
        /// </summary>
        public abstract IMethodCallTranslator CompositeMethodCallTranslator { get; }

        /// <summary>
        ///     Gets the <see cref="IMemberTranslator" /> for the database provider.
        /// </summary>
        public abstract IMemberTranslator CompositeMemberTranslator { get; }

        /// <summary>
        ///     Gets the <see cref="IHistoryRepository" /> for the database provider.
        /// </summary>
        public abstract IHistoryRepository HistoryRepository { get; }

        /// <summary>
        ///     Gets the <see cref="IRelationalConnection" /> for the database provider.
        /// </summary>
        public abstract IRelationalConnection RelationalConnection { get; }

        /// <summary>
        ///     Gets the <see cref="ISqlGenerationHelper" /> for the database provider.
        /// </summary>
        public abstract ISqlGenerationHelper SqlGenerationHelper { get; }

        /// <summary>
        ///     Gets the <see cref="IUpdateSqlGenerator" /> for the database provider.
        /// </summary>
        public abstract IUpdateSqlGenerator UpdateSqlGenerator { get; }

        /// <summary>
        ///     Gets the <see cref="IModificationCommandBatchFactory" /> for the database provider.
        /// </summary>
        public abstract IModificationCommandBatchFactory ModificationCommandBatchFactory { get; }

        /// <summary>
        ///     Gets the <see cref="IRelationalDatabaseCreator" /> for the database provider.
        /// </summary>
        public abstract IRelationalDatabaseCreator RelationalDatabaseCreator { get; }

        /// <summary>
        ///     Gets the <see cref="IRelationalAnnotationProvider" /> for the database provider.
        /// </summary>
        public abstract IRelationalAnnotationProvider AnnotationProvider { get; }

        /// <summary>
        ///     Gets the <see cref="IQuerySqlGeneratorFactory" /> for the database provider.
        /// </summary>
        public abstract IQuerySqlGeneratorFactory QuerySqlGeneratorFactory { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override IExecutionStrategyFactory ExecutionStrategyFactory => GetService<RelationalExecutionStrategyFactory>();
    }
}
