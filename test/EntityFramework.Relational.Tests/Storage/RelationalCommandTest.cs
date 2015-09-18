// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Tests.TestUtilities;
using Microsoft.Data.Entity.TestUtilities.FakeProvider;
using Microsoft.Framework.Logging;
using Xunit;

namespace Microsoft.Data.Entity.Tests
{
    public class RelationalCommandTest
    {
        [Fact]
        public void Configures_DbCommand()
        {
            var fakeConnection = CreateConnection();

            var relationalCommand = new RelationalCommand(
                CreateLoggerFactory(),
                new FakeRelationalTypeMapper(),
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
                CreateLoggerFactory(),
                new FakeRelationalTypeMapper(),
                "CommandText",
                new RelationalParameter[0]);

            relationalCommand.ExecuteNonQuery(fakeConnection);

            Assert.Equal(1, fakeConnection.DbConnections.Count);
            Assert.Equal(1, fakeConnection.DbConnections[0].DbCommands.Count);

            var command = fakeConnection.DbConnections[0].DbCommands[0];

            Assert.Same(relationalTransaction.GetService(), command.Transaction);
        }

        [Fact]
        public void Configures_DbCommand_with_timeout()
        {
            var fakeConnection = CreateConnection(e => e.CommandTimeout = 42);

            var relationalCommand = new RelationalCommand(
                CreateLoggerFactory(),
                new FakeRelationalTypeMapper(),
                "CommandText",
                new RelationalParameter[0]);

            relationalCommand.ExecuteNonQuery(fakeConnection);

            Assert.Equal(1, fakeConnection.DbConnections.Count);
            Assert.Equal(1, fakeConnection.DbConnections[0].DbCommands.Count);

            var command = fakeConnection.DbConnections[0].DbCommands[0];

            Assert.Equal(42, command.CommandTimeout);
        }

