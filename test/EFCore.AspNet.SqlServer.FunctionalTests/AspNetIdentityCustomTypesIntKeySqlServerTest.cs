// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

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
