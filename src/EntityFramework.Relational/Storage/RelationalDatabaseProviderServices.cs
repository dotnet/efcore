// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Migrations;
using Microsoft.Data.Entity.Query;
using Microsoft.Data.Entity.Query.ExpressionTranslators;
using Microsoft.Data.Entity.Query.ExpressionVisitors;
using Microsoft.Data.Entity.Query.Internal;
using Microsoft.Data.Entity.Query.Sql;
using Microsoft.Data.Entity.Update;
using Microsoft.Data.Entity.ValueGeneration;

namespace Microsoft.Data.Entity.Storage
{
    public abstract class RelationalDatabaseProviderServices : DatabaseProviderServices, IRelationalDatabaseProviderServices
    {
        protected RelationalDatabaseProviderServices([NotNull] IServiceProvider services)
            : base(services)
        {
        }

        public override IDatabase Database => GetService<RelationalDatabase>();
        public override IModelValidator ModelValidator => GetService<RelationalModelValidator>();
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
        public virtual ISqlStatementExecutor SqlStatementExecutor => GetService<SqlStatementExecutor>();
        public virtual IParameterNameGeneratorFactory ParameterNameGeneratorFactory => GetService<ParameterNameGeneratorFactory>();
        public virtual IMigrationsSqlGenerator MigrationsSqlGenerator => GetService<MigrationsSqlGenerator>();

        public abstract IMethodCallTranslator CompositeMethodCallTranslator { get; }
        public abstract IMemberTranslator CompositeMemberTranslator { get; }
        public abstract IExpressionFragmentTranslator CompositeExpressionFragmentTranslator { get; }
        public abstract IHistoryRepository HistoryRepository { get; }
        public abstract IRelationalConnection RelationalConnection { get; }
        public abstract IUpdateSqlGenerator UpdateSqlGenerator { get; }
        public abstract IModificationCommandBatchFactory ModificationCommandBatchFactory { get; }
        public abstract IRelationalDatabaseCreator RelationalDatabaseCreator { get; }
        public abstract IRelationalMetadataExtensionProvider MetadataExtensionProvider { get; }
        public abstract ISqlQueryGeneratorFactory SqlQueryGeneratorFactory { get; }
    }
}
