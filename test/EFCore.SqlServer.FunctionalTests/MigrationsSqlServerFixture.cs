// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore
{
    public class MigrationsSqlServerFixture : MigrationsFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory => SqlServerTestStoreFactory.Instance;

        public MigrationsSqlServerFixture()
        {
            ((SqlServerTestStore)TestStore).ExecuteNonQuery(
                @"USE master
IF EXISTS(select * from sys.databases where name='TransactionSuppressed')
DROP DATABASE TransactionSuppressed");
        }

        public override MigrationsContext CreateContext()
        {
            var options = AddOptions(
                    new DbContextOptionsBuilder()
                        .UseSqlServer(TestStore.ConnectionString, b => b.ApplyConfiguration()))
                .UseInternalServiceProvider(ServiceProvider)
                .Options;
            return new MigrationsContext(options);
        }
    }
}
