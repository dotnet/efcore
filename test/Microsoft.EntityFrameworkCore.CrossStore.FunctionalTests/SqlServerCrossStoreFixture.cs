// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.FunctionalTests.TestModels;
using Microsoft.EntityFrameworkCore.SqlServer.FunctionalTests;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.EntityFrameworkCore.FunctionalTests
{
    public class SqlServerCrossStoreFixture : CrossStoreFixture
    {
        private readonly SharedCrossStoreFixture _sharedCrossStoreFixture;

        public SqlServerCrossStoreFixture()
        {
            _sharedCrossStoreFixture = new SharedCrossStoreFixture(
                new ServiceCollection()
                    .AddEntityFramework()
                    .AddSqlServer()
                    .AddInMemoryDatabase()
                    .ServiceCollection()
                    .BuildServiceProvider());
        }

        public override TestStore CreateTestStore(Type testStoreType)
        {
            Assert.Equal(typeof(SqlServerTestStore), testStoreType);

            return _sharedCrossStoreFixture.CreateTestStore(testStoreType);
        }

        public override CrossStoreContext CreateContext(TestStore testStore)
        {
            return _sharedCrossStoreFixture.CreateContext(testStore);
        }
    }
}
