// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.SqlServer.Design.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore;

public class DesignTimeSqlServerTest : DesignTimeTestBase<DesignTimeSqlServerTest.DesignTimeSqlServerFixture>
{
    public DesignTimeSqlServerTest(DesignTimeSqlServerFixture fixture)
        : base(fixture)
    {
    }

    protected override Assembly ProviderAssembly
        => typeof(SqlServerDesignTimeServices).Assembly;

    public class DesignTimeSqlServerFixture : DesignTimeFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => SqlServerTestStoreFactory.Instance;
    }
}
