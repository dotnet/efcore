// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class InheritanceSqlServerFixture : InheritanceRelationalFixture<SqlServerTestStore>
    {
        public TestSqlLoggerFactory TestSqlLoggerFactory { get; } = new TestSqlLoggerFactory();
        private const string DatabaseName = "InheritanceSqlServerTest";

        protected override void ClearLog()
            => TestSqlLoggerFactory.Clear();

        public override SqlServerTestStore CreateTestStore()
            => SqlServerTestStore.GetOrCreateShared(DatabaseName, () =>
                {
                    using (var context = CreateContext())
                    {
                        context.Database.EnsureCreated();
                        SeedData(context);
                    }
                });

        public override DbContextOptions BuildOptions()
            => new DbContextOptionsBuilder()
                .EnableSensitiveDataLogging()
                .UseSqlServer(
                    SqlServerTestStore.CreateConnectionString(DatabaseName),
                    b => b.ApplyConfiguration())
                .UseInternalServiceProvider(
                    new ServiceCollection()
                        .AddEntityFrameworkSqlServer()
                        .AddSingleton(TestModelSource.GetFactory(OnModelCreating))
                        .AddSingleton<ILoggerFactory>(TestSqlLoggerFactory)
                        .BuildServiceProvider())
                .Options;
    }
}
