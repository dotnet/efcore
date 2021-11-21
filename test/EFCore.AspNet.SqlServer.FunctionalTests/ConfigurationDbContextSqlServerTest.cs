// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore;

public class ConfigurationDbContextSqlServerTest
    : ConfigurationDbContextTestBase<ConfigurationDbContextSqlServerTest.ConfigurationDbContextSqlServerFixture>
{
    public ConfigurationDbContextSqlServerTest(ConfigurationDbContextSqlServerFixture fixture)
        : base(fixture)
    {
    }

    public class ConfigurationDbContextSqlServerFixture : ConfigurationDbContextFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => SqlServerTestStoreFactory.Instance;

        protected override string StoreName
            => "ConfigurationDbContext";
    }
}
