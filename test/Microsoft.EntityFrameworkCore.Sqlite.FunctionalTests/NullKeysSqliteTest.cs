// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Specification.Tests;
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
            private readonly DbContextOptions _options;

            public NullKeysSqliteFixture()
            {
                var serviceProvider = new ServiceCollection()
                    .AddEntityFrameworkSqlite()
                    .AddSingleton(TestSqliteModelSource.GetFactory(OnModelCreating))
                    .BuildServiceProvider();

                _options = new DbContextOptionsBuilder()
                    .UseSqlite(SqliteTestStore.CreateConnectionString("StringsContext"))
                    .UseInternalServiceProvider(serviceProvider)
                    .Options;

                CreateContext().Database.EnsureClean();
                EnsureCreated();
            }

            public override DbContext CreateContext()
                => new DbContext(_options);
        }
    }
}
