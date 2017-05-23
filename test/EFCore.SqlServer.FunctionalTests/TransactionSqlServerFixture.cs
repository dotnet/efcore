// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data.Common;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore
{
    public class TransactionSqlServerFixture : TransactionFixtureBase<SqlServerTestStore>
    {
        private readonly IServiceProvider _serviceProvider;

        public TransactionSqlServerFixture()
        {
            _serviceProvider = new ServiceCollection()
                .AddEntityFrameworkSqlServer()
                .AddSingleton(TestModelSource.GetFactory(OnModelCreating))
                .BuildServiceProvider();
        }

        public override SqlServerTestStore CreateTestStore()
            => SqlServerTestStore.GetOrCreateShared(DatabaseName, () =>
                {
                    var optionsBuilder = new DbContextOptionsBuilder()
                        .UseSqlServer(SqlServerTestStore.CreateConnectionString(DatabaseName), b => b.ApplyConfiguration())
                        .UseInternalServiceProvider(_serviceProvider);

                    using (var context = new DbContext(optionsBuilder.Options))
                    {
                        context.Database.EnsureCreated();

                        var connection = context.Database.GetDbConnection();

                        connection.Open();

                        using (var command = connection.CreateCommand())
                        {
                            command.CommandTimeout = 600;
                            command.CommandText = "ALTER DATABASE [" + connection.Database + "] SET ALLOW_SNAPSHOT_ISOLATION ON";
                            command.ExecuteNonQuery();
                        }

                        using (var command = connection.CreateCommand())
                        {
                            command.CommandTimeout = 600;
                            command.CommandText = "ALTER DATABASE [" + connection.Database + "] SET READ_COMMITTED_SNAPSHOT ON";
                            command.ExecuteNonQuery();
                        }

                        connection.Close();
                    }
                });

        public override DbContext CreateContext(SqlServerTestStore testStore)
            => new DbContext(new DbContextOptionsBuilder()
                .UseSqlServer(testStore.ConnectionString, b =>
                    {
                        b.ApplyConfiguration();
                        b.MaxBatchSize(1);
                    })
                .UseInternalServiceProvider(_serviceProvider).Options);

        public override DbContext CreateContext(DbConnection connection)
            => new DbContext(new DbContextOptionsBuilder()
                .UseSqlServer(connection, b =>
                    {
                        b.ApplyConfiguration();
                        b.MaxBatchSize(1);
                    })
                .UseInternalServiceProvider(_serviceProvider).Options);
    }
}
