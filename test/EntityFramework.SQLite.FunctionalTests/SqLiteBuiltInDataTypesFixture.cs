// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.FunctionalTests;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.DependencyInjection.Fallback;

namespace Microsoft.Data.Entity.Sqlite.FunctionalTests
{
    public class SqLiteBuiltInDataTypesFixture : BuiltInDataTypesFixtureBase<SqLiteTestStore>
    {
        private readonly IServiceProvider _serviceProvider;

        public SqLiteBuiltInDataTypesFixture()
        {
            _serviceProvider = new ServiceCollection()
                .AddEntityFramework()
                .AddSqlite()
                .ServiceCollection
                .AddTestModelSource(OnModelCreating)
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
                .UseSqlite(testStore.Connection.ConnectionString);

            return new DbContext(_serviceProvider, options);
        }

        public override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<BuiltInNonNullableDataTypes>(b => b.Ignore(dt => dt.TestCharacter));
            modelBuilder.Entity<BuiltInNullableDataTypes>(b => b.Ignore(dt => dt.TestNullableCharacter));
        }
    }
}
