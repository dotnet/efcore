// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore
{
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
}
