// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class NullKeysSqliteTest : NullKeysTestBase<NullKeysSqliteTest.NullKeysSqliteFixture>
    {
        public NullKeysSqliteTest(NullKeysSqliteFixture fixture)
            : base(fixture)
        {
        }

        public class NullKeysSqliteFixture : NullKeysFixtureBase
        {
            protected override ITestStoreFactory<TestStore> TestStoreFactory => SqliteTestStoreFactory.Instance;
        }
    }
}
