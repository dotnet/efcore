// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Specification.Tests;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.InMemory.FunctionalTests
{
    public class InheritanceInMemoryFixture : InheritanceFixtureBase
    {
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
