// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.FunctionalTests;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.DependencyInjection.Fallback;

namespace Microsoft.Data.Entity.AzureTableStorage.FunctionalTests
{
    public class AtsBuiltInDataTypesFixture : BuiltInDataTypesFixtureBase<AtsTestStore>, IDisposable
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly string _tableSuffix = Guid.NewGuid().ToString().Replace("-", "");

        public AtsBuiltInDataTypesFixture()
        {
            _serviceProvider = new ServiceCollection()
                .AddEntityFramework()
                .AddAzureTableStorage()
                .ServiceCollection
                .BuildServiceProvider();
        }

        public override AtsTestStore CreateTestStore()
        {
            var store = new AtsTestStore(_tableSuffix);
            using (var context = CreateContext(store))
            {
                context.Database.EnsureCreated();
            }

            store.CleanupAction = () =>
            {
                using (var context = CreateContext(store))
                {
                    Cleanup(context);
                }
            };

            return store;
        }

        public override DbContext CreateContext(AtsTestStore testStore)
        {
            var options = new DbContextOptions()
                .UseModel(CreateModel())
                .UseAzureTableStorage(testStore.ConnectionString);

            return new DbContext(_serviceProvider, options);
        }

        public override void OnModelCreating(BasicModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<BuiltInNonNullableDataTypes>(b =>
                {
                    b.ForAzureTableStorage(ab =>
                        {
                            ab.PartitionAndRowKey(dt => dt.Id0, dt => dt.Id1);
                            ab.Timestamp("Timestamp", true);
                            ab.Table("BuiltInNonNullableDataTypes" + _tableSuffix);
                        });

                    b.Key(dt => dt.Id0); // See issue #632
                });

            modelBuilder.Entity<BuiltInNullableDataTypes>(b =>
                {
                    b.ForAzureTableStorage(ab =>
                        {
                            ab.PartitionAndRowKey(dt => dt.Id0, dt => dt.Id1);
                            ab.Timestamp("Timestamp", true);
                            ab.Table("BuiltInNullableDataTypes" + _tableSuffix);
                        });

                    b.Key(dt => dt.Id0); // See issue #632
                });
        }

        public void Dispose()
        {
            var testStore = CreateTestStore();
            using (var context = CreateContext(testStore))
            {
                context.Database.EnsureDeleted();
            }
        }
    }
}
