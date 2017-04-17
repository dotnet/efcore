// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Specification.Tests;
using Microsoft.EntityFrameworkCore.Specification.Tests.TestModels.ComplexNavigationsModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Sqlite.FunctionalTests
{
    public class ComplexNavigationsOwnedQuerySqliteFixture : ComplexNavigationsOwnedQueryFixtureBase<SqliteTestStore>
    {
        public static readonly string DatabaseName = "ComplexNavigationsOwned";

        private readonly IServiceProvider _serviceProvider;

        private readonly string _connectionString = SqliteTestStore.CreateConnectionString(DatabaseName);

        public TestSqlLoggerFactory TestSqlLoggerFactory { get; } = new TestSqlLoggerFactory();

        public ComplexNavigationsOwnedQuerySqliteFixture()
        {
            _serviceProvider = new ServiceCollection()
                .AddEntityFrameworkSqlite()
                .AddSingleton(TestModelSource.GetFactory(OnModelCreating))
                .AddSingleton<ILoggerFactory>(TestSqlLoggerFactory)
                .BuildServiceProvider();
        }

        public override SqliteTestStore CreateTestStore() =>
            SqliteTestStore.GetOrCreateShared(
                DatabaseName,
                () =>
                    {
                        var optionsBuilder = new DbContextOptionsBuilder()
                            .UseSqlite(_connectionString)
                            .UseInternalServiceProvider(_serviceProvider);

                        using (var context = new ComplexNavigationsContext(optionsBuilder.Options))
                        {
                            context.Database.EnsureClean();
                            ComplexNavigationsModelInitializer.Seed(context);
                        }
                    });

        public override ComplexNavigationsContext CreateContext(SqliteTestStore testStore)
        {
            var optionsBuilder = new DbContextOptionsBuilder()
                .UseSqlite(
                    testStore.Connection,
                    b => b.SuppressForeignKeyEnforcement())
                .UseInternalServiceProvider(_serviceProvider);

            var context = new ComplexNavigationsContext(optionsBuilder.Options);

            context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

            context.Database.UseTransaction(testStore.Transaction);

            return context;
        }

        protected override void Configure(ReferenceOwnershipBuilder<Level1, Level2> l2)
        {
            base.Configure(l2);

            l2.ForSqliteToTable("Level2");
        }

        protected override void Configure(ReferenceOwnershipBuilder<Level2, Level3> l3)
        {
            base.Configure(l3);

            l3.ForSqliteToTable("Level3");
        }

        protected override void Configure(ReferenceOwnershipBuilder<Level3, Level4> l4)
        {
            base.Configure(l4);

            l4.ForSqliteToTable("Level4");
        }
    }
}
