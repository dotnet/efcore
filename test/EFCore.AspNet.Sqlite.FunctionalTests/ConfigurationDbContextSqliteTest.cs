// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore;

public class ConfigurationDbContextSqliteTest
    : ConfigurationDbContextTestBase<ConfigurationDbContextSqliteTest.ConfigurationDbContextSqliteFixture>
{
    public ConfigurationDbContextSqliteTest(ConfigurationDbContextSqliteFixture fixture)
        : base(fixture)
    {
    }

    public class ConfigurationDbContextSqliteFixture : ConfigurationDbContextFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => SqliteTestStoreFactory.Instance;

        protected override string StoreName
            => "ConfigurationDbContext";
    }
}
