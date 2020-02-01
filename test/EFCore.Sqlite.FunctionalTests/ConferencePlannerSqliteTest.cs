// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore
{
    public class ConferencePlannerSqliteTest : ConferencePlannerTestBase<ConferencePlannerSqliteTest.ConferencePlannerSqliteFixture>
    {
        public ConferencePlannerSqliteTest(ConferencePlannerSqliteFixture fixture)
            : base(fixture)
        {
        }

        protected override void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
            => facade.UseTransaction(transaction.GetDbTransaction());

        public class ConferencePlannerSqliteFixture : ConferencePlannerFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory => SqliteTestStoreFactory.Instance;
        }
    }
}
