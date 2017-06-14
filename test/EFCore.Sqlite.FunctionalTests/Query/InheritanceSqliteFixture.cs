// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class InheritanceSqliteFixture : InheritanceRelationalFixture
    {
        public TestSqlLoggerFactory TestSqlLoggerFactory { get; } = new TestSqlLoggerFactory();

        public override DbContextOptions BuildOptions()
        {
            return
                new DbContextOptionsBuilder()
                    .UseSqlite(SqliteTestStore.CreateConnectionString("InheritanceSqliteTest"))
                    .UseInternalServiceProvider(
                        new ServiceCollection()
                            .AddEntityFrameworkSqlite()
                            .AddSingleton(TestModelSource.GetFactory(OnModelCreating))
                            .AddSingleton<ILoggerFactory>(TestSqlLoggerFactory)
                            .BuildServiceProvider())
                    .Options;
        }
    }
}
