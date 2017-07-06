// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.TestModels.Inheritance;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class InheritanceSqlServerFixture : InheritanceRelationalFixture<SqlServerTestStore>
    {
        protected virtual string DatabaseName => "InheritanceSqlServerTest";

        private readonly DbContextOptions _options;

        public TestSqlLoggerFactory TestSqlLoggerFactory { get; } = new TestSqlLoggerFactory();

        public InheritanceSqlServerFixture()
        {
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkSqlServer()
                .AddSingleton(TestModelSource.GetFactory(OnModelCreating))
                .AddSingleton<ILoggerFactory>(TestSqlLoggerFactory)
                .BuildServiceProvider(validateScopes: true);

            _options = new DbContextOptionsBuilder()
                .EnableSensitiveDataLogging()
                .UseInternalServiceProvider(serviceProvider)
                .Options;
        }

        public override SqlServerTestStore CreateTestStore()
        {
            return SqlServerTestStore.GetOrCreateShared(
                DatabaseName, () =>
                    {
                        using (var context = new InheritanceContext(
                            new DbContextOptionsBuilder(_options)
                                .UseSqlServer(
                                    SqlServerTestStore.CreateConnectionString(DatabaseName),
                                    b => b.ApplyConfiguration())
                                .Options))
                        {
                            context.Database.EnsureCreated();
                            InheritanceModelInitializer.SeedData(context);
                        }
                    });
        }

        public override InheritanceContext CreateContext(SqlServerTestStore testStore)
        {
            var context = new InheritanceContext(
                new DbContextOptionsBuilder(_options)
                    .UseSqlServer(testStore.Connection, b => b.ApplyConfiguration())
                    .Options);

            context.Database.UseTransaction(testStore.Transaction);

            return context;
        }
    }
}
