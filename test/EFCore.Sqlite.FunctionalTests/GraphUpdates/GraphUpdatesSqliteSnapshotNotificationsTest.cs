// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

#nullable disable

public class GraphUpdatesSqliteSnapshotNotificationsTest(GraphUpdatesSqliteSnapshotNotificationsTest.SqliteFixture fixture)
    : GraphUpdatesSqliteTestBase<GraphUpdatesSqliteSnapshotNotificationsTest.SqliteFixture>(fixture)
{
    protected override void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
        => facade.UseTransaction(transaction.GetDbTransaction());

    public class SqliteFixture : GraphUpdatesSqliteFixtureBase
    {
        protected override string StoreName
            => "GraphUpdatesSnapshotTest";

        public override bool AutoDetectChanges
            => true;

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            modelBuilder.HasChangeTrackingStrategy(ChangeTrackingStrategy.Snapshot);

            base.OnModelCreating(modelBuilder, context);
        }
    }
}
