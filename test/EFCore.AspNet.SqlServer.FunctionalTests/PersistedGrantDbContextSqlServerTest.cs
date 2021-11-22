// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

public class PersistedGrantDbContextSqlServerTest
    : PersistedGrantDbContextTestBase<PersistedGrantDbContextSqlServerTest.PersistedGrantDbContextSqlServerFixture>
{
    public PersistedGrantDbContextSqlServerTest(PersistedGrantDbContextSqlServerFixture fixture)
        : base(fixture)
    {
    }

    public class PersistedGrantDbContextSqlServerFixture : PersistedGrantDbContextFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => SqlServerTestStoreFactory.Instance;

        protected override string StoreName
            => "PersistedGrantDbContext";
    }
}
