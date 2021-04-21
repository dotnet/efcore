// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore
{
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
}
