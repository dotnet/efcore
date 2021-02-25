// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore
{
    public class AspNetIdentityCustomTypesIntKeySqliteTest
        : AspNetIdentityCustomTypesIntKeyTestBase<AspNetIdentityCustomTypesIntKeySqliteTest.AspNetIdentityCustomTypesIntKeySqliteFixture>
    {
        public AspNetIdentityCustomTypesIntKeySqliteTest(AspNetIdentityCustomTypesIntKeySqliteFixture fixture)
            : base(fixture)
        {
        }

        public class AspNetIdentityCustomTypesIntKeySqliteFixture : AspNetIdentityFixtureBase
        {
            public TestSqlLoggerFactory TestSqlLoggerFactory
                => (TestSqlLoggerFactory)ListLoggerFactory;

            protected override ITestStoreFactory TestStoreFactory
                => SqliteTestStoreFactory.Instance;

            protected override string StoreName
                => "AspNetCustomTypesIntKeyIdentity";
        }
    }
}
