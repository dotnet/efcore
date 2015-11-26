// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.FunctionalTests;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.Data.Entity.SqlServer.FunctionalTests
{
    public class OneToOneQuerySqlServerFixture : OneToOneQueryFixtureBase, IDisposable
    {
        private readonly DbContextOptions _options;
        private readonly IServiceProvider _serviceProvider;
        private readonly SqlServerTestStore _testStore;

        public OneToOneQuerySqlServerFixture()
        {
            _serviceProvider
                = new ServiceCollection()
                    .AddEntityFramework()
                    .AddSqlServer()
                    .ServiceCollection()
                    .AddSingleton(TestSqlServerModelSource.GetFactory(OnModelCreating))
                    .AddSingleton<ILoggerFactory>(new TestSqlLoggerFactory())
                    .BuildServiceProvider();

            _testStore = SqlServerTestStore.CreateScratch();

            var optionsBuilder = new DbContextOptionsBuilder();
            optionsBuilder.UseSqlServer(_testStore.Connection.ConnectionString);
            _options = optionsBuilder.Options;

            using (var context = new DbContext(_serviceProvider, _options))
            {
                context.Database.EnsureCreated();

                AddTestData(context);
            }
        }

        public DbContext CreateContext() => new DbContext(_serviceProvider, _options);
        public void Dispose() => _testStore.Dispose();
    }
}
