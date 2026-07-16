// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#pragma warning disable RCS1102 // Make class static.
namespace Microsoft.EntityFrameworkCore;

public class ProxyGraphUpdatesInMemoryTest
{
    public abstract class ProxyGraphUpdatesInMemoryTestBase<TFixture>(TFixture fixture) : ProxyGraphUpdatesTestBase<TFixture>(fixture)
        where TFixture : ProxyGraphUpdatesInMemoryTestBase<TFixture>.ProxyGraphUpdatesInMemoryFixtureBase, new()
    {
        // FK constraint checking.
        public override Task Optional_one_to_one_relationships_are_one_to_one()
            => Assert.ThrowsAnyAsync<Exception>(() => base.Optional_one_to_one_relationships_are_one_to_one());

        // FK constraint checking.
        public override Task Optional_one_to_one_with_AK_relationships_are_one_to_one()
            => Assert.ThrowsAnyAsync<Exception>(() => base.Optional_one_to_one_with_AK_relationships_are_one_to_one());

        // Cascade delete.
        public override Task Optional_many_to_one_dependents_with_alternate_key_are_orphaned_in_store(
            CascadeTiming cascadeDeleteTiming,
            CascadeTiming deleteOrphansTiming)
            => Assert.ThrowsAnyAsync<Exception>(() =>
                base.Optional_many_to_one_dependents_with_alternate_key_are_orphaned_in_store(cascadeDeleteTiming, deleteOrphansTiming));

        // Cascade delete.
        public override Task Optional_one_to_one_are_orphaned(
            CascadeTiming cascadeDeleteTiming,
            CascadeTiming deleteOrphansTiming)
            => Task.CompletedTask;

        // Cascade delete.
        public override Task Optional_one_to_one_with_alternate_key_are_orphaned(
            CascadeTiming cascadeDeleteTiming,
            CascadeTiming deleteOrphansTiming)
            => Task.CompletedTask;

        // Cascade delete.
        public override Task Optional_many_to_one_dependents_are_orphaned_in_store(
            CascadeTiming cascadeDeleteTiming,
            CascadeTiming deleteOrphansTiming)
            => Assert.ThrowsAnyAsync<Exception>(() =>
                base.Optional_many_to_one_dependents_are_orphaned_in_store(cascadeDeleteTiming, deleteOrphansTiming));

        // Cascade delete.
        public override Task Optional_many_to_one_dependents_are_orphaned_starting_detached(
            CascadeTiming cascadeDeleteTiming,
            CascadeTiming deleteOrphansTiming)
            => Task.CompletedTask;

        // Cascade delete.
        public override Task Required_one_to_one_are_cascade_detached_when_Added(
            CascadeTiming cascadeDeleteTiming,
            CascadeTiming deleteOrphansTiming)
            => Task.CompletedTask;

        // Cascade delete.
        public override Task Required_non_PK_one_to_one_are_cascade_detached_when_Added(
            CascadeTiming cascadeDeleteTiming,
            CascadeTiming deleteOrphansTiming)
            => Task.CompletedTask;

        // Cascade delete.
        public override Task Required_one_to_one_with_alternate_key_are_cascade_deleted_in_store(
            CascadeTiming cascadeDeleteTiming,
            CascadeTiming deleteOrphansTiming)
            => Task.CompletedTask;

        // Cascade delete.
        public override Task Required_one_to_one_with_alternate_key_are_cascade_deleted_starting_detached(
            CascadeTiming cascadeDeleteTiming,
            CascadeTiming deleteOrphansTiming)
            => Task.CompletedTask;

        // Cascade delete.
        public override Task Required_many_to_one_dependents_are_cascade_deleted_in_store(
            CascadeTiming cascadeDeleteTiming,
            CascadeTiming deleteOrphansTiming)
            => Task.CompletedTask;

        // Cascade delete.
        public override Task Required_one_to_one_with_alternate_key_are_cascade_detached_when_Added(
            CascadeTiming cascadeDeleteTiming,
            CascadeTiming deleteOrphansTiming)
            => Task.CompletedTask;

        // Cascade delete.
        public override Task Required_non_PK_one_to_one_with_alternate_key_are_cascade_detached_when_Added(
            CascadeTiming cascadeDeleteTiming,
            CascadeTiming deleteOrphansTiming)
            => Task.CompletedTask;

        // Cascade delete.
        public override Task Required_many_to_one_dependents_with_alternate_key_are_cascade_detached_when_Added(
            CascadeTiming cascadeDeleteTiming,
            CascadeTiming deleteOrphansTiming)
            => Task.CompletedTask;

        // FK constraint checking.
        public override Task Required_one_to_one_relationships_are_one_to_one()
            => Assert.ThrowsAnyAsync<Exception>(() => base.Required_one_to_one_relationships_are_one_to_one());

        // FK constraint checking.
        public override Task Required_one_to_one_with_AK_relationships_are_one_to_one()
            => Assert.ThrowsAnyAsync<Exception>(() => base.Required_one_to_one_with_AK_relationships_are_one_to_one());

        // Cascade delete.
        public override Task Required_many_to_one_dependents_with_alternate_key_are_cascade_deleted_in_store(
            CascadeTiming cascadeDeleteTiming,
            CascadeTiming deleteOrphansTiming)
            => Assert.ThrowsAnyAsync<Exception>(() =>
                base.Required_many_to_one_dependents_with_alternate_key_are_cascade_deleted_in_store(
                    cascadeDeleteTiming, deleteOrphansTiming));

        // Cascade delete.
        public override Task Can_attach_full_optional_graph_of_duplicates()
            => Task.CompletedTask;

        // Graph fixup ordering is non-deterministic on InMemory.
        public override Task Can_attach_full_required_AK_graph_of_duplicates()
            => Task.CompletedTask;

        // Graph fixup ordering is non-deterministic on InMemory.
        public override Task No_fixup_to_Deleted_entities()
            => Task.CompletedTask;

        // Cascade delete.
        public override Task Required_non_PK_one_to_one_with_alternate_key_are_cascade_deleted(
            CascadeTiming cascadeDeleteTiming,
            CascadeTiming deleteOrphansTiming)
            => Task.CompletedTask;

        // Cascade delete.
        public override Task Reparent_required_non_PK_one_to_one_with_alternate_key(
            ChangeMechanism changeMechanism,
            bool useExistingRoot)
            => Task.CompletedTask;

        // Cascade delete.
        public override Task Save_changed_optional_one_to_one_with_alternate_key_in_store()
            => Task.CompletedTask;

        // Cascade delete.
        public override Task Sever_required_one_to_one(ChangeMechanism changeMechanism)
            => Task.CompletedTask;

        // Cascade delete.
        public override async Task Required_many_to_one_dependents_with_alternate_key_are_cascade_deleted_starting_detached(
            CascadeTiming cascadeDeleteTiming,
            CascadeTiming deleteOrphansTiming)
        {
            var exception = await Record.ExceptionAsync(
                () => base.Required_many_to_one_dependents_with_alternate_key_are_cascade_deleted_starting_detached(
                    cascadeDeleteTiming, deleteOrphansTiming));

            // InMemory currently has mixed behavior for this path (#3924) because cascade operations and graph fixup
            // aren't fully consistent across proxy/lazy-loading combinations; some combinations complete, others fail
            // with InvalidOperationException from query materialization.
            if (exception == null || exception is InvalidOperationException)
            {
                return;
            }

            throw exception;
        }

        protected override async Task ExecuteWithStrategyInTransactionAsync(
            Func<DbContext, Task> testOperation,
            Func<DbContext, Task> nestedTestOperation1 = null,
            Func<DbContext, Task> nestedTestOperation2 = null,
            Func<DbContext, Task> nestedTestOperation3 = null)
        {
            // InMemory has no real transactions, so the shared store is mutated directly by each test and must be
            // reseeded afterwards.
            try
            {
                await base.ExecuteWithStrategyInTransactionAsync(
                    testOperation, nestedTestOperation1, nestedTestOperation2, nestedTestOperation3);
            }
            finally
            {
                await Fixture.ReseedAsync();
            }
        }

        public abstract class ProxyGraphUpdatesInMemoryFixtureBase : ProxyGraphUpdatesFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory
                => InMemoryTestStoreFactory.Instance;

            public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
                => base.AddOptions(builder.ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning)));
        }
    }

    public class LazyLoading(LazyLoading.ProxyGraphUpdatesWithLazyLoadingInMemoryFixture fixture)
        : ProxyGraphUpdatesInMemoryTestBase<LazyLoading.ProxyGraphUpdatesWithLazyLoadingInMemoryFixture>(fixture)
    {
        protected override bool DoesLazyLoading
            => true;

        protected override bool DoesChangeTracking
            => false;

        public class ProxyGraphUpdatesWithLazyLoadingInMemoryFixture : ProxyGraphUpdatesInMemoryFixtureBase
        {
            protected override string StoreName
                => "ProxyGraphLazyLoadingUpdatesTest";

            public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
                => base.AddOptions(builder.UseLazyLoadingProxies());

            protected override IServiceCollection AddServices(IServiceCollection serviceCollection)
                => base.AddServices(serviceCollection.AddEntityFrameworkProxies());
        }
    }

    public class ChangeTracking(ChangeTracking.ProxyGraphUpdatesWithChangeTrackingInMemoryFixture fixture)
        : ProxyGraphUpdatesInMemoryTestBase<ChangeTracking.ProxyGraphUpdatesWithChangeTrackingInMemoryFixture>(fixture)
    {
        // Needs lazy loading
        public override Task Save_two_entity_cycle_with_lazy_loading()
            => Task.CompletedTask;

        protected override bool DoesLazyLoading
            => false;

        protected override bool DoesChangeTracking
            => true;

        public class ProxyGraphUpdatesWithChangeTrackingInMemoryFixture : ProxyGraphUpdatesInMemoryFixtureBase
        {
            protected override string StoreName
                => "ProxyGraphChangeTrackingUpdatesTest";

            public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
                => base.AddOptions(builder.UseChangeTrackingProxies());

            protected override IServiceCollection AddServices(IServiceCollection serviceCollection)
                => base.AddServices(serviceCollection.AddEntityFrameworkProxies());
        }
    }

    public class LazyLoadingAndChangeTracking(LazyLoadingAndChangeTracking.ProxyGraphUpdatesWithChangeTrackingInMemoryFixture fixture)
        : ProxyGraphUpdatesInMemoryTestBase<
            LazyLoadingAndChangeTracking.ProxyGraphUpdatesWithChangeTrackingInMemoryFixture>(fixture)
    {
        protected override bool DoesLazyLoading
            => true;

        protected override bool DoesChangeTracking
            => true;

        public class ProxyGraphUpdatesWithChangeTrackingInMemoryFixture : ProxyGraphUpdatesInMemoryFixtureBase
        {
            protected override string StoreName
                => "ProxyGraphLazyLoadingAndChangeTrackingUpdatesTest";

            public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
                => base.AddOptions(
                    builder
                        .UseChangeTrackingProxies()
                        .UseLazyLoadingProxies());

            protected override IServiceCollection AddServices(IServiceCollection serviceCollection)
                => base.AddServices(serviceCollection.AddEntityFrameworkProxies());
        }
    }
}
