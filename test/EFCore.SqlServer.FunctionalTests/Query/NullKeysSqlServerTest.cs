// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under t
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class NullKeysSqlServerTest : NullKeysTestBase<NullKeysSqlServerTest.NullKeysSqlServerFixture>
    {
        public NullKeysSqlServerTest(NullKeysSqlServerFixture fixture)
            : base(fixture)
        {
        }

        public class NullKeysSqlServerFixture : NullKeysFixtureBase
        {
            protected override ITestStoreFactory<TestStore> TestStoreFactory => SqlServerTestStoreFactory.Instance;
        }
    }
}
