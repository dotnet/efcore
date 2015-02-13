// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.Identity;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Relational;
using Microsoft.Data.Entity.Relational.Migrations;
using Microsoft.Data.Entity.Relational.Migrations.History;
using Microsoft.Data.Entity.Relational.Migrations.Infrastructure;
using Microsoft.Data.Entity.Relational.Migrations.Sql;
using Microsoft.Data.Entity.Relational.Update;
using Microsoft.Data.Entity.SqlServer.Migrations;
using Microsoft.Data.Entity.SqlServer.Update;
using Microsoft.Data.Entity.Tests;
using Microsoft.Framework.DependencyInjection;
using Xunit;

namespace Microsoft.Data.Entity.SqlServer.Tests
{
    public class SqlServerEntityServicesBuilderExtensionsTest : EntityServiceCollectionExtensionsTest
    {
        [Fact]
        public override void Services_wire_up_correctly()
        {
            base.Services_wire_up_correctly();

            // Relational
            VerifySingleton<RelationalObjectArrayValueReaderFactory>();
            VerifySingleton<RelationalTypedValueReaderFactory>();
            VerifySingleton<ModificationCommandComparer>();

            // SQL Server dingletones
            VerifySingleton<SqlServerModelBuilderFactory>();
            VerifySingleton<SqlServerValueGeneratorCache>();
            VerifySingleton<SqlServerValueGeneratorSelector>();
            VerifySingleton<SimpleValueGeneratorFactory<SequentialGuidValueGenerator>>();
            VerifySingleton<SqlServerSequenceValueGeneratorFactory>();
            VerifySingleton<SqlServerSqlGenerator>();
            VerifySingleton<SqlStatementExecutor>();
            VerifySingleton<SqlServerTypeMapper>();
            VerifySingleton<SqlServerModificationCommandBatchFactory>();
            VerifySingleton<SqlServerCommandBatchPreparer>();
            VerifySingleton<SqlServerModelSource>();

            // SQL Server scoped
            VerifyScoped<SqlServerBatchExecutor>();
            VerifyScoped<SqlServerDataStoreServices>();
            VerifyScoped<SqlServerDataStore>();
            VerifyScoped<SqlServerConnection>();
            VerifyScoped<SqlServerModelDiffer>();
            VerifyScoped<SqlServerDatabase>();
            VerifyScoped<SqlServerMigrationSqlGenerator>();
            VerifyScoped<SqlServerDataStoreCreator>();
            VerifyScoped<SqlServerHistoryRepository>();

            VerifyCommonDataStoreServices();

            // Migrations
            VerifyScoped<MigrationAssembly>();
            VerifyScoped<DbContextService<IHistoryRepository>>();
            VerifyScoped<Migrator>();
            VerifySingleton<MigrationIdGenerator>();
            VerifyScoped<DbContextService<ModelDiffer>>();
            VerifyScoped<DbContextService<MigrationSqlGenerator>>();
        }

        protected override IServiceCollection GetServices(IServiceCollection services = null)
        {
            return (services ?? new ServiceCollection())
                .AddEntityFramework()
                .AddSqlServer()
                .ServiceCollection();
        }

        protected override DbContext CreateContext(IServiceProvider serviceProvider)
        {
            return SqlServerTestHelpers.Instance.CreateContext(serviceProvider);
        }
    }
}
