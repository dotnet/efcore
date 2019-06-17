// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore
{
    public abstract class GraphUpdatesSqliteTest
    {
        public abstract class GraphUpdatesSqliteTestBase<TFixture> : GraphUpdatesTestBase<TFixture>
            where TFixture : GraphUpdatesSqliteTestBase<TFixture>.GraphUpdatesSqliteFixtureBase, new()
        {
            protected GraphUpdatesSqliteTestBase(TFixture fixture)
                : base(fixture)
            {
            }

            protected override void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
                => facade.UseTransaction(transaction.GetDbTransaction());

            public abstract class GraphUpdatesSqliteFixtureBase : GraphUpdatesFixtureBase
            {
                public TestSqlLoggerFactory TestSqlLoggerFactory => (TestSqlLoggerFactory)ListLoggerFactory;
                protected override ITestStoreFactory TestStoreFactory => SqliteTestStoreFactory.Instance;
                protected virtual bool AutoDetectChanges => false;

                public override PoolableDbContext CreateContext()
                {
                    var context = base.CreateContext();
                    context.ChangeTracker.AutoDetectChangesEnabled = AutoDetectChanges;

                    return context;
                }
            }
        }

        public class SnapshotNotifications
            : GraphUpdatesSqliteTestBase<SnapshotNotifications.SnapshotNotificationsFixture>
        {
            public SnapshotNotifications(SnapshotNotificationsFixture fixture, ITestOutputHelper testOutputHelper)
                : base(fixture)
            {
                //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
            }

            public class SnapshotNotificationsFixture : GraphUpdatesSqliteFixtureBase
            {
                protected override string StoreName { get; } = "GraphUpdatesSnapshotTest";
                protected override bool AutoDetectChanges => true;

                protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
                {
                    modelBuilder.HasChangeTrackingStrategy(ChangeTrackingStrategy.Snapshot);

                    base.OnModelCreating(modelBuilder, context);
                }
            }
        }

        public class ChangedNotifications
            : GraphUpdatesSqliteTestBase<ChangedNotifications.ChangedNotificationsFixture>
        {
            public ChangedNotifications(ChangedNotificationsFixture fixture)
                : base(fixture)
            {
            }

            public class ChangedNotificationsFixture : GraphUpdatesSqliteFixtureBase
            {
                protected override string StoreName { get; } = "GraphUpdatesChangedTest";

                protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
                {
                    modelBuilder.HasChangeTrackingStrategy(ChangeTrackingStrategy.ChangedNotifications);

                    base.OnModelCreating(modelBuilder, context);
                }
            }
        }

        public class ChangedChangingNotifications
            : GraphUpdatesSqliteTestBase<ChangedChangingNotifications.ChangedChangingNotificationsFixture>
        {
            public ChangedChangingNotifications(ChangedChangingNotificationsFixture fixture)
                : base(fixture)
            {
            }

            public class ChangedChangingNotificationsFixture : GraphUpdatesSqliteFixtureBase
            {
                protected override string StoreName { get; } = "GraphUpdatesFullTest";

                protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
                {
                    modelBuilder.HasChangeTrackingStrategy(ChangeTrackingStrategy.ChangingAndChangedNotifications);

                    base.OnModelCreating(modelBuilder, context);
                }
            }
        }

        public class FullWithOriginalsNotifications
            : GraphUpdatesSqliteTestBase<FullWithOriginalsNotifications.FullWithOriginalsNotificationsFixture>
        {
            public FullWithOriginalsNotifications(FullWithOriginalsNotificationsFixture fixture)
                : base(fixture)
            {
            }

            public class FullWithOriginalsNotificationsFixture : GraphUpdatesSqliteFixtureBase
            {
                protected override string StoreName { get; } = "GraphUpdatesOriginalsTest";

                protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
                {
                    modelBuilder.HasChangeTrackingStrategy(ChangeTrackingStrategy.ChangingAndChangedNotificationsWithOriginalValues);

                    base.OnModelCreating(modelBuilder, context);
                }
            }
        }
    }
}
