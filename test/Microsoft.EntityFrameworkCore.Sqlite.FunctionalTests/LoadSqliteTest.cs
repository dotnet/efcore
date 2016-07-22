// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Specification.Tests;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Sqlite.FunctionalTests
{
    public class LoadSqliteTest
        : LoadTestBase<LoadSqliteTest.LoadSqliteFixture>
    {
        public LoadSqliteTest(LoadSqliteFixture fixture)
            : base(fixture)
        {
        }

        public class LoadSqliteFixture : LoadFixtureBase
        {
            private readonly IServiceProvider _serviceProvider;

            public LoadSqliteFixture()
            {
                _serviceProvider = new ServiceCollection()
                    .AddEntityFrameworkSqlite()
                    .AddSingleton(TestSqliteModelSource.GetFactory(OnModelCreating))
                    .BuildServiceProvider();
            }

            public override void CreateTestStore()
            {
                using (var context = CreateContext())
                {
                    context.Database.EnsureClean();
                    Seed(context);
                }
            }

            public override DbContext CreateContext()
                => new LoadContext(new DbContextOptionsBuilder()
                    .UseSqlite(SqliteTestStore.CreateConnectionString("LoadTest"))
                    .UseInternalServiceProvider(_serviceProvider)
                    .Options);
        }
    }
}
