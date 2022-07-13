// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

public class NullKeysSqlServerTest : NullKeysTestBase<NullKeysSqlServerTest.NullKeysSqlServerFixture>
{
    public NullKeysSqlServerTest(NullKeysSqlServerFixture fixture)
        : base(fixture)
    {
    }

    public class NullKeysSqlServerFixture : NullKeysFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => SqlServerTestStoreFactory.Instance;
    }
}
