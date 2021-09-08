// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore
{
    public class GraphUpdatesSqliteChangedChangingNotificationsTest
        : GraphUpdatesSqliteTestBase<GraphUpdatesSqliteChangedChangingNotificationsTest.SqliteFixture>
    {
        public GraphUpdatesSqliteChangedChangingNotificationsTest(SqliteFixture fixture)
            : base(fixture)
        {
        }

        protected override void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
            => facade.UseTransaction(transaction.GetDbTransaction());

        public class SqliteFixture : GraphUpdatesSqliteFixtureBase
        {
            protected override string StoreName { get; } = "GraphUpdatesChangedChangingTest";

            protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
            {
                modelBuilder.HasChangeTrackingStrategy(ChangeTrackingStrategy.ChangingAndChangedNotifications);

                base.OnModelCreating(modelBuilder, context);
            }
        }
    }
}
