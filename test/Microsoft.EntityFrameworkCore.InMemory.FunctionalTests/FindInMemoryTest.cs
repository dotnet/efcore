// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Specification.Tests;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.InMemory.FunctionalTests
{
    public class FindInMemoryTest
        : FindTestBase<FindInMemoryTest.FindInMemoryFixture>
    {
        public FindInMemoryTest(FindInMemoryFixture fixture)
            : base(fixture)
        {
        }

        public class FindInMemoryFixture : FindFixtureBase
        {
            private readonly IServiceProvider _serviceProvider;

            public FindInMemoryFixture()
            {
                _serviceProvider = new ServiceCollection()
                    .AddEntityFrameworkInMemoryDatabase()
                    .AddSingleton(TestInMemoryModelSource.GetFactory(OnModelCreating))
                    .BuildServiceProvider();
            }

            public override void CreateTestStore()
            {
                using (var context = CreateContext())
                {
                    Seed(context);
                }
            }

            public override DbContext CreateContext()
                => new FindContext(new DbContextOptionsBuilder()
                    .UseInMemoryDatabase("FindTest")
                    .UseInternalServiceProvider(_serviceProvider).Options);
        }
    }
}
