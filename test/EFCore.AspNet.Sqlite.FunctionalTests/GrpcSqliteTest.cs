// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

public class GrpcSqliteTest : GrpcTestBase<GrpcSqliteTest.GrpcSqliteFixture>
{
    public GrpcSqliteTest(GrpcSqliteFixture fixture)
        : base(fixture)
    {
    }

    public class GrpcSqliteFixture : GrpcFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => SqliteTestStoreFactory.Instance;
    }
}
