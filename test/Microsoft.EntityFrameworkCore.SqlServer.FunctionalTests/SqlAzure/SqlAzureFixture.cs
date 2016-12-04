// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Specification.Tests;
using Microsoft.EntityFrameworkCore.SqlServer.FunctionalTests.SqlAzure.Model;
using Microsoft.EntityFrameworkCore.SqlServer.FunctionalTests.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.SqlServer.FunctionalTests.SqlAzure
{
    public class SqlAzureFixture
    {
        protected DbContextOptions Options { get; }
        protected IServiceProvider Services { get; }

        public SqlAzureFixture()
        {
            SqlServerTestStore.GetOrCreateShared(
                "adventureworks",
                () => SqlServerTestStore.ExecuteScript("adventureworks", "SqlAzure/adventureworks.sql"),
                cleanDatabase: false);

            Services = new ServiceCollection()
                .AddEntityFrameworkSqlServer()
                .AddSingleton<ILoggerFactory>(new TestSqlLoggerFactory()).BuildServiceProvider();

            Options = new DbContextOptionsBuilder()
                .UseInternalServiceProvider(Services)
                .EnableSensitiveDataLogging()
                .UseSqlServer(SqlServerTestStore.CreateConnectionString("adventureworks"), b => b.ApplyConfiguration()).Options;
        }

        public virtual AdventureWorksContext CreateContext() => new AdventureWorksContext(Options);
    }
}
