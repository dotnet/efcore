// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.TestUtilities.FakeProvider;
using Microsoft.Extensions.Logging;
using Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Storage
{
    using CommandAction = Action<
        IRelationalConnection,
        IRelationalCommand,
        IReadOnlyDictionary<string, object>,
        IDiagnosticsLogger<DbLoggerCategory.Database.Command>>;
    using CommandFunc = Func<
        IRelationalConnection,
        IRelationalCommand,
        IReadOnlyDictionary<string, object>,
        IDiagnosticsLogger<DbLoggerCategory.Database.Command>,
        Task>;

    public class RelationalCommandTest
    {
        private static readonly string _eol = Environment.NewLine;

        [ConditionalFact]
        public void Configures_DbCommand()
        {
            var fakeConnection = CreateConnection();
            var relationalCommand = CreateRelationalCommand(commandText: "CommandText");

            relationalCommand.ExecuteNonQuery(
                new RelationalCommandParameterObject(fakeConnection, null, null, null));

            Assert.Equal(1, fakeConnection.DbConnections.Count);
            Assert.Equal(1, fakeConnection.DbConnections[0].DbCommands.Count);

            var command = fakeConnection.DbConnections[0].DbCommands[0];

            Assert.Equal("CommandText", command.CommandText);
            Assert.Null(command.Transaction);
            Assert.Equal(FakeDbCommand.DefaultCommandTimeout, command.CommandTimeout);
        }

        [ConditionalFact]
        public void Configures_DbCommand_with_transaction()
        {
            var fakeConnection = CreateConnection();

            var relationalTransaction = fakeConnection.BeginTransaction();

            var relationalCommand = CreateRelationalCommand();

            relationalCommand.ExecuteNonQuery(
                new RelationalCommandParameterObject(fakeConnection, null, null, null));

            Assert.Equal(1, fakeConnection.DbConnections.Count);
            Assert.Equal(1, fakeConnection.DbConnections[0].DbCommands.Count);

            var command = fakeConnection.DbConnections[0].DbCommands[0];

            Assert.Same(relationalTransaction.GetDbTransaction(), command.Transaction);
        }

        [ConditionalFact]
        public void Configures_DbCommand_with_timeout()
        {
            var optionsExtension = new FakeRelationalOptionsExtension()
                .WithConnectionString(ConnectionString)
                .WithCommandTimeout(42);

            var fakeConnection = CreateConnection(CreateOptions(optionsExtension));

            var relationalCommand = CreateRelationalCommand();

            relationalCommand.ExecuteNonQuery(
                new RelationalCommandParameterObject(fakeConnection, null, null, null));

            Assert.Equal(1, fakeConnection.DbConnections.Count);
            Assert.Equal(1, fakeConnection.DbConnections[0].DbCommands.Count);

            var command = fakeConnection.DbConnections[0].DbCommands[0];

            Assert.Equal(42, command.CommandTimeout);
        }

        [ConditionalFact]
        public void Can_ExecuteNonQuery()
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

            var optionsExtension = new FakeRelationalOptionsExtension().WithConnection(fakeDbConnection);

            var options = CreateOptions(optionsExtension);

            var relationalCommand = CreateRelationalCommand();

            var result = relationalCommand.ExecuteNonQuery(
                new RelationalCommandParameterObject(
                    new FakeRelationalConnection(options), null, null, null));

            Assert.Equal(1, result);

            var expectedCount = 1;
            Assert.Equal(expectedCount, fakeDbConnection.OpenCount);
            Assert.Equal(expectedCount, fakeDbConnection.CloseCount);

            // During command execution
            Assert.Equal(1, executeNonQueryCount);
            Assert.Equal(0, disposeCount);

            // After command execution
            Assert.Equal(1, fakeDbConnection.DbCommands[0].DisposeCount);
        }

        [ConditionalFact]
        public virtual async Task Can_ExecuteNonQueryAsync()
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

            var optionsExtension = new FakeRelationalOptionsExtension().WithConnection(fakeDbConnection);

            var options = CreateOptions(optionsExtension);

            var relationalCommand = CreateRelationalCommand();

            var result = await relationalCommand.ExecuteNonQueryAsync(
                new RelationalCommandParameterObject(
                    new FakeRelationalConnection(options), null, null, null));

            Assert.Equal(1, result);

            var expectedCount = 1;
            Assert.Equal(expectedCount, fakeDbConnection.OpenCount);
            Assert.Equal(expectedCount, fakeDbConnection.CloseCount);

            // During command execution
            Assert.Equal(1, executeNonQueryCount);
            Assert.Equal(0, disposeCount);

            // After command execution
            Assert.Equal(1, fakeDbConnection.DbCommands[0].DisposeCount);
        }

        [ConditionalFact]
        public void Can_ExecuteScalar()
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

            var optionsExtension = new FakeRelationalOptionsExtension().WithConnection(fakeDbConnection);

            var options = CreateOptions(optionsExtension);

            var relationalCommand = CreateRelationalCommand();

            var result = (string)relationalCommand.ExecuteScalar(
                new RelationalCommandParameterObject(
                    new FakeRelationalConnection(options), null, null, null));

            Assert.Equal("ExecuteScalar Result", result);

            var expectedCount = 1;
            Assert.Equal(expectedCount, fakeDbConnection.OpenCount);
            Assert.Equal(expectedCount, fakeDbConnection.CloseCount);

            // During command execution
            Assert.Equal(1, executeScalarCount);
            Assert.Equal(0, disposeCount);

            // After command execution
            Assert.Equal(1, fakeDbConnection.DbCommands[0].DisposeCount);
        }

        [ConditionalFact]
        public async Task Can_ExecuteScalarAsync()
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

            var optionsExtension = new FakeRelationalOptionsExtension().WithConnection(fakeDbConnection);

            var options = CreateOptions(optionsExtension);

            var relationalCommand = CreateRelationalCommand();

            var result = (string)await relationalCommand.ExecuteScalarAsync(
                new RelationalCommandParameterObject(
                    new FakeRelationalConnection(options), null, null, null));

            Assert.Equal("ExecuteScalar Result", result);

            var expectedCount = 1;
            Assert.Equal(expectedCount, fakeDbConnection.OpenCount);
            Assert.Equal(expectedCount, fakeDbConnection.CloseCount);

            // During command execution
            Assert.Equal(1, executeScalarCount);
            Assert.Equal(0, disposeCount);

            // After command execution
            Assert.Equal(1, fakeDbConnection.DbCommands[0].DisposeCount);
        }

        [ConditionalFact]
        public void Can_ExecuteReader()
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

            var optionsExtension = new FakeRelationalOptionsExtension().WithConnection(fakeDbConnection);

            var options = CreateOptions(optionsExtension);

            var relationalCommand = CreateRelationalCommand();

            var result = relationalCommand.ExecuteReader(
                new RelationalCommandParameterObject(
                    new FakeRelationalConnection(options), null, null, null));

            Assert.Same(dbDataReader, result.DbDataReader);
            Assert.Equal(0, fakeDbConnection.CloseCount);

            var expectedCount = 1;
            Assert.Equal(expectedCount, fakeDbConnection.OpenCount);

            // During command execution
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

        [ConditionalFact]
        public async Task Can_ExecuteReaderAsync()
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

            var optionsExtension = new FakeRelationalOptionsExtension().WithConnection(fakeDbConnection);

            var options = CreateOptions(optionsExtension);

            var relationalCommand = CreateRelationalCommand();

            var result = await relationalCommand.ExecuteReaderAsync(
                new RelationalCommandParameterObject(
                    new FakeRelationalConnection(options), null, null, null));

            Assert.Same(dbDataReader, result.DbDataReader);
            Assert.Equal(0, fakeDbConnection.CloseCount);

            var expectedCount = 1;
            Assert.Equal(expectedCount, fakeDbConnection.OpenCount);

            // During command execution
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
            => new TheoryData<Delegate, DbCommandMethod, bool>
            {
                {
                    new CommandAction(
                        (connection, command, parameterValues, logger)
                            => command.ExecuteNonQuery(
                                new RelationalCommandParameterObject(connection, parameterValues, null, logger))),
                    DbCommandMethod.ExecuteNonQuery,
                    false
                },
                {
                    new CommandAction(
                        (connection, command, parameterValues, logger)
                            => command.ExecuteScalar(
                                new RelationalCommandParameterObject(connection, parameterValues, null, logger))),
                    DbCommandMethod.ExecuteScalar,
                    false
                },
                {
                    new CommandAction(
                        (connection, command, parameterValues, logger)
                            => command.ExecuteReader(
                                new RelationalCommandParameterObject(connection, parameterValues, null, logger))),
                    DbCommandMethod.ExecuteReader,
                    false
                },
                {
                    new CommandFunc(
                        (connection, command, parameterValues, logger)
                            => command.ExecuteNonQueryAsync(
                                new RelationalCommandParameterObject(connection, parameterValues, null, logger))),
                    DbCommandMethod.ExecuteNonQuery,
                    true
                },
                {
                    new CommandFunc(
                        (connection, command, parameterValues, logger)
                            => command.ExecuteScalarAsync(
                                new RelationalCommandParameterObject(connection, parameterValues, null, logger))),
                    DbCommandMethod.ExecuteScalar,
                    true
                },
                {
                    new CommandFunc(
                        (connection, command, parameterValues, logger)
                            => command.ExecuteReaderAsync(
                                new RelationalCommandParameterObject(connection, parameterValues, null, logger))),
                    DbCommandMethod.ExecuteReader,
                    true
                }
            };

        [ConditionalTheory]
        [MemberData(nameof(CommandActions))]
        public async Task Throws_when_parameters_are_configured_and_parameter_values_is_null(
            Delegate commandDelegate,
            string telemetryName,
            bool async)
        {
            var fakeConnection = CreateConnection();

            var relationalCommand = CreateRelationalCommand(
                parameters: new[]
                {
                    new TypeMappedRelationalParameter("FirstInvariant", "FirstParameter", new IntTypeMapping("int", DbType.Int32), false),
                    new TypeMappedRelationalParameter(
                        "SecondInvariant", "SecondParameter", new LongTypeMapping("long", DbType.Int64), true),
                    new TypeMappedRelationalParameter("ThirdInvariant", "ThirdParameter", RelationalTypeMapping.NullMapping, null)
                });

            if (async)
            {
                Assert.Equal(
                    RelationalStrings.MissingParameterValue("FirstInvariant"),
                    (await Assert.ThrowsAsync<InvalidOperationException>(
                        async ()
                            => await ((CommandFunc)commandDelegate)(fakeConnection, relationalCommand, null, null))).Message);
            }
            else
            {
                Assert.Equal(
                    RelationalStrings.MissingParameterValue("FirstInvariant"),
                    Assert.Throws<InvalidOperationException>(
                            ()
                                => ((CommandAction)commandDelegate)(fakeConnection, relationalCommand, null, null))
                        .Message);
            }
        }

        [ConditionalTheory]
        [MemberData(nameof(CommandActions))]
        public async Task Throws_when_parameters_are_configured_and_value_is_missing(
            Delegate commandDelegate,
            string telemetryName,
            bool async)
        {
            var fakeConnection = CreateConnection();

            var relationalCommand = CreateRelationalCommand(
                parameters: new[]
                {
                    new TypeMappedRelationalParameter("FirstInvariant", "FirstParameter", new IntTypeMapping("int", DbType.Int32), false),
                    new TypeMappedRelationalParameter(
                        "SecondInvariant", "SecondParameter", new LongTypeMapping("long", DbType.Int64), true),
                    new TypeMappedRelationalParameter("ThirdInvariant", "ThirdParameter", RelationalTypeMapping.NullMapping, null)
                });

            var parameterValues = new Dictionary<string, object>
            {
                { "FirstInvariant", 17 },
                { "SecondInvariant", 18L }
            };

            if (async)
            {
                Assert.Equal(
                    RelationalStrings.MissingParameterValue("ThirdInvariant"),
                    (await Assert.ThrowsAsync<InvalidOperationException>(
                        async ()
                            => await ((CommandFunc)commandDelegate)(fakeConnection, relationalCommand, parameterValues, null))).Message);
            }
            else
            {
                Assert.Equal(
                    RelationalStrings.MissingParameterValue("ThirdInvariant"),
                    Assert.Throws<InvalidOperationException>(
                            ()
                                => ((CommandAction)commandDelegate)(fakeConnection, relationalCommand, parameterValues, null))
                        .Message);
            }
        }

        [ConditionalTheory]
        [MemberData(nameof(CommandActions))]
        public async Task Configures_DbCommand_with_type_mapped_parameters(
            Delegate commandDelegate,
            string telemetryName,
            bool async)
        {
            var fakeConnection = CreateConnection();

            var relationalCommand = CreateRelationalCommand(
                parameters: new[]
                {
                    new TypeMappedRelationalParameter("FirstInvariant", "FirstParameter", new IntTypeMapping("int", DbType.Int32), false),
                    new TypeMappedRelationalParameter(
                        "SecondInvariant", "SecondParameter", new LongTypeMapping("long", DbType.Int64), true),
                    new TypeMappedRelationalParameter("ThirdInvariant", "ThirdParameter", RelationalTypeMapping.NullMapping, null)
                });

            var parameterValues = new Dictionary<string, object>
            {
                { "FirstInvariant", 17 },
                { "SecondInvariant", 18L },
                { "ThirdInvariant", null }
            };

            if (async)
            {
                await ((CommandFunc)commandDelegate)(fakeConnection, relationalCommand, parameterValues, null);
            }
            else
            {
                ((CommandAction)commandDelegate)(fakeConnection, relationalCommand, parameterValues, null);
            }

            Assert.Equal(1, fakeConnection.DbConnections.Count);
            Assert.Equal(1, fakeConnection.DbConnections[0].DbCommands.Count);
            Assert.Equal(3, fakeConnection.DbConnections[0].DbCommands[0].Parameters.Count);

            var parameter = fakeConnection.DbConnections[0].DbCommands[0].Parameters[0];

            Assert.Equal("FirstParameter", parameter.ParameterName);
            Assert.Equal(17, parameter.Value);
            Assert.Equal(ParameterDirection.Input, parameter.Direction);
            Assert.False(parameter.IsNullable);
            Assert.Equal(DbType.Int32, parameter.DbType);

            parameter = fakeConnection.DbConnections[0].DbCommands[0].Parameters[1];

            Assert.Equal("SecondParameter", parameter.ParameterName);
            Assert.Equal(18L, parameter.Value);
            Assert.Equal(ParameterDirection.Input, parameter.Direction);
            Assert.True(parameter.IsNullable);
            Assert.Equal(DbType.Int64, parameter.DbType);

            parameter = fakeConnection.DbConnections[0].DbCommands[0].Parameters[2];

            Assert.Equal("ThirdParameter", parameter.ParameterName);
            Assert.Equal(DBNull.Value, parameter.Value);
            Assert.Equal(ParameterDirection.Input, parameter.Direction);
            Assert.Equal(FakeDbParameter.DefaultIsNullable, parameter.IsNullable);
            Assert.Equal(FakeDbParameter.DefaultDbType, parameter.DbType);
        }

        [ConditionalTheory]
        [MemberData(nameof(CommandActions))]
        public async Task Configures_DbCommand_with_dynamic_parameters(
            Delegate commandDelegate,
            string telemetryName,
            bool async)
        {
            var fakeConnection = CreateConnection();

            var typeMapper = (IRelationalTypeMappingSource)new TestRelationalTypeMappingSource(
                TestServiceFactory.Instance.Create<TypeMappingSourceDependencies>(),
                TestServiceFactory.Instance.Create<RelationalTypeMappingSourceDependencies>());

            var dbParameter = new FakeDbParameter
            {
                ParameterName = "FirstParameter",
                Value = 17,
                DbType = DbType.Int32
            };

            var relationalCommand = CreateRelationalCommand(
                parameters: new[]
                {
                    new DynamicRelationalParameter("FirstInvariant", "FirstParameter", typeMapper),
                    new DynamicRelationalParameter("SecondInvariant", "SecondParameter", typeMapper),
                    new DynamicRelationalParameter("ThirdInvariant", "ThirdParameter", typeMapper)
                });

            var parameterValues = new Dictionary<string, object>
            {
                { "FirstInvariant", dbParameter },
                { "SecondInvariant", 18L },
                { "ThirdInvariant", null }
            };

            if (async)
            {
                await ((CommandFunc)commandDelegate)(fakeConnection, relationalCommand, parameterValues, null);
            }
            else
            {
                ((CommandAction)commandDelegate)(fakeConnection, relationalCommand, parameterValues, null);
            }

            Assert.Equal(1, fakeConnection.DbConnections.Count);
            Assert.Equal(1, fakeConnection.DbConnections[0].DbCommands.Count);
            Assert.Equal(3, fakeConnection.DbConnections[0].DbCommands[0].Parameters.Count);

            var parameter = fakeConnection.DbConnections[0].DbCommands[0].Parameters[0];

            Assert.Equal(parameter, fakeConnection.DbConnections[0].DbCommands[0].Parameters[0]);

            parameter = fakeConnection.DbConnections[0].DbCommands[0].Parameters[1];
            var mapping = typeMapper.FindMapping(18L.GetType());

            Assert.Equal("SecondParameter", parameter.ParameterName);
            Assert.Equal(18L, parameter.Value);
            Assert.Equal(ParameterDirection.Input, parameter.Direction);
            Assert.False(parameter.IsNullable);
            Assert.Equal(mapping.DbType, parameter.DbType);

            parameter = fakeConnection.DbConnections[0].DbCommands[0].Parameters[2];

            Assert.Equal("ThirdParameter", parameter.ParameterName);
            Assert.Equal(DBNull.Value, parameter.Value);
            Assert.Equal(ParameterDirection.Input, parameter.Direction);
            Assert.Equal(FakeDbParameter.DefaultIsNullable, parameter.IsNullable);
            Assert.Equal(FakeDbParameter.DefaultDbType, parameter.DbType);
        }

        [ConditionalTheory]
        [MemberData(nameof(CommandActions))]
        public async Task Configures_DbCommand_with_composite_parameters(
            Delegate commandDelegate,
            string telemetryName,
            bool async)
        {
            var fakeConnection = CreateConnection();

            var relationalCommand = CreateRelationalCommand(
                parameters: new[]
                {
                    new CompositeRelationalParameter(
                        "CompositeInvariant",
                        new[]
                        {
                            new TypeMappedRelationalParameter(
                                "FirstInvariant", "FirstParameter", new IntTypeMapping("int", DbType.Int32), false),
                            new TypeMappedRelationalParameter(
                                "SecondInvariant", "SecondParameter", new LongTypeMapping("long", DbType.Int64), true),
                            new TypeMappedRelationalParameter("ThirdInvariant", "ThirdParameter", RelationalTypeMapping.NullMapping, null)
                        })
                });

            var parameterValues = new Dictionary<string, object>
            {
                { "CompositeInvariant", new object[] { 17, 18L, null } }
            };

            if (async)
            {
                await ((CommandFunc)commandDelegate)(fakeConnection, relationalCommand, parameterValues, null);
            }
            else
            {
                ((CommandAction)commandDelegate)(fakeConnection, relationalCommand, parameterValues, null);
            }

            Assert.Equal(1, fakeConnection.DbConnections.Count);
            Assert.Equal(1, fakeConnection.DbConnections[0].DbCommands.Count);
            Assert.Equal(3, fakeConnection.DbConnections[0].DbCommands[0].Parameters.Count);

            var parameter = fakeConnection.DbConnections[0].DbCommands[0].Parameters[0];

            Assert.Equal("FirstParameter", parameter.ParameterName);
            Assert.Equal(17, parameter.Value);
            Assert.Equal(ParameterDirection.Input, parameter.Direction);
            Assert.False(parameter.IsNullable);
            Assert.Equal(DbType.Int32, parameter.DbType);

            parameter = fakeConnection.DbConnections[0].DbCommands[0].Parameters[1];

            Assert.Equal("SecondParameter", parameter.ParameterName);
            Assert.Equal(18L, parameter.Value);
            Assert.Equal(ParameterDirection.Input, parameter.Direction);
            Assert.True(parameter.IsNullable);
            Assert.Equal(DbType.Int64, parameter.DbType);

            parameter = fakeConnection.DbConnections[0].DbCommands[0].Parameters[2];

            Assert.Equal("ThirdParameter", parameter.ParameterName);
            Assert.Equal(DBNull.Value, parameter.Value);
            Assert.Equal(ParameterDirection.Input, parameter.Direction);
            Assert.Equal(FakeDbParameter.DefaultIsNullable, parameter.IsNullable);
            Assert.Equal(FakeDbParameter.DefaultDbType, parameter.DbType);
        }

        [ConditionalTheory]
        [MemberData(nameof(CommandActions))]
        public async Task Throws_when_composite_parameters_are_configured_and_value_is_missing(
            Delegate commandDelegate,
            string telemetryName,
            bool async)
        {
            var fakeConnection = CreateConnection();

            var relationalCommand = CreateRelationalCommand(
                parameters: new[]
                {
                    new CompositeRelationalParameter(
                        "CompositeInvariant",
                        new[]
                        {
                            new TypeMappedRelationalParameter(
                                "FirstInvariant", "FirstParameter", new IntTypeMapping("int", DbType.Int32), false),
                            new TypeMappedRelationalParameter(
                                "SecondInvariant", "SecondParameter", new LongTypeMapping("long", DbType.Int64), true),
                            new TypeMappedRelationalParameter("ThirdInvariant", "ThirdParameter", RelationalTypeMapping.NullMapping, null)
                        })
                });

            var parameterValues = new Dictionary<string, object>
            {
                { "CompositeInvariant", new object[] { 17, 18L } }
            };

            if (async)
            {
                Assert.Equal(
                    RelationalStrings.MissingParameterValue("ThirdInvariant"),
                    (await Assert.ThrowsAsync<InvalidOperationException>(
                        async ()
                            => await ((CommandFunc)commandDelegate)(fakeConnection, relationalCommand, parameterValues, null))).Message);
            }
            else
            {
                Assert.Equal(
                    RelationalStrings.MissingParameterValue("ThirdInvariant"),
                    Assert.Throws<InvalidOperationException>(
                            ()
                                => ((CommandAction)commandDelegate)(fakeConnection, relationalCommand, parameterValues, null))
                        .Message);
            }
        }

        [ConditionalTheory]
        [MemberData(nameof(CommandActions))]
        public async Task Throws_when_composite_parameters_are_configured_and_value_is_not_object_array(
            Delegate commandDelegate,
            string telemetryName,
            bool async)
        {
            var fakeConnection = CreateConnection();

            var relationalCommand = CreateRelationalCommand(
                parameters: new[]
                {
                    new CompositeRelationalParameter(
                        "CompositeInvariant",
                        new[]
                        {
                            new TypeMappedRelationalParameter(
                                "FirstInvariant", "FirstParameter", new IntTypeMapping("int", DbType.Int32), false)
                        })
                });

            var parameterValues = new Dictionary<string, object>
            {
                { "CompositeInvariant", 17 }
            };

            if (async)
            {
                Assert.Equal(
                    RelationalStrings.ParameterNotObjectArray("CompositeInvariant"),
                    (await Assert.ThrowsAsync<InvalidOperationException>(
                        async ()
                            => await ((CommandFunc)commandDelegate)(fakeConnection, relationalCommand, parameterValues, null))).Message);
            }
            else
            {
                Assert.Equal(
                    RelationalStrings.ParameterNotObjectArray("CompositeInvariant"),
                    Assert.Throws<InvalidOperationException>(
                            ()
                                => ((CommandAction)commandDelegate)(fakeConnection, relationalCommand, parameterValues, null))
                        .Message);
            }
        }

        [ConditionalTheory]
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
                    c => throw exception,
                    c => throw exception,
                    (c, cb) => throw exception,
                    (c, ct) => throw exception,
                    (c, ct) => throw exception,
                    (c, cb, ct) => throw exception));

            var optionsExtension = new FakeRelationalOptionsExtension().WithConnection(fakeDbConnection);

            var options = CreateOptions(optionsExtension);

            var fakeConnection = new FakeRelationalConnection(options);

            var relationalCommand = CreateRelationalCommand();

            if (async)
            {
                await Assert.ThrowsAsync<InvalidOperationException>(
                    async ()
                        => await ((CommandFunc)commandDelegate)(fakeConnection, relationalCommand, null, null));
            }
            else
            {
                Assert.Throws<InvalidOperationException>(
                    ()
                        => ((CommandAction)commandDelegate)(fakeConnection, relationalCommand, null, null));
            }

            Assert.Equal(1, fakeDbConnection.DbCommands[0].DisposeCount);
        }

        [ConditionalTheory]
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
                    c => throw exception,
                    c => throw exception,
                    (c, cb) => throw exception,
                    (c, ct) => throw exception,
                    (c, ct) => throw exception,
                    (c, cb, ct) => throw exception));

            var optionsExtension = new FakeRelationalOptionsExtension().WithConnection(fakeDbConnection);

            var options = CreateOptions(optionsExtension);

            var fakeConnection = new FakeRelationalConnection(options);

            var relationalCommand = CreateRelationalCommand();

            if (async)
            {
                await Assert.ThrowsAsync<InvalidOperationException>(
                    async ()
                        => await ((CommandFunc)commandDelegate)(fakeConnection, relationalCommand, null, null));

                Assert.Equal(1, fakeDbConnection.OpenAsyncCount);
            }
            else
            {
                Assert.Throws<InvalidOperationException>(
                    ()
                        => ((CommandAction)commandDelegate)(fakeConnection, relationalCommand, null, null));

                Assert.Equal(1, fakeDbConnection.OpenCount);
            }

            Assert.Equal(1, fakeDbConnection.CloseCount);
        }

        [ConditionalTheory]
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
                    c => throw exception,
                    c => throw exception,
                    (c, cb) => throw exception,
                    (c, ct) => throw exception,
                    (c, ct) => throw exception,
                    (c, cb, ct) => throw exception));

            var optionsExtension = new FakeRelationalOptionsExtension().WithConnection(fakeDbConnection);

            var options = CreateOptions(optionsExtension);

            var fakeConnection = new FakeRelationalConnection(options);

            var relationalCommand = CreateRelationalCommand();

            if (async)
            {
                await Assert.ThrowsAsync<InvalidOperationException>(
                    async ()
                        => await ((CommandFunc)commandDelegate)(fakeConnection, relationalCommand, null, null));

                Assert.Equal(1, fakeDbConnection.OpenAsyncCount);
            }
            else
            {
                Assert.Throws<InvalidOperationException>(
                    ()
                        => ((CommandAction)commandDelegate)(fakeConnection, relationalCommand, null, null));

                Assert.Equal(1, fakeDbConnection.OpenCount);
            }

            Assert.Equal(1, fakeDbConnection.CloseCount);
        }

        [ConditionalTheory]
        [MemberData(nameof(CommandActions))]
        public async Task Logs_commands_without_parameter_values(
            Delegate commandDelegate,
            string diagnosticName,
            bool async)
        {
            var options = CreateOptions();

            var logFactory = new ListLoggerFactory();

            var fakeConnection = new FakeRelationalConnection(options);

            var logger = new DiagnosticsLogger<DbLoggerCategory.Database.Command>(
                logFactory,
                new FakeLoggingOptions(false),
                new DiagnosticListener("Fake"),
                new TestRelationalLoggingDefinitions());

            var relationalCommand = CreateRelationalCommand(
                commandText: "Logged Command",
                parameters: new[]
                {
                    new TypeMappedRelationalParameter("FirstInvariant", "FirstParameter", new IntTypeMapping("int", DbType.Int32), false)
                });

            var parameterValues = new Dictionary<string, object>
            {
                { "FirstInvariant", 17 }
            };

            if (async)
            {
                await ((CommandFunc)commandDelegate)(fakeConnection, relationalCommand, parameterValues, logger);
            }
            else
            {
                ((CommandAction)commandDelegate)(fakeConnection, relationalCommand, parameterValues, logger);
            }

            Assert.Equal(4, logFactory.Log.Count);

            Assert.Equal(LogLevel.Debug, logFactory.Log[0].Level);
            Assert.Equal(LogLevel.Debug, logFactory.Log[1].Level);
            Assert.Equal(LogLevel.Information, logFactory.Log[2].Level);
            Assert.Equal(LogLevel.Debug, logFactory.Log[3].Level);

            foreach (var (_, _, message, _, _) in logFactory.Log.Skip(2))
            {
                Assert.EndsWith(
                    "[Parameters=[FirstParameter='?' (DbType = Int32)], CommandType='0', CommandTimeout='30']" + _eol +
                    "Logged Command",
                    message);
            }
        }

        [ConditionalTheory]
        [MemberData(nameof(CommandActions))]
        public async Task Logs_commands_parameter_values(
            Delegate commandDelegate,
            string diagnosticName,
            bool async)
        {
            var optionsExtension = new FakeRelationalOptionsExtension().WithConnectionString(ConnectionString);

            var options = CreateOptions(optionsExtension);

            var logFactory = new ListLoggerFactory();

            var fakeConnection = new FakeRelationalConnection(options);

            var logger = new DiagnosticsLogger<DbLoggerCategory.Database.Command>(
                logFactory,
                new FakeLoggingOptions(true),
                new DiagnosticListener("Fake"),
                new TestRelationalLoggingDefinitions());

            var relationalCommand = CreateRelationalCommand(
                commandText: "Logged Command",
                parameters: new[]
                {
                    new TypeMappedRelationalParameter("FirstInvariant", "FirstParameter", new IntTypeMapping("int", DbType.Int32), false)
                });

            var parameterValues = new Dictionary<string, object>
            {
                { "FirstInvariant", 17 }
            };

            if (async)
            {
                await ((CommandFunc)commandDelegate)(fakeConnection, relationalCommand, parameterValues, logger);
            }
            else
            {
                ((CommandAction)commandDelegate)(fakeConnection, relationalCommand, parameterValues, logger);
            }

            Assert.Equal(5, logFactory.Log.Count);
            Assert.Equal(LogLevel.Debug, logFactory.Log[0].Level);
            Assert.Equal(LogLevel.Debug, logFactory.Log[1].Level);
            Assert.Equal(LogLevel.Warning, logFactory.Log[2].Level);
            Assert.Equal(CoreResources.LogSensitiveDataLoggingEnabled(new TestLogger<TestRelationalLoggingDefinitions>()).GenerateMessage(), logFactory.Log[2].Message);

            Assert.Equal(LogLevel.Information, logFactory.Log[3].Level);
            Assert.Equal(LogLevel.Debug, logFactory.Log[4].Level);

            foreach (var (_, _, message, _, _) in logFactory.Log.Skip(3))
            {
                Assert.EndsWith(
                    "[Parameters=[FirstParameter='17'], CommandType='0', CommandTimeout='30']" + _eol +
                    "Logged Command",
                    message);
            }
        }

        [ConditionalTheory]
        [MemberData(nameof(CommandActions))]
        public async Task Reports_command_diagnostic(
            Delegate commandDelegate,
            DbCommandMethod diagnosticName,
            bool async)
        {
            var options = CreateOptions();

            var fakeConnection = new FakeRelationalConnection(options);

            var diagnostic = new List<Tuple<string, object>>();

            var logger = new DiagnosticsLogger<DbLoggerCategory.Database.Command>(
                new ListLoggerFactory(),
                new FakeLoggingOptions(false),
                new ListDiagnosticSource(diagnostic),
                new TestRelationalLoggingDefinitions());

            var relationalCommand = CreateRelationalCommand(
                parameters: new[]
                {
                    new TypeMappedRelationalParameter("FirstInvariant", "FirstParameter", new IntTypeMapping("int", DbType.Int32), false)
                });

            var parameterValues = new Dictionary<string, object>
            {
                { "FirstInvariant", 17 }
            };

            if (async)
            {
                await ((CommandFunc)commandDelegate)(fakeConnection, relationalCommand, parameterValues, logger);
            }
            else
            {
                ((CommandAction)commandDelegate)(fakeConnection, relationalCommand, parameterValues, logger);
            }

            Assert.Equal(4, diagnostic.Count);
            Assert.Equal(RelationalEventId.CommandCreating.Name, diagnostic[0].Item1);
            Assert.Equal(RelationalEventId.CommandCreated.Name, diagnostic[1].Item1);
            Assert.Equal(RelationalEventId.CommandExecuting.Name, diagnostic[2].Item1);
            Assert.Equal(RelationalEventId.CommandExecuted.Name, diagnostic[3].Item1);

            var beforeData = (CommandEventData)diagnostic[2].Item2;
            var afterData = (CommandExecutedEventData)diagnostic[3].Item2;

            Assert.Equal(fakeConnection.DbConnections[0].DbCommands[0], beforeData.Command);
            Assert.Equal(fakeConnection.DbConnections[0].DbCommands[0], afterData.Command);

            Assert.Equal(diagnosticName, beforeData.ExecuteMethod);
            Assert.Equal(diagnosticName, afterData.ExecuteMethod);

            Assert.Equal(async, beforeData.IsAsync);
            Assert.Equal(async, afterData.IsAsync);
        }

        [ConditionalTheory]
        [MemberData(nameof(CommandActions))]
        public async Task Reports_command_diagnostic_on_exception(
            Delegate commandDelegate,
            DbCommandMethod diagnosticName,
            bool async)
        {
            var exception = new InvalidOperationException();

            var fakeDbConnection = new FakeDbConnection(
                ConnectionString,
                new FakeCommandExecutor(
                    c => throw exception,
                    c => throw exception,
                    (c, cb) => throw exception,
                    (c, ct) => throw exception,
                    (c, ct) => throw exception,
                    (c, cb, ct) => throw exception));

            var optionsExtension = new FakeRelationalOptionsExtension().WithConnection(fakeDbConnection);

            var options = CreateOptions(optionsExtension);

            var diagnostic = new List<Tuple<string, object>>();

            var fakeConnection = new FakeRelationalConnection(options);

            var logger = new DiagnosticsLogger<DbLoggerCategory.Database.Command>(
                new ListLoggerFactory(),
                new FakeLoggingOptions(false),
                new ListDiagnosticSource(diagnostic),
                new TestRelationalLoggingDefinitions());

            var relationalCommand = CreateRelationalCommand(
                parameters: new[]
                {
                    new TypeMappedRelationalParameter("FirstInvariant", "FirstParameter", new IntTypeMapping("int", DbType.Int32), false)
                });

            var parameterValues = new Dictionary<string, object>
            {
                { "FirstInvariant", 17 }
            };

            if (async)
            {
                await Assert.ThrowsAsync<InvalidOperationException>(
                    async ()
                        => await ((CommandFunc)commandDelegate)(fakeConnection, relationalCommand, parameterValues, logger));
            }
            else
            {
                Assert.Throws<InvalidOperationException>(
                    ()
                        => ((CommandAction)commandDelegate)(fakeConnection, relationalCommand, parameterValues, logger));
            }

            Assert.Equal(4, diagnostic.Count);
            Assert.Equal(RelationalEventId.CommandCreating.Name, diagnostic[0].Item1);
            Assert.Equal(RelationalEventId.CommandCreated.Name, diagnostic[1].Item1);
            Assert.Equal(RelationalEventId.CommandExecuting.Name, diagnostic[2].Item1);
            Assert.Equal(RelationalEventId.CommandError.Name, diagnostic[3].Item1);

            var beforeData = (CommandEventData)diagnostic[2].Item2;
            var afterData = (CommandErrorEventData)diagnostic[3].Item2;

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

        private static IDbContextOptions CreateOptions(
            RelationalOptionsExtension optionsExtension = null)
        {
            var optionsBuilder = new DbContextOptionsBuilder();

            ((IDbContextOptionsBuilderInfrastructure)optionsBuilder)
                .AddOrUpdateExtension(
                    optionsExtension
                    ?? new FakeRelationalOptionsExtension().WithConnectionString(ConnectionString));

            return optionsBuilder.Options;
        }

        private class FakeLoggingOptions : ILoggingOptions
        {
            public FakeLoggingOptions(bool sensitiveDataLoggingEnabled)
            {
                IsSensitiveDataLoggingEnabled = sensitiveDataLoggingEnabled;
            }

            public void Initialize(IDbContextOptions options)
            {
            }

            public void Validate(IDbContextOptions options)
            {
            }

            public bool IsSensitiveDataLoggingEnabled { get; }
            public bool IsSensitiveDataLoggingWarned { get; set; }
            public WarningsConfiguration WarningsConfiguration => null;
        }

        private IRelationalCommand CreateRelationalCommand(
            string commandText = "Command Text",
            IReadOnlyList<IRelationalParameter> parameters = null)
            => new RelationalCommand(
                new RelationalCommandBuilderDependencies(
                    new TestRelationalTypeMappingSource(
                        TestServiceFactory.Instance.Create<TypeMappingSourceDependencies>(),
                        TestServiceFactory.Instance.Create<RelationalTypeMappingSourceDependencies>())),
                commandText,
                parameters ?? Array.Empty<IRelationalParameter>());
    }
}
