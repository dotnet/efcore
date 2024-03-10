// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

#nullable disable

public class GraphUpdatesSqlServerSequenceTest(GraphUpdatesSqlServerSequenceTest.SqlServerFixture fixture) : GraphUpdatesSqlServerTestBase<GraphUpdatesSqlServerSequenceTest.SqlServerFixture>(fixture)
{
    protected override void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
        => facade.UseTransaction(transaction.GetDbTransaction());

    public class SqlServerFixture : GraphUpdatesSqlServerFixtureBase
    {
        protected override string StoreName
            => "GraphSequenceUpdatesTest";

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            modelBuilder.UseKeySequences();

            base.OnModelCreating(modelBuilder, context);
        }
    }
}
