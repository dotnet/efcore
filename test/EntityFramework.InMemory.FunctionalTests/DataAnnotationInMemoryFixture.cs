// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.FunctionalTests;
using Microsoft.Framework.DependencyInjection;

namespace Microsoft.Data.Entity.InMemory.FunctionalTests
{
    public class DataAnnotationInMemoryFixture : DataAnnotationFixtureBase<InMemoryTestStore>
    {
        public static readonly string DatabaseName = "DataAnnotations";

        private readonly IServiceProvider _serviceProvider;

        public DataAnnotationInMemoryFixture()
        {
            _serviceProvider = new ServiceCollection()
                   .AddEntityFramework()
                   .AddInMemoryDatabase()
                   .ServiceCollection()
                   .AddSingleton(TestInMemoryModelSource.GetFactory(OnModelCreating))
                   .BuildServiceProvider();
        }

        public override InMemoryTestStore CreateTestStore()
        {
            return InMemoryTestStore.GetOrCreateShared(DatabaseName, () =>
            {
                var optionsBuilder = new DbContextOptionsBuilder();
                optionsBuilder.UseInMemoryDatabase();

                using (var context = new DataAnnotationContext(_serviceProvider, optionsBuilder.Options))
                {
                    context.Database.EnsureDeleted();
                    if (context.Database.EnsureCreated())
                    {
                        DataAnnotationModelInitializer.Seed(context);
                    }
                }
            });
        }

        public override DataAnnotationContext CreateContext(InMemoryTestStore testStore)
        {
            var optionsBuilder = new DbContextOptionsBuilder();
            optionsBuilder.UseInMemoryDatabase();

            var context = new DataAnnotationContext(_serviceProvider, optionsBuilder.Options);

            return context;
        }
    }
}
