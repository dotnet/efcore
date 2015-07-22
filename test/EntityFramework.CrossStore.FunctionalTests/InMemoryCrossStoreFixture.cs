// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.FunctionalTests.TestModels;
using Microsoft.Data.Entity.InMemory.FunctionalTests;
using Microsoft.Framework.DependencyInjection;
using Xunit;

namespace Microsoft.Data.Entity.FunctionalTests
{
    public class InMemoryCrossStoreFixture : CrossStoreFixture
    {
        private readonly SharedCrossStoreFixture _sharedCrossStoreFixture;

        public InMemoryCrossStoreFixture()
        {
            _sharedCrossStoreFixture = new SharedCrossStoreFixture(
                new ServiceCollection()
                    .AddEntityFramework()
                    .AddInMemoryDatabase()
                    .AddSqlServer()
                    .ServiceCollection()
                    .BuildServiceProvider());
        }

        public override TestStore CreateTestStore(Type testStoreType)
        {
            Assert.Equal(typeof(InMemoryTestStore), testStoreType);

            return _sharedCrossStoreFixture.CreateTestStore(testStoreType);
        }

        public override CrossStoreContext CreateContext(TestStore testStore)
        {
            return _sharedCrossStoreFixture.CreateContext(testStore);
        }
    }
}
