// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public class NullKeysSqlServerTest(NullKeysSqlServerTest.NullKeysSqlServerFixture fixture) : NullKeysTestBase<NullKeysSqlServerTest.NullKeysSqlServerFixture>(fixture)
{
    public class NullKeysSqlServerFixture : NullKeysFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => SqlServerTestStoreFactory.Instance;
    }
}
