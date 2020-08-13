// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.EntityFrameworkCore
{
    public class ProviderSpecificServicesTest
    {
        [ConditionalFact]
        public void Throws_with_new_when_non_relational_provider_in_use()
        {
            var options = new DbContextOptionsBuilder<ConstructorTestContext1A>()
                .UseInternalServiceProvider(
                    new ServiceCollection()
                        .AddEntityFrameworkInMemoryDatabase()
                        .BuildServiceProvider())
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            using (var context = new ConstructorTestContext1A(options))
            {
                Assert.Equal(
                    RelationalStrings.RelationalNotInUse,
                    Assert.Throws<InvalidOperationException>(() => context.Database.GetDbConnection()).Message);
            }
        }

        [ConditionalFact]
        public void Throws_with_add_when_non_relational_provider_in_use()
        {
            var appServiceProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .AddDbContext<ConstructorTestContext1A>(
                    (p, b) => b.UseInMemoryDatabase(Guid.NewGuid().ToString()).UseInternalServiceProvider(p))
                .BuildServiceProvider();

            using (var serviceScope = appServiceProvider
                .GetRequiredService<IServiceScopeFactory>()
                .CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetService<ConstructorTestContext1A>();

                Assert.Equal(
                    RelationalStrings.RelationalNotInUse,
                    Assert.Throws<InvalidOperationException>(() => context.Database.GetDbConnection()).Message);
            }
        }

        private class ConstructorTestContext1A : DbContext
        {
            public ConstructorTestContext1A(DbContextOptions options)
                : base(options)
            {
            }
        }
    }
}
