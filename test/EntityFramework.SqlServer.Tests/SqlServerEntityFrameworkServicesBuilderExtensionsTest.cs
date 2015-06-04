// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Relational;
using Microsoft.Data.Entity.Relational.Migrations;
using Microsoft.Data.Entity.Relational.Migrations.History;
using Microsoft.Data.Entity.Relational.Migrations.Infrastructure;
using Microsoft.Data.Entity.Relational.Migrations.Sql;
using Microsoft.Data.Entity.Relational.Update;
using Microsoft.Data.Entity.SqlServer.Metadata;
using Microsoft.Data.Entity.SqlServer.Migrations;
using Microsoft.Data.Entity.SqlServer.Update;
using Microsoft.Data.Entity.SqlServer.ValueGeneration;
using Microsoft.Data.Entity.Tests;
using Microsoft.Framework.DependencyInjection;
using Xunit;

namespace Microsoft.Data.Entity.SqlServer.Tests
{
    public class SqlServerEntityFrameworkServicesBuilderExtensionsTest : EntityFrameworkServiceCollectionExtensionsTest
    {
        [Fact]
        public override void Services_wire_up_correctly()
        {
            base.Services_wire_up_correctly();

            // Relational
            VerifySingleton<IComparer<ModificationCommand>>();

            // SQL Server dingletones
            VerifySingleton<SqlServerModelBuilderFactory>();
            VerifySingleton<ISqlServerValueGeneratorCache>();
            VerifySingleton<ISqlServerSqlGenerator>();
            VerifySingleton<ISqlStatementExecutor>();
            VerifySingleton<SqlServerTypeMapper>();
            VerifySingleton<SqlServerModelSource>();
            VerifySingleton<SqlServerMetadataExtensionProvider>();

            // SQL Server scoped
            VerifyScoped<SqlServerModificationCommandBatchFactory>();
            VerifyScoped<ISqlServerSequenceValueGeneratorFactory>();
            VerifyScoped<SqlServerValueGeneratorSelector>();
            VerifyScoped<SqlServerDataStoreServices>();
            VerifyScoped<SqlServerDataStore>();
            VerifyScoped<ISqlServerConnection>();
            VerifyScoped<SqlServerModelDiffer>();
            VerifyScoped<SqlServerMigrationSqlGenerator>();
            VerifyScoped<SqlServerDataStoreCreator>();
            VerifyScoped<SqlServerHistoryRepository>();

            VerifyCommonDataStoreServices();

            // Migrations
            VerifyScoped<IMigrationAssembly>();
            VerifyScoped<IHistoryRepository>();
            VerifyScoped<IMigrator>();
            VerifySingleton<IMigrationIdGenerator>();
            VerifyScoped<IModelDiffer>();
            VerifyScoped<IMigrationSqlGenerator>();
        }

        protected override IServiceCollection GetServices(IServiceCollection services = null)
        {
            return (services ?? new ServiceCollection())
                .AddEntityFramework()
                .AddSqlServer()
                .ServiceCollection();
        }

        protected override EntityOptions GetOptions()
        {
            return SqlServerTestHelpers.Instance.CreateOptions();
        }

        protected override DbContext CreateContext(IServiceProvider serviceProvider)
        {
            return SqlServerTestHelpers.Instance.CreateContext(serviceProvider);
        }
    }
}
