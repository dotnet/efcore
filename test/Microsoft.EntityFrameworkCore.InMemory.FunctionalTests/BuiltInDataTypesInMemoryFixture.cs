// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Specification.Tests;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.InMemory.FunctionalTests
{
    public class BuiltInDataTypesInMemoryFixture : BuiltInDataTypesFixtureBase
    {
        private readonly DbContextOptions _options;
        private readonly InMemoryTestStore _testStore;

        public BuiltInDataTypesInMemoryFixture()
        {
            _testStore = new InMemoryTestStore();
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .AddSingleton(TestInMemoryModelSource.GetFactory(OnModelCreating))
                .BuildServiceProvider();

            _options = new DbContextOptionsBuilder()
                .UseInMemoryDatabase()
                .UseInternalServiceProvider(serviceProvider)
                .Options;
        }

        public override DbContext CreateContext() => new DbContext(_options);

        public override void Dispose() => _testStore.Dispose();

        public override bool SupportsBinaryKeys => false;

        public override DateTime DefaultDateTime => new DateTime();
    }
}
