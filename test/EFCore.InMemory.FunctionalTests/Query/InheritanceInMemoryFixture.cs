// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.TestModels.Inheritance;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class InheritanceInMemoryFixture : InheritanceFixtureBase
    {
        private readonly object _sync = new object();
        private bool _seeded;

        public override InheritanceContext CreateContext(bool enableFilters = false)
        {
            EnableFilters = enableFilters;

            if (!_seeded)
            {
                lock (_sync)
                {
                    if (!_seeded)
                    {
                        using (var context = CreateContextCore())
                        {
                            if (context.Database.EnsureCreated())
                            {
                                SeedData(context);
                            }
                        }

                        ClearLog();

                        _seeded = true;
                    }
                }
            }

            return CreateContextCore();
        }

        public override DbContextOptions BuildOptions()
        {
            return
                new DbContextOptionsBuilder()
                    .UseInMemoryDatabase(nameof(InheritanceInMemoryFixture))
                    .UseInternalServiceProvider(
                        new ServiceCollection()
                            .AddEntityFrameworkInMemoryDatabase()
                            .AddSingleton(TestModelSource.GetFactory(OnModelCreating))
                            .BuildServiceProvider())
                    .Options;
        }
    }
}
