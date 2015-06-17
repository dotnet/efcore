// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
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

namespace Microsoft.Data.Entity.SqlServer.Tests
{
    public class SqlServerEntityFrameworkServicesBuilderExtensionsTest : EntityFrameworkServiceCollectionExtensionsTest
    {
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

            // Migrations
            VerifyScoped<IMigrationAssembly>();
            VerifyScoped<IHistoryRepository>();
            VerifyScoped<IMigrator>();
            VerifySingleton<IMigrationIdGenerator>();
            VerifyScoped<IModelDiffer>();
            VerifyScoped<IMigrationSqlGenerator>();
        }

        protected override string AddProviderMethodName => "AddSqlServer";

        public SqlServerEntityFrameworkServicesBuilderExtensionsTest()
            : base(SqlServerTestHelpers.Instance)
        {
        }
    }
}
