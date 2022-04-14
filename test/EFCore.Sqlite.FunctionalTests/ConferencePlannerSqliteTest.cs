// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

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
        protected override ITestStoreFactory TestStoreFactory
            => SqliteTestStoreFactory.Instance;
    }
}
