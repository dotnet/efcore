// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using ConcurrencyModel;
using Microsoft.Data.Entity.AzureTableStorage.Metadata;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.FunctionalTests;
using Microsoft.Data.Entity.Identity;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.DependencyInjection.Fallback;

namespace Microsoft.Data.Entity.AzureTableStorage.FunctionalTests
{
    [RunIfConfigured]
    public class OptimisticConcurrencyTests : OptimisticConcurrencyTestBase<OptimisticConcurrencyTests.AtsTestStore>, IDisposable
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IModel _model;

        public OptimisticConcurrencyTests()
        {
            var tableSuffix = Guid.NewGuid().ToString().Replace("-", "");
            _model = AddAtsMetadata(F1Context.CreateModel(), tableSuffix);

            _serviceProvider = new ServiceCollection()
                .AddEntityFramework()
                .AddAzureTableStorage()
                .ServiceCollection
                .AddScoped<AtsValueGeneratorCache, TestValueGeneratorCache>()
                .BuildServiceProvider();
        }

        protected override async Task<AtsTestStore> CreateTestDatabaseAsync()
        {
            var db = new AtsTestStore();
            using (var context = await CreateF1ContextAsync(db))
            {
                await ConcurrencyModelInitializer.SeedAsync(context);
            }
            return db;
        }

        protected override void ResolveConcurrencyTokens(StateEntry stateEntry)
        {
            var property = stateEntry.EntityType.GetProperty("ETag");
            stateEntry[property] = "*";
            //TODO use the actual ETag instead of force rewrite. This will require refactoring the test base to read shadow state properties
        }

        protected override Task<F1Context> CreateF1ContextAsync(AtsTestStore testDatabase)
        {
            var options = new DbContextOptions()
                .UseModel(_model)
                .UseAzureTableStorage(TestConfig.Instance.ConnectionString);
            return Task.FromResult(new F1Context(_serviceProvider, options));
        }

        private static IModel AddAtsMetadata(ModelBuilder builder, string tableSuffix)
        {
            builder.Entity<Chassis>()
                .PartitionAndRowKey(c => c.Name, c => c.TeamId)
                .Key(c => c.TeamId)
                .Properties(pb => pb.Property<Chassis>("ETag", true))
                .TableName("Chassis" + tableSuffix);
            builder.Entity<Team>()
                .PartitionAndRowKey(c => c.Name, c => c.Id)
                .Key(c => c.Id)
                .Properties(pb => pb.Property<Team>("ETag", true))
                .TableName("Teams" + tableSuffix);
            builder.Entity<Driver>()
                .PartitionAndRowKey(c => c.Name, c => c.Id)
                .Key(c => c.Id)
                .Properties(pb => pb.Property<Driver>("ETag", true))
                .TableName("Drivers" + tableSuffix);
            builder.Entity<Engine>()
                .PartitionAndRowKey(c => c.Name, c => c.Id)
                .Key(c => c.Id)
                .Properties(pb => pb.Property<Engine>("ETag", true))
                .TableName("Engines" + tableSuffix);
            builder.Entity<EngineSupplier>()
                .PartitionAndRowKey(c => c.Name, c => c.Id)
                .Key(c => c.Id)
                .Properties(pb => pb.Property<EngineSupplier>("ETag", true))
                .TableName("EngineSuppliers" + tableSuffix);
            builder.Entity<Gearbox>()
                .PartitionAndRowKey(c => c.Name, c => c.Id)
                .Key(c => c.Id)
                .Properties(pb => pb.Property<Gearbox>("ETag", true))
                .TableName("Gearboxes" + tableSuffix);
            builder.Entity<Sponsor>()
                .PartitionAndRowKey(c => c.Name, c => c.Id)
                .Key(c => c.Id)
                .Properties(pb => pb.Property<Sponsor>("ETag", true))
                .TableName("Sponsors" + tableSuffix);
            builder.Entity<TestDriver>()
                .PartitionAndRowKey(c => c.Name, c => c.Id)
                .Key(c => c.Id)
                .Properties(pb => pb.Property<TestDriver>("ETag", true))
                .TableName("TestDrivers" + tableSuffix);
            builder.Entity<TitleSponsor>()
                .PartitionAndRowKey(c => c.Name, c => c.Id)
                .Key(c => c.Id)
                .Properties(pb => pb.Property<TitleSponsor>("ETag", true))
                .TableName("TitleSponsors" + tableSuffix);
            return builder.Model;
        }

        public class AtsTestStore : TestStore
        {
            public override void Dispose()
            {
            }
        }

        public void Dispose()
        {
            using (var db = CreateF1ContextAsync(null).Result)
            {
                db.Database.EnsureDeleted();
            }
        }

        // Added as a hack to get these tests working correctly
        //TODO consider adding this to the provider

        internal class TestValueGeneratorCache : AtsValueGeneratorCache
        {
            private class IntGenerator : SimpleValueGenerator
            {
                public override object Next(StateEntry entry, IProperty property)
                {
                    return Guid.NewGuid().GetHashCode();
                }
            }

            public override IValueGenerator GetGenerator(IProperty property)
            {
                return new IntGenerator();
            }
        }
    }
}
