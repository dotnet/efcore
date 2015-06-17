// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.Data.Entity.Relational.Migrations;
using Microsoft.Data.Entity.Relational.Migrations.History;
using Microsoft.Data.Entity.Relational.Migrations.Infrastructure;
using Microsoft.Data.Entity.Relational.Migrations.Sql;
using Microsoft.Data.Entity.Relational.Update;
using Microsoft.Data.Entity.Sqlite.Metadata;
using Microsoft.Data.Entity.Sqlite.Migrations;
using Microsoft.Data.Entity.Sqlite.Update;
using Microsoft.Data.Entity.Sqlite.ValueGeneration;
using Microsoft.Data.Entity.Tests;

namespace Microsoft.Data.Entity.Sqlite
{
    public class SqliteEntityFrameworkServicesBuilderExtensionsTest : EntityFrameworkServiceCollectionExtensionsTest
    {
        public override void Services_wire_up_correctly()
        {
            base.Services_wire_up_correctly();

            // Relational
            VerifySingleton<IComparer<ModificationCommand>>();

            // Sqlite dingletones
            VerifySingleton<SqliteValueGeneratorCache>();
            VerifySingleton<SqliteSqlGenerator>();
            VerifySingleton<SqliteMetadataExtensionProvider>();
            VerifySingleton<SqliteTypeMapper>();
            VerifySingleton<SqliteModelSource>();

            // Sqlite scoped
            VerifyScoped<SqliteModificationCommandBatchFactory>();
            VerifyScoped<SqliteDataStoreServices>();
            VerifyScoped<SqliteDataStore>();
            VerifyScoped<SqliteDataStoreConnection>();
            VerifyScoped<SqliteMigrationSqlGenerator>();
            VerifyScoped<SqliteDataStoreCreator>();
            VerifyScoped<SqliteHistoryRepository>();

            // Migrations
            VerifyScoped<IMigrationAssembly>();
            VerifyScoped<IHistoryRepository>();
            VerifyScoped<IMigrator>();
            VerifySingleton<IMigrationIdGenerator>();
            VerifyScoped<IModelDiffer>();
            VerifyScoped<IMigrationSqlGenerator>();
        }

        protected override string AddProviderMethodName => "AddSqlite";

        public SqliteEntityFrameworkServicesBuilderExtensionsTest()
            : base(SqliteTestHelpers.Instance)
        {
        }
    }
}
