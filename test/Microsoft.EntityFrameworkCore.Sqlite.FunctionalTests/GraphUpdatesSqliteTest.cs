// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Specification.Tests;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Sqlite.FunctionalTests
{
    public abstract class GraphUpdatesSqliteTest
    {
        public abstract class GraphUpdatesSqliteTestBase<TFixture> : GraphUpdatesTestBase<SqliteTestStore, TFixture>
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
                protected abstract string DatabaseName { get; }

                protected abstract bool AutoDetectChanges { get; }

                private readonly IServiceProvider _serviceProvider;

                protected GraphUpdatesSqliteFixtureBase()
                {
                    _serviceProvider = new ServiceCollection()
                        .AddEntityFrameworkSqlite()
                        .AddSingleton(TestSqliteModelSource.GetFactory(OnModelCreating))
                        .BuildServiceProvider();
                }

                public override SqliteTestStore CreateTestStore()
                {
                    return SqliteTestStore.GetOrCreateShared(DatabaseName, () =>
                        {
                            var optionsBuilder = new DbContextOptionsBuilder()
                                .UseSqlite(SqliteTestStore.CreateConnectionString(DatabaseName))
                                .UseInternalServiceProvider(_serviceProvider);

                            using (var context = new GraphUpdatesContext(optionsBuilder.Options))
                            {
                                context.Database.EnsureClean();
                                Seed(context);
                            }
                        });
                }

                public override DbContext CreateContext(SqliteTestStore testStore)
                {
                    var optionsBuilder = new DbContextOptionsBuilder()
                        .UseSqlite(testStore.Connection)
                        .UseInternalServiceProvider(_serviceProvider);

                    var context = new GraphUpdatesContext(optionsBuilder.Options);
                    context.Database.UseTransaction(testStore.Transaction);

                    context.ChangeTracker.AutoDetectChangesEnabled = AutoDetectChanges;

                    return context;
                }
            }
        }

        public class SnapshotNotificationsTest
            : GraphUpdatesSqliteTestBase<SnapshotNotificationsTest.SnapshotNotificationsFixture>
        {
            public SnapshotNotificationsTest(SnapshotNotificationsFixture fixture)
                : base(fixture)
            {
            }

            public class SnapshotNotificationsFixture : GraphUpdatesSqliteFixtureBase
            {
                protected override string DatabaseName => "GraphUpdatesSnapshotTest";

                protected override bool AutoDetectChanges => true;

                protected override void OnModelCreating(ModelBuilder modelBuilder)
                {
                    modelBuilder.HasChangeTrackingStrategy(ChangeTrackingStrategy.Snapshot);

                    base.OnModelCreating(modelBuilder);
                }
            }
        }

        public class ChangedNotificationsTest
            : GraphUpdatesSqliteTestBase<ChangedNotificationsTest.ChangedNotificationsFixture>
        {
            public ChangedNotificationsTest(ChangedNotificationsFixture fixture)
                : base(fixture)
            {
            }

            public class ChangedNotificationsFixture : GraphUpdatesSqliteFixtureBase
            {
                protected override string DatabaseName => "GraphUpdatesChangedTest";

                protected override bool AutoDetectChanges => false;

                protected override void OnModelCreating(ModelBuilder modelBuilder)
                {
                    modelBuilder.HasChangeTrackingStrategy(ChangeTrackingStrategy.ChangedNotifications);

                    base.OnModelCreating(modelBuilder);
                }
            }
        }

        public class ChangedChangingNotificationsTest
            : GraphUpdatesSqliteTestBase<ChangedChangingNotificationsTest.ChangedChangingNotificationsFixture>
        {
            public ChangedChangingNotificationsTest(ChangedChangingNotificationsFixture fixture)
                : base(fixture)
            {
            }

            public class ChangedChangingNotificationsFixture : GraphUpdatesSqliteFixtureBase
            {
                protected override string DatabaseName => "GraphUpdatesFullTest";

                protected override bool AutoDetectChanges => false;

                protected override void OnModelCreating(ModelBuilder modelBuilder)
                {
                    modelBuilder.HasChangeTrackingStrategy(ChangeTrackingStrategy.ChangingAndChangedNotifications);

                    base.OnModelCreating(modelBuilder);
                }
            }
        }

        public class FullWithOriginalsNotificationsTest
            : GraphUpdatesSqliteTestBase<FullWithOriginalsNotificationsTest.FullWithOriginalsNotificationsFixture>
        {
            public FullWithOriginalsNotificationsTest(FullWithOriginalsNotificationsFixture fixture)
                : base(fixture)
            {
            }

            public class FullWithOriginalsNotificationsFixture : GraphUpdatesSqliteFixtureBase
            {
                protected override string DatabaseName => "GraphUpdatesOriginalsTest";

                protected override bool AutoDetectChanges => false;

                protected override void OnModelCreating(ModelBuilder modelBuilder)
                {
                    modelBuilder.HasChangeTrackingStrategy(ChangeTrackingStrategy.ChangingAndChangedNotificationsWithOriginalValues);

                    base.OnModelCreating(modelBuilder);
                }
            }
        }
    }
}
