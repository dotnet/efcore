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

        public override IDataStoreConnection Connection => GetService<SqliteDataStoreConnection>();
        public override IDataStoreCreator Creator => GetService<SqliteDataStoreCreator>();
        public override IHistoryRepository HistoryRepository => GetService<SqliteHistoryRepository>();
        public override IMigrationSqlGenerator MigrationSqlGenerator => GetService<SqliteMigrationSqlGenerator>();
        public override IModelSource ModelSource => GetService<SqliteModelSource>();
        public override IRelationalConnection RelationalConnection => GetService<SqliteDataStoreConnection>();
        public override ISqlGenerator SqlGenerator => GetService<SqliteSqlGenerator>();
        public override IDataStore Store => GetService<SqliteDataStore>();
        public override IValueGeneratorCache ValueGeneratorCache => GetService<SqliteValueGeneratorCache>();
        public override IRelationalTypeMapper TypeMapper => GetService<SqliteTypeMapper>();
        public override IModificationCommandBatchFactory ModificationCommandBatchFactory => GetService<SqliteModificationCommandBatchFactory>();
        public override ICommandBatchPreparer CommandBatchPreparer => GetService<SqliteCommandBatchPreparer>();
        public override IRelationalDataStoreCreator RelationalDataStoreCreator => GetService<SqliteDataStoreCreator>();
    }
}
