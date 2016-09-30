// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Internal;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators.Internal;
using Microsoft.EntityFrameworkCore.Relational.Tests;
using Microsoft.EntityFrameworkCore.Sqlite.FunctionalTests;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.EntityFrameworkCore.Update.Internal;
using Microsoft.EntityFrameworkCore.ValueGeneration.Internal;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Sqlite.Tests.Infrastructure
{
    public class SqliteServiceCollectionExtensionsTest : RelationalServiceCollectionExtensionsTest
    {
        [Fact]
        public void Calling_AddEntityFramework_explicitly_does_not_change_services()
            => AssertServicesSame(
                new ServiceCollection().AddEntityFrameworkSqlite(),
                new ServiceCollection().AddEntityFramework().AddEntityFrameworkSqlite());

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

        public SqliteServiceCollectionExtensionsTest()
            : base(SqliteTestHelpers.Instance)
        {
        }
    }
}
