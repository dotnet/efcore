// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Internal;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators.Internal;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.EntityFrameworkCore.Tests;
using Microsoft.EntityFrameworkCore.Update.Internal;
using Microsoft.EntityFrameworkCore.ValueGeneration.Internal;

namespace Microsoft.EntityFrameworkCore
{
    public class SqlServerEntityFrameworkServicesBuilderExtensionsTest : RelationalEntityFrameworkServicesBuilderExtensionsTest
    {
        public override void Services_wire_up_correctly()
        {
            base.Services_wire_up_correctly();

            // SQL Server dingletones
            VerifySingleton<ISqlServerValueGeneratorCache>();
            VerifySingleton<SqlServerTypeMapper>();
            VerifySingleton<SqlServerModelSource>();
            VerifySingleton<SqlServerAnnotationProvider>();
            VerifySingleton<SqlServerMigrationsAnnotationProvider>();

            // SQL Server scoped
            VerifyScoped<SqlServerConventionSetBuilder>();
            VerifyScoped<ISqlServerUpdateSqlGenerator>();
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
        }

        public SqlServerEntityFrameworkServicesBuilderExtensionsTest()
            : base(SqlServerTestHelpers.Instance)
        {
        }
    }
}
