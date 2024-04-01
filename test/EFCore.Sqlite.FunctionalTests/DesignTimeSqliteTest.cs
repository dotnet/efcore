// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Sqlite.Design.Internal;

namespace Microsoft.EntityFrameworkCore;

#nullable disable

public class DesignTimeSqliteTest(DesignTimeSqliteTest.DesignTimeSqliteFixture fixture) : DesignTimeTestBase<DesignTimeSqliteTest.DesignTimeSqliteFixture>(fixture)
{
    protected override Assembly ProviderAssembly
        => typeof(SqliteDesignTimeServices).Assembly;

    public class DesignTimeSqliteFixture : DesignTimeFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => SqliteTestStoreFactory.Instance;
    }
}
