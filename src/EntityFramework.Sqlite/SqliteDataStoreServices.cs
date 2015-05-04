// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Relational;
using Microsoft.Data.Entity.Relational.Migrations.History;
using Microsoft.Data.Entity.Relational.Migrations.Sql;
using Microsoft.Data.Entity.Relational.Update;
using Microsoft.Data.Entity.Sqlite.Migrations;
using Microsoft.Data.Entity.Sqlite.Update;
using Microsoft.Data.Entity.Sqlite.ValueGeneration;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.ValueGeneration;
using Microsoft.Framework.DependencyInjection;

namespace Microsoft.Data.Entity.Sqlite
{
    public class SqliteDataStoreServices : RelationalDataStoreServices
    {
        public SqliteDataStoreServices([NotNull] IServiceProvider services)
            : base(services)
        {
        }

        public override IDataStoreConnection Connection => Services.GetRequiredService<SqliteDataStoreConnection>();
        public override IDataStoreCreator Creator => Services.GetRequiredService<SqliteDataStoreCreator>();
        public override IHistoryRepository HistoryRepository => Services.GetRequiredService<SqliteHistoryRepository>();
        public override IMigrationSqlGenerator MigrationSqlGenerator => Services.GetRequiredService<SqliteMigrationSqlGenerator>();
        public override IModelSource ModelSource => Services.GetRequiredService<SqliteModelSource>();
        public override IRelationalConnection RelationalConnection => Services.GetRequiredService<SqliteDataStoreConnection>();
        public override ISqlGenerator SqlGenerator => Services.GetRequiredService<SqliteSqlGenerator>();
        public override IDataStore Store => Services.GetRequiredService<SqliteDataStore>();
        public override IValueGeneratorCache ValueGeneratorCache => Services.GetRequiredService<SqliteValueGeneratorCache>();
        public override IRelationalTypeMapper TypeMapper => Services.GetRequiredService<SqliteTypeMapper>();
        public override IModificationCommandBatchFactory ModificationCommandBatchFactory => Services.GetRequiredService<SqliteModificationCommandBatchFactory>();
        public override ICommandBatchPreparer CommandBatchPreparer => Services.GetRequiredService<SqliteCommandBatchPreparer>();
        public override IRelationalDataStoreCreator RelationalDataStoreCreator => Services.GetRequiredService<SqliteDataStoreCreator>();
    }
}
