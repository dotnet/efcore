// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.EntityFrameworkCore.CrossStore.FunctionalTests
{
    public class ProviderSpecificServicesTest
    {
        [Fact]
        public void Throws_with_new_when_non_relational_provider_in_use1110938893()
        {
            var options = new DbContextOptionsBuilder<ConstructorTestContext1A>()
                .UseInternalServiceProvider(
                    new ServiceCollection()
                        .AddEntityFrameworkSqlServer()
                        .AddEntityFrameworkInMemoryDatabase()
                        .BuildServiceProvider())
                .UseInMemoryDatabase()
                .Options;

            using (var context = new ConstructorTestContext1A(options))
            {
                Assert.Equal(
                    RelationalStrings.RelationalNotInUse,
                    Assert.Throws<InvalidOperationException>(() => context.Database.GetDbConnection()).Message);
            }
        }

        [Fact]
        public void Throws_with_add_when_non_relational_provider_in_use547424486()
        {
            var appServiceProivder = new ServiceCollection()
                .AddEntityFrameworkSqlServer()
                .AddEntityFrameworkInMemoryDatabase()
                .AddDbContext<ConstructorTestContext1A>(
                    (p, b) => b.UseInMemoryDatabase().UseInternalServiceProvider(p))
                .BuildServiceProvider();

            using (var serviceScope = appServiceProivder
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
