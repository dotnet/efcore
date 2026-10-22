// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

#nullable disable

public class LoadSqliteTest(LoadSqliteTest.LoadSqliteFixture fixture) : LoadTestBase<LoadSqliteTest.LoadSqliteFixture>(fixture)
{
    public class LoadSqliteFixture : LoadFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => SqliteTestStoreFactory.Instance;
    }
}
