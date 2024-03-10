// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

#nullable disable

public class ConferencePlannerSqlServerTest(ConferencePlannerSqlServerTest.ConferencePlannerSqlServerFixture fixture) : ConferencePlannerTestBase<ConferencePlannerSqlServerTest.ConferencePlannerSqlServerFixture
>(fixture)
{
    protected override void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
        => facade.UseTransaction(transaction.GetDbTransaction());

    public class ConferencePlannerSqlServerFixture : ConferencePlannerFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => SqlServerTestStoreFactory.Instance;
    }
}
