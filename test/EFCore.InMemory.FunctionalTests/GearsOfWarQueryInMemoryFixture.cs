// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.TestModels.GearsOfWarModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore
{
    public class GearsOfWarQueryInMemoryFixture : GearsOfWarQueryFixtureBase<InMemoryTestStore>
    {
        public const string DatabaseName = "GearsOfWarQueryTest";

        private readonly DbContextOptions _options;

        public GearsOfWarQueryInMemoryFixture()
        {
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .AddSingleton(TestModelSource.GetFactory(OnModelCreating))
                .AddSingleton<ILoggerFactory>(new TestLoggerFactory())
                .BuildServiceProvider();

            var optionsBuilder = new DbContextOptionsBuilder()
                .UseInMemoryDatabase(DatabaseName)
                .UseInternalServiceProvider(serviceProvider);

            _options = optionsBuilder.Options;
        }

        public override InMemoryTestStore CreateTestStore()
        {
            return InMemoryTestStore.GetOrCreateShared(DatabaseName, () =>
                {
                    using (var context = new GearsOfWarContext(_options))
                    {
                        GearsOfWarModelInitializer.Seed(context);
                    }
                });
        }

        public override GearsOfWarContext CreateContext(InMemoryTestStore _)
        {
            var context = new GearsOfWarContext(_options);

            context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

            return context;
        }
    }
}
