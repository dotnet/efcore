// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

public class PersistedGrantDbContextSqliteTest(PersistedGrantDbContextSqliteTest.PersistedGrantDbContextSqliteFixture fixture)
    : PersistedGrantDbContextTestBase<PersistedGrantDbContextSqliteTest.PersistedGrantDbContextSqliteFixture>(fixture)
{
    public class PersistedGrantDbContextSqliteFixture : PersistedGrantDbContextFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => SqliteTestStoreFactory.Instance;

        protected override string StoreName
            => "PersistedGrantDbContext";
    }
}
