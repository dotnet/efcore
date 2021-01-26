// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore
{
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
}
