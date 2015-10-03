// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Storage.Internal;
using Microsoft.Data.Entity.Tests;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

// ReSharper disable ClassNeverInstantiated.Local
// ReSharper disable MemberCanBePrivate.Local

namespace Microsoft.Data.Entity.SqlServer.Tests
{
    public class SqlServerDatabaseCreatorTest
    {
        [Fact]
        public async Task Create_checks_for_existence_and_retries_if_no_proccess_until_it_passes()
        {
            await Create_checks_for_existence_and_retries_until_it_passes(233, async: false);
        }

        [Fact]
        public async Task Create_checks_for_existence_and_retries_if_timeout_until_it_passes()
        {
            await Create_checks_for_existence_and_retries_until_it_passes(-2, async: false);
        }

        [Fact]
        public async Task Create_checks_for_existence_and_retries_if_cannot_open_until_it_passes()
        {
            await Create_checks_for_existence_and_retries_until_it_passes(4060, async: false);
        }

        [Fact]
        public async Task CreateAsync_checks_for_existence_and_retries_if_no_proccess_until_it_passes()
        {
            await Create_checks_for_existence_and_retries_until_it_passes(233, async: true);
        }

        [Fact]
        public async Task CreateAsync_checks_for_existence_and_retries_if_timeout_until_it_passes()
        {
            await Create_checks_for_existence_and_retries_until_it_passes(-2, async: true);
        }

        [Fact]
        public async Task CreateAsync_checks_for_existence_and_retries_if_cannot_open_until_it_passes()
        {
            await Create_checks_for_existence_and_retries_until_it_passes(4060, async: true);
        }

        private async Task Create_checks_for_existence_and_retries_until_it_passes(int errorNumber, bool async)
        {
            var customServices = new ServiceCollection()
                .AddScoped<ISqlServerConnection, FakeSqlServerConnection>()
                .AddScoped<ISqlStatementExecutor, FakeSqlStatementExecutor>();

            var contextServices = SqlServerTestHelpers.Instance.CreateContextServices(customServices);

            var connection = (FakeSqlServerConnection)contextServices.GetRequiredService<ISqlServerConnection>();

            connection.ErrorNumber = errorNumber;
            connection.FailAfter = 5;

            var creator = contextServices.GetRequiredService<IRelationalDatabaseCreator>();

            if (async)
            {
                await creator.CreateAsync();
            }
            else
            {
                creator.Create();
            }

            Assert.Equal(5, connection.OpenCount);
        }

        [Fact]
        public async Task Create_checks_for_existence_and_ultimately_gives_up_waiting()
        {
            await Create_checks_for_existence_and_ultimately_gives_up_waiting_test(async: false);
        }

        [Fact]
        public async Task CreateAsync_checks_for_existence_and_ultimately_gives_up_waiting()
        {
            await Create_checks_for_existence_and_ultimately_gives_up_waiting_test(async: true);
        }

        private async Task Create_checks_for_existence_and_ultimately_gives_up_waiting_test(bool async)
        {
            var customServices = new ServiceCollection()
                .AddScoped<ISqlServerConnection, FakeSqlServerConnection>()
                .AddScoped<ISqlStatementExecutor, FakeSqlStatementExecutor>();

            var contextServices = SqlServerTestHelpers.Instance.CreateContextServices(customServices);

            var connection = (FakeSqlServerConnection)contextServices.GetRequiredService<ISqlServerConnection>();

            connection.ErrorNumber = 233;
            connection.FailAfter = 100;

            var creator = contextServices.GetRequiredService<IRelationalDatabaseCreator>();

            if (async)
            {
                await Assert.ThrowsAsync<SqlException>(async () => await creator.CreateAsync());
            }
            else
            {
                Assert.Throws<SqlException>(() => creator.Create());
            }
        }

        private class FakeSqlServerConnection : SqlServerConnection
        {
            public FakeSqlServerConnection(IDbContextOptions options, ILoggerFactory loggerFactory)
                : base(options, new Logger<SqlServerConnection>(loggerFactory))
            {
            }

            public int ErrorNumber { get; set; }
            public int FailAfter { get; set; }
            public int OpenCount { get; set; }

            public override void Open()
            {
                if (++OpenCount < FailAfter)
                {
                    throw CreateSqlException(ErrorNumber);
                }
            }

            public override Task OpenAsync(CancellationToken cancellationToken = new CancellationToken())
            {
                if (++OpenCount < FailAfter)
                {
                    throw CreateSqlException(ErrorNumber);
                }

                return Task.FromResult(0);
            }
        }

        private class FakeSqlStatementExecutor : SqlStatementExecutor
        {
            public FakeSqlStatementExecutor(
                IRelationalCommandBuilderFactory commandBuilderFactory,
                ISensitiveDataLogger<FakeSqlStatementExecutor> logger)
                : base(commandBuilderFactory, logger)
            {
            }

            public override void ExecuteNonQuery(IRelationalConnection connection, IEnumerable<RelationalCommand> relationalCommands)
            {
            }

            public override Task ExecuteNonQueryAsync(
                IRelationalConnection connection,
                IEnumerable<RelationalCommand> relationalCommands,
                CancellationToken cancellationToken = default(CancellationToken))
                => Task.FromResult(0);
        }

        private static SqlException CreateSqlException(int number)
        {
            var error = (SqlError)Activator.CreateInstance(
                typeof(SqlError), BindingFlags.Instance | BindingFlags.NonPublic, null,
                new object[] { number, (byte)0, (byte)0, "Server", "ErrorMessage", "Procedure", 0 }, null);

            var errors = (SqlErrorCollection)Activator.CreateInstance(
                typeof(SqlErrorCollection), BindingFlags.Instance | BindingFlags.NonPublic, null,
                null, null);

            typeof(SqlErrorCollection).GetTypeInfo().GetRuntimeMethods().Single(m => m.Name == "Add").Invoke(errors, new object[] { error });

            return (SqlException)Activator.CreateInstance(
                typeof(SqlException), BindingFlags.Instance | BindingFlags.NonPublic, null,
                new object[] { "Bang!", errors, null, Guid.NewGuid() }, null);
        }
    }
}
