// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore
{
    public class LoadSqliteTest
        : LoadTestBase<SqliteTestStore, LoadSqliteTest.LoadSqliteFixture>
    {
        public LoadSqliteTest(LoadSqliteFixture fixture)
            : base(fixture)
        {
        }

        public class LoadSqliteFixture : LoadFixtureBase
        {
            private readonly DbContextOptions _options;
            private readonly string DatabaseName = "LoadTest";

            public LoadSqliteFixture()
            {
                var serviceProvider = new ServiceCollection()
                    .AddEntityFrameworkSqlite()
                    .AddSingleton(TestModelSource.GetFactory(OnModelCreating))
                    .BuildServiceProvider(validateScopes: true);

                _options = new DbContextOptionsBuilder()
                    .UseSqlite(SqliteTestStore.CreateConnectionString(DatabaseName))
                    .UseInternalServiceProvider(serviceProvider)
                    .Options;
            }

            public override SqliteTestStore CreateTestStore()
            {
                return SqliteTestStore.GetOrCreateShared(DatabaseName, () =>
                    {
                        using (var context = new LoadContext(_options))
                        {
                            context.Database.EnsureClean();
                            Seed(context);
                        }
                    });
            }

            public override DbContext CreateContext(SqliteTestStore testStore)
                => new LoadContext(_options);
        }
    }
}
