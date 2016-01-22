// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.FunctionalTests;
using Microsoft.EntityFrameworkCore.FunctionalTests.TestModels.Inheritance;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.InMemory.FunctionalTests
{
    public class InheritanceInMemoryFixture : InheritanceFixtureBase
    {
        private readonly DbContextOptionsBuilder _optionsBuilder = new DbContextOptionsBuilder();
        private readonly IServiceProvider _serviceProvider;

        public InheritanceInMemoryFixture()
        {
            _serviceProvider
                = new ServiceCollection()
                    .AddEntityFramework()
                    .AddInMemoryDatabase()
                    .ServiceCollection()
                    .AddSingleton(TestInMemoryModelSource.GetFactory(OnModelCreating))
                    .BuildServiceProvider();

            _optionsBuilder.UseInMemoryDatabase();

            using (var context = CreateContext())
            {
                SeedData(context);
            }
        }

        public override InheritanceContext CreateContext()
        {
            return new InheritanceContext(_serviceProvider, _optionsBuilder.Options);
        }
    }
}
