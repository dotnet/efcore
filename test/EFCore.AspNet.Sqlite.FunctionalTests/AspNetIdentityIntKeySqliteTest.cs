// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore;

public class AspNetIdentityIntKeySqliteTest
    : AspNetIdentityIntKeyTestBase<AspNetIdentityIntKeySqliteTest.AspNetIdentityIntKeySqliteFixture>
{
    public AspNetIdentityIntKeySqliteTest(AspNetIdentityIntKeySqliteFixture fixture)
        : base(fixture)
    {
    }

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
