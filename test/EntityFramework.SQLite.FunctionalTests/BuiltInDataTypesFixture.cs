// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.FunctionalTests;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.DependencyInjection.Fallback;

namespace Microsoft.Data.Entity.SQLite.FunctionalTests
{
    public class BuiltInDataTypesFixture : BuiltInDataTypesFixtureBase
    {
        private readonly IServiceProvider _serviceProvider;

        public BuiltInDataTypesFixture()
        {
            _serviceProvider
                = new ServiceCollection()
                    .AddEntityFramework()
                    .AddSQLite()
                    .ServiceCollection
                    .BuildServiceProvider();
        }


        public override DbContext CreateContext()
        {
            // do not use this method for SQLite tests
            throw new NotImplementedException();
        }

        public SQLiteTestDatabase CreateSQLiteTestDatabase()
        {
            var db = SQLiteTestDatabase.Scratch().Result;
            using (var context = CreateSQLiteContext(db))
            {
                context.Database.EnsureCreated();
            }

            return db;
        }

        public DbContext CreateSQLiteContext(SQLiteTestDatabase testDatabase)
        {
            var options
                = new DbContextOptions()
                    .UseModel(CreateModel())
                    .UseSQLite(testDatabase.Connection.ConnectionString);

            return new DbContext(_serviceProvider, options);
        }
    }
}
