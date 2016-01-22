// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.FunctionalTests;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.SqlServer.FunctionalTests.SqlAzure.Model;
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
            SqlServerTestStore.CreateDatabase("adventureworks", scriptPath: "SqlAzure/adventureworks.sql", recreateIfAlreadyExists: false);

            var optionsBuilder = new DbContextOptionsBuilder();
            optionsBuilder.EnableSensitiveDataLogging()
                .UseSqlServer(SqlServerTestStore.CreateConnectionString("adventureworks"));
            Options = optionsBuilder.Options;

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddEntityFramework()
                .AddSqlServer();
            serviceCollection.AddSingleton<ILoggerFactory>(new TestSqlLoggerFactory());
            Services = serviceCollection.BuildServiceProvider();
        }

        public virtual AdventureWorksContext CreateContext() => new AdventureWorksContext(Services, Options);
    }
}