// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;

#pragma warning disable RCS1102 // Make class static.
namespace Microsoft.EntityFrameworkCore
{
    public class ProxyGraphUpdatesInMemoryTest
    {
        public abstract class ProxyGraphUpdatesInMemoryTestBase<TFixture> : ProxyGraphUpdatesTestBase<TFixture>
            where TFixture : ProxyGraphUpdatesInMemoryTestBase<TFixture>.ProxyGraphUpdatesInMemoryFixtureBase, new()
        {
            protected ProxyGraphUpdatesInMemoryTestBase(TFixture fixture)
                : base(fixture)
            {
            }

            // #11552
            public override void Save_required_one_to_one_changed_by_reference(ChangeMechanism changeMechanism)
            {
            }

            public override void Optional_one_to_one_relationships_are_one_to_one()
            {
            }

            public override void Optional_one_to_one_with_AK_relationships_are_one_to_one()
            {
            }

            public override void Optional_many_to_one_dependents_with_alternate_key_are_orphaned_in_store(
                CascadeTiming cascadeDeleteTiming,
                CascadeTiming deleteOrphansTiming)
            {
            }

            public override void Optional_many_to_one_dependents_are_orphaned_in_store(
                CascadeTiming cascadeDeleteTiming,
                CascadeTiming deleteOrphansTiming)
            {
            }

            public override void Required_one_to_one_are_cascade_detached_when_Added(
                CascadeTiming cascadeDeleteTiming,
                CascadeTiming deleteOrphansTiming)
            {
            }

            public override void Required_one_to_one_relationships_are_one_to_one()
            {
            }

            public override void Required_one_to_one_with_AK_relationships_are_one_to_one()
            {
            }

            public override void Required_one_to_one_with_alternate_key_are_cascade_detached_when_Added(
                CascadeTiming cascadeDeleteTiming,
                CascadeTiming deleteOrphansTiming)
            {
            }

            public override void Required_one_to_one_with_alternate_key_are_cascade_deleted_in_store(
                CascadeTiming cascadeDeleteTiming,
                CascadeTiming deleteOrphansTiming)
            {
            }

            public override void Required_many_to_one_dependents_are_cascade_deleted_in_store(
                CascadeTiming cascadeDeleteTiming,
                CascadeTiming deleteOrphansTiming)
            {
            }

            public override void Required_many_to_one_dependents_with_alternate_key_are_cascade_deleted_in_store(
                CascadeTiming cascadeDeleteTiming,
                CascadeTiming deleteOrphansTiming)
            {
            }

            public override void Required_non_PK_one_to_one_are_cascade_detached_when_Added(
                CascadeTiming cascadeDeleteTiming,
                CascadeTiming deleteOrphansTiming)
            {
            }

            public override void Required_non_PK_one_to_one_with_alternate_key_are_cascade_detached_when_Added(
                CascadeTiming cascadeDeleteTiming,
                CascadeTiming deleteOrphansTiming)
            {
            }

            protected override void ExecuteWithStrategyInTransaction(
                Action<DbContext> testOperation,
                Action<DbContext> nestedTestOperation1 = null,
                Action<DbContext> nestedTestOperation2 = null,
                Action<DbContext> nestedTestOperation3 = null)
            {
                base.ExecuteWithStrategyInTransaction(testOperation, nestedTestOperation1, nestedTestOperation2, nestedTestOperation3);
                Fixture.Reseed();
            }

            public abstract class ProxyGraphUpdatesInMemoryFixtureBase : ProxyGraphUpdatesFixtureBase
            {
                protected override ITestStoreFactory TestStoreFactory
                    => InMemoryTestStoreFactory.Instance;

                public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
                    => base.AddOptions(builder.ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning)));
            }
        }

        public class LazyLoading : ProxyGraphUpdatesInMemoryTestBase<LazyLoading.ProxyGraphUpdatesWithLazyLoadingInMemoryFixture>
        {
            public LazyLoading(ProxyGraphUpdatesWithLazyLoadingInMemoryFixture fixture)
                : base(fixture)
            {
            }

            protected override bool DoesLazyLoading
                => true;

            protected override bool DoesChangeTracking
                => false;

            public class ProxyGraphUpdatesWithLazyLoadingInMemoryFixture : ProxyGraphUpdatesInMemoryFixtureBase
            {
                protected override string StoreName { get; } = "ProxyGraphLazyLoadingUpdatesTest";

                public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
                    => base.AddOptions(builder.UseLazyLoadingProxies());

                protected override IServiceCollection AddServices(IServiceCollection serviceCollection)
                    => base.AddServices(serviceCollection.AddEntityFrameworkProxies());
            }
        }

        public class ChangeTracking : ProxyGraphUpdatesInMemoryTestBase<ChangeTracking.ProxyGraphUpdatesWithChangeTrackingInMemoryFixture>
        {
            public ChangeTracking(ProxyGraphUpdatesWithChangeTrackingInMemoryFixture fixture)
                : base(fixture)
            {
            }

            protected override bool DoesLazyLoading
                => false;

            protected override bool DoesChangeTracking
                => true;

            public class ProxyGraphUpdatesWithChangeTrackingInMemoryFixture : ProxyGraphUpdatesInMemoryFixtureBase
            {
                protected override string StoreName { get; } = "ProxyGraphChangeTrackingUpdatesTest";

                public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
                    => base.AddOptions(builder.UseChangeTrackingProxies());

                protected override IServiceCollection AddServices(IServiceCollection serviceCollection)
                    => base.AddServices(serviceCollection.AddEntityFrameworkProxies());
            }
        }

        public class LazyLoadingAndChangeTracking : ProxyGraphUpdatesInMemoryTestBase<
            LazyLoadingAndChangeTracking.ProxyGraphUpdatesWithChangeTrackingInMemoryFixture>
        {
            public LazyLoadingAndChangeTracking(ProxyGraphUpdatesWithChangeTrackingInMemoryFixture fixture)
                : base(fixture)
            {
            }

            protected override bool DoesLazyLoading
                => true;

            protected override bool DoesChangeTracking
                => true;

            public class ProxyGraphUpdatesWithChangeTrackingInMemoryFixture : ProxyGraphUpdatesInMemoryFixtureBase
            {
                protected override string StoreName { get; } = "ProxyGraphLazyLoadingAndChangeTrackingUpdatesTest";

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
}
