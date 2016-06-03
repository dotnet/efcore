// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Query.Sql;
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.EntityFrameworkCore.Update.Internal;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace Microsoft.EntityFrameworkCore.Storage
{
    public abstract class RelationalDatabaseProviderServices : DatabaseProviderServices, IRelationalDatabaseProviderServices
    {
        protected RelationalDatabaseProviderServices([NotNull] IServiceProvider services)
            : base(services)
        {
        }

        public override IDatabase Database => GetService<RelationalDatabase>();
        public override IDbContextTransactionManager TransactionManager => GetService<IRelationalConnection>();
        public override IModelValidator ModelValidator => GetService<RelationalModelValidator>();
        public override ICompiledQueryCacheKeyGenerator CompiledQueryCacheKeyGenerator => GetService<RelationalCompiledQueryCacheKeyGenerator>();
        public override IValueGeneratorSelector ValueGeneratorSelector => GetService<RelationalValueGeneratorSelector>();
        public override IExpressionPrinter ExpressionPrinter => GetService<RelationalExpressionPrinter>();
        public override IResultOperatorHandler ResultOperatorHandler => GetService<RelationalResultOperatorHandler>();
        public override IQueryContextFactory QueryContextFactory => GetService<RelationalQueryContextFactory>();
        public override IQueryCompilationContextFactory QueryCompilationContextFactory => GetService<RelationalQueryCompilationContextFactory>();
        public override IEntityQueryableExpressionVisitorFactory EntityQueryableExpressionVisitorFactory => GetService<RelationalEntityQueryableExpressionVisitorFactory>();
        public override IEntityQueryModelVisitorFactory EntityQueryModelVisitorFactory => GetService<RelationalQueryModelVisitorFactory>();
        public override IProjectionExpressionVisitorFactory ProjectionExpressionVisitorFactory => GetService<RelationalProjectionExpressionVisitorFactory>();

        public virtual IRelationalTypeMapper TypeMapper => GetService<RelationalTypeMapper>();
        public virtual IMigrationsAnnotationProvider MigrationsAnnotationProvider => GetService<MigrationsAnnotationProvider>();
        public virtual IBatchExecutor BatchExecutor => GetService<BatchExecutor>();
        public virtual IRelationalValueBufferFactoryFactory ValueBufferFactoryFactory => GetService<TypedRelationalValueBufferFactoryFactory>();
        public virtual ICommandBatchPreparer CommandBatchPreparer => GetService<CommandBatchPreparer>();
        public virtual IParameterNameGeneratorFactory ParameterNameGeneratorFactory => GetService<ParameterNameGeneratorFactory>();
        public virtual IMigrationsSqlGenerator MigrationsSqlGenerator => GetService<MigrationsSqlGenerator>();
        public virtual IExpressionFragmentTranslator CompositeExpressionFragmentTranslator => GetService<RelationalCompositeExpressionFragmentTranslator>();

        public abstract IMethodCallTranslator CompositeMethodCallTranslator { get; }
        public abstract IMemberTranslator CompositeMemberTranslator { get; }
        public abstract IHistoryRepository HistoryRepository { get; }
        public abstract IRelationalConnection RelationalConnection { get; }
        public abstract ISqlGenerationHelper SqlGenerationHelper { get; }
        public abstract IUpdateSqlGenerator UpdateSqlGenerator { get; }
        public abstract IModificationCommandBatchFactory ModificationCommandBatchFactory { get; }
        public abstract IRelationalDatabaseCreator RelationalDatabaseCreator { get; }
        public abstract IRelationalAnnotationProvider AnnotationProvider { get; }
        public abstract IQuerySqlGeneratorFactory QuerySqlGeneratorFactory { get; }
    }
}
