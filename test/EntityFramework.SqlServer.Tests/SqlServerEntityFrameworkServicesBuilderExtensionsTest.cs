// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Migrations;
using Microsoft.Data.Entity.Query;
using Microsoft.Data.Entity.SqlServer;
using Microsoft.Data.Entity.SqlServer.Metadata;
using Microsoft.Data.Entity.SqlServer.Query.ExpressionTranslators;
using Microsoft.Data.Entity.SqlServer.Update;
using Microsoft.Data.Entity.SqlServer.ValueGeneration;
using Microsoft.Data.Entity.Tests;
using Microsoft.Data.Entity.Update;

namespace Microsoft.Data.Entity
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
            VerifySingleton<SqlServerMigrationsAnnotationProvider>();

            // SQL Server scoped
            VerifyScoped<ISqlServerSequenceValueGeneratorFactory>();
            VerifyScoped<SqlServerModificationCommandBatchFactory>();
            VerifyScoped<SqlServerValueGeneratorSelector>();
            VerifyScoped<SqlServerDatabaseProviderServices>();
            VerifyScoped<ISqlServerConnection>();
            VerifyScoped<SqlServerMigrationsSqlGenerator>();
            VerifyScoped<SqlServerDatabaseCreator>();
            VerifyScoped<SqlServerHistoryRepository>();
            VerifyScoped<SqlServerCompositeMethodCallTranslator>();
            VerifyScoped<SqlServerCompositeMemberTranslator>();
            VerifyScoped<SqlServerCompositeExpressionFragmentTranslator>();
        }

        public SqlServerEntityFrameworkServicesBuilderExtensionsTest()
            : base(SqlServerTestHelpers.Instance)
        {
        }
    }
}
