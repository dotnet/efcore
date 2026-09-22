// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

public class AspNetIdentityIntKeySqliteTest(AspNetIdentityIntKeySqliteTest.AspNetIdentityIntKeySqliteFixture fixture)
    : AspNetIdentityIntKeyTestBase<AspNetIdentityIntKeySqliteTest.AspNetIdentityIntKeySqliteFixture>(fixture)
{
    public class AspNetIdentityIntKeySqliteFixture : AspNetIdentityFixtureBase
    {
        public TestSqlLoggerFactory TestSqlLoggerFactory
            => (TestSqlLoggerFactory)ListLoggerFactory;

        protected override ITestStoreFactory TestStoreFactory
            => SqliteTestStoreFactory.Instance;

        protected override string StoreName
            => "AspNetIntKeyIdentity";
    }
}
