// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.FunctionalTests.TestModels;
using Microsoft.Data.Entity.SqlServer.FunctionalTests;
using Microsoft.Framework.DependencyInjection;
using Xunit;

namespace Microsoft.Data.Entity.FunctionalTests
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
