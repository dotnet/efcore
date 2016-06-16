// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Specification.Tests;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Sqlite.FunctionalTests
{
    public class FindSqliteTest
        : FindTestBase<FindSqliteTest.FindSqliteFixture>
    {
        public FindSqliteTest(FindSqliteFixture fixture)
            : base(fixture)
        {
        }

        public class FindSqliteFixture : FindFixtureBase
        {
            private readonly IServiceProvider _serviceProvider;

            public FindSqliteFixture()
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
                    context.Database.EnsureDeleted();
                    context.Database.EnsureCreated();
                    Seed(context);
                }
            }

            public override DbContext CreateContext()
                => new FindContext(new DbContextOptionsBuilder()
                    .UseSqlite(SqliteTestStore.CreateConnectionString("FindTest"))
                    .UseInternalServiceProvider(_serviceProvider).Options);
        }
    }
}
