// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.FunctionalTests;
using Microsoft.EntityFrameworkCore.FunctionalTests.TestModels.UpdatesModel;
using Microsoft.EntityFrameworkCore.SqlServer.FunctionalTests.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.SqlServer.FunctionalTests
{
    public class UpdatesSqlServerFixture : UpdatesFixtureBase<SqlServerTestStore>
    {
        private readonly IServiceProvider _serviceProvider;

        public UpdatesSqlServerFixture()
        {
            _serviceProvider = new ServiceCollection()
                .AddEntityFramework()
                .AddSqlServer()
                .ServiceCollection()
                .BuildServiceProvider();
        }

        protected virtual string DatabaseName => "PartialUpdateSqlServerTest";

        public override SqlServerTestStore CreateTestStore()
        {
            return SqlServerTestStore.GetOrCreateShared(DatabaseName, () =>
            {
                var optionsBuilder = new DbContextOptionsBuilder();
                optionsBuilder.UseSqlServer(SqlServerTestStore.CreateConnectionString(DatabaseName));

                using (var context = new UpdatesContext(_serviceProvider, optionsBuilder.Options))
                {
                    context.Database.EnsureDeleted();
                    if (context.Database.EnsureCreated())
                    {
                        UpdatesModelInitializer.Seed(context);
                    }
                }
            });
        }

        public override UpdatesContext CreateContext(SqlServerTestStore testStore)
        {
            var optionsBuilder = new DbContextOptionsBuilder();
            optionsBuilder.UseSqlServer(testStore.Connection);

            var context = new UpdatesContext(_serviceProvider, optionsBuilder.Options);
            context.Database.UseTransaction(testStore.Transaction);
            return context;
        }
    }
}
