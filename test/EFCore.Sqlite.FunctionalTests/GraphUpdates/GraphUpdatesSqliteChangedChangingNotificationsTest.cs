// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

#nullable disable

public class GraphUpdatesSqliteChangedChangingNotificationsTest(GraphUpdatesSqliteChangedChangingNotificationsTest.SqliteFixture fixture)
    : GraphUpdatesSqliteTestBase<GraphUpdatesSqliteChangedChangingNotificationsTest.SqliteFixture>(fixture)
{
    protected override void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
        => facade.UseTransaction(transaction.GetDbTransaction());

    public class SqliteFixture : GraphUpdatesSqliteFixtureBase
    {
        protected override string StoreName
            => "GraphUpdatesChangedChangingTest";

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            modelBuilder.HasChangeTrackingStrategy(ChangeTrackingStrategy.ChangingAndChangedNotifications);

            base.OnModelCreating(modelBuilder, context);
        }
    }
}
