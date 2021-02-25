// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore
{
    public class ManyToManyLoadSqliteTest
        : ManyToManyLoadSqliteTestBase<ManyToManyLoadSqliteTest.ManyToManyLoadSqliteFixture>
    {
        public ManyToManyLoadSqliteTest(ManyToManyLoadSqliteFixture fixture)
            : base(fixture)
        {
        }

        public class ManyToManyLoadSqliteFixture : ManyToManyLoadSqliteFixtureBase
        {
        }
    }
}
