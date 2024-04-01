// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

#nullable disable

public class GraphUpdatesSqlServerIdentityTest(GraphUpdatesSqlServerIdentityTest.SqlServerFixture fixture) : GraphUpdatesSqlServerTestBase<GraphUpdatesSqlServerIdentityTest.SqlServerFixture>(fixture)
{
    protected override void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
        => facade.UseTransaction(transaction.GetDbTransaction());

    public class SqlServerFixture : GraphUpdatesSqlServerFixtureBase
    {
        protected override string StoreName
            => "GraphIdentityUpdatesTest";

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            modelBuilder.UseIdentityColumns();

            base.OnModelCreating(modelBuilder, context);
        }
    }
}
