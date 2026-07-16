// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.SqlServer.Design.Internal;

namespace Microsoft.EntityFrameworkCore;

public class RuntimeMigrationSqlServerTest(RuntimeMigrationSqlServerTest.RuntimeMigrationSqlServerFixture fixture)
    : RuntimeMigrationTestBase<RuntimeMigrationSqlServerTest.RuntimeMigrationSqlServerFixture>(fixture)
{
    protected override Assembly ProviderAssembly
        => typeof(SqlServerDesignTimeServices).Assembly;

    public class RuntimeMigrationSqlServerFixture : RuntimeMigrationFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => SqlServerTestStoreFactory.Instance;
    }
}
