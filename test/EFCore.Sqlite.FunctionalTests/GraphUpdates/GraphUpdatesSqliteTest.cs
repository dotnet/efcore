// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Linq;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore
{
    public class GraphUpdatesSqliteTest
    {
        public class ChangedChangingNotifications
            : GraphUpdatesTestBase<ChangedChangingNotifications.SqliteFixture>
        {
            public ChangedChangingNotifications(SqliteFixture fixture)
                : base(fixture)
            {
            }

            protected override void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
                => facade.UseTransaction(transaction.GetDbTransaction());

            public class SqliteFixture : GraphUpdatesSqliteTestBase<SqliteFixture>.GraphUpdatesSqliteFixtureBase
            {
                protected override string StoreName { get; } = "GraphUpdatesChangedChangingTest";

                protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
                {
                    modelBuilder.HasChangeTrackingStrategy(ChangeTrackingStrategy.ChangingAndChangedNotifications);

                    base.OnModelCreating(modelBuilder, context);
                }
            }
        }

        public class ChangedNotifications
            : GraphUpdatesTestBase<ChangedNotifications.SqliteFixture>
        {
            public ChangedNotifications(SqliteFixture fixture)
                : base(fixture)
            {
            }

            protected override void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
                => facade.UseTransaction(transaction.GetDbTransaction());

            public class SqliteFixture : GraphUpdatesSqliteTestBase<SqliteFixture>.GraphUpdatesSqliteFixtureBase
            {
                protected override string StoreName { get; } = "GraphUpdatesChangedTest";

                protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
                {
                    modelBuilder.HasChangeTrackingStrategy(ChangeTrackingStrategy.ChangedNotifications);

                    base.OnModelCreating(modelBuilder, context);
                }
            }
        }

        public class FullWithOriginalsNotifications
            : GraphUpdatesTestBase<FullWithOriginalsNotifications.SqliteFixture>
        {
            public FullWithOriginalsNotifications(SqliteFixture fixture)
                : base(fixture)
            {
            }

            protected override void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
                => facade.UseTransaction(transaction.GetDbTransaction());

            public class SqliteFixture : GraphUpdatesSqliteTestBase<SqliteFixture>.GraphUpdatesSqliteFixtureBase
            {
                protected override string StoreName { get; } = "GraphUpdatesFullWithOriginalsTest";

                protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
                {
                    modelBuilder.HasChangeTrackingStrategy(ChangeTrackingStrategy.ChangingAndChangedNotificationsWithOriginalValues);

                    base.OnModelCreating(modelBuilder, context);
                }
            }
        }

        public class SnapshotNotifications
            : GraphUpdatesTestBase<SnapshotNotifications.SqliteFixture>
        {
            public SnapshotNotifications(SqliteFixture fixture)
                : base(fixture)
            {
            }

            protected override void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
                => facade.UseTransaction(transaction.GetDbTransaction());

            public class SqliteFixture : GraphUpdatesSqliteTestBase<SqliteFixture>.GraphUpdatesSqliteFixtureBase
            {
                protected override string StoreName { get; } = "GraphUpdatesSnapshotTest";

                protected override bool AutoDetectChanges
                    => true;

                protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
                {
                    modelBuilder.HasChangeTrackingStrategy(ChangeTrackingStrategy.Snapshot);

                    base.OnModelCreating(modelBuilder, context);
                }
            }
        }

        public abstract class GraphUpdatesSqliteTestBase<TFixture> : GraphUpdatesTestBase<TFixture>
            where TFixture : GraphUpdatesSqliteTestBase<TFixture>.GraphUpdatesSqliteFixtureBase, new()
        {
            protected GraphUpdatesSqliteTestBase(TFixture fixture)
                : base(fixture)
            {
            }

            protected override IQueryable<Root> ModifyQueryRoot(IQueryable<Root> query)
                => query.AsSplitQuery();

            protected override void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
                => facade.UseTransaction(transaction.GetDbTransaction());

            public abstract class GraphUpdatesSqliteFixtureBase : GraphUpdatesFixtureBase
            {
                public TestSqlLoggerFactory TestSqlLoggerFactory
                    => (TestSqlLoggerFactory)ListLoggerFactory;

                protected override ITestStoreFactory TestStoreFactory
                    => SqliteTestStoreFactory.Instance;

                protected virtual bool AutoDetectChanges
                    => false;

                public override PoolableDbContext CreateContext()
                {
                    var context = base.CreateContext();
                    context.ChangeTracker.AutoDetectChangesEnabled = AutoDetectChanges;

                    return context;
                }
            }
        }
    }
}
