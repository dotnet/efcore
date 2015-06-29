// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Migrations.History;
using Microsoft.Data.Entity.Migrations.Infrastructure;
using Microsoft.Data.Entity.Migrations.Sql;
using Microsoft.Data.Entity.Query;
using Microsoft.Data.Entity.Query.Methods;
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

        public override IQueryContextFactory QueryContextFactory => GetService<RelationalQueryContextFactory>();
        public override IValueGeneratorSelector ValueGeneratorSelector => GetService<RelationalValueGeneratorSelector>();
        public override IModelValidator ModelValidator => GetService<RelationalModelValidator>();

        public virtual IRelationalTypeMapper TypeMapper => GetService<RelationalTypeMapper>();
        public virtual IMigrationAnnotationProvider MigrationAnnotationProvider => GetService<MigrationAnnotationProvider>();
        public virtual IBatchExecutor BatchExecutor => GetService<BatchExecutor>();
        public virtual IRelationalValueBufferFactoryFactory ValueBufferFactoryFactory => GetService<TypedValueBufferFactoryFactory>();
        public virtual ICommandBatchPreparer CommandBatchPreparer => GetService<CommandBatchPreparer>();
        public virtual ISqlStatementExecutor SqlStatementExecutor => GetService<SqlStatementExecutor>();
        public virtual IParameterNameGeneratorFactory ParameterNameGeneratorFactory => GetService<ParameterNameGeneratorFactory>();

        public abstract IMethodCallTranslator CompositeMethodCallTranslator { get; }
        public abstract IMemberTranslator CompositeMemberTranslator { get; }
        public abstract IHistoryRepository HistoryRepository { get; }
        public abstract IMigrationSqlGenerator MigrationSqlGenerator { get; }
        public abstract IRelationalConnection RelationalConnection { get; }
        public abstract IUpdateSqlGenerator UpdateSqlGenerator { get; }
        public abstract IModificationCommandBatchFactory ModificationCommandBatchFactory { get; }
        public abstract IRelationalDatabaseCreator RelationalDatabaseCreator { get; }
        public abstract IRelationalMetadataExtensionProvider MetadataExtensionProvider { get; }
    }
}
