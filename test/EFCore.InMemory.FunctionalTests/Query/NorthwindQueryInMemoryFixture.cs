// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class NorthwindQueryInMemoryFixture : NorthwindQueryFixtureBase
    {
        public override NorthwindContext CreateContext(
            QueryTrackingBehavior queryTrackingBehavior = QueryTrackingBehavior.TrackAll,
            bool enableFilters = false)
        {
            if (!IsSeeded)
            {
                using (var context = base.CreateContext(queryTrackingBehavior, enableFilters))
                {
                    NorthwindData.Seed(context);
                }

                IsSeeded = true;
            }

            return base.CreateContext(queryTrackingBehavior, enableFilters);
        }

        private bool IsSeeded { get; set; }

        public override DbContextOptions BuildOptions(IServiceCollection serviceCollection = null)
            => new DbContextOptionsBuilder()
                .UseInMemoryDatabase(nameof(NorthwindQueryInMemoryFixture))
                .UseInternalServiceProvider(
                    (serviceCollection ?? new ServiceCollection())
                    .AddEntityFrameworkInMemoryDatabase()
                    .AddSingleton(TestModelSource.GetFactory(OnModelCreating))
                    .AddSingleton<ILoggerFactory>(new TestLoggerFactory())
                    .BuildServiceProvider(validateScopes: true))
                .Options;
    }
}
