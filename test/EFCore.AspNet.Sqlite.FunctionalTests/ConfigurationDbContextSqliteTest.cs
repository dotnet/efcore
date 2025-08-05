// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

public class ConfigurationDbContextSqliteTest(ConfigurationDbContextSqliteTest.ConfigurationDbContextSqliteFixture fixture)
    : ConfigurationDbContextTestBase<ConfigurationDbContextSqliteTest.ConfigurationDbContextSqliteFixture>(fixture)
{
    public class ConfigurationDbContextSqliteFixture : ConfigurationDbContextFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => SqliteTestStoreFactory.Instance;

        protected override string StoreName
            => "ConfigurationDbContext";
    }
}
