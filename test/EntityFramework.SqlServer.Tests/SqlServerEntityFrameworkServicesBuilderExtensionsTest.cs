// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.SqlServer.Metadata;
using Microsoft.Data.Entity.SqlServer.Migrations;
using Microsoft.Data.Entity.SqlServer.Update;
using Microsoft.Data.Entity.SqlServer.ValueGeneration;
using Microsoft.Data.Entity.Tests;
using Microsoft.Data.Entity.Update;

namespace Microsoft.Data.Entity.SqlServer.Tests
{
    public class SqlServerEntityFrameworkServicesBuilderExtensionsTest : RelationalEntityFrameworkServicesBuilderExtensionsTest
    {
        public override void Services_wire_up_correctly()
        {
            base.Services_wire_up_correctly();

            // SQL Server dingletones
            VerifySingleton<SqlServerConventionSetBuilder>();
            VerifySingleton<ISqlServerValueGeneratorCache>();
            VerifySingleton<ISqlServerUpdateSqlGenerator>();
            VerifySingleton<SqlServerTypeMapper>();
            VerifySingleton<SqlServerModelSource>();
            VerifySingleton<SqlServerMetadataExtensionProvider>();
            VerifySingleton<SqlServerMigrationAnnotationProvider>();

            // SQL Server scoped
            VerifyScoped<ISqlServerSequenceValueGeneratorFactory>();
            VerifyScoped<SqlServerModificationCommandBatchFactory>();
            VerifyScoped<SqlServerValueGeneratorSelector>();
            VerifyScoped<SqlServerDatabaseProviderServices>();
            VerifyScoped<SqlServerDatabase>();
            VerifyScoped<ISqlServerConnection>();
            VerifyScoped<SqlServerMigrationSqlGenerator>();
            VerifyScoped<SqlServerDatabaseCreator>();
            VerifyScoped<SqlServerHistoryRepository>();
            VerifyScoped<SqlServerCompositeMethodCallTranslator>();
            VerifyScoped<SqlServerCompositeMemberTranslator>();
        }

        public SqlServerEntityFrameworkServicesBuilderExtensionsTest()
            : base(SqlServerTestHelpers.Instance)
        {
        }
    }
}
