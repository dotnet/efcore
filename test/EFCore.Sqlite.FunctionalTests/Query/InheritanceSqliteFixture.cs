// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.TestModels.Inheritance;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class InheritanceSqliteFixture : InheritanceRelationalFixture<SqliteTestStore>
    {
        protected virtual string DatabaseName => "InheritanceSqliteTest";

        private readonly IServiceProvider _serviceProvider;

        public TestSqlLoggerFactory TestSqlLoggerFactory { get; } = new TestSqlLoggerFactory();

        public InheritanceSqliteFixture()
        {
            _serviceProvider = new ServiceCollection()
                .AddEntityFrameworkSqlite()
                .AddSingleton(TestModelSource.GetFactory(OnModelCreating))
                .AddSingleton<ILoggerFactory>(TestSqlLoggerFactory)
                .BuildServiceProvider(validateScopes: true);
        }

        public override SqliteTestStore CreateTestStore()
        {
            return SqliteTestStore.GetOrCreateShared(
                DatabaseName, () =>
                    {
                        using (var context = new InheritanceContext(
                            new DbContextOptionsBuilder()
                                .UseSqlite(SqliteTestStore.CreateConnectionString(DatabaseName))
                                .UseInternalServiceProvider(_serviceProvider)
                                .Options))
                        {
                            context.Database.EnsureClean();
                            InheritanceModelInitializer.SeedData(context);
                        }
                    });
        }

        public override InheritanceContext CreateContext(SqliteTestStore testStore)
        {
            var context = new InheritanceContext(
                new DbContextOptionsBuilder()
                    .UseSqlite(
                        testStore.Connection,
                        b => b.SuppressForeignKeyEnforcement())
                    .UseInternalServiceProvider(_serviceProvider).Options);

            context.Database.UseTransaction(testStore.Transaction);

            return context;
        }
    }
}
