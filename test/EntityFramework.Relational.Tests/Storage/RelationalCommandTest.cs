// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Storage.Internal;
using Microsoft.Data.Entity.Tests.TestUtilities;
using Microsoft.Data.Entity.TestUtilities;
using Microsoft.Data.Entity.TestUtilities.FakeProvider;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Microsoft.Data.Entity.Storage
{
    public class RelationalCommandTest
    {
        [Fact]
        public void Configures_DbCommand()
        {
            var fakeConnection = CreateConnection();

            var relationalCommand = new RelationalCommand(
                new FakeSensitiveDataLogger<RelationalCommand>(),
                new DiagnosticListener("Fake"),
                "CommandText",
                new RelationalParameter[0]);

            relationalCommand.ExecuteNonQuery(fakeConnection);

            Assert.Equal(1, fakeConnection.DbConnections.Count);
            Assert.Equal(1, fakeConnection.DbConnections[0].DbCommands.Count);

            var command = fakeConnection.DbConnections[0].DbCommands[0];

            Assert.Equal("CommandText", command.CommandText);
            Assert.Null(command.Transaction);
            Assert.Equal(FakeDbCommand.DefaultCommandTimeout, command.CommandTimeout);
        }

        [Fact]
        public void Configures_DbCommand_with_transaction()
        {
            var fakeConnection = CreateConnection();

            var relationalTransaction = fakeConnection.BeginTransaction();

            var relationalCommand = new RelationalCommand(
                new FakeSensitiveDataLogger<RelationalCommand>(),
                new DiagnosticListener("Fake"),
                "CommandText",
                new RelationalParameter[0]);

            relationalCommand.ExecuteNonQuery(fakeConnection);

            Assert.Equal(1, fakeConnection.DbConnections.Count);
            Assert.Equal(1, fakeConnection.DbConnections[0].DbCommands.Count);

            var command = fakeConnection.DbConnections[0].DbCommands[0];

            Assert.Same(relationalTransaction.GetDbTransaction(), command.Transaction);
        }

        [Fact]
        public void Configures_DbCommand_with_timeout()
        {
            var optionsExtension = new FakeRelationalOptionsExtension
            {
                ConnectionString = ConnectionString,
                CommandTimeout = 42
            };

            var fakeConnection = CreateConnection(CreateOptions(optionsExtension));

            var relationalCommand = new RelationalCommand(
                new FakeSensitiveDataLogger<RelationalCommand>(),
                new DiagnosticListener("Fake"),
                "CommandText",
                new RelationalParameter[0]);

            relationalCommand.ExecuteNonQuery(fakeConnection);

            Assert.Equal(1, fakeConnection.DbConnections.Count);
            Assert.Equal(1, fakeConnection.DbConnections[0].DbCommands.Count);

            var command = fakeConnection.DbConnections[0].DbCommands[0];

            Assert.Equal(42, command.CommandTimeout);
        }

        [Fact]
        public void Configures_DbCommand_with_parameters()
        {
            var fakeConnection = CreateConnection();

            var relationalCommand = new RelationalCommand(
                new FakeSensitiveDataLogger<RelationalCommand>(),
                new DiagnosticListener("Fake"),
                "CommandText",
                new[]
                {
                    new RelationalParameter("FirstParameter", 17, new RelationalTypeMapping("int", typeof(int), DbType.Int32), false, null),
                    new RelationalParameter("SecondParameter", 18L,  new RelationalTypeMapping("long", typeof(long), DbType.Int64), true, null),
                    new RelationalParameter("ThirdParameter", null,  RelationalTypeMapping.NullMapping, null, null)
                });

            relationalCommand.ExecuteNonQuery(fakeConnection);

            Assert.Equal(1, fakeConnection.DbConnections.Count);
            Assert.Equal(1, fakeConnection.DbConnections[0].DbCommands.Count);
            Assert.Equal(3, fakeConnection.DbConnections[0].DbCommands[0].Parameters.Count);

            var parameter = fakeConnection.DbConnections[0].DbCommands[0].Parameters[0];

            Assert.Equal("FirstParameter", parameter.ParameterName);
            Assert.Equal(17, parameter.Value);
            Assert.Equal(ParameterDirection.Input, parameter.Direction);
            Assert.Equal(false, parameter.IsNullable);
            Assert.Equal(DbType.Int32, parameter.DbType);

            parameter = fakeConnection.DbConnections[0].DbCommands[0].Parameters[1];

            Assert.Equal("SecondParameter", parameter.ParameterName);
            Assert.Equal(18L, parameter.Value);
            Assert.Equal(ParameterDirection.Input, parameter.Direction);
            Assert.Equal(true, parameter.IsNullable);
            Assert.Equal(DbType.Int64, parameter.DbType);

            parameter = fakeConnection.DbConnections[0].DbCommands[0].Parameters[2];

            Assert.Equal("ThirdParameter", parameter.ParameterName);
            Assert.Equal(DBNull.Value, parameter.Value);
            Assert.Equal(ParameterDirection.Input, parameter.Direction);
            Assert.Equal(FakeDbParameter.DefaultDbType, parameter.DbType);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Can_ExecuteNonQuery(bool manageConnection)
        {
            var executeNonQueryCount = 0;
            var disposeCount = -1;

            var fakeDbConnection = new FakeDbConnection(
                ConnectionString,
                new FakeCommandExecutor(
                    executeNonQuery: c =>
                    {
                        executeNonQueryCount++;
                        disposeCount = c.DisposeCount;
                        return 1;
                    }));

            var optionsExtension = new FakeRelationalOptionsExtension { Connection = fakeDbConnection };

            var options = CreateOptions(optionsExtension);

            var fakeConnection = new FakeRelationalConnection(options);

            var relationalCommand = new RelationalCommand(
                new FakeSensitiveDataLogger<RelationalCommand>(),
                new DiagnosticListener("Fake"),
                "ExecuteNonQuery Command",
                new RelationalParameter[0]);

            relationalCommand.ExecuteNonQuery(fakeConnection, manageConnection: manageConnection);

            var expectedCount = manageConnection ? 1 : 0;
            Assert.Equal(expectedCount, fakeDbConnection.OpenCount);
            Assert.Equal(expectedCount, fakeDbConnection.CloseCount);

            // Durring command execution
            Assert.Equal(1, executeNonQueryCount);
            Assert.Equal(0, disposeCount);

            // After command execution
            Assert.Equal(1, fakeDbConnection.DbCommands[0].DisposeCount);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual async Task Can_ExecuteNonQueryAsync(bool manageConnection)
        {
            var executeNonQueryCount = 0;
            var disposeCount = -1;

            var fakeDbConnection = new FakeDbConnection(
                ConnectionString,
                new FakeCommandExecutor(
                    executeNonQueryAsync: (c, ct) =>
                    {
                        executeNonQueryCount++;
                        disposeCount = c.DisposeCount;
                        return Task.FromResult(1);
                    }));

            var optionsExtension = new FakeRelationalOptionsExtension { Connection = fakeDbConnection };

            var options = CreateOptions(optionsExtension);

            var fakeConnection = new FakeRelationalConnection(options);

            var relationalCommand = new RelationalCommand(
                new FakeSensitiveDataLogger<RelationalCommand>(),
                new DiagnosticListener("Fake"),
                "ExecuteNonQuery Command",
                new RelationalParameter[0]);

            await relationalCommand.ExecuteNonQueryAsync(fakeConnection, manageConnection: manageConnection);

            var expectedCount = manageConnection ? 1 : 0;
            Assert.Equal(expectedCount, fakeDbConnection.OpenCount);
            Assert.Equal(expectedCount, fakeDbConnection.CloseCount);

            // Durring command execution
            Assert.Equal(1, executeNonQueryCount);
            Assert.Equal(0, disposeCount);

            // After command execution
            Assert.Equal(1, fakeDbConnection.DbCommands[0].DisposeCount);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Can_ExecuteScalar(bool manageConnection)
        {
            var executeScalarCount = 0;
            var disposeCount = -1;

            var fakeDbConnection = new FakeDbConnection(
                ConnectionString,
                new FakeCommandExecutor(
                    executeScalar: c =>
                    {
                        executeScalarCount++;
                        disposeCount = c.DisposeCount;
                        return "ExecuteScalar Result";
                    }));

            var optionsExtension = new FakeRelationalOptionsExtension { Connection = fakeDbConnection };

            var options = CreateOptions(optionsExtension);

            var fakeConnection = new FakeRelationalConnection(options);

            var relationalCommand = new RelationalCommand(
                new FakeSensitiveDataLogger<RelationalCommand>(),
                new DiagnosticListener("Fake"),
                "ExecuteScalar Command",
                new RelationalParameter[0]);

            var result = (string)relationalCommand.ExecuteScalar(fakeConnection, manageConnection: manageConnection);

            Assert.Equal("ExecuteScalar Result", result);

            var expectedCount = manageConnection ? 1 : 0;
            Assert.Equal(expectedCount, fakeDbConnection.OpenCount);
            Assert.Equal(expectedCount, fakeDbConnection.CloseCount);

            // Durring command execution
            Assert.Equal(1, executeScalarCount);
            Assert.Equal(0, disposeCount);

            // After command execution
            Assert.Equal(1, fakeDbConnection.DbCommands[0].DisposeCount);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Can_ExecuteScalarAsync(bool manageConnection)
        {
            var executeScalarCount = 0;
            var disposeCount = -1;

            var fakeDbConnection = new FakeDbConnection(
                ConnectionString,
                new FakeCommandExecutor(
                    executeScalarAsync: (c, ct) =>
                    {
                        executeScalarCount++;
                        disposeCount = c.DisposeCount;
                        return Task.FromResult<object>("ExecuteScalar Result");
                    }));

            var optionsExtension = new FakeRelationalOptionsExtension { Connection = fakeDbConnection };

            var options = CreateOptions(optionsExtension);

            var fakeConnection = new FakeRelationalConnection(options);

            var relationalCommand = new RelationalCommand(
                new FakeSensitiveDataLogger<RelationalCommand>(),
                new DiagnosticListener("Fake"),
                "ExecuteScalar Command",
                new RelationalParameter[0]);

            var result = (string)await relationalCommand.ExecuteScalarAsync(fakeConnection, manageConnection: manageConnection);

            Assert.Equal("ExecuteScalar Result", result);

            var expectedCount = manageConnection ? 1 : 0;
            Assert.Equal(expectedCount, fakeDbConnection.OpenCount);
            Assert.Equal(expectedCount, fakeDbConnection.CloseCount);

            // Durring command execution
            Assert.Equal(1, executeScalarCount);
            Assert.Equal(0, disposeCount);

            // After command execution
            Assert.Equal(1, fakeDbConnection.DbCommands[0].DisposeCount);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Can_ExecuteReader(bool manageConnection)
        {
            var executeReaderCount = 0;
            var disposeCount = -1;

            var dbDataReader = new FakeDbDataReader();

            var fakeDbConnection = new FakeDbConnection(
                ConnectionString,
                new FakeCommandExecutor(
                    executeReader: (c, b) =>
                    {
                        executeReaderCount++;
                        disposeCount = c.DisposeCount;
                        return dbDataReader;
                    }));

            var optionsExtension = new FakeRelationalOptionsExtension { Connection = fakeDbConnection };

            var options = CreateOptions(optionsExtension);

            var fakeConnection = new FakeRelationalConnection(options);

            var relationalCommand = new RelationalCommand(
                new FakeSensitiveDataLogger<RelationalCommand>(),
                new DiagnosticListener("Fake"),
                "ExecuteReader Command",
                new RelationalParameter[0]);

            var result = relationalCommand.ExecuteReader(fakeConnection, manageConnection: manageConnection);

            Assert.Same(dbDataReader, result.DbDataReader);
            Assert.Equal(0, fakeDbConnection.CloseCount);

            var expectedCount = manageConnection ? 1 : 0;
            Assert.Equal(expectedCount, fakeDbConnection.OpenCount);

            // Durring command execution
            Assert.Equal(1, executeReaderCount);
            Assert.Equal(0, disposeCount);

            // After command execution
            Assert.Equal(0, dbDataReader.DisposeCount);
            Assert.Equal(0, fakeDbConnection.DbCommands[0].DisposeCount);

            // After reader dispose
            result.Dispose();
            Assert.Equal(1, dbDataReader.DisposeCount);
            Assert.Equal(1, fakeDbConnection.DbCommands[0].DisposeCount);
            Assert.Equal(expectedCount, fakeDbConnection.CloseCount);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Can_ExecuteReaderAsync(bool manageConnection)
        {
            var executeReaderCount = 0;
            var disposeCount = -1;

            var dbDataReader = new FakeDbDataReader();

            var fakeDbConnection = new FakeDbConnection(
                ConnectionString,
                new FakeCommandExecutor(
                    executeReaderAsync: (c, b, ct) =>
                    {
                        executeReaderCount++;
                        disposeCount = c.DisposeCount;
                        return Task.FromResult<DbDataReader>(dbDataReader);
                    }));

            var optionsExtension = new FakeRelationalOptionsExtension { Connection = fakeDbConnection };

            var options = CreateOptions(optionsExtension);

            var fakeConnection = new FakeRelationalConnection(options);

            var relationalCommand = new RelationalCommand(
                new FakeSensitiveDataLogger<RelationalCommand>(),
                new DiagnosticListener("Fake"),
                "ExecuteReader Command",
                new RelationalParameter[0]);

            var result = await relationalCommand.ExecuteReaderAsync(fakeConnection, manageConnection: manageConnection);

            Assert.Same(dbDataReader, result.DbDataReader);
            Assert.Equal(0, fakeDbConnection.CloseCount);

            var expectedCount = manageConnection ? 1 : 0;
            Assert.Equal(expectedCount, fakeDbConnection.OpenCount);

            // Durring command execution
            Assert.Equal(1, executeReaderCount);
            Assert.Equal(0, disposeCount);

            // After command execution
            Assert.Equal(0, dbDataReader.DisposeCount);
            Assert.Equal(0, fakeDbConnection.DbCommands[0].DisposeCount);

            // After reader dispose
            result.Dispose();
            Assert.Equal(1, dbDataReader.DisposeCount);
            Assert.Equal(1, fakeDbConnection.DbCommands[0].DisposeCount);
            Assert.Equal(expectedCount, fakeDbConnection.CloseCount);
        }

        public static TheoryData CommandActions
            => new TheoryData<Delegate, string, bool>
                {
                    {
                        new Action<RelationalCommand, bool, IRelationalConnection>( (command, manage, connection) => command.ExecuteNonQuery(connection, manageConnection: manage)),
                        "ExecuteNonQuery",
                        false
                    },
                    {
                        new Action<RelationalCommand, bool, IRelationalConnection>( (command, manage, connection) => command.ExecuteScalar(connection, manageConnection: manage)),
                        "ExecuteScalar",
                        false
                    },
                    {
                        new Action<RelationalCommand, bool, IRelationalConnection>( (command, manage, connection) => command.ExecuteReader(connection, manageConnection: manage)),
                        "ExecuteReader",
                        false
                    },
                    {
                        new Func<RelationalCommand, bool, IRelationalConnection, Task>( (command, manage, connection) => command.ExecuteNonQueryAsync(connection, manageConnection: manage)),
                        "ExecuteNonQuery",
                        true
                    },
                    {
                        new Func<RelationalCommand, bool, IRelationalConnection, Task>( (command, manage, connection) => command.ExecuteScalarAsync(connection, manageConnection: manage)),
                        "ExecuteScalar",
                        true
                    },
                    {
                        new Func<RelationalCommand, bool, IRelationalConnection, Task>( (command, manage, connection) => command.ExecuteReaderAsync(connection, manageConnection: manage)),
                        "ExecuteReader",
                        true
                    }
                };

        [Theory]
        [MemberData(nameof(CommandActions))]
        public async Task Disposes_command_on_exception(
            Delegate commandDelegate,
            string telemetryName,
            bool async)
        {
            var exception = new InvalidOperationException();

            var fakeDbConnection = new FakeDbConnection(
                ConnectionString,
                new FakeCommandExecutor(
                    c => { throw exception; },
                    c => { throw exception; },
                    (c, cb) => { throw exception; },
                    (c, ct) => { throw exception; },
                    (c, ct) => { throw exception; },
                    (c, cb, ct) => { throw exception; }));

            var optionsExtension = new FakeRelationalOptionsExtension { Connection = fakeDbConnection };

            var options = CreateOptions(optionsExtension);

            var fakeConnection = new FakeRelationalConnection(options);

            var relationalCommand = new RelationalCommand(
                new FakeSensitiveDataLogger<RelationalCommand>(),
                new DiagnosticListener("Fake"),
                "ExecuteReader Command",
                new RelationalParameter[0]);

            if (async)
            {
                await Assert.ThrowsAsync<InvalidOperationException>(
                    async ()
                        => await ((Func<RelationalCommand, bool, IRelationalConnection, Task>)commandDelegate)(relationalCommand, true, fakeConnection));
            }
            else
            {
                Assert.Throws<InvalidOperationException>(()
                    => ((Action<RelationalCommand, bool, IRelationalConnection>)commandDelegate)(relationalCommand, true, fakeConnection));
            }

            Assert.Equal(1, fakeDbConnection.DbCommands[0].DisposeCount);
        }

        [Theory]
        [MemberData(nameof(CommandActions))]
        public async Task Closes_managed_connections_on_exception(
            Delegate commandDelegate,
            string telemetryName,
            bool async)
        {
            var exception = new InvalidOperationException();

            var fakeDbConnection = new FakeDbConnection(
                ConnectionString,
                new FakeCommandExecutor(
                    c => { throw exception; },
                    c => { throw exception; },
                    (c, cb) => { throw exception; },
                    (c, ct) => { throw exception; },
                    (c, ct) => { throw exception; },
                    (c, cb, ct) => { throw exception; }));

            var optionsExtension = new FakeRelationalOptionsExtension { Connection = fakeDbConnection };

            var options = CreateOptions(optionsExtension);

            var fakeConnection = new FakeRelationalConnection(options);

            var relationalCommand = new RelationalCommand(
                new FakeSensitiveDataLogger<RelationalCommand>(),
                new DiagnosticListener("Fake"),
                "ExecuteReader Command",
                new RelationalParameter[0]);

            if (async)
            {
                await Assert.ThrowsAsync<InvalidOperationException>(
                    async ()
                        => await ((Func<RelationalCommand, bool, IRelationalConnection, Task>)commandDelegate)(relationalCommand, true, fakeConnection));

                Assert.Equal(1, fakeDbConnection.OpenAsyncCount);
            }
            else
            {
                Assert.Throws<InvalidOperationException>(()
                    => ((Action<RelationalCommand, bool, IRelationalConnection>)commandDelegate)(relationalCommand, true, fakeConnection));

                Assert.Equal(1, fakeDbConnection.OpenCount);
            }

            Assert.Equal(1, fakeDbConnection.CloseCount);
        }

        [Theory]
        [MemberData(nameof(CommandActions))]
        public async Task Does_not_close_unmanaged_connections_on_exception(
            Delegate commandDelegate,
            string telemetryName,
            bool async)
        {
            var exception = new InvalidOperationException();

            var fakeDbConnection = new FakeDbConnection(
                ConnectionString,
                new FakeCommandExecutor(
                    c => { throw exception; },
                    c => { throw exception; },
                    (c, cb) => { throw exception; },
                    (c, ct) => { throw exception; },
                    (c, ct) => { throw exception; },
                    (c, cb, ct) => { throw exception; }));

            var optionsExtension = new FakeRelationalOptionsExtension { Connection = fakeDbConnection };

            var options = CreateOptions(optionsExtension);

            var fakeConnection = new FakeRelationalConnection(options);

            var relationalCommand = new RelationalCommand(
                new FakeSensitiveDataLogger<RelationalCommand>(),
                new DiagnosticListener("Fake"),
                "ExecuteReader Command",
                new RelationalParameter[0]);

            if (async)
            {
                await Assert.ThrowsAsync<InvalidOperationException>(
                    async ()
                        => await ((Func<RelationalCommand, bool, IRelationalConnection, Task>)commandDelegate)(relationalCommand, false, fakeConnection));

                Assert.Equal(0, fakeDbConnection.OpenAsyncCount);
            }
            else
            {
                Assert.Throws<InvalidOperationException>(()
                    => ((Action<RelationalCommand, bool, IRelationalConnection>)commandDelegate)(relationalCommand, false, fakeConnection));

                Assert.Equal(0, fakeDbConnection.OpenCount);
            }

            Assert.Equal(0, fakeDbConnection.CloseCount);
        }


        [Theory]
        [MemberData(nameof(CommandActions))]
        public async Task Logs_commands_without_parameter_values(
            Delegate commandDelegate,
            string diagnosticName,
            bool async)
        {
            var options = CreateOptions();

            var fakeConnection = new FakeRelationalConnection(options);

            var log = new List<Tuple<LogLevel, string>>();

            var relationalCommand = new RelationalCommand(
                new SensitiveDataLogger<RelationalCommand>(
                    new ListLogger<RelationalCommand>(log),
                    options),
                new DiagnosticListener("Fake"),
                "Command Text",
                new[]
                {
                    new RelationalParameter("FirstParameter", 17, new RelationalTypeMapping("int", typeof(int), DbType.Int32), false, null)
                });

            if (async)
            {
                await ((Func<RelationalCommand, bool, IRelationalConnection, Task>)commandDelegate)(relationalCommand, true, fakeConnection);
            }
            else
            {
                ((Action<RelationalCommand, bool, IRelationalConnection>)commandDelegate)(relationalCommand, true, fakeConnection);
            }

            Assert.Equal(1, log.Count);
            Assert.Equal(LogLevel.Information, log[0].Item1);
            Assert.EndsWith(
                @"[Parameters=[FirstParameter='?'], CommandType='0', CommandTimeout='30']
Command Text",
                log[0].Item2);
        }

        [Theory]
        [MemberData(nameof(CommandActions))]
        public async Task Logs_commands_parameter_values(
            Delegate commandDelegate,
            string diagnosticName,
            bool async)
        {
            var optionsExtension = new FakeRelationalOptionsExtension
            {
                ConnectionString = ConnectionString
            };

            var options = CreateOptions(optionsExtension, logParameters: true);

            var fakeConnection = new FakeRelationalConnection(options);

            var log = new List<Tuple<LogLevel, string>>();

            var relationalCommand = new RelationalCommand(
                new SensitiveDataLogger<RelationalCommand>(
                    new ListLogger<RelationalCommand>(log),
                    options),
                new DiagnosticListener("Fake"),
                "Command Text",
                new[]
                {
                    new RelationalParameter("FirstParameter", 17, new RelationalTypeMapping("int", typeof(int), DbType.Int32), false, null)
                });

            if (async)
            {
                await ((Func<RelationalCommand, bool, IRelationalConnection, Task>)commandDelegate)(relationalCommand, true, fakeConnection);
            }
            else
            {
                ((Action<RelationalCommand, bool, IRelationalConnection>)commandDelegate)(relationalCommand, true, fakeConnection);
            }

            Assert.Equal(2, log.Count);
            Assert.Equal(LogLevel.Warning, log[0].Item1);
            Assert.Equal(CoreStrings.SensitiveDataLoggingEnabled, log[0].Item2);

            Assert.Equal(LogLevel.Information, log[1].Item1);
            Assert.EndsWith(
                @"ms) [Parameters=[FirstParameter='17'], CommandType='0', CommandTimeout='30']
Command Text",
                log[1].Item2);
        }

        [Theory]
        [MemberData(nameof(CommandActions))]
        public async Task Reports_command_diagnostic(
            Delegate commandDelegate,
            string diagnosticName,
            bool async)
        {
            var options = CreateOptions();

            var fakeConnection = new FakeRelationalConnection(options);

            var diagnostic = new List<Tuple<string, object>>();

            var relationalCommand = new RelationalCommand(
                new SensitiveDataLogger<RelationalCommand>(
                    new FakeSensitiveDataLogger<RelationalCommand>(),
                    options),
                new ListDiagnosticSource(diagnostic),
                "Command Text",
                new[]
                {
                    new RelationalParameter("FirstParameter", 17, new RelationalTypeMapping("int", typeof(int), DbType.Int32), false, null)
                });

            if (async)
            {
                await ((Func<RelationalCommand, bool, IRelationalConnection, Task>)commandDelegate)(relationalCommand, true, fakeConnection);
            }
            else
            {
                ((Action<RelationalCommand, bool, IRelationalConnection>)commandDelegate)(relationalCommand, true, fakeConnection);
            }

            Assert.Equal(2, diagnostic.Count);
            Assert.Equal(RelationalDiagnostics.BeforeExecuteCommand, diagnostic[0].Item1);
            Assert.Equal(RelationalDiagnostics.AfterExecuteCommand, diagnostic[1].Item1);

            dynamic beforeData = diagnostic[0].Item2;
            dynamic afterData = diagnostic[1].Item2;

            Assert.Equal(fakeConnection.DbConnections[0].DbCommands[0], beforeData.Command);
            Assert.Equal(fakeConnection.DbConnections[0].DbCommands[0], afterData.Command);

            Assert.Equal(diagnosticName, beforeData.ExecuteMethod);
            Assert.Equal(diagnosticName, afterData.ExecuteMethod);

            Assert.Equal(async, beforeData.IsAsync);
            Assert.Equal(async, afterData.IsAsync);
        }

        [Theory]
        [MemberData(nameof(CommandActions))]
        public async Task Reports_command_diagnostic_on_exception(
            Delegate commandDelegate,
            string diagnosticName,
            bool async)
        {
            var exception = new InvalidOperationException();

            var fakeDbConnection = new FakeDbConnection(
                ConnectionString,
                new FakeCommandExecutor(
                    c => { throw exception; },
                    c => { throw exception; },
                    (c, cb) => { throw exception; },
                    (c, ct) => { throw exception; },
                    (c, ct) => { throw exception; },
                    (c, cb, ct) => { throw exception; }));

            var optionsExtension = new FakeRelationalOptionsExtension { Connection = fakeDbConnection };

            var options = CreateOptions(optionsExtension);

            var fakeConnection = new FakeRelationalConnection(options);

            var diagnostic = new List<Tuple<string, object>>();

            var relationalCommand = new RelationalCommand(
                new SensitiveDataLogger<RelationalCommand>(
                    new FakeSensitiveDataLogger<RelationalCommand>(),
                    options),
                new ListDiagnosticSource(diagnostic),
                "Command Text",
                new[]
                {
                    new RelationalParameter("FirstParameter", 17, new RelationalTypeMapping("int", typeof(int), DbType.Int32), false, null)
                });

            if (async)
            {
                await Assert.ThrowsAsync<InvalidOperationException>(
                    async ()
                        => await ((Func<RelationalCommand, bool, IRelationalConnection, Task>)commandDelegate)(relationalCommand, true, fakeConnection));
            }
            else
            {
                Assert.Throws<InvalidOperationException>(()
                    => ((Action<RelationalCommand, bool, IRelationalConnection>)commandDelegate)(relationalCommand, true, fakeConnection));
            }

            Assert.Equal(2, diagnostic.Count);
            Assert.Equal(RelationalDiagnostics.BeforeExecuteCommand, diagnostic[0].Item1);
            Assert.Equal(RelationalDiagnostics.CommandExecutionError, diagnostic[1].Item1);

            dynamic beforeData = diagnostic[0].Item2;
            dynamic afterData = diagnostic[1].Item2;

            Assert.Equal(fakeDbConnection.DbCommands[0], beforeData.Command);
            Assert.Equal(fakeDbConnection.DbCommands[0], afterData.Command);

            Assert.Equal(diagnosticName, beforeData.ExecuteMethod);
            Assert.Equal(diagnosticName, afterData.ExecuteMethod);

            Assert.Equal(async, beforeData.IsAsync);
            Assert.Equal(async, afterData.IsAsync);

            Assert.Equal(exception, afterData.Exception);
        }

        private const string ConnectionString = "Fake Connection String";

        private static FakeRelationalConnection CreateConnection(IDbContextOptions options = null)
            => new FakeRelationalConnection(options ?? CreateOptions());

        public static IDbContextOptions CreateOptions(
            FakeRelationalOptionsExtension optionsExtension = null, bool logParameters = false)
        {
            var optionsBuilder = new DbContextOptionsBuilder();

            if (logParameters)
            {
                optionsBuilder.EnableSensitiveDataLogging();
            }

            ((IDbContextOptionsBuilderInfrastructure)optionsBuilder)
                .AddOrUpdateExtension(optionsExtension ?? new FakeRelationalOptionsExtension { ConnectionString = ConnectionString });

            return optionsBuilder.Options;
        }
    }
}
