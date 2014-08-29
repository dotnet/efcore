// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.Data.Entity.AzureTableStorage.Metadata;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.FunctionalTests;
using Microsoft.Data.Entity.FunctionalTests.TestModels.ConcurrencyModel;
using Microsoft.Data.Entity.Identity;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Storage;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.DependencyInjection.Fallback;

namespace Microsoft.Data.Entity.AzureTableStorage.FunctionalTests
{
    [RunIfConfigured]
    public class AtsOptimisticConcurrencyTests : OptimisticConcurrencyTestBase<AtsOptimisticConcurrencyTests.AtsTestStore>, IDisposable
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IModel _model;

        public AtsOptimisticConcurrencyTests()
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
            using (var context = CreateF1Context(db))
            {
                await ConcurrencyModelInitializer.SeedAsync(context);
            }

            return db;
        }

        protected override DataStoreTransaction BeginTransaction(F1Context context, AtsTestStore testStore, Action<F1Context> prepareStore)
        {
            return new AtsTransaction();
        }

        protected override void ResolveConcurrencyTokens(StateEntry stateEntry)
        {
            var property = stateEntry.EntityType.GetProperty("ETag");
            stateEntry[property] = "*";
            //TODO use the actual ETag instead of force rewrite. This will require refactoring the test base to read shadow state properties
        }

        protected override F1Context CreateF1Context(AtsTestStore testStore)
        {
            var options = new DbContextOptions()
                .UseModel(_model)
                .UseAzureTableStorage(TestConfig.Instance.ConnectionString);
            return new F1Context(_serviceProvider, options);
        }

        private static IModel AddAtsMetadata(ModelBuilder builder, string tableSuffix)
        {
            builder.Entity<Chassis>(
                b =>
                    {
                        b.PartitionAndRowKey(c => c.Name, c => c.TeamId);
                        b.Key(c => c.TeamId);
                        b.Property<string>("ETag");
                        b.TableName("Chassis" + tableSuffix);
                    });

            builder.Entity<Team>(
                b =>
                    {
                        b.PartitionAndRowKey(c => c.Name, c => c.Id);
                        b.Key(c => c.Id);
                        b.Property<string>("ETag");
                        b.TableName("Teams" + tableSuffix);
                    });

            builder.Entity<Driver>(
                b =>
                    {
                        b.PartitionAndRowKey(c => c.Name, c => c.Id);
                        b.Key(c => c.Id);
                        b.Property<string>("ETag");
                        b.TableName("Drivers" + tableSuffix);
                    });

            builder.Entity<Engine>(
                b =>
                    {
                        b.PartitionAndRowKey(c => c.Name, c => c.Id);
                        b.Key(c => c.Id);
                        b.Property<string>("ETag");
                        b.TableName("Engines" + tableSuffix);
                    });

            builder.Entity<EngineSupplier>(
                b =>
                    {
                        b.PartitionAndRowKey(c => c.Name, c => c.Id);
                        b.Key(c => c.Id);
                        b.Property<string>("ETag");
                        b.TableName("EngineSuppliers" + tableSuffix);
                    });

            builder.Entity<Gearbox>(
                b =>
                    {
                        b.PartitionAndRowKey(c => c.Name, c => c.Id);
                        b.Key(c => c.Id);
                        b.Property<string>("ETag");
                        b.TableName("Gearboxes" + tableSuffix);
                    });

            builder.Entity<Sponsor>(
                b =>
                    {
                        b.PartitionAndRowKey(c => c.Name, c => c.Id);
                        b.Key(c => c.Id);
                        b.Property<string>("ETag");
                        b.TableName("Sponsors" + tableSuffix);
                    });

            builder.Entity<TestDriver>(
                b =>
                    {
                        b.PartitionAndRowKey(c => c.Name, c => c.Id);
                        b.Key(c => c.Id);
                        b.Property<string>("ETag");
                        b.TableName("TestDrivers" + tableSuffix);
                    });

            builder.Entity<TitleSponsor>(
                b =>
                    {
                        b.PartitionAndRowKey(c => c.Name, c => c.Id);
                        b.Key(c => c.Id);
                        b.Property<string>("ETag");
                        b.TableName("TitleSponsors" + tableSuffix);
                    });

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
            using (var db = CreateF1Context(new AtsTestStore()))
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

        internal class AtsTransaction : DataStoreTransaction
        {
            public override void Commit()
            {
            }

            public override void Rollback()
            {
            }

            public override void Dispose()
            {
            }
        }
    }
}
