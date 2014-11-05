// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.FunctionalTests;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.DependencyInjection.Fallback;

namespace Microsoft.Data.Entity.SQLite.FunctionalTests
{
    public class SqLiteBuiltInDataTypesFixture : BuiltInDataTypesFixtureBase<SqLiteTestStore>
    {
        private readonly IServiceProvider _serviceProvider;

        public SqLiteBuiltInDataTypesFixture()
        {
            _serviceProvider = new ServiceCollection()
                .AddEntityFramework()
                .AddSQLite()
                .ServiceCollection
                .BuildServiceProvider();
        }

        public override SqLiteTestStore CreateTestStore()
        {
            var testStore = SqLiteTestStore.CreateScratchAsync().Result;
            using (var context = CreateContext(testStore))
            {
                context.Database.EnsureCreated();
            }

            return testStore;
        }

        public override DbContext CreateContext(SqLiteTestStore testStore)
        {
            var options = new DbContextOptions()
                .UseModel(CreateModel())
                .UseSQLite(testStore.Connection.ConnectionString);

            return new DbContext(_serviceProvider, options);
        }

        public override void OnModelCreating(BasicModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<BuiltInNonNullableDataTypes>(b =>
            {
                b.Property(dt => dt.TestInt16);
                b.Property(dt => dt.TestUnsignedInt16);
                b.Property(dt => dt.TestUnsignedInt32);
                b.Property(dt => dt.TestUnsignedInt64);
                b.Property(dt => dt.TestSignedByte);
            });

            modelBuilder.Entity<BuiltInNullableDataTypes>(b =>
            {
                b.Property(dt => dt.TestNullableInt16);
                b.Property(dt => dt.TestNullableUnsignedInt16);
                b.Property(dt => dt.TestNullableUnsignedInt32);
                b.Property(dt => dt.TestNullableUnsignedInt64);
                b.Property(dt => dt.TestNullableSignedByte);
            });
        }
    }
}
