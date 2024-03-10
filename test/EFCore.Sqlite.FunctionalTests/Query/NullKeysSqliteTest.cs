// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public class NullKeysSqliteTest(NullKeysSqliteTest.NullKeysSqliteFixture fixture) : NullKeysTestBase<NullKeysSqliteTest.NullKeysSqliteFixture>(fixture)
{
    public class NullKeysSqliteFixture : NullKeysFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => SqliteTestStoreFactory.Instance;
    }
}
