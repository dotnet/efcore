// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

#if !EXCLUDE_ON_MAC

public class GrpcSqlServerTest(GrpcSqlServerTest.GrpcSqlServerFixture fixture) : GrpcTestBase<GrpcSqlServerTest.GrpcSqlServerFixture>(fixture)
{
    public class GrpcSqlServerFixture : GrpcFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => SqlServerTestStoreFactory.Instance;
    }
}

#endif
