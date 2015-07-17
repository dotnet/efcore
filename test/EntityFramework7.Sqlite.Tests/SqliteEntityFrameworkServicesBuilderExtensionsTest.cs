// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Sqlite.Metadata;
using Microsoft.Data.Entity.Sqlite.Migrations;
using Microsoft.Data.Entity.Sqlite.Update;
using Microsoft.Data.Entity.Sqlite.ValueGeneration;
using Microsoft.Data.Entity.Tests;
using Microsoft.Data.Entity.Update;

namespace Microsoft.Data.Entity.Sqlite.Tests
{
    public class SqliteEntityFrameworkServicesBuilderExtensionsTest : RelationalEntityFrameworkServicesBuilderExtensionsTest
    {
        public override void Services_wire_up_correctly()
        {
            base.Services_wire_up_correctly();

            // SQLite dingletones
            VerifySingleton<SqliteValueGeneratorCache>();
            VerifySingleton<SqliteUpdateSqlGenerator>();
            VerifySingleton<SqliteMetadataExtensionProvider>();
            VerifySingleton<SqliteTypeMapper>();
            VerifySingleton<SqliteModelSource>();
            VerifySingleton<SqliteMigrationAnnotationProvider>();
            VerifySingleton<SqliteConventionSetBuilder>();

            // SQLite scoped
            VerifyScoped<SqliteModificationCommandBatchFactory>();
            VerifyScoped<SqliteDatabaseProviderServices>();
            VerifyScoped<SqliteDatabase>();
            VerifyScoped<SqliteDatabaseConnection>();
            VerifyScoped<SqliteMigrationSqlGenerator>();
            VerifyScoped<SqliteDatabaseCreator>();
            VerifyScoped<SqliteHistoryRepository>();
            VerifyScoped<SqliteCompositeMethodCallTranslator>();
            VerifyScoped<SqliteCompositeMemberTranslator>();
        }

        public SqliteEntityFrameworkServicesBuilderExtensionsTest()
            : base(SqliteTestHelpers.Instance)
        {
        }
    }
}
