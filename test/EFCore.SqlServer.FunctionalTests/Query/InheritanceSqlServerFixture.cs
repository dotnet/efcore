// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class InheritanceSqlServerFixture : InheritanceRelationalFixture
    {
        public TestSqlLoggerFactory TestSqlLoggerFactory { get; } = new TestSqlLoggerFactory();

        protected override void ClearLog()
        {
            TestSqlLoggerFactory.Clear();
        }

        public override DbContextOptions BuildOptions()
        {
            return
                new DbContextOptionsBuilder()
                    .EnableSensitiveDataLogging()
                    .UseSqlServer(
                        SqlServerTestStore.CreateConnectionString("InheritanceSqlServerTest"),
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
}
