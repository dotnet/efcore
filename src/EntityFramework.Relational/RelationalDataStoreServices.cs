// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Query;
using Microsoft.Data.Entity.Relational.Metadata;
using Microsoft.Data.Entity.Relational.Migrations.History;
using Microsoft.Data.Entity.Relational.Migrations.Infrastructure;
using Microsoft.Data.Entity.Relational.Migrations.Sql;
using Microsoft.Data.Entity.Relational.Query;
using Microsoft.Data.Entity.Relational.Query.Methods;
using Microsoft.Data.Entity.Relational.Update;
using Microsoft.Data.Entity.Relational.ValueGeneration;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.ValueGeneration;

namespace Microsoft.Data.Entity.Relational
{
    public abstract class RelationalDataStoreServices : DataStoreServices, IRelationalDataStoreServices
    {
        protected RelationalDataStoreServices([NotNull] IServiceProvider services)
            : base(services)
        {
        }

        public override IDatabaseFactory DatabaseFactory => GetService<RelationalDatabaseFactory>();
        public override IQueryContextFactory QueryContextFactory => GetService<RelationalQueryContextFactory>();
        public override IValueGeneratorSelector ValueGeneratorSelector => GetService<RelationalValueGeneratorSelector>();
        public virtual IRelationalTypeMapper TypeMapper => GetService<RelationalTypeMapper>();
        public virtual IModelDiffer ModelDiffer => GetService<ModelDiffer>();
        public virtual IBatchExecutor BatchExecutor => GetService<BatchExecutor>();
        public virtual IRelationalValueBufferFactoryFactory ValueBufferFactoryFactory => GetService<TypedValueBufferFactoryFactory>();
        public virtual ICommandBatchPreparer CommandBatchPreparer => GetService<CommandBatchPreparer>();
        public virtual ISqlStatementExecutor SqlStatementExecutor => GetService<SqlStatementExecutor>();
        public override IModelValidator ModelValidator => GetService<RelationalModelValidator>();

        public abstract IMethodCallTranslator CompositeMethodCallTranslator { get; }
        public abstract IMemberTranslator CompositeMemberTranslator { get; }
        public abstract IHistoryRepository HistoryRepository { get; }
        public abstract IMigrationSqlGenerator MigrationSqlGenerator { get; }
        public abstract IRelationalConnection RelationalConnection { get; }
        public abstract ISqlGenerator SqlGenerator { get; }
        public abstract IModificationCommandBatchFactory ModificationCommandBatchFactory { get; }
        public abstract IRelationalDataStoreCreator RelationalDataStoreCreator { get; }
        public abstract IRelationalMetadataExtensionProvider MetadataExtensionProvider { get; }
    }
}
