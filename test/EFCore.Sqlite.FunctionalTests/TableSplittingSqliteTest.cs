// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.TestModels.TransportationModel;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore
{
    public class TableSplittingSqliteTest : TableSplittingTestBase<SqliteTestStore>
    {
        public override SqliteTestStore CreateTestStore(Action<ModelBuilder> onModelCreating)
            => SqliteTestStore.GetOrCreateShared(DatabaseName, false, true, () =>
                {
                    var optionsBuilder = new DbContextOptionsBuilder()
                        .UseSqlite(SqliteTestStore.CreateConnectionString(DatabaseName))
                        .EnableSensitiveDataLogging()
                        .UseInternalServiceProvider(BuildServiceProvider(onModelCreating));

                    using (var context = new TransportationContext(optionsBuilder.Options))
                    {
                        context.Database.EnsureClean();
                        context.Seed();
                    }
                });

        public override TransportationContext CreateContext(SqliteTestStore testStore, Action<ModelBuilder> onModelCreating)
        {
            var optionsBuilder = new DbContextOptionsBuilder()
                .UseSqlite(SqliteTestStore.CreateConnectionString(DatabaseName))
                .EnableSensitiveDataLogging()
                .UseInternalServiceProvider(BuildServiceProvider(onModelCreating));

            return new TransportationContext(optionsBuilder.Options);
        }

        private IServiceProvider BuildServiceProvider(Action<ModelBuilder> onModelCreating)
            => new ServiceCollection()
                .AddEntityFrameworkSqlite()
                .AddSingleton(TestModelSource.GetFactory(onModelCreating))
                .BuildServiceProvider();
    }
}
