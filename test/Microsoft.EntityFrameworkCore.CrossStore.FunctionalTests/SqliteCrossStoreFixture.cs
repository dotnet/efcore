// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.CrossStore.FunctionalTests.TestModels;
using Microsoft.EntityFrameworkCore.Specification.Tests;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.CrossStore.FunctionalTests
{
    public class SqliteCrossStoreFixture : CrossStoreFixture
    {
        private readonly SharedCrossStoreFixture _sharedCrossStoreFixture;

        public SqliteCrossStoreFixture()
        {
            _sharedCrossStoreFixture = new SharedCrossStoreFixture(
                new ServiceCollection()
                    .AddEntityFrameworkSqlite()
                    .BuildServiceProvider());
        }

        public override TestStore CreateTestStore(Type testStoreType) => _sharedCrossStoreFixture.CreateTestStore(testStoreType);
        public override CrossStoreContext CreateContext(TestStore testStore) => _sharedCrossStoreFixture.CreateContext(testStore);
    }
}
