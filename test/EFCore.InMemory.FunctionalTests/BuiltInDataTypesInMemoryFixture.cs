// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore
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
                .AddSingleton(TestModelSource.GetFactory(OnModelCreating))
                .BuildServiceProvider(validateScopes: true);

            _options = new DbContextOptionsBuilder()
                .UseInMemoryDatabase(nameof(BuiltInDataTypesInMemoryFixture))
                .UseInternalServiceProvider(serviceProvider)
                .Options;
        }

        public override DbContext CreateContext() => new DbContext(_options);

        public override void Dispose() => _testStore.Dispose();

        public override bool SupportsBinaryKeys => false;

        public override DateTime DefaultDateTime => new DateTime();
    }
}
