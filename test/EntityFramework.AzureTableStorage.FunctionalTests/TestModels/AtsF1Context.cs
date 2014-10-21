// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.FunctionalTests.TestModels.ConcurrencyModel;
using Microsoft.Data.Entity.Identity;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.DependencyInjection.Fallback;

namespace Microsoft.Data.Entity.AzureTableStorage.FunctionalTests.TestModels
{
    public class AtsF1Context : F1Context
    {
        private readonly string _tableSuffix;

        public AtsF1Context(IServiceProvider serviceProvider, DbContextOptions options, string tableSuffix)
            : base(serviceProvider, options)
        {
            _tableSuffix = tableSuffix;
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
                        b.Property(c => c.Id).GenerateValuesOnAdd();
                        b.Key(c => c.Id);
                        b.Property<string>("ETag");
                        b.ForAzureTableStorage().Table("Teams" + _tableSuffix);
                    });

            modelBuilder.Entity<Driver>(
                b =>
                    {
                        b.ForAzureTableStorage().PartitionAndRowKey(c => c.Name, c => c.Id);
                        b.Property(c => c.Id).GenerateValuesOnAdd();
                        b.Key(c => c.Id);
                        b.Property<string>("ETag");
                        b.ForAzureTableStorage().Table("Drivers" + _tableSuffix);
                    });

            modelBuilder.Entity<Engine>(
                b =>
                    {
                        b.ForAzureTableStorage().PartitionAndRowKey(c => c.Name, c => c.Id);
                        b.Property(c => c.Id).GenerateValuesOnAdd();
                        b.Key(c => c.Id);
                        b.Property<string>("ETag");
                        b.ForAzureTableStorage().Table("Engines" + _tableSuffix);
                    });

            modelBuilder.Entity<EngineSupplier>(
                b =>
                    {
                        b.ForAzureTableStorage().PartitionAndRowKey(c => c.Name, c => c.Id);
                        b.Property(c => c.Id).GenerateValuesOnAdd();
                        b.Key(c => c.Id);
                        b.Property<string>("ETag");
                        b.ForAzureTableStorage().Table("EngineSuppliers" + _tableSuffix);
                    });

            modelBuilder.Entity<Gearbox>(
                b =>
                    {
                        b.ForAzureTableStorage().PartitionAndRowKey(c => c.Name, c => c.Id);
                        b.Property(c => c.Id).GenerateValuesOnAdd();
                        b.Key(c => c.Id);
                        b.Property<string>("ETag");
                        b.ForAzureTableStorage().Table("Gearboxes" + _tableSuffix);
                    });

            modelBuilder.Entity<Sponsor>(
                b =>
                    {
                        b.ForAzureTableStorage().PartitionAndRowKey(c => c.Name, c => c.Id);
                        b.Property(c => c.Id).GenerateValuesOnAdd();
                        b.Key(c => c.Id);
                        b.Property<string>("ETag");
                        b.ForAzureTableStorage().Table("Sponsors" + _tableSuffix);
                    });

            modelBuilder.Entity<TestDriver>(
                b =>
                    {
                        // TODO: Remove this configuration when inheritance is available
                        b.ForAzureTableStorage().PartitionAndRowKey(c => c.Name, c => c.Id);
                        b.Property(c => c.Id).GenerateValuesOnAdd();
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

        public static async Task<AtsTestStore> CreateMutableTestStoreAsync()
        {
            var store = new AtsTestStore();
            var context = Create(store);
            await ConcurrencyModelInitializer.SeedAsync(context);

            store.ContextForDeletion = context;

            return store;
        }

        public static F1Context Create(AtsTestStore testStore)
        {
            var serviceProvider = new ServiceCollection()
                .AddEntityFramework()
                .AddAzureTableStorage()
                .ServiceCollection
                .AddSingleton<AtsValueGeneratorCache, TestValueGeneratorCache>()
                .BuildServiceProvider();

            var options = new DbContextOptions()
                .UseAzureTableStorage(TestConfig.Instance.ConnectionString);

            return new AtsF1Context(serviceProvider, options, testStore.TableSuffix);
        }

        // Added as a hack to get tests working correctly
        // TODO consider adding this to the provider
        // Issue #961
        internal class TestValueGeneratorCache : AtsValueGeneratorCache
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
