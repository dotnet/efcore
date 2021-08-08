// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.InMemory.Infrastructure.Internal;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.EntityFrameworkCore
{
    public class InMemoryDbContextOptionsBuilderExtensionsTest
    {
        [ConditionalFact]
        public void Service_collection_extension_method_can_configure_InMemory_options()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddInMemoryDatabase<ApplicationDbContext>(
                "Crunchie",
                inMemoryOptions =>
                {
                    inMemoryOptions.EnableNullabilityCheck(false);
                },
                dbContextOption =>
                {
                    dbContextOption.EnableDetailedErrors();
                });

            var services = serviceCollection.BuildServiceProvider(validateScopes: true);

            using (var serviceScope = services
                .GetRequiredService<IServiceScopeFactory>()
                .CreateScope())
            {
                var coreOptions = serviceScope.ServiceProvider
                    .GetRequiredService<DbContextOptions<ApplicationDbContext>>().GetExtension<CoreOptionsExtension>();

                Assert.True(coreOptions.DetailedErrorsEnabled);

                var inMemoryOptions = serviceScope.ServiceProvider
                    .GetRequiredService<DbContextOptions<ApplicationDbContext>>().GetExtension<InMemoryOptionsExtension>();

                Assert.False(inMemoryOptions.IsNullabilityCheckEnabled);
                Assert.Equal("Crunchie", inMemoryOptions.StoreName);
            }
        }

        private class ApplicationDbContext : DbContext
        {
            public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
                : base(options)
            {
            }
        }
    }
}
