// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.AzureTableStorage.Metadata;
using Microsoft.Data.Entity.FunctionalTests;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.AzureTableStorage.FunctionalTests
{
    public class BuiltInDataTypesFixture : BuiltInDataTypesFixtureBase, IDisposable
    {
        private ThreadSafeLazyRef<DbContext> _context;

        public BuiltInDataTypesFixture()
        {
            _context = new ThreadSafeLazyRef<DbContext>(() => GetNewContext());
        }

        public override DbContext CreateContext()
        {
            return _context.Value;
        }

        private DbContext GetNewContext()
        {
            var options = new DbContextOptions()
                .UseModel(CreateModel())
                .UseAzureTableStorage(TestConfig.Instance.ConnectionString, batchRequests: false);

            var context = new DbContext(options);
            context.Database.EnsureCreated();
            return context;
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
            if (_context != null && _context.HasValue)
            {
                _context.Value.Database.EnsureDeleted();
            }

        }
    }
}
