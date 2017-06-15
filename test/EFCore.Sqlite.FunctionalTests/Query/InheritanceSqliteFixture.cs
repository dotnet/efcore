// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class InheritanceSqliteFixture : InheritanceRelationalFixture<SqliteTestStore>
    {
        public TestSqlLoggerFactory TestSqlLoggerFactory { get; } = new TestSqlLoggerFactory();
        private readonly string DatabaseName = "InheritanceSqliteTest";

        public override SqliteTestStore CreateTestStore()
            => SqliteTestStore.GetOrCreateShared(DatabaseName, () =>
                {
                    using (var context = CreateContext())
                    {
                        context.Database.EnsureClean();
                        SeedData(context);
                    }
                });

        public override DbContextOptions BuildOptions()
            => new DbContextOptionsBuilder()
                .UseSqlite(SqliteTestStore.CreateConnectionString(DatabaseName))
                .UseInternalServiceProvider(
                    new ServiceCollection()
                        .AddEntityFrameworkSqlite()
                        .AddSingleton(TestModelSource.GetFactory(OnModelCreating))
                        .AddSingleton<ILoggerFactory>(TestSqlLoggerFactory)
                        .BuildServiceProvider())
                .Options;
    }
}
