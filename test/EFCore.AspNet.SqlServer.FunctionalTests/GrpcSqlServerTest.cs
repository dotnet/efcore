// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore
{
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
}
