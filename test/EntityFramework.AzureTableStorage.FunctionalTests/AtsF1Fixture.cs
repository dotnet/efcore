// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.FunctionalTests;
using Microsoft.Data.Entity.FunctionalTests.TestModels.ConcurrencyModel;
using Microsoft.Data.Entity.Identity;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.DependencyInjection.Fallback;

namespace Microsoft.Data.Entity.AzureTableStorage.FunctionalTests
{
    public class AtsF1Fixture : F1FixtureBase<AtsTestStore>, IDisposable
    {
        private readonly IServiceProvider _serviceProvider;

        private readonly string _tableSuffix = Guid.NewGuid().ToString().Replace("-", "");

        public AtsF1Fixture()
        {
            _serviceProvider = new ServiceCollection()
                .AddEntityFramework()
                .AddAzureTableStorage()
                .ServiceCollection
                .AddSingleton<AtsValueGeneratorCache, TestValueGeneratorCache>()
                .AddTestModelSource(OnModelCreating)
                .BuildServiceProvider();
        }

        public override AtsTestStore CreateTestStore()
        {
            var store = new AtsTestStore(_tableSuffix);
            using (var context = CreateContext(store))
            {
                if (!(context.Database.EnsureCreated()))
                {
                    ConcurrencyModelInitializer.Cleanup(context);
                }
                ConcurrencyModelInitializer.Seed(context);
            }

            store.CleanupAction = () =>
                {
                    using (var context = CreateContext(store))
                    {
                        ConcurrencyModelInitializer.Cleanup(context);
                    }
                };

            return store;
        }

        public override F1Context CreateContext(AtsTestStore testStore)
        {
            var options = new DbContextOptions()
                .UseAzureTableStorage(testStore.ConnectionString);

            return new F1Context(_serviceProvider, options);
        }

        public void Dispose()
        {
            var testStore = CreateTestStore();
            using (var context = CreateContext(testStore))
            {
                context.Database.EnsureDeleted();
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Chassis>(
                b =>
                {
                    b.ForAzureTableStorage().PartitionAndRowKey(c => c.Name, c => c.TeamId);
                    b.Key(c => c.TeamId);
                    b.Property<string>("ETag");
                    b.ForAzureTableStorage().Table("Chassis" + _tableSuffix);
                });

            modelBuilder.Entity<Team>(
                b =>
                {
                    b.ForAzureTableStorage().PartitionAndRowKey(c => c.Name, c => c.Id);
                    b.Property(c => c.Id).GenerateValueOnAdd();
                    b.Key(c => c.Id);
                    b.Property<string>("ETag");
                    b.ForAzureTableStorage().Table("Teams" + _tableSuffix);
                });

            modelBuilder.Entity<Driver>(
                b =>
                {
                    b.ForAzureTableStorage().PartitionAndRowKey(c => c.Name, c => c.Id);
                    b.Property(c => c.Id).GenerateValueOnAdd();
                    b.Key(c => c.Id);
                    b.Property<string>("ETag");
                    b.ForAzureTableStorage().Table("Drivers" + _tableSuffix);
                });

            modelBuilder.Entity<Engine>(
                b =>
                {
                    b.ForAzureTableStorage().PartitionAndRowKey(c => c.Name, c => c.Id);
                    b.Property(c => c.Id).GenerateValueOnAdd();
                    b.Key(c => c.Id);
                    b.Property<string>("ETag");
                    b.ForAzureTableStorage().Table("Engines" + _tableSuffix);
                });

            modelBuilder.Entity<EngineSupplier>(
                b =>
                {
                    b.ForAzureTableStorage().PartitionAndRowKey(c => c.Name, c => c.Id);
                    b.Property(c => c.Id).GenerateValueOnAdd();
                    b.Key(c => c.Id);
                    b.Property<string>("ETag");
                    b.ForAzureTableStorage().Table("EngineSuppliers" + _tableSuffix);
                });

            modelBuilder.Entity<Gearbox>(
                b =>
                {
                    b.ForAzureTableStorage().PartitionAndRowKey(c => c.Name, c => c.Id);
                    b.Property(c => c.Id).GenerateValueOnAdd();
                    b.Key(c => c.Id);
                    b.Property<string>("ETag");
                    b.ForAzureTableStorage().Table("Gearboxes" + _tableSuffix);
                });

            modelBuilder.Entity<Sponsor>(
                b =>
                {
                    b.ForAzureTableStorage().PartitionAndRowKey(c => c.Name, c => c.Id);
                    b.Property(c => c.Id).GenerateValueOnAdd();
                    b.Key(c => c.Id);
                    b.Property<string>("ETag");
                    b.ForAzureTableStorage().Table("Sponsors" + _tableSuffix);
                });

            modelBuilder.Entity<TestDriver>(
                b =>
                {
                    // TODO: Remove this configuration when inheritance is available
                    b.ForAzureTableStorage().PartitionAndRowKey(c => c.Name, c => c.Id);
                    b.Property(c => c.Id).GenerateValueOnAdd();
                    b.Key(c => c.Id);
                    b.Property<string>("ETag");
                    b.ForAzureTableStorage().Table("TestDrivers" + _tableSuffix);
                });

            modelBuilder.Entity<TitleSponsor>(
                b =>
                {
                    b.ForAzureTableStorage().PartitionAndRowKey(c => c.Name, c => c.Id);
                    b.Key(c => c.Id);
                    b.Property<string>("ETag");
                    b.ForAzureTableStorage().Table("TitleSponsors" + _tableSuffix);
                });

            base.OnModelCreating(modelBuilder);
        }

        // Added as a hack to get tests working correctly
        // TODO consider adding this to the provider
        // Issue #961
        private class TestValueGeneratorCache : AtsValueGeneratorCache
        {
            private class IntGenerator : SimpleValueGenerator
            {
                public override void Next(StateEntry stateEntry, IProperty property)
                {
                    stateEntry[property] = Guid.NewGuid().GetHashCode();
                }
            }

            public override IValueGenerator GetGenerator(IProperty property)
            {
                return new IntGenerator();
            }
        }
    }
}
