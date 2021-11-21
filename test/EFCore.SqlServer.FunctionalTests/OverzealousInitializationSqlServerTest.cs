// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore;

public class OverzealousInitializationSqlServerTest
    : OverzealousInitializationTestBase<OverzealousInitializationSqlServerTest.OverzealousInitializationSqlServerFixture>
{
    public OverzealousInitializationSqlServerTest(OverzealousInitializationSqlServerFixture fixture)
        : base(fixture)
    {
    }

    public class OverzealousInitializationSqlServerFixture : OverzealousInitializationFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => SqlServerTestStoreFactory.Instance;
    }
}