        [Fact]
        public void Configures_DbCommand_with_default_parameters()
        {
            var fakeConnection = CreateConnection();

            var relationalCommand = new RelationalCommand(
                CreateLoggerFactory(),
                new FakeRelationalTypeMapper(),
                "CommandText",
                new[]
                {
                    new RelationalParameter("FirstParameter", 17),
                    new RelationalParameter("SecondParameter", 18L),
                    new RelationalParameter("ThirdParameter", null)
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
            Assert.Equal(false, parameter.IsNullable);
            Assert.Equal(DbType.Int64, parameter.DbType);

            parameter = fakeConnection.DbConnections[0].DbCommands[0].Parameters[2];

            Assert.Equal("ThirdParameter", parameter.ParameterName);
            Assert.Equal(DBNull.Value, parameter.Value);
            Assert.Equal(ParameterDirection.Input, parameter.Direction);
            Assert.Equal(FakeDbParameter.DefaultDbType, parameter.DbType);
        }

        [Fact]
        public void Configures_DbCommand_with_property_parameters()
        {
            var fakeConnection = CreateConnection();

            var relationalCommand = new RelationalCommand(
                CreateLoggerFactory(),
                new FakeRelationalTypeMapper(),
                "CommandText",
                new[]
                {
                    new RelationalParameter("FirstParameter", 17, CreateProperty(typeof(int), "DefaultInt")),
                    new RelationalParameter("SecondParameter", 18L, CreateProperty(typeof(long), "DefaultLong")),
                    new RelationalParameter("ThirdParameter", 19, CreateProperty(typeof(int?), "DefaultInt")),
                    new RelationalParameter("FourthParameter", null, CreateProperty(typeof(long?), "DefaultLong")),
                });

            relationalCommand.ExecuteNonQuery(fakeConnection);

            Assert.Equal(1, fakeConnection.DbConnections.Count);
            Assert.Equal(1, fakeConnection.DbConnections[0].DbCommands.Count);
            Assert.Equal(4, fakeConnection.DbConnections[0].DbCommands[0].Parameters.Count);

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
            Assert.Equal(false, parameter.IsNullable);
            Assert.Equal(DbType.Int64, parameter.DbType);

            parameter = fakeConnection.DbConnections[0].DbCommands[0].Parameters[2];

            Assert.Equal("ThirdParameter", parameter.ParameterName);
            Assert.Equal(19, parameter.Value);
            Assert.Equal(ParameterDirection.Input, parameter.Direction);
            Assert.Equal(true, parameter.IsNullable);
            Assert.Equal(DbType.Int32, parameter.DbType);

            parameter = fakeConnection.DbConnections[0].DbCommands[0].Parameters[3];

            Assert.Equal("FourthParameter", parameter.ParameterName);
            Assert.Equal(DBNull.Value, parameter.Value);
            Assert.Equal(ParameterDirection.Input, parameter.Direction);
            Assert.Equal(true, parameter.IsNullable);
            Assert.Equal(DbType.Int64, parameter.DbType);
        }

        [Fact]
        public void Can_ExecuteNonQuery_and_log()
        {
            var log = new List<Tuple<LogLevel, string>>();
            var executeNonQueryCount = 0;
            var connectionState = ConnectionState.Closed;
            var commandText = string.Empty;

            var fakeConnection = CreateConnection(
                new FakeCommandExecutor(
                    executeNonQuery: c =>
                    {
                        executeNonQueryCount++;
                        connectionState = c.Connection.State;
                        commandText = c.CommandText;
                        return 1;
                    }));

            var relationalCommand = new RelationalCommand(
                CreateLoggerFactory(log),
                new FakeRelationalTypeMapper(),
                "ExecuteNonQuery Command",
                new RelationalParameter[0]);

            relationalCommand.ExecuteNonQuery(fakeConnection);

            Assert.Equal(1, log.Count);
            Assert.Equal(LogLevel.Verbose, log[0].Item1);
            Assert.Equal("ExecuteNonQuery Command", log[0].Item2);

            Assert.Equal(1, executeNonQueryCount);
            Assert.Equal(ConnectionState.Open, connectionState);
            Assert.Equal("ExecuteNonQuery Command", commandText);
        }

        [Fact]
        public void Can_ExecuteScalar_and_log()
        {
            var log = new List<Tuple<LogLevel, string>>();
            var executeScalarCount = 0;
            var connectionState = ConnectionState.Closed;
            var commandText = string.Empty;

            var fakeConnection = CreateConnection(
                new FakeCommandExecutor(
                    executeScalar: c =>
                    {
                        executeScalarCount++;
                        connectionState = c.Connection.State;
                        commandText = c.CommandText;
                        return "ExecuteScalar Result";
                    }));

            var relationalCommand = new RelationalCommand(
                CreateLoggerFactory(log),
                new FakeRelationalTypeMapper(),
                "ExecuteScalar Command",
                new RelationalParameter[0]);

            var result = (string)relationalCommand.ExecuteScalar(fakeConnection);

            Assert.Equal(1, log.Count);
            Assert.Equal(LogLevel.Verbose, log[0].Item1);
            Assert.Equal("ExecuteScalar Command", log[0].Item2);

            Assert.Equal(1, executeScalarCount);
            Assert.Equal(ConnectionState.Open, connectionState);
            Assert.Equal("ExecuteScalar Command", commandText);
            Assert.Equal("ExecuteScalar Result", result);
        }

        [Fact]
        public void Can_ExecuteReader_and_log()
        {
            var log = new List<Tuple<LogLevel, string>>();
            var executeReaderCount = 0;
            var connectionState = ConnectionState.Closed;
            var commandText = string.Empty;

            var dbDataReader = new FakeDbDataReader();

            var fakeConnection = CreateConnection(
                new FakeCommandExecutor(
                    executeReader: (c, b) =>
                    {
                        executeReaderCount++;
                        connectionState = c.Connection.State;
                        commandText = c.CommandText;
                        return dbDataReader;
                    }));

            var relationalCommand = new RelationalCommand(
                CreateLoggerFactory(log),
                new FakeRelationalTypeMapper(),
                "ExecuteReader Command",
                new RelationalParameter[0]);

            var result = relationalCommand.ExecuteReader(fakeConnection);

            Assert.Equal(1, log.Count);
            Assert.Equal(LogLevel.Verbose, log[0].Item1);
            Assert.Equal("ExecuteReader Command", log[0].Item2);

            Assert.Equal(1, executeReaderCount);
            Assert.Equal(ConnectionState.Open, connectionState);
            Assert.Equal("ExecuteReader Command", commandText);
            Assert.Same(dbDataReader, result);
        }

        [Fact]
        public async Task Can_ExecuteNonQueryAsync_and_log()
        {
            var log = new List<Tuple<LogLevel, string>>();
            var executeNonQueryAsyncCount = 0;
            var connectionState = ConnectionState.Closed;
            var commandText = string.Empty;

            var fakeConnection = CreateConnection(
                new FakeCommandExecutor(
                    executeNonQueryAsync: (c, ct) =>
                    {
                        executeNonQueryAsyncCount++;
                        connectionState = c.Connection.State;
                        commandText = c.CommandText;
                        return Task.FromResult(1);
                    }));

            var relationalCommand = new RelationalCommand(
                CreateLoggerFactory(log),
                new FakeRelationalTypeMapper(),
                "ExecuteNonQueryAsync Command",
                new RelationalParameter[0]);

            await relationalCommand.ExecuteNonQueryAsync(fakeConnection);

            Assert.Equal(1, log.Count);
            Assert.Equal(LogLevel.Verbose, log[0].Item1);
            Assert.Equal("ExecuteNonQueryAsync Command", log[0].Item2);

            Assert.Equal(1, executeNonQueryAsyncCount);
            Assert.Equal(ConnectionState.Open, connectionState);
            Assert.Equal("ExecuteNonQueryAsync Command", commandText);
        }

        [Fact]
        public async Task Can_ExecuteScalarAsync_and_log()
        {
            var log = new List<Tuple<LogLevel, string>>();
            var executeScalarAsyncCount = 0;
            var connectionState = ConnectionState.Closed;
            var commandText = string.Empty;

            var fakeConnection = CreateConnection(
                new FakeCommandExecutor(
                    executeScalarAsync: (c, ct) =>
                    {
                        executeScalarAsyncCount++;
                        connectionState = c.Connection.State;
                        commandText = c.CommandText;
                        return Task.FromResult<object>("ExecuteScalarAsync Result");
                    }));

            var relationalCommand = new RelationalCommand(
                CreateLoggerFactory(log),
                new FakeRelationalTypeMapper(),
                "ExecuteScalarAsync Command",
                new RelationalParameter[0]);

            var result = (string)await relationalCommand.ExecuteScalarAsync(fakeConnection);

            Assert.Equal(1, log.Count);
            Assert.Equal(LogLevel.Verbose, log[0].Item1);
            Assert.Equal("ExecuteScalarAsync Command", log[0].Item2);

            Assert.Equal(1, executeScalarAsyncCount);
            Assert.Equal(ConnectionState.Open, connectionState);
            Assert.Equal("ExecuteScalarAsync Command", commandText);
            Assert.Equal("ExecuteScalarAsync Result", result);
        }

        [Fact]
        public async Task Can_ExecuteReaderAsync_and_log()
        {
            var log = new List<Tuple<LogLevel, string>>();
            var executeReaderAsyncCount = 0;
            var connectionState = ConnectionState.Closed;
            var commandText = string.Empty;

            var dbDataReader = new FakeDbDataReader();

            var fakeConnection = CreateConnection(
                new FakeCommandExecutor(
                    executeReaderAsync: (c, b, ct) =>
                    {
                        executeReaderAsyncCount++;
                        connectionState = c.Connection.State;
                        commandText = c.CommandText;
                        return Task.FromResult<DbDataReader>(dbDataReader);
                    }));

            var relationalCommand = new RelationalCommand(
                CreateLoggerFactory(log),
                new FakeRelationalTypeMapper(),
                "ExecuteReaderAsync Command",
                new RelationalParameter[0]);

            var result = await relationalCommand.ExecuteReaderAsync(fakeConnection);

            Assert.Equal(1, log.Count);
            Assert.Equal(LogLevel.Verbose, log[0].Item1);
            Assert.Equal("ExecuteReaderAsync Command", log[0].Item2);

            Assert.Equal(1, executeReaderAsyncCount);
            Assert.Equal(ConnectionState.Open, connectionState);
            Assert.Equal("ExecuteReaderAsync Command", commandText);
            Assert.Same(dbDataReader, result);
        }

        private static IProperty CreateProperty(Type propertyType, string typeName)
        {
            var property = new Model().AddEntityType("MyType").AddProperty("MyProp", propertyType);

            property.Relational().ColumnType = typeName;

            return property;
        }

        private static FakeRelationalConnection CreateConnection()
            => FakeProviderTestHelpers.CreateConnection();


        private static FakeRelationalConnection CreateConnection(Action<FakeRelationalOptionsExtension> setup)
        {
            var optionsExtension = FakeProviderTestHelpers.CreateOptionsExtension();

            setup(optionsExtension);

            return FakeProviderTestHelpers.CreateConnection(
                FakeProviderTestHelpers.CreateOptions(
                    optionsExtension));
        }

        private static FakeRelationalConnection CreateConnection(FakeCommandExecutor commandExecutor)
            => FakeProviderTestHelpers.CreateConnection(
                FakeProviderTestHelpers.CreateOptions(
                    FakeProviderTestHelpers.CreateOptionsExtension(
                        FakeProviderTestHelpers.CreateDbConnection(commandExecutor))));

        private static ILoggerFactory CreateLoggerFactory(List<Tuple<LogLevel, string>> log = null)
            => log != null
                ? new ListLoggerFactory(log, n => n == "Microsoft.Data.Entity.Storage.RelationalCommand")
                : new ListLoggerFactory(null, n => false);
    }
}
