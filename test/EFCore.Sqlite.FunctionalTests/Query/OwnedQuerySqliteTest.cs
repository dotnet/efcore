// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class OwnedQuerySqliteTest : RelationalOwnedQueryTestBase<OwnedQuerySqliteTest.OwnedQuerySqliteFixture>
    {
        public OwnedQuerySqliteTest(OwnedQuerySqliteFixture fixture)
            : base(fixture)
        {
        }

        public class OwnedQuerySqliteFixture : RelationalOwnedQueryFixture
        {
            protected override ITestStoreFactory TestStoreFactory => SqliteTestStoreFactory.Instance;
        }
    }
}
