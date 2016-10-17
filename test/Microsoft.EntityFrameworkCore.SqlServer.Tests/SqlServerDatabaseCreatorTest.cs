// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.FunctionalTests.Utilities;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

// ReSharper disable UnassignedGetOnlyAutoProperty

// ReSharper disable ClassNeverInstantiated.Local
// ReSharper disable MemberCanBePrivate.Local
namespace Microsoft.EntityFrameworkCore.SqlServer.Tests
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
        public async Task Create_checks_for_existence_and_retries_if_cannot_attach_file_until_it_passes()
        {
            await Create_checks_for_existence_and_retries_until_it_passes(1832, async: false);
        }

        [Fact]
        public async Task Create_checks_for_existence_and_retries_if_cannot_open_file_until_it_passes()
        {
            await Create_checks_for_existence_and_retries_until_it_passes(5120, async: false);
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

        [Fact]
        public async Task CreateAsync_checks_for_existence_and_retries_if_cannot_attach_file_until_it_passes()
        {
            await Create_checks_for_existence_and_retries_until_it_passes(1832, async: true);
        }

        [Fact]
        public async Task CreateAsync_checks_for_existence_and_retries_if_cannot_open_file_until_it_passes()
        {
            await Create_checks_for_existence_and_retries_until_it_passes(5120, async: true);
        }

        private async Task Create_checks_for_existence_and_retries_until_it_passes(int errorNumber, bool async)
        {
            var customServices = new ServiceCollection()
                .AddScoped<ISqlServerConnection, FakeSqlServerConnection>()
                .AddScoped<IRelationalCommandBuilderFactory, FakeRelationalCommandBuilderFactory>()
                .AddScoped<IExecutionStrategyFactory, ExecutionStrategyFactory>();

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
                .AddScoped<IRelationalCommandBuilderFactory, FakeRelationalCommandBuilderFactory>()
                .AddScoped<IExecutionStrategyFactory, ExecutionStrategyFactory>();

            var contextServices = SqlServerTestHelpers.Instance.CreateContextServices(customServices);

            var connection = (FakeSqlServerConnection)contextServices.GetRequiredService<ISqlServerConnection>();

            connection.ErrorNumber = 233;
            connection.FailAfter = 100;

            var creator = contextServices.GetRequiredService<IRelationalDatabaseCreator>();

            if (async)
            {
                await Assert.ThrowsAsync<SqlException>(() => creator.CreateAsync());
            }
            else
            {
                Assert.Throws<SqlException>(() => creator.Create());
            }
        }

        private class FakeSqlServerConnection : SqlServerConnection
        {
            private readonly IDbContextOptions _options;
            private readonly ILoggerFactory _loggerFactory;

            public FakeSqlServerConnection(IDbContextOptions options, ILoggerFactory loggerFactory)
                : base(options, new Logger<SqlServerConnection>(loggerFactory))
            {
                _options = options;
                _loggerFactory = loggerFactory;
            }

            public int ErrorNumber { get; set; }
            public int FailAfter { get; set; }
            public int OpenCount { get; set; }

            public override void Open()
            {
                if (++OpenCount < FailAfter)
                {
                    throw SqlExceptionFactory.CreateSqlException(ErrorNumber);
                }
            }

            public override Task OpenAsync(CancellationToken cancellationToken = new CancellationToken())
            {
                if (++OpenCount < FailAfter)
                {
                    throw SqlExceptionFactory.CreateSqlException(ErrorNumber);
                }

                return Task.FromResult(0);
            }

            public override ISqlServerConnection CreateMasterConnection() => new FakeSqlServerConnection(_options, _loggerFactory);
        }

        private class FakeRelationalCommandBuilderFactory : IRelationalCommandBuilderFactory
        {
            public IRelationalCommandBuilder Create() => new FakeRelationalCommandBuilder();
        }

        private class FakeRelationalCommandBuilder : IRelationalCommandBuilder
        {
            public IndentedStringBuilder Instance { get; } = new IndentedStringBuilder();

            public IRelationalParameterBuilder ParameterBuilder
            {
                get { throw new NotImplementedException(); }
            }

            public IRelationalCommand Build() => new FakeRelationalCommand();
        }

        private class FakeRelationalCommand : IRelationalCommand
        {
            public string CommandText { get; }

            public IReadOnlyList<IRelationalParameter> Parameters { get; }

            public IReadOnlyDictionary<string, object> ParameterValues
            {
                get { throw new NotImplementedException(); }
            }

            public int ExecuteNonQuery(IRelationalConnection connection, IReadOnlyDictionary<string, object> parameterValues)
            {
                return 0;
            }

            int IRelationalCommand.ExecuteNonQuery(IRelationalConnection connection, IReadOnlyDictionary<string, object> parameterValues, bool manageConnection)
            {
                throw new NotImplementedException();
            }

            public Task<int> ExecuteNonQueryAsync(IRelationalConnection connection, IReadOnlyDictionary<string, object> parameterValues, CancellationToken cancellationToken = default(CancellationToken))
                => Task.FromResult(0);

            Task<int> IRelationalCommand.ExecuteNonQueryAsync(IRelationalConnection connection, IReadOnlyDictionary<string, object> parameterValues, bool manageConnection, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            Task<object> IRelationalCommand.ExecuteScalarAsync(IRelationalConnection connection, IReadOnlyDictionary<string, object> parameterValues, bool manageConnection, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            public RelationalDataReader ExecuteReader(IRelationalConnection connection, IReadOnlyDictionary<string, object> parameterValues)
            {
                throw new NotImplementedException();
            }

            RelationalDataReader IRelationalCommand.ExecuteReader(IRelationalConnection connection, IReadOnlyDictionary<string, object> parameterValues, bool manageConnection)
            {
                throw new NotImplementedException();
            }

            public Task<RelationalDataReader> ExecuteReaderAsync(IRelationalConnection connection, IReadOnlyDictionary<string, object> parameterValues, CancellationToken cancellationToken = default(CancellationToken))
            {
                throw new NotImplementedException();
            }

            Task<RelationalDataReader> IRelationalCommand.ExecuteReaderAsync(IRelationalConnection connection, IReadOnlyDictionary<string, object> parameterValues, bool manageConnection, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            public object ExecuteScalar(IRelationalConnection connection, IReadOnlyDictionary<string, object> parameterValues)
            {
                throw new NotImplementedException();
            }

            object IRelationalCommand.ExecuteScalar(IRelationalConnection connection, IReadOnlyDictionary<string, object> parameterValues, bool manageConnection)
            {
                throw new NotImplementedException();
            }

            public Task<object> ExecuteScalarAsync(IRelationalConnection connection, IReadOnlyDictionary<string, object> parameterValues, CancellationToken cancellationToken = default(CancellationToken))
            {
                throw new NotImplementedException();
            }
        }
    }
}
