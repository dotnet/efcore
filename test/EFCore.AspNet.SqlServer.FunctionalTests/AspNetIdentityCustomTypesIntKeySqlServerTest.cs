// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore
{
    public class AspNetIdentityCustomTypesIntKeySqlServerTest
        : AspNetIdentityCustomTypesIntKeyTestBase<
            AspNetIdentityCustomTypesIntKeySqlServerTest.AspNetIdentityCustomTypesIntKeySqlServerFixture>
    {
        public AspNetIdentityCustomTypesIntKeySqlServerTest(AspNetIdentityCustomTypesIntKeySqlServerFixture fixture)
            : base(fixture)
        {
        }

        public class AspNetIdentityCustomTypesIntKeySqlServerFixture : AspNetIdentityFixtureBase
        {
            public TestSqlLoggerFactory TestSqlLoggerFactory
                => (TestSqlLoggerFactory)ListLoggerFactory;

            protected override ITestStoreFactory TestStoreFactory
                => SqlServerTestStoreFactory.Instance;

            protected override string StoreName
                => "AspNetCustomTypesIntKeyIdentity";
        }
    }
}
