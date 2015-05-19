// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.FunctionalTests;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Framework.DependencyInjection;

namespace Microsoft.Data.Entity.Sqlite.FunctionalTests
{
    public class BuiltInDataTypesSqliteFixture : BuiltInDataTypesFixtureBase
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly DbContextOptions _options;
        private readonly SqliteTestStore _testStore;

        public BuiltInDataTypesSqliteFixture()
        {
            _testStore = SqliteTestStore.CreateScratch();

            _serviceProvider = new ServiceCollection()
                .AddEntityFramework()
                .AddSqlite()
                .ServiceCollection()
                .AddSingleton(TestSqliteModelSource.GetFactory(OnModelCreating))
                .BuildServiceProvider();

            var optionsBuilder = new DbContextOptionsBuilder();
            optionsBuilder.UseSqlite(_testStore.Connection);
            _options = optionsBuilder.Options;

            using (var context = new DbContext(_serviceProvider, _options))
            {
               context.Database.EnsureCreated();
            }
        }

        public override DbContext CreateContext()
        {
            var context = new DbContext(_serviceProvider, _options);
            context.Database.AsRelational().Connection.UseTransaction(_testStore.Transaction);

            return context;
        }

        public override void Dispose()
        {
            _testStore.Dispose();
        }
    }
}
