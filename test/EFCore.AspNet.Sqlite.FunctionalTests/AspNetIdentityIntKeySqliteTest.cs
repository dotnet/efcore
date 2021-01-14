// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore
{
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
}
