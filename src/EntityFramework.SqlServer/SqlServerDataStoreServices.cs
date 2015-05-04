// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata.Builders;
using Microsoft.Data.Entity.Relational;
using Microsoft.Data.Entity.Relational.Migrations.History;
using Microsoft.Data.Entity.Relational.Migrations.Infrastructure;
using Microsoft.Data.Entity.Relational.Migrations.Sql;
using Microsoft.Data.Entity.Relational.Update;
using Microsoft.Data.Entity.SqlServer.Migrations;
using Microsoft.Data.Entity.SqlServer.Update;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.ValueGeneration;
using Microsoft.Framework.DependencyInjection;

namespace Microsoft.Data.Entity.SqlServer
{
    public class SqlServerDataStoreServices : RelationalDataStoreServices
    {
        public SqlServerDataStoreServices([NotNull] IServiceProvider services)
            : base(services)
        {
        }

        public override IDataStore Store => Services.GetRequiredService<SqlServerDataStore>();
        public override IDataStoreCreator Creator => Services.GetRequiredService<SqlServerDataStoreCreator>();
        public override IDataStoreConnection Connection => Services.GetRequiredService<ISqlServerConnection>();
        public override IRelationalConnection RelationalConnection => Services.GetRequiredService<ISqlServerConnection>();
        public override IValueGeneratorSelector ValueGeneratorSelector => Services.GetRequiredService<SqlServerValueGeneratorSelector>();
        public override IRelationalDataStoreCreator RelationalDataStoreCreator => Services.GetRequiredService<SqlServerDataStoreCreator>();
        public override IModelBuilderFactory ModelBuilderFactory => Services.GetRequiredService<SqlServerModelBuilderFactory>();
        public override IModelDiffer ModelDiffer => Services.GetRequiredService<SqlServerModelDiffer>();
        public override IHistoryRepository HistoryRepository => Services.GetRequiredService<SqlServerHistoryRepository>();
        public override IMigrationSqlGenerator MigrationSqlGenerator => Services.GetRequiredService<SqlServerMigrationSqlGenerator>();
        public override IModelSource ModelSource => Services.GetRequiredService<SqlServerModelSource>();
        public override ISqlGenerator SqlGenerator => Services.GetRequiredService<ISqlServerSqlGenerator>();
        public override IValueGeneratorCache ValueGeneratorCache => Services.GetRequiredService<ISqlServerValueGeneratorCache>();
        public override IRelationalTypeMapper TypeMapper => Services.GetRequiredService<SqlServerTypeMapper>();
        public override IModificationCommandBatchFactory ModificationCommandBatchFactory => Services.GetRequiredService<SqlServerModificationCommandBatchFactory>();
        public override ICommandBatchPreparer CommandBatchPreparer => Services.GetRequiredService<SqlServerCommandBatchPreparer>();
        public override IRelationalValueBufferFactoryFactory ValueBufferFactoryFactory => Services.GetRequiredService<UntypedValueBufferFactoryFactory>();
    }
}
