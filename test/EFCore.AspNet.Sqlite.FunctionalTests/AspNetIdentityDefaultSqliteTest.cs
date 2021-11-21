// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore;

public class AspNetIdentityDefaultSqliteTest
    : AspNetIdentityDefaultTestBase<AspNetIdentityDefaultSqliteTest.AspNetDefaultIdentitySqliteFixture>
{
    public AspNetIdentityDefaultSqliteTest(AspNetDefaultIdentitySqliteFixture fixture)
        : base(fixture)
    {
    }

    public class AspNetDefaultIdentitySqliteFixture : AspNetIdentityFixtureBase
    {
        public TestSqlLoggerFactory TestSqlLoggerFactory
            => (TestSqlLoggerFactory)ListLoggerFactory;

        protected override ITestStoreFactory TestStoreFactory
            => SqliteTestStoreFactory.Instance;

        protected override string StoreName
            => "AspNetDefaultIdentity";
    }
}
