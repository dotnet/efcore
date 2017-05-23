// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.TestModels;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.EntityFrameworkCore
{
    public class InMemoryCrossStoreFixture : CrossStoreFixture
    {
        private readonly SharedCrossStoreFixture _sharedCrossStoreFixture;

        public InMemoryCrossStoreFixture()
        {
            _sharedCrossStoreFixture = new SharedCrossStoreFixture(
                new ServiceCollection()
                    .AddEntityFrameworkInMemoryDatabase()
                    .BuildServiceProvider());
        }

        public override TestStore CreateTestStore(Type testStoreType)
        {
            Assert.Equal(typeof(InMemoryTestStore), testStoreType);

            return _sharedCrossStoreFixture.CreateTestStore(testStoreType);
        }

        public override CrossStoreContext CreateContext(TestStore testStore) => _sharedCrossStoreFixture.CreateContext(testStore);
    }
}
