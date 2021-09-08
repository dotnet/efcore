// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore
{
    public class GraphUpdatesSqliteChangedNotificationsTest
        : GraphUpdatesSqliteTestBase<GraphUpdatesSqliteChangedNotificationsTest.SqliteFixture>
    {
        public GraphUpdatesSqliteChangedNotificationsTest(SqliteFixture fixture)
            : base(fixture)
        {
        }

        protected override void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
            => facade.UseTransaction(transaction.GetDbTransaction());

        public class SqliteFixture : GraphUpdatesSqliteFixtureBase
        {
            protected override string StoreName { get; } = "GraphUpdatesChangedTest";

            protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
            {
                modelBuilder.HasChangeTrackingStrategy(ChangeTrackingStrategy.ChangedNotifications);

                base.OnModelCreating(modelBuilder, context);
            }
        }
    }
}
