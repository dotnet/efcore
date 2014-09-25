// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.AzureTableStorage.Metadata;
using Microsoft.Data.Entity.FunctionalTests;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.DependencyInjection.Advanced;
using Microsoft.Framework.DependencyInjection.Fallback;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.AzureTableStorage.FunctionalTests
{
    public class BuiltInDataTypesFixture : BuiltInDataTypesFixtureBase, IDisposable
    {
        private DbContextOptions _options;
        private IServiceProvider _serviceProvider;

        public BuiltInDataTypesFixture()
        {
            _options = new DbContextOptions()
                .UseModel(CreateModel())
                .UseAzureTableStorage(TestConfig.Instance.ConnectionString, batchRequests: false);

            var services = new ServiceCollection();
            services.AddEntityFramework().UseLoggerFactory(TestFileLogger.Factory).AddAzureTableStorage();
            _serviceProvider = services.BuildServiceProvider();

            using (var context = new DbContext(_serviceProvider, _options))
            {
                context.Database.EnsureCreated();
            }
        }

        public override DbContext CreateContext()
        {
            return new DbContext(_serviceProvider, _options);
        }

        public override IModel CreateModel()
        {
            var model = (Model)base.CreateModel();
            var builder = new BasicModelBuilder(model);
            builder.Entity<BuiltInNonNullableDataTypes>(b =>
            {
                b.PartitionAndRowKey(dt => dt.Id0, dt => dt.Id1);
                b.Timestamp("Timestamp", true);
                b.Key(dt => dt.Id0); // See issue #632
            });

            builder.Entity<BuiltInNullableDataTypes>(b =>
            {
                b.PartitionAndRowKey(dt => dt.Id0, dt => dt.Id1);
                b.Timestamp("Timestamp", true);
                b.Key(dt => dt.Id0); // See issue #632
            });

            return builder.Model;
        }

        void IDisposable.Dispose()
        {
            if (_options != null)
            {
                using (var context = new DbContext(_serviceProvider, _options))
                {
                    context.Database.EnsureDeleted();
                    _options = null;
                    _serviceProvider = null;
                }
            }
        }
    }
}
