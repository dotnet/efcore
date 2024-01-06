// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

#if !EXCLUDE_ON_MAC

public class GrpcSqliteTest(GrpcSqliteTest.GrpcSqliteFixture fixture) : GrpcTestBase<GrpcSqliteTest.GrpcSqliteFixture>(fixture)
{
    public class GrpcSqliteFixture : GrpcFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => SqliteTestStoreFactory.Instance;
    }
}

#endif
