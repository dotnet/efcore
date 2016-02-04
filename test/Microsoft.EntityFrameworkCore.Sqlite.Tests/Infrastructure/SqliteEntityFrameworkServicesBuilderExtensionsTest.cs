// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Internal;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators.Internal;
using Microsoft.EntityFrameworkCore.Sqlite.Tests;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.EntityFrameworkCore.Tests;
using Microsoft.EntityFrameworkCore.Update.Internal;
using Microsoft.EntityFrameworkCore.ValueGeneration.Internal;

namespace Microsoft.EntityFrameworkCore
{
    public class SqliteEntityFrameworkServicesBuilderExtensionsTest : RelationalEntityFrameworkServicesBuilderExtensionsTest
    {
        public override void Services_wire_up_correctly()
        {
            base.Services_wire_up_correctly();

            // SQLite dingletones
            VerifySingleton<SqliteValueGeneratorCache>();
            VerifySingleton<SqliteAnnotationProvider>();
            VerifySingleton<SqliteTypeMapper>();
            VerifySingleton<SqliteModelSource>();
            VerifySingleton<SqliteMigrationsAnnotationProvider>();

            // SQLite scoped
            VerifyScoped<SqliteConventionSetBuilder>();
            VerifyScoped<SqliteUpdateSqlGenerator>();
            VerifyScoped<SqliteModificationCommandBatchFactory>();
            VerifyScoped<SqliteDatabaseProviderServices>();
            VerifyScoped<SqliteRelationalConnection>();
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
