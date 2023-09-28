// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

public class GrpcSqlServerTest : GrpcTestBase<GrpcSqlServerTest.GrpcSqlServerFixture>
{
    public GrpcSqlServerTest(GrpcSqlServerFixture fixture)
        : base(fixture)
    {
    }

    public class GrpcSqlServerFixture : GrpcFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => SqlServerTestStoreFactory.Instance;
    }
}
