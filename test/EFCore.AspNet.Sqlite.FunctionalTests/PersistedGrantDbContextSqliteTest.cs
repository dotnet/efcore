// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore;

public class PersistedGrantDbContextSqliteTest
    : PersistedGrantDbContextTestBase<PersistedGrantDbContextSqliteTest.PersistedGrantDbContextSqliteFixture>
{
    public PersistedGrantDbContextSqliteTest(PersistedGrantDbContextSqliteFixture fixture)
        : base(fixture)
    {
    }

    public class PersistedGrantDbContextSqliteFixture : PersistedGrantDbContextFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => SqliteTestStoreFactory.Instance;

        protected override string StoreName
            => "PersistedGrantDbContext";
    }
}
