// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.FunctionalTests;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.DependencyInjection.Fallback;
using Microsoft.Framework.Logging;
using Microsoft.Data.Entity.Relational.FunctionalTests;

namespace Microsoft.Data.Entity.SQLite.FunctionalTests
{
    public class SQLiteOneToOneQueryFixture : OneToOneQueryFixtureBase
    {
        private readonly DbContextOptions _options;
        private readonly IServiceProvider _serviceProvider;

        public SQLiteOneToOneQueryFixture()
        {
            _serviceProvider
                = new ServiceCollection()
                    .AddEntityFramework()
                    .AddSQLite()
                    .ServiceCollection
                    .AddInstance<ILoggerFactory>(new TestSqlLoggerFactory())
                    .BuildServiceProvider();

            var model = CreateModel();
            var database = SqLiteTestStore.CreateScratchAsync().Result;

            _options
                = new DbContextOptions()
                    .UseModel(model)
                    .UseSQLite(database.Connection.ConnectionString);
            
            using (var context = new DbContext(_serviceProvider, _options))
            {
                context.Database.EnsureCreated();

                AddTestData(context);
            }
        }

        public DbContext CreateContext()
        {
            return new DbContext(_serviceProvider, _options);
        }
    }
}
