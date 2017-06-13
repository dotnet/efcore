// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore
{
    public class DataAnnotationSqliteFixture : DataAnnotationFixtureBase<SqliteTestStore>
    {
        public static readonly string DatabaseName = "DataAnnotations";

        private readonly DbContextOptions _options;

        private readonly string _connectionString = SqliteTestStore.CreateConnectionString(DatabaseName);

        public TestSqlLoggerFactory TestSqlLoggerFactory { get; } = new TestSqlLoggerFactory();

        public DataAnnotationSqliteFixture()
        {
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkSqlite()
                .AddSingleton(TestModelSource.GetFactory(OnModelCreating))
                .AddSingleton<ILoggerFactory>(TestSqlLoggerFactory)
                .BuildServiceProvider();

            _options = new DbContextOptionsBuilder()
                .EnableSensitiveDataLogging()
                .UseInternalServiceProvider(serviceProvider)
                .ConfigureWarnings(w =>
                    {
                        w.Default(WarningBehavior.Throw);
                        w.Ignore(CoreEventId.SensitiveDataLoggingEnabledWarning);
                    })
                .Options;
        }

        public override SqliteTestStore CreateTestStore()
            => SqliteTestStore.GetOrCreateShared(DatabaseName, () =>
                {
                    var optionsBuilder = new DbContextOptionsBuilder(_options)
                        .UseSqlite(_connectionString);

                    using (var context = new DataAnnotationContext(optionsBuilder.Options))
                    {
                        context.Database.EnsureClean();
                        DataAnnotationModelInitializer.Seed(context);
                    }
                });

        public override DataAnnotationContext CreateContext(SqliteTestStore testStore)
        {
            var optionsBuilder = new DbContextOptionsBuilder(_options)
                .UseSqlite(
                    testStore.Connection,
                    b => b.SuppressForeignKeyEnforcement());

            var context = new DataAnnotationContext(optionsBuilder.Options);
            context.Database.UseTransaction(testStore.Transaction);
            return context;
        }
    }
}
