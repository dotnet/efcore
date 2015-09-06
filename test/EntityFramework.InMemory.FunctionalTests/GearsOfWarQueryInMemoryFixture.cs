// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.FunctionalTests;
using Microsoft.Data.Entity.FunctionalTests.TestModels.GearsOfWarModel;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Framework.DependencyInjection;

namespace Microsoft.Data.Entity.InMemory.FunctionalTests
{
    public class GearsOfWarQueryInMemoryFixture : GearsOfWarQueryFixtureBase<InMemoryTestStore>
    {
        public const string DatabaseName = "GearsOfWarQueryTest";

        private readonly IServiceProvider _serviceProvider;
        private readonly DbContextOptions _options;

        public GearsOfWarQueryInMemoryFixture()
        {
            _serviceProvider
                = new ServiceCollection()
                    .AddEntityFramework()
                    .AddInMemoryDatabase()
                    .ServiceCollection()
                    .AddSingleton(TestInMemoryModelSource.GetFactory(OnModelCreating))
                    .BuildServiceProvider();

            var optionsBuilder = new DbContextOptionsBuilder();

            optionsBuilder.UseInMemoryDatabase();

            _options = optionsBuilder.Options;
        }

        public override InMemoryTestStore CreateTestStore()
        {
            return InMemoryTestStore.GetOrCreateShared(DatabaseName, () =>
                {
                    using (var context = new GearsOfWarContext(_serviceProvider, _options))
                    {
                        GearsOfWarModelInitializer.Seed(context);
                    }
                });
        }

        public override GearsOfWarContext CreateContext(InMemoryTestStore _)
        {
            var context = new GearsOfWarContext(_serviceProvider, _options);

            context.ChangeTracker.AutoDetectChangesEnabled = false;

            return context;
        }
    }
}
