// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore
{
    public class AspNetIdentityIntKeySqlServerTest
        : AspNetIdentityIntKeyTestBase<AspNetIdentityIntKeySqlServerTest.AspNetIdentityIntKeySqlServerFixture>
    {
        public AspNetIdentityIntKeySqlServerTest(AspNetIdentityIntKeySqlServerFixture fixture)
            : base(fixture)
        {
        }

        public class AspNetIdentityIntKeySqlServerFixture : AspNetIdentityFixtureBase
        {
            public TestSqlLoggerFactory TestSqlLoggerFactory
                => (TestSqlLoggerFactory)ListLoggerFactory;

            protected override ITestStoreFactory TestStoreFactory
                => SqlServerTestStoreFactory.Instance;

            protected override string StoreName
                => "AspNetIntKeyIdentity";
        }
    }
}
