// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

#nullable disable

public class OverzealousInitializationSqliteTest(OverzealousInitializationSqliteTest.OverzealousInitializationSqliteFixture fixture)
    : OverzealousInitializationTestBase<OverzealousInitializationSqliteTest.OverzealousInitializationSqliteFixture>(fixture)
{
    public class OverzealousInitializationSqliteFixture : OverzealousInitializationFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => SqliteTestStoreFactory.Instance;
    }
}
