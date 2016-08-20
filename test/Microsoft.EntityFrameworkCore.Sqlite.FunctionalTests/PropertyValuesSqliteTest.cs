// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Specification.Tests;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Sqlite.FunctionalTests
{
    public class PropertyValuesSqliteTest
        : PropertyValuesTestBase<SqliteTestStore, PropertyValuesSqliteTest.PropertyValuesSqliteFixture>
    {
        public PropertyValuesSqliteTest(PropertyValuesSqliteFixture fixture)
            : base(fixture)
        {
        }

        public class PropertyValuesSqliteFixture : PropertyValuesFixtureBase
        {
            private const string DatabaseName = "PropertyValues";

            private readonly IServiceProvider _serviceProvider;

            public PropertyValuesSqliteFixture()
            {
                _serviceProvider = new ServiceCollection()
                    .AddEntityFrameworkSqlite()
                    .AddSingleton(TestSqliteModelSource.GetFactory(OnModelCreating))
                    .BuildServiceProvider();
            }

            public override SqliteTestStore CreateTestStore()
            {
                return SqliteTestStore.GetOrCreateShared(DatabaseName, () =>
                    {
                        var optionsBuilder = new DbContextOptionsBuilder()
                            .UseSqlite(SqliteTestStore.CreateConnectionString(DatabaseName))
                            .UseInternalServiceProvider(_serviceProvider);

                        using (var context = new AdvancedPatternsMasterContext(optionsBuilder.Options))
                        {
                            context.Database.EnsureClean();
                            Seed(context);
                        }
                    });
            }

            public override DbContext CreateContext(SqliteTestStore testStore)
            {
                var optionsBuilder = new DbContextOptionsBuilder()
                    .UseSqlite(testStore.Connection)
                    .UseInternalServiceProvider(_serviceProvider);

                var context = new AdvancedPatternsMasterContext(optionsBuilder.Options);
                context.Database.UseTransaction(testStore.Transaction);

                return context;
            }
        }
    }
}
