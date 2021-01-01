// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore
{
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
}
