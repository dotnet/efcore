// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Specification.Tests;
using Microsoft.EntityFrameworkCore.Specification.Tests.TestModels.Inheritance;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.InMemory.FunctionalTests
{
    public class InheritanceInMemoryFixture : InheritanceFixtureBase
    {
        private readonly DbContextOptionsBuilder _optionsBuilder = new DbContextOptionsBuilder();

        public InheritanceInMemoryFixture()
        {
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .AddSingleton(TestInMemoryModelSource.GetFactory(OnModelCreating))
                .BuildServiceProvider();

            _optionsBuilder.UseInMemoryDatabase().UseInternalServiceProvider(serviceProvider);

            using (var context = CreateContext())
            {
                SeedData(context);
            }
        }

        public override InheritanceContext CreateContext()
            => new InheritanceContext(_optionsBuilder.Options);
    }
}
