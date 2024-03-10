// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.SqlServer.Design.Internal;

namespace Microsoft.EntityFrameworkCore;

#nullable disable

public class DesignTimeSqlServerTest(DesignTimeSqlServerTest.DesignTimeSqlServerFixture fixture) : DesignTimeTestBase<DesignTimeSqlServerTest.DesignTimeSqlServerFixture>(fixture)
{
    protected override Assembly ProviderAssembly
        => typeof(SqlServerDesignTimeServices).Assembly;

    public class DesignTimeSqlServerFixture : DesignTimeFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => SqlServerTestStoreFactory.Instance;
    }
}
