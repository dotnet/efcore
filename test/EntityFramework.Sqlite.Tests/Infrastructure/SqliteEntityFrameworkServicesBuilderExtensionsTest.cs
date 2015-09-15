// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata.Conventions.Internal;
using Microsoft.Data.Entity.Migrations;
using Microsoft.Data.Entity.Query.ExpressionTranslators;
using Microsoft.Data.Entity.Sqlite.Metadata;
using Microsoft.Data.Entity.Sqlite.Tests;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Tests;
using Microsoft.Data.Entity.Update;
using Microsoft.Data.Entity.ValueGeneration;

namespace Microsoft.Data.Entity
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
            VerifySingleton<SqliteMigrationsAnnotationProvider>();
            VerifySingleton<SqliteConventionSetBuilder>();

            // SQLite scoped
            VerifyScoped<SqliteModificationCommandBatchFactory>();
            VerifyScoped<SqliteDatabaseProviderServices>();
            VerifyScoped<SqliteDatabaseConnection>();
            VerifyScoped<SqliteMigrationsSqlGenerator>();
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
