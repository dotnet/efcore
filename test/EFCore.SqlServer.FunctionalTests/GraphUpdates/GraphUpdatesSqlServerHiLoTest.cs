// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

#nullable disable

public class GraphUpdatesSqlServerHiLoTest(GraphUpdatesSqlServerHiLoTest.SqlServerFixture fixture) : GraphUpdatesSqlServerTestBase<GraphUpdatesSqlServerHiLoTest.SqlServerFixture>(fixture)
{
    protected override void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
        => facade.UseTransaction(transaction.GetDbTransaction());

    public class SqlServerFixture : GraphUpdatesSqlServerFixtureBase
    {
        protected override string StoreName
            => "GraphHiLoUpdatesTest";

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            modelBuilder.UseHiLo();

            base.OnModelCreating(modelBuilder, context);
        }
    }
}
