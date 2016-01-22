// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.FunctionalTests.TestModels;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.FunctionalTests
{
    public class SqliteCrossStoreFixture : CrossStoreFixture
    {
        private readonly SharedCrossStoreFixture _sharedCrossStoreFixture;

        public SqliteCrossStoreFixture()
        {
            _sharedCrossStoreFixture = new SharedCrossStoreFixture(
                new ServiceCollection()
                    .AddEntityFramework()
                    .AddSqlite()
                    .ServiceCollection()
                    .BuildServiceProvider());
        }

        public override TestStore CreateTestStore(Type testStoreType) => _sharedCrossStoreFixture.CreateTestStore(testStoreType);
        public override CrossStoreContext CreateContext(TestStore testStore) => _sharedCrossStoreFixture.CreateContext(testStore);
    }
}
