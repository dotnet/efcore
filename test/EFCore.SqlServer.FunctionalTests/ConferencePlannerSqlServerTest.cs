// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore;

public class ConferencePlannerSqlServerTest : ConferencePlannerTestBase<ConferencePlannerSqlServerTest.ConferencePlannerSqlServerFixture
>
{
    public ConferencePlannerSqlServerTest(ConferencePlannerSqlServerFixture fixture)
        : base(fixture)
    {
    }

    protected override void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
        => facade.UseTransaction(transaction.GetDbTransaction());

    public class ConferencePlannerSqlServerFixture : ConferencePlannerFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => SqlServerTestStoreFactory.Instance;
    }
}
