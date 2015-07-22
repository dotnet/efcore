// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.FunctionalTests;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Framework.DependencyInjection;

namespace Microsoft.Data.Entity.InMemory.FunctionalTests
{
    public class BuiltInDataTypesInMemoryFixture : BuiltInDataTypesFixtureBase
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly DbContextOptions _options;
        private readonly InMemoryTestStore _testStore;

        public BuiltInDataTypesInMemoryFixture()
        {
            _testStore = new InMemoryTestStore();
            _serviceProvider = new ServiceCollection()
                .AddEntityFramework()
                .AddInMemoryDatabase()
                .ServiceCollection()
                .AddSingleton(TestInMemoryModelSource.GetFactory(OnModelCreating))
                .BuildServiceProvider();

            var optionsBuilder = new DbContextOptionsBuilder();
            optionsBuilder.UseInMemoryDatabase();
            _options = optionsBuilder.Options;
        }
        
        public override DbContext CreateContext() => new DbContext(_serviceProvider, _options);
        public override void Dispose()
        {
            _testStore.Dispose();
        }

        public override bool SupportsBinaryKeys => false;

        public override DateTime DefaultDateTime => new DateTime();
    }
}
