// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.FunctionalTests;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Sqlite.FunctionalTests
{
    public class NullKeysSqliteTest : NullKeysTestBase<NullKeysSqliteTest.NullKeysSqliteFixture>
    {
        public NullKeysSqliteTest(NullKeysSqliteFixture fixture)
            : base(fixture)
        {
        }

        public class NullKeysSqliteFixture : NullKeysFixtureBase
        {
            private readonly IServiceProvider _serviceProvider;
            private readonly DbContextOptions _options;

            public NullKeysSqliteFixture()
            {
                _serviceProvider = new ServiceCollection()
                    .AddEntityFrameworkSqlite()
                    .AddSingleton(TestSqliteModelSource.GetFactory(OnModelCreating))
                    .BuildServiceProvider();

                _options = new DbContextOptionsBuilder()
                    .UseSqlite(SqliteTestStore.CreateConnectionString("StringsContext"))
                    .UseInternalServiceProvider(_serviceProvider)
                    .Options;

                EnsureCreated();
            }

            public override DbContext CreateContext()
                => new DbContext(_options);
        }
    }
}
